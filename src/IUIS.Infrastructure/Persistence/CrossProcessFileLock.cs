using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class CrossProcessFileLock : IDisposable
    {
        private readonly Mutex _mutex;
        private readonly FileStream _stream;
        private bool _disposed;

        private CrossProcessFileLock(Mutex mutex, FileStream stream)
        {
            _mutex = mutex;
            _stream = stream;
        }

        public static CrossProcessFileLock Acquire(string targetPath, TimeSpan timeout)
        {
            if (string.IsNullOrWhiteSpace(targetPath)) throw new ArgumentException("Target path is required.", nameof(targetPath));
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

            var canonical = Path.GetFullPath(targetPath).ToUpperInvariant();
            string name;
            using (var sha = SHA256.Create())
            {
                name = "Local\\IUIS_" + Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical)))
                    .Replace("/", "_").Replace("+", "-").TrimEnd('=');
            }

            var mutex = new Mutex(false, name);
            var acquired = false;
            try
            {
                try { acquired = mutex.WaitOne(timeout); }
                catch (AbandonedMutexException) { acquired = true; }
                if (!acquired) throw new TimeoutException("Timed out waiting for repository lock: " + canonical);

                var lockPath = canonical + ".lock";
                Directory.CreateDirectory(Path.GetDirectoryName(lockPath));
                var stream = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return new CrossProcessFileLock(mutex, stream);
            }
            catch
            {
                if (acquired) mutex.ReleaseMutex();
                mutex.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _stream.Dispose();
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}
