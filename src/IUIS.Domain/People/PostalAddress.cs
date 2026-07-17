using System;
using System.Collections.Generic;

using IUIS.Domain.Common;

namespace IUIS.Domain.People
{
    public sealed class PostalAddress : IEquatable<PostalAddress>
    {
        public PostalAddress(
            string addressLine1,
            string addressLine2,
            string barangay,
            string cityMunicipality,
            string province,
            string postalCode,
            string countryCode)
        {
            AddressLine1 = TextNormalizer.Required(addressLine1, nameof(addressLine1), 160);
            AddressLine2 = TextNormalizer.Optional(addressLine2, nameof(addressLine2), 160);
            Barangay = TextNormalizer.Optional(barangay, nameof(barangay), 120);
            CityMunicipality = TextNormalizer.Required(
                cityMunicipality,
                nameof(cityMunicipality),
                120);
            Province = TextNormalizer.Required(province, nameof(province), 120);
            PostalCode = TextNormalizer.Optional(postalCode, nameof(postalCode), 20);
            CountryCode = NormalizeCountryCode(countryCode);
        }

        public string AddressLine1 { get; }

        public string AddressLine2 { get; }

        public string Barangay { get; }

        public string CityMunicipality { get; }

        public string Province { get; }

        public string PostalCode { get; }

        public string CountryCode { get; }

        public string SingleLine
        {
            get
            {
                var parts = new List<string> { AddressLine1 };
                AddOptional(parts, AddressLine2);
                AddOptional(parts, Barangay);
                parts.Add(CityMunicipality);
                parts.Add(Province);
                AddOptional(parts, PostalCode);
                parts.Add(CountryCode);
                return string.Join(", ", parts);
            }
        }

        public bool Equals(PostalAddress other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualsText(AddressLine1, other.AddressLine1)
                && EqualsText(AddressLine2, other.AddressLine2)
                && EqualsText(Barangay, other.Barangay)
                && EqualsText(CityMunicipality, other.CityMunicipality)
                && EqualsText(Province, other.Province)
                && EqualsText(PostalCode, other.PostalCode)
                && EqualsText(CountryCode, other.CountryCode);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PostalAddress);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = GetTextHashCode(AddressLine1);
                hashCode = (hashCode * 397) ^ GetTextHashCode(AddressLine2);
                hashCode = (hashCode * 397) ^ GetTextHashCode(Barangay);
                hashCode = (hashCode * 397) ^ GetTextHashCode(CityMunicipality);
                hashCode = (hashCode * 397) ^ GetTextHashCode(Province);
                hashCode = (hashCode * 397) ^ GetTextHashCode(PostalCode);
                hashCode = (hashCode * 397) ^ GetTextHashCode(CountryCode);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return SingleLine;
        }

        private static string NormalizeCountryCode(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 2).ToUpperInvariant();
            if (normalized.Length != 2
                || normalized[0] < 'A'
                || normalized[0] > 'Z'
                || normalized[1] < 'A'
                || normalized[1] > 'Z')
            {
                throw new DomainValidationException(
                    "CountryCode must contain exactly two ASCII letters.");
            }

            return normalized;
        }

        private static void AddOptional(ICollection<string> parts, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add(value);
            }
        }

        private static bool EqualsText(string left, string right)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(left, right);
        }

        private static int GetTextHashCode(string value)
        {
            return value == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value);
        }
    }
}
