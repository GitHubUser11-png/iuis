using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class AtomicFileWriter
    {
        public void WriteUtf8(string targetPath, string content)
        {
            if (string.IsNullOrWhiteSpace(targetPath)) throw new ArgumentException("Target path is required.", nameof(targetPath));
            if (content == null) throw new ArgumentNullException(nameof(content));

            targetPath = Path.GetFullPath(targetPath);
            var directory = Path.GetDirectoryName(targetPath);
            Directory.CreateDirectory(directory);
            var tempPath = Path.Combine(directory, "." + Path.GetFileName(targetPath) + "." + Guid.NewGuid().ToString("N") + ".tmp");
            var backupPath = targetPath + ".bak";

            try
            {
                var bytes = new UTF8Encoding(false).GetBytes(content);
                using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (!HashesEqual(bytes, File.ReadAllBytes(tempPath)))
                    throw new IOException("Atomic write verification failed before publication.");

                if (File.Exists(targetPath))
                {
                    File.Replace(tempPath, targetPath, backupPath, true);
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                }
                else
                {
                    File.Move(tempPath, targetPath);
                }
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                if (File.Exists(backupPath)) File.Delete(backupPath);
            }
        }

        public static string ComputeSha256(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static bool HashesEqual(byte[] left, byte[] right)
        {
            using (var sha = SHA256.Create())
            {
                var a = sha.ComputeHash(left);
                var b = sha.ComputeHash(right);
                if (a.Length != b.Length) return false;
                var diff = 0;
                for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
                return diff == 0;
            }
        }
    }
}
