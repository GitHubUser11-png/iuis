using System;

using IUIS.Application.Repositories;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Infrastructure.Identity
{
    public sealed class ApplicationIdentifierAllocator
        : IApplicationIdentifierAllocator
    {
        private readonly CentralIdSequenceService _sequences;

        public ApplicationIdentifierAllocator(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _sequences = new CentralIdSequenceService(
                catalog ?? throw new ArgumentNullException(nameof(catalog)),
                options ?? throw new ArgumentNullException(nameof(options)));
        }

        public string Allocate(
            string prefix,
            int year,
            string actorUserId)
        {
            return _sequences.Allocate(prefix, year, actorUserId);
        }
    }
}
