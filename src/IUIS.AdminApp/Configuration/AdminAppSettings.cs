using System.Configuration;

namespace IUIS.AdminApp.Configuration
{
    internal static class AdminAppSettings
    {
        public static string ResolveDataRoot()
        {
            var configured = ConfigurationManager.AppSettings["DataRoot"];
            if (!string.IsNullOrWhiteSpace(configured))
                return configured.Trim();

            var environmentValue = System.Environment.GetEnvironmentVariable("IUIS_DATA_ROOT");
            if (!string.IsNullOrWhiteSpace(environmentValue))
                return environmentValue.Trim();

            return "..\\..\\..\\templates\\production-data";
        }
    }
}
