using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using IUIS.Application.Repositories;
using IUIS.Domain.Common;

namespace IUIS.Infrastructure.Persistence
{
    public interface IJsonRecordMapper<T> where T : class, IEntity
    {
        T FromJson(JsonElement element, JsonSerializerOptions options);
        JsonElement ToJson(T value, JsonSerializerOptions options);
    }

    public sealed class SystemTextJsonRecordMapper<T> : IJsonRecordMapper<T>
        where T : class, IEntity
    {
        public T FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<T>(element.GetRawText(), options);
            if (value == null)
                throw new InvalidOperationException("A persisted aggregate could not be hydrated.");
            return value;
        }

        public JsonElement ToJson(T value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return JsonSerializer.SerializeToElement(value, options);
        }
    }

    internal interface ITransactionParticipant<T> where T : class, IEntity
    {
        TransactionMutation PrepareMutation(
            IReadOnlyCollection<T> records,
            long expectedRevision,
            string updatedByUserId);
    }

    public class MappedJsonRepository<T> : IVersionedRepository<T>, ITransactionParticipant<T>
        where T : class, IEntity
    {
        private readonly JsonRepositoryStore _store;
        private readonly IJsonRecordMapper<T> _mapper;
        private readonly JsonSerializerOptions _json;

        public MappedJsonRepository(string repositoryName, JsonRepositoryStore store, IJsonRecordMapper<T> mapper)
        {
            if (string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("A repository name is required.", nameof(repositoryName));
            RepositoryName = repositoryName.Trim().ToLowerInvariant();
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _json = new JsonSerializerOptions
            { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public string RepositoryName { get; private set; }

        public RepositorySnapshot<T> Read()
        {
            var envelope = _store.Read<JsonElement>(RepositoryName);
            var records = envelope.Records.Select(item => _mapper.FromJson(item, _json)).ToList();
            return new RepositorySnapshot<T>(RepositoryName, envelope.Revision, records);
        }

        public T FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An aggregate ID is required.", nameof(id));
            return Read().Records.SingleOrDefault(
                item => string.Equals(item.Id, id.Trim(), StringComparison.Ordinal));
        }

        public void Write(IReadOnlyCollection<T> records, long expectedRevision, string updatedByUserId)
        {
            var envelope = CreateEnvelope(records, expectedRevision, updatedByUserId, false);
            _store.Write(RepositoryName, envelope, expectedRevision);
        }

        TransactionMutation ITransactionParticipant<T>.PrepareMutation(
            IReadOnlyCollection<T> records, long expectedRevision, string updatedByUserId)
        {
            var current = _store.Read<JsonElement>(RepositoryName);
            if (current.Revision != expectedRevision)
                throw new InvalidOperationException("Repository revision conflict for " + RepositoryName + ".");
            var envelope = CreateEnvelope(records, expectedRevision, updatedByUserId, true);
            return new TransactionMutation(
                RepositoryName,
                JsonSerializer.Serialize(envelope, _json),
                expectedRevision);
        }

        private RepositoryEnvelope<JsonElement> CreateEnvelope(
            IReadOnlyCollection<T> records, long expectedRevision,
            string updatedByUserId, bool incrementRevision)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));
            if (expectedRevision < 0) throw new ArgumentOutOfRangeException(nameof(expectedRevision));
            if (string.IsNullOrWhiteSpace(updatedByUserId))
                throw new ArgumentException("An updating User ID is required.", nameof(updatedByUserId));
            return new RepositoryEnvelope<JsonElement>
            {
                Repository = RepositoryName,
                SchemaVersion = 1,
                Revision = incrementRevision ? checked(expectedRevision + 1) : expectedRevision,
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedByUserId = updatedByUserId.Trim(),
                Records = records.Select(item => _mapper.ToJson(item, _json)).ToList()
            };
        }
    }

    public sealed class JournaledApplicationTransactionCoordinator : IApplicationTransactionCoordinator
    {
        private readonly JournaledTransactionCoordinator _coordinator;
        public JournaledApplicationTransactionCoordinator(JournaledTransactionCoordinator coordinator)
        { _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator)); }

        public string Execute(Action<IRepositoryTransactionScope> stageMutations)
        {
            if (stageMutations == null) throw new ArgumentNullException(nameof(stageMutations));
            var scope = new Scope();
            stageMutations(scope);
            return _coordinator.Execute(scope.Mutations);
        }

        private sealed class Scope : IRepositoryTransactionScope
        {
            private readonly List<TransactionMutation> _mutations = new List<TransactionMutation>();
            public IEnumerable<TransactionMutation> Mutations { get { return _mutations; } }

            public void Stage<T>(IVersionedRepository<T> repository,
                IReadOnlyCollection<T> records, long expectedRevision,
                string updatedByUserId) where T : class, IEntity
            {
                if (repository == null) throw new ArgumentNullException(nameof(repository));
                var participant = repository as ITransactionParticipant<T>;
                if (participant == null)
                    throw new InvalidOperationException(
                        "The repository does not support journaled application transactions.");
                _mutations.Add(participant.PrepareMutation(records, expectedRevision, updatedByUserId));
            }
        }
    }
}
