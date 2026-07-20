using System;
using System.Security.Cryptography;
using System.Text;

namespace IUIS.Infrastructure.Security
{
    public sealed class SessionTokenProtector
    {
        public const int CurrentDigestVersion = 1;
        private const string Prefix = "sha256:";

        public string IssueRawToken()
        {
            var bytes = new byte[32];
            using (var random = RandomNumberGenerator.Create())
                random.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public string ComputeDigest(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                throw new ArgumentException("A raw session token is required.", nameof(rawToken));
            var bytes = Encoding.UTF8.GetBytes(rawToken.Trim());
            using (var sha = SHA256.Create())
                return Prefix + Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        public bool Verify(string storedDigest, string presentedRawToken)
        {
            if (string.IsNullOrWhiteSpace(storedDigest)
                || string.IsNullOrWhiteSpace(presentedRawToken)
                || !storedDigest.StartsWith(Prefix, StringComparison.Ordinal))
                return false;

            string computed;
            try
            {
                computed = ComputeDigest(presentedRawToken);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return FixedTimeEquals(storedDigest, computed);
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left ?? string.Empty);
            var rightBytes = Encoding.UTF8.GetBytes(right ?? string.Empty);
            var difference = leftBytes.Length ^ rightBytes.Length;
            var length = Math.Max(leftBytes.Length, rightBytes.Length);
            for (var index = 0; index < length; index++)
            {
                var leftByte = index < leftBytes.Length ? leftBytes[index] : (byte)0;
                var rightByte = index < rightBytes.Length ? rightBytes[index] : (byte)0;
                difference |= leftByte ^ rightByte;
            }
            return difference == 0;
        }
    }
}
