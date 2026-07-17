using System;
using System.Collections.Generic;

using IUIS.Domain.Common;

namespace IUIS.Domain.People
{
    public sealed class PersonName : IEquatable<PersonName>
    {
        public PersonName(
            string givenName,
            string middleName,
            string familyName,
            string suffix)
        {
            GivenName = TextNormalizer.Required(givenName, nameof(givenName), 100);
            MiddleName = TextNormalizer.Optional(middleName, nameof(middleName), 100);
            FamilyName = TextNormalizer.Required(familyName, nameof(familyName), 100);
            Suffix = TextNormalizer.Optional(suffix, nameof(suffix), 30);
        }

        public string GivenName { get; }

        public string MiddleName { get; }

        public string FamilyName { get; }

        public string Suffix { get; }

        public string DisplayName
        {
            get
            {
                var parts = new List<string> { GivenName };
                if (!string.IsNullOrEmpty(MiddleName))
                {
                    parts.Add(MiddleName);
                }

                parts.Add(FamilyName);
                var displayName = string.Join(" ", parts);

                if (!string.IsNullOrEmpty(Suffix))
                {
                    displayName += ", " + Suffix;
                }

                return displayName;
            }
        }

        public string SortName
        {
            get
            {
                var givenParts = new List<string> { GivenName };
                if (!string.IsNullOrEmpty(MiddleName))
                {
                    givenParts.Add(MiddleName);
                }

                var sortName = FamilyName + ", " + string.Join(" ", givenParts);
                if (!string.IsNullOrEmpty(Suffix))
                {
                    sortName += " " + Suffix;
                }

                return sortName;
            }
        }

        public bool Equals(PersonName other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(GivenName, other.GivenName)
                && StringComparer.OrdinalIgnoreCase.Equals(MiddleName, other.MiddleName)
                && StringComparer.OrdinalIgnoreCase.Equals(FamilyName, other.FamilyName)
                && StringComparer.OrdinalIgnoreCase.Equals(Suffix, other.Suffix);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PersonName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(GivenName);
                hashCode = (hashCode * 397) ^ GetOptionalHashCode(MiddleName);
                hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(FamilyName);
                hashCode = (hashCode * 397) ^ GetOptionalHashCode(Suffix);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        private static int GetOptionalHashCode(string value)
        {
            return value == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(value);
        }
    }
}
