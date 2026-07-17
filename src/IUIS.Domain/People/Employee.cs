using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Domain.People
{
    public sealed class Employee : PersonRecordBase
    {
        private Employee()
        {
            DepartmentCode = string.Empty;
            JobTitle = string.Empty;
        }

        public Employee(
            string id,
            PersonName name,
            ContactInformation contactInformation,
            PostalAddress postalAddress,
            InstitutionLocalDate? dateOfBirth,
            string departmentCode,
            string jobTitle,
            EmploymentCategory category,
            EmploymentStatus status,
            InstitutionLocalDate hireDate,
            InstitutionLocalDate statusEffectiveDate,
            InstitutionLocalDate? separationDate,
            bool teachingEligible,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                IdentifierPolicy.Require(id, IdentifierPrefixes.Employee, nameof(id)),
                PersonRecordKind.EmployeeFaculty,
                name,
                contactInformation,
                postalAddress,
                dateOfBirth,
                createdAtUtc,
                createdByUserId)
        {
            DepartmentCode = RequireDepartmentCode(departmentCode, nameof(departmentCode));
            JobTitle = TextNormalizer.Required(jobTitle, nameof(jobTitle), 120);
            Category = RequireCategory(category, nameof(category));
            Status = RequireStatus(status, nameof(status));
            HireDate = hireDate;
            StatusEffectiveDate = statusEffectiveDate;

            if (statusEffectiveDate < hireDate)
            {
                throw new DomainValidationException(
                    nameof(statusEffectiveDate) + " cannot be earlier than the hire date.");
            }

            ValidateSeparationDate(status, hireDate, statusEffectiveDate, separationDate);
            SeparationDate = separationDate;
            TeachingEligible = IsTerminal(status) ? false : teachingEligible;
        }

        public string EmployeeNumber
        {
            get { return Id; }
        }

        public string DepartmentCode { get; private set; }

        public string JobTitle { get; private set; }

        public EmploymentCategory Category { get; private set; }

        public EmploymentStatus Status { get; private set; }

        public InstitutionLocalDate HireDate { get; private set; }

        public InstitutionLocalDate StatusEffectiveDate { get; private set; }

        public InstitutionLocalDate? SeparationDate { get; private set; }

        public bool TeachingEligible { get; private set; }

        public void ChangeWorkAssignment(
            string departmentCode,
            string jobTitle,
            EmploymentCategory category,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotTerminal();

            var normalizedDepartmentCode = RequireDepartmentCode(
                departmentCode,
                nameof(departmentCode));
            var normalizedJobTitle = TextNormalizer.Required(jobTitle, nameof(jobTitle), 120);
            var normalizedCategory = RequireCategory(category, nameof(category));

            RecordChange(changedAtUtc, changedByUserId);
            DepartmentCode = normalizedDepartmentCode;
            JobTitle = normalizedJobTitle;
            Category = normalizedCategory;
        }

        public void SetTeachingEligibility(
            bool teachingEligible,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotTerminal();

            if (TeachingEligible == teachingEligible)
            {
                throw new DomainValidationException(
                    "The Employee already has the requested teaching-eligibility value.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            TeachingEligible = teachingEligible;
        }

        public void ChangeStatus(
            EmploymentStatus newStatus,
            InstitutionLocalDate effectiveDate,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RequireStatus(newStatus, nameof(newStatus));

            if (newStatus == Status)
            {
                throw new DomainValidationException("The Employee already has the requested status.");
            }

            if (!CanTransition(Status, newStatus))
            {
                throw new DomainValidationException(
                    "The Employee status cannot transition from " + Status + " to " + newStatus + ".");
            }

            if (effectiveDate < StatusEffectiveDate || effectiveDate < HireDate)
            {
                throw new DomainValidationException(
                    nameof(effectiveDate) + " cannot be earlier than the current status-effective or hire date.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            Status = newStatus;
            StatusEffectiveDate = effectiveDate;

            if (IsTerminal(newStatus))
            {
                SeparationDate = effectiveDate;
                TeachingEligible = false;
            }
            else
            {
                SeparationDate = null;
            }
        }

        public static bool CanTransition(
            EmploymentStatus currentStatus,
            EmploymentStatus newStatus)
        {
            if (currentStatus == EmploymentStatus.PendingActivation)
            {
                return newStatus == EmploymentStatus.Active
                    || newStatus == EmploymentStatus.Separated;
            }

            if (currentStatus == EmploymentStatus.Active)
            {
                return newStatus == EmploymentStatus.OnLeave
                    || newStatus == EmploymentStatus.Suspended
                    || newStatus == EmploymentStatus.Separated
                    || newStatus == EmploymentStatus.Retired;
            }

            if (currentStatus == EmploymentStatus.OnLeave)
            {
                return newStatus == EmploymentStatus.Active
                    || newStatus == EmploymentStatus.Separated
                    || newStatus == EmploymentStatus.Retired;
            }

            if (currentStatus == EmploymentStatus.Suspended)
            {
                return newStatus == EmploymentStatus.Active
                    || newStatus == EmploymentStatus.Separated;
            }

            return false;
        }

        private static EmploymentCategory RequireCategory(
            EmploymentCategory category,
            string parameterName)
        {
            if (category == EmploymentCategory.Unspecified
                || !Enum.IsDefined(typeof(EmploymentCategory), category))
            {
                throw new DomainValidationException(parameterName + " is invalid.");
            }

            return category;
        }

        private static EmploymentStatus RequireStatus(
            EmploymentStatus status,
            string parameterName)
        {
            if (status == EmploymentStatus.Unspecified
                || !Enum.IsDefined(typeof(EmploymentStatus), status))
            {
                throw new DomainValidationException(parameterName + " is invalid.");
            }

            return status;
        }

        private static bool IsTerminal(EmploymentStatus status)
        {
            return status == EmploymentStatus.Separated
                || status == EmploymentStatus.Retired;
        }

        private static void ValidateSeparationDate(
            EmploymentStatus status,
            InstitutionLocalDate hireDate,
            InstitutionLocalDate statusEffectiveDate,
            InstitutionLocalDate? separationDate)
        {
            if (IsTerminal(status))
            {
                if (!separationDate.HasValue)
                {
                    throw new DomainValidationException(
                        nameof(separationDate) + " is required for a terminal employment status.");
                }

                if (separationDate.Value < hireDate
                    || separationDate.Value < statusEffectiveDate)
                {
                    throw new DomainValidationException(
                        nameof(separationDate) + " cannot precede the hire or status-effective date.");
                }
            }
            else if (separationDate.HasValue)
            {
                throw new DomainValidationException(
                    nameof(separationDate) + " is allowed only for a terminal employment status.");
            }
        }

        private void EnsureNotTerminal()
        {
            if (IsTerminal(Status))
            {
                throw new DomainValidationException(
                    "The Employee cannot be changed after reaching a terminal employment status.");
            }
        }
    }
}
