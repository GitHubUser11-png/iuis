using System;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class JsonInfrastructureOptions
    {
        public JsonInfrastructureOptions(string dataRoot)
        {
            if (string.IsNullOrWhiteSpace(dataRoot)) throw new ArgumentException("Data root is required.", nameof(dataRoot));
            DataRoot = System.IO.Path.GetFullPath(dataRoot);
            LockTimeout = TimeSpan.FromSeconds(30);
            TempFileMaximumAge = TimeSpan.FromHours(24);
        }

        public string DataRoot { get; private set; }
        public TimeSpan LockTimeout { get; set; }
        public TimeSpan TempFileMaximumAge { get; set; }
    }
}
