using System;
using System.Security.Cryptography;

namespace IUIS.Infrastructure.Security
{
    public sealed class PasswordHasher
    {
        private const string Scheme = "pbkdf2-sha256";

        public string Hash(string password, int iterations)
        {
            ValidatePassword(password);
            if (iterations < 100000) throw new ArgumentOutOfRangeException(nameof(iterations));
            var salt = new byte[32];
            using (var random = RandomNumberGenerator.Create()) random.GetBytes(salt);
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var hash = derive.GetBytes(32);
                return Scheme + "$" + iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
            }
        }

        public bool Verify(string password, string encoded)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(encoded)) return false;
            var parts = encoded.Split('$');
            int iterations;
            if (parts.Length != 4 || parts[0] != Scheme || !int.TryParse(parts[1], out iterations) || iterations < 100000) return false;
            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);
                using (var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                    return FixedTimeEquals(expected, derive.GetBytes(expected.Length));
            }
            catch (FormatException) { return false; }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            var difference = 0;
            for (var i = 0; i < left.Length; i++) difference |= left[i] ^ right[i];
            return difference == 0;
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is required.", nameof(password));
        }
    }
}
