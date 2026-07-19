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