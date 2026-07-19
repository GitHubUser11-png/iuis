using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Academic
{
    public sealed class Course : EntityBase
    {
        private Course()
        {
            Code = string.Empty;
            Name = string.Empty;
            DepartmentId = string.Empty;
        }

        public Course(
            string id,
            string code,
            string name,
            string departmentId,
            int durationYears,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "CRS", "Course"), createdAtUtc, createdByUserId)
        {
            Code = NormalizeCode(code);
            Name = TextNormalizer.Required(name, nameof(name), 200);
            DepartmentId = DomainGuard.RequiredIdentifier(departmentId, nameof(departmentId));
            DurationYears = RequireDurationYears(durationYears);
            Status = CourseStatus.Draft;
        }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public string DepartmentId { get; private set; }

        public int DurationYears { get; private set; }

        public CourseStatus Status { get; private set; }

        public void UpdateDetails(
            string name,
            string departmentId,
            int durationYears,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedName = TextNormalizer.Required(name, nameof(name), 200);
            var normalizedDepartmentId = DomainGuard.RequiredIdentifier(
                departmentId,
                nameof(departmentId));
            var normalizedDurationYears = RequireDurationYears(durationYears);

            RecordChange(changedAtUtc, changedByUserId);
            Name = normalizedName;
            DepartmentId = normalizedDepartmentId;
            DurationYears = normalizedDurationYears;
        }

        public void ChangeStatus(
            CourseStatus targetStatus,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            ValidateTransition(Status, targetStatus);
            RecordChange(changedAtUtc, changedByUserId);
            Status = targetStatus;
        }

        private static string NormalizeCode(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 30).ToUpperInvariant();
            for (var index = 0; index < normalized.Length; index++)
            {
                var character = normalized[index];
                if ((character >= 'A' && character <= 'Z')
                    || (character >= '0' && character <= '9')
                    || character == '-'
                    || character == '.')
                {
                    continue;
                }

                throw new DomainValidationException(
                    "Course code may contain only letters, digits, hyphens, and periods.");
            }

            return normalized;
        }

        private static int RequireDurationYears(int value)
        {
            if (value < 1 || value > 10)
            {
                throw new DomainValidationException(
                    "Course duration must be between 1 and 10 years.");
            }

            return value;
        }

        private static void ValidateTransition(CourseStatus current, CourseStatus target)
        {
            if (!Enum.IsDefined(typeof(CourseStatus), target)
                || target == CourseStatus.Unspecified
                || target == current)
            {
                throw new DomainValidationException("The requested Course status transition is invalid.");
            }

            var valid = (current == CourseStatus.Draft
                    && (target == CourseStatus.Active || target == CourseStatus.Retired))
                || (current == CourseStatus.Active
                    && (target == CourseStatus.Inactive || target == CourseStatus.Retired))
                || (current == CourseStatus.Inactive
                    && (target == CourseStatus.Active || target == CourseStatus.Retired));

            if (!valid)
            {
                throw new DomainValidationException(
                    "Course status cannot transition from " + current + " to " + target + ".");
            }
        }

        private static string RequireIdentifier(string value, string prefix, string entityName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
            {
                throw new DomainValidationException(
                    entityName + " IDs must use the " + prefix + " prefix.");
            }

            return identifier.Value;
        }
    }
}
