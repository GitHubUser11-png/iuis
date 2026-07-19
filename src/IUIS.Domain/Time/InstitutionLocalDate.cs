using System;
using System.Globalization;

using IUIS.Domain.Common;

namespace IUIS.Domain.Time
{
    public struct InstitutionLocalDate :
        IComparable<InstitutionLocalDate>,
        IEquatable<InstitutionLocalDate>
    {
        private const string CanonicalFormat = "yyyy-MM-dd";
        private readonly DateTime _value;

        public InstitutionLocalDate(int year, int month, int day)
        {
            try
            {
                _value = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new DomainValidationException(
                    "InstitutionLocalDate requires a valid calendar date.",
                    exception);
            }
        }

        private InstitutionLocalDate(DateTime value)
        {
            _value = DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);
        }

        public int Year
        {
            get { return _value.Year; }
        }

        public int Month
        {
            get { return _value.Month; }
        }

        public int Day
        {
            get { return _value.Day; }
        }

        public static InstitutionLocalDate FromDateTime(DateTime value)
        {
            return new InstitutionLocalDate(value);
        }

        public static InstitutionLocalDate Parse(string value)
        {
            InstitutionLocalDate parsed;
            if (!TryParse(value, out parsed))
            {
                throw new DomainValidationException(
                    "InstitutionLocalDate must use the yyyy-MM-dd format and contain a valid date.");
            }

            return parsed;
        }

        public static bool TryParse(string value, out InstitutionLocalDate result)
        {
            DateTime parsed;
            if (!DateTime.TryParseExact(
                value,
                CanonicalFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed))
            {
                result = default(InstitutionLocalDate);
                return false;
            }

            result = new InstitutionLocalDate(parsed);
            return true;
        }

        public DateTime ToDateTimeUnspecified()
        {
            return DateTime.SpecifyKind(_value, DateTimeKind.Unspecified);
        }

        public int CompareTo(InstitutionLocalDate other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(InstitutionLocalDate other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is InstitutionLocalDate && Equals((InstitutionLocalDate)obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString(CanonicalFormat, CultureInfo.InvariantCulture);
        }

        public static bool operator ==(
            InstitutionLocalDate left,
            InstitutionLocalDate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            InstitutionLocalDate left,
            InstitutionLocalDate right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(
            InstitutionLocalDate left,
            InstitutionLocalDate right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(
            InstitutionLocalDate left,
            InstitutionLocalDate right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(
            InstitutionLocalDate left,
            InstitutionLocalDate right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(
            InstitutionLocalDate left,
            InstitutionLocalDate right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}
