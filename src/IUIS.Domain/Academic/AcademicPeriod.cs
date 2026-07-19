using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Domain.Academic
{
    public sealed class AcademicPeriod : EntityBase
    {
        private AcademicPeriod()
        {
            Code = string.Empty;
            Name = string.Empty;
        }

        public AcademicPeriod(
            string id,
            string code,
            string name,
            InstitutionLocalDate enrollmentOpenDate,
            InstitutionLocalDate enrollmentCloseDate,
            InstitutionLocalDate startDate,
            InstitutionLocalDate endDate,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id), createdAtUtc, createdByUserId)
        {
            Code = NormalizeCode(code);
            Name = TextNormalizer.Required(name, nameof(name), 120);
            ValidateSchedule(
                enrollmentOpenDate,
                enrollmentCloseDate,
                startDate,
                endDate);
            EnrollmentOpenDate = enrollmentOpenDate;
            EnrollmentCloseDate = enrollmentCloseDate;
            StartDate = startDate;
            EndDate = endDate;
            Status = AcademicPeriodStatus.Draft;
        }

        public string Code { get; private set; }

        public string Name { get; private set; }

        public InstitutionLocalDate EnrollmentOpenDate { get; private set; }

        public InstitutionLocalDate EnrollmentCloseDate { get; private set; }

        public InstitutionLocalDate StartDate { get; private set; }

        public InstitutionLocalDate EndDate { get; private set; }

        public AcademicPeriodStatus Status { get; private set; }

        public void UpdateSchedule(
            string name,
            InstitutionLocalDate enrollmentOpenDate,
            InstitutionLocalDate enrollmentCloseDate,
            InstitutionLocalDate startDate,
            InstitutionLocalDate endDate,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != AcademicPeriodStatus.Draft)
            {
                throw new DomainValidationException(
                    "Academic Period schedule can be changed only while Draft.");
            }

            var normalizedName = TextNormalizer.Required(name, nameof(name), 120);
            ValidateSchedule(
                enrollmentOpenDate,
                enrollmentCloseDate,
                startDate,
                endDate);

            RecordChange(changedAtUtc, changedByUserId);
            Name = normalizedName;
            EnrollmentOpenDate = enrollmentOpenDate;
            EnrollmentCloseDate = enrollmentCloseDate;
            StartDate = startDate;
            EndDate = endDate;
        }

        public void TransitionTo(
            AcademicPeriodStatus targetStatus,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            ValidateTransition(Status, targetStatus);
            RecordChange(changedAtUtc, changedByUserId);
            Status = targetStatus;
        }

        private static void ValidateSchedule(
            InstitutionLocalDate enrollmentOpenDate,
            InstitutionLocalDate enrollmentCloseDate,
            InstitutionLocalDate startDate,
            InstitutionLocalDate endDate)
        {
            RequireSupportedYear(enrollmentOpenDate, nameof(enrollmentOpenDate));
            RequireSupportedYear(enrollmentCloseDate, nameof(enrollmentCloseDate));
            RequireSupportedYear(startDate, nameof(startDate));
            RequireSupportedYear(endDate, nameof(endDate));

            if (enrollmentCloseDate < enrollmentOpenDate)
            {
                throw new DomainValidationException(
                    "Enrollment close date cannot be earlier than enrollment open date.");
            }

            if (endDate < startDate)
            {
                throw new DomainValidationException(
                    "Academic Period end date cannot be earlier than start date.");
            }

            if (endDate < enrollmentOpenDate || endDate < enrollmentCloseDate)
            {
                throw new DomainValidationException(
                    "Enrollment dates cannot occur after the Academic Period end date.");
            }
        }

        private static void RequireSupportedYear(
            InstitutionLocalDate value,
            string parameterName)
        {
            if (value.Year < 2000 || value.Year > 9999)
            {
                throw new DomainValidationException(
                    parameterName + " must use an institution-supported year from 2000 onward.");
            }
        }

        private static void ValidateTransition(
            AcademicPeriodStatus current,
            AcademicPeriodStatus target)
        {
            if (!Enum.IsDefined(typeof(AcademicPeriodStatus), target)
                || target == AcademicPeriodStatus.Unspecified
                || target == current)
            {
                throw new DomainValidationException(
                    "The requested Academic Period status transition is invalid.");
            }

            var valid = (current == AcademicPeriodStatus.Draft
                    && (target == AcademicPeriodStatus.Scheduled
                        || target == AcademicPeriodStatus.Cancelled))
                || (current == AcademicPeriodStatus.Scheduled
                    && (target == AcademicPeriodStatus.EnrollmentOpen
                        || target == AcademicPeriodStatus.Cancelled))
                || (current == AcademicPeriodStatus.EnrollmentOpen
                    && (target == AcademicPeriodStatus.EnrollmentClosed
                        || target == AcademicPeriodStatus.Cancelled))
                || (current == AcademicPeriodStatus.EnrollmentClosed
                    && (target == AcademicPeriodStatus.InProgress
                        || target == AcademicPeriodStatus.Cancelled))
                || (current == AcademicPeriodStatus.InProgress
                    && target == AcademicPeriodStatus.Completed);

            if (!valid)
            {
                throw new DomainValidationException(
                    "Academic Period status cannot transition from "
                    + current + " to " + target + ".");
            }
        }

        private static string NormalizeCode(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 40).ToUpperInvariant();
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
                    "Academic Period code may contain only letters, digits, hyphens, and periods.");
            }

            return normalized;
        }

        private static string RequireIdentifier(string value)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, "APD"))
            {
                throw new DomainValidationException(
                    "Academic Period IDs must use the APD prefix.");
            }

            return identifier.Value;
        }
    }
}
