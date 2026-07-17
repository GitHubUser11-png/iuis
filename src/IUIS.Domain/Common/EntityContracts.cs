using System;

namespace IUIS.Domain.Common
{
    public interface IEntity
    {
        string Id { get; }
    }

    public interface IVersionedEntity
    {
        long Version { get; }
    }

    public interface IArchivableEntity
    {
        bool IsArchived { get; }

        DateTime? ArchivedAtUtc { get; }

        string ArchivedByUserId { get; }
    }
}
