using System;
using System.IO;
using System.Linq;

using IUIS.Application.Abstractions.Security;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Infrastructure.Presentation
{
    public sealed class StartupReadinessService : IStartupReadinessService
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;

        public StartupReadinessService(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public bool IsRepositoryReady(out string statusMessage)
        {
            statusMessage = "Repository ready.";
            if (!Directory.Exists(_options.DataRoot))
            {
                statusMessage = "Production repository has not been initialized.";
                return false;
            }

            var files = Directory.GetFiles(_options.DataRoot, "*.json", SearchOption.TopDirectoryOnly);
            if (files.Length != 49)
            {
                statusMessage = "Production repository is incomplete (" + files.Length + " of 49 files).";
                return false;
            }

            var usersPath = _catalog.ResolvePath(_options.DataRoot, "users");
            if (!File.Exists(usersPath))
            {
                statusMessage = "User repository is missing.";
                return false;
            }

            return true;
        }

        public bool RequiresBootstrap(out string statusMessage)
        {
            statusMessage = "Bootstrap complete.";
            if (!IsRepositoryReady(out statusMessage))
                return true;

            try
            {
                var usersPath = _catalog.ResolvePath(_options.DataRoot, "users");
                var content = File.ReadAllText(usersPath);
                if (content.Contains("\"records\": []") || content.Contains("\"records\":[]"))
                {
                    statusMessage = "Administrator bootstrap is required.";
                    return true;
                }
            }
            catch
            {
                statusMessage = "Repository state could not be verified.";
                return true;
            }

            return false;
        }
    }
}
