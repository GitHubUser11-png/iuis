using System;

namespace IUIS.Domain.Common
{
    public abstract class EntityBase : IEntity, IVersionedEntity, IArchivableEntity
    {
        protected EntityBase()
        {
            Id = string.Empty;
            CreatedByUserId = string.Empty;
            UpdatedByUserId = string.Empty;
        }

        protected EntityBase(string id, DateTime createdAtUtc, string createdByUserId)
        {
            Id = DomainGuard.RequiredIdentifier(id, nameof(id));
            CreatedAtUtc = DomainGuard.RequireUtc(createdAtUtc, nameof(createdAtUtc));
            CreatedByUserId = DomainGuard.RequiredActorIdentifier(
                createdByUserId,
                nameof(createdByUserId));
            UpdatedAtUtc = CreatedAtUtc;
            UpdatedByUserId = CreatedByUserId;
            Version = 1L;
        }

        public string Id { get; protected set; }

        public long Version { get; protected set; }

        public bool IsArchived { get; protected set; }

        public DateTime CreatedAtUtc { get; protected set; }

        public string CreatedByUserId { get; protected set; }

        public DateTime UpdatedAtUtc { get; protected set; }

        public string UpdatedByUserId { get; protected set; }

        public DateTime? ArchivedAtUtc { get; protected set; }

        public string ArchivedByUserId { get; protected set; }

        protected void RestorePersistenceState(
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            if (version < 1L)
            {
                throw new DomainValidationException(
                    "Persisted entity versions must be greater than or equal to one.");
            }

            var normalizedCreatedAtUtc = DomainGuard.RequireUtc(
                createdAtUtc,
                nameof(createdAtUtc));
            var normalizedUpdatedAtUtc = DomainGuard.RequireUtc(
                updatedAtUtc,
                nameof(updatedAtUtc));
            DomainGuard.RequireChronological(
                normalizedCreatedAtUtc,
                normalizedUpdatedAtUtc,
                nameof(updatedAtUtc));

            var normalizedCreatedByUserId = DomainGuard.RequiredActorIdentifier(
                createdByUserId,
                nameof(createdByUserId));
            var normalizedUpdatedByUserId = DomainGuard.RequiredActorIdentifier(
                updatedByUserId,
                nameof(updatedByUserId));

            DateTime? normalizedArchivedAtUtc = null;
            string normalizedArchivedByUserId = null;
            if (isArchived)
            {
                if (!archivedAtUtc.HasValue)
                {
                    throw new DomainValidationException(
                        "Archived persisted entities require an archive timestamp.");
                }

                normalizedArchivedAtUtc = DomainGuard.RequireUtc(
                    archivedAtUtc.Value,
                    nameof(archivedAtUtc));
                DomainGuard.RequireChronological(
                    normalizedCreatedAtUtc,
                    normalizedArchivedAtUtc.Value,
                    nameof(archivedAtUtc));
                if (normalizedArchivedAtUtc.Value != normalizedUpdatedAtUtc)
                {
                    throw new DomainValidationException(
                        "An archived entity archive timestamp must equal its latest update timestamp.");
                }

                normalizedArchivedByUserId = DomainGuard.RequiredActorIdentifier(
                    archivedByUserId,
                    nameof(archivedByUserId));
                if (!StringComparer.Ordinal.Equals(
                    normalizedArchivedByUserId,
                    normalizedUpdatedByUserId))
                {
                    throw new DomainValidationException(
                        "An archived entity archive actor must equal its latest update actor.");
                }
            }
            else if (archivedAtUtc.HasValue || !string.IsNullOrWhiteSpace(archivedByUserId))
            {
                throw new DomainValidationException(
                    "Active persisted entities cannot retain archive metadata.");
            }

            Version = version;
            IsArchived = isArchived;
            CreatedAtUtc = normalizedCreatedAtUtc;
            CreatedByUserId = normalizedCreatedByUserId;
            UpdatedAtUtc = normalizedUpdatedAtUtc;
            UpdatedByUserId = normalizedUpdatedByUserId;
            ArchivedAtUtc = normalizedArchivedAtUtc;
            ArchivedByUserId = normalizedArchivedByUserId;
        }

        protected void RecordChange(DateTime changedAtUtc, string changedByUserId)
        {
            if (IsArchived)
            {
                throw new DomainValidationException("Archived entities cannot be changed.");
            }

            ApplyChangeMetadata(changedAtUtc, changedByUserId);
        }

        public virtual void Archive(DateTime archivedAtUtc, string archivedByUserId)
        {
            if (IsArchived)
            {
                throw new DomainValidationException("The entity is already archived.");
            }

            ApplyChangeMetadata(archivedAtUtc, archivedByUserId);
            IsArchived = true;
            ArchivedAtUtc = UpdatedAtUtc;
            ArchivedByUserId = UpdatedByUserId;
        }

        public virtual void Restore(DateTime restoredAtUtc, string restoredByUserId)
        {
            if (!IsArchived)
            {
                throw new DomainValidationException("Only archived entities can be restored.");
            }

            ApplyChangeMetadata(restoredAtUtc, restoredByUserId);
            IsArchived = false;
            ArchivedAtUtc = null;
            ArchivedByUserId = null;
        }

        private void ApplyChangeMetadata(DateTime changedAtUtc, string changedByUserId)
        {
            DomainGuard.RequireChronological(
                UpdatedAtUtc,
                changedAtUtc,
                nameof(changedAtUtc));

            UpdatedAtUtc = changedAtUtc;
            UpdatedByUserId = DomainGuard.RequiredActorIdentifier(
                changedByUserId,
                nameof(changedByUserId));
            Version = DomainGuard.IncrementVersion(Version);
        }
    }
}
