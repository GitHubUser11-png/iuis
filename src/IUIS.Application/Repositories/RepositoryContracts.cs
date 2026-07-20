using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;

namespace IUIS.Application.Repositories
{
    public sealed class RepositorySnapshot<T> where T : class, IEntity
    {
        private readonly IReadOnlyList<T> _records;

        public RepositorySnapshot(string repositoryName, long revision, IEnumerable<T> records)
        {
            if (string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("A repository name is required.", nameof(repositoryName));
            if (revision < 0) throw new ArgumentOutOfRangeException(nameof(revision));
            RepositoryName = repositoryName.Trim().ToLowerInvariant();
            Revision = revision;
            _records = (records ?? Enumerable.Empty<T>()).Where(item => item != null).ToList().AsReadOnly();
        }

        public string RepositoryName { get; private set; }
        public long Revision { get; private set; }
        public IReadOnlyList<T> Records { get { return _records; } }
    }

    public interface IVersionedRepository<T> where T : class, IEntity
    {
        string RepositoryName { get; }
        RepositorySnapshot<T> Read();
        T FindById(string id);
        void Write(IReadOnlyCollection<T> records, long expectedRevision, string updatedByUserId);
    }

    public interface IRepositoryTransactionScope
    {
        void Stage<T>(
            IVersionedRepository<T> repository,
            IReadOnlyCollection<T> records,
            long expectedRevision,
            string updatedByUserId) where T : class, IEntity;
    }

    public interface IApplicationTransactionCoordinator
    {
        string Execute(Action<IRepositoryTransactionScope> stageMutations);
    }
}
