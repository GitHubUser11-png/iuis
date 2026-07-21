using System.Configuration;

namespace IUIS.UserApp.Configuration
{
    internal static class UserAppSettings
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
