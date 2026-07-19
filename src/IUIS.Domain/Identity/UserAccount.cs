using System;

using IUIS.Domain.Common;

namespace IUIS.Domain.Identity
{
    public sealed class UserAccount : EntityBase
    {
        private UserAccount()
        {
            LoginId = string.Empty;
            PersonRecordId = string.Empty;
            CredentialHash = string.Empty;
            SecurityStamp = string.Empty;
        }

        public UserAccount(
            string id,
            string loginId,
            PrimaryRole primaryRole,
            PersonRecordKind personRecordKind,
            string personRecordId,
            string credentialHash,
            string securityStamp,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(id, createdAtUtc, createdByUserId)
        {
            var parsedId = InstitutionIdentifier.Parse(id);
            if (!StringComparer.Ordinal.Equals(parsedId.Prefix, "USR"))
            {
                throw new DomainValidationException(
                    "User account IDs must use the USR prefix.");
            }

            LoginId = NormalizeLoginId(loginId);
            PrimaryRole = primaryRole;
            PersonRecordKind = personRecordKind;
            PersonRecordId = DomainGuard.RequiredIdentifier(
                personRecordId,
                nameof(personRecordId));
            CredentialHash = TextNormalizer.Required(
                credentialHash,
                nameof(credentialHash),
                2048);
            SecurityStamp = TextNormalizer.Required(
                securityStamp,
                nameof(securityStamp),
                128);
            Status = UserAccountStatus.Active;
            ValidateRoleCompatibility();
        }

        public string LoginId { get; private set; }

        public PrimaryRole PrimaryRole { get; private set; }

        public PersonRecordKind PersonRecordKind { get; private set; }

        public string PersonRecordId { get; private set; }

        public string CredentialHash { get; private set; }

        public string SecurityStamp { get; private set; }

        public UserAccountStatus Status { get; private set; }

        public bool MustChangePassword { get; private set; }

        public DateTime? CredentialExpiresAtUtc { get; private set; }

        public void ChangeLoginId(
            string loginId,
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedLoginId = NormalizeLoginId(loginId);
            var normalizedSecurityStamp = TextNormalizer.Required(
                newSecurityStamp,
                nameof(newSecurityStamp),
                128);

            RecordChange(changedAtUtc, changedByUserId);
            LoginId = normalizedLoginId;
            SecurityStamp = normalizedSecurityStamp;
        }

        public void ReplaceCredential(
            string credentialHash,
            string newSecurityStamp,
            bool mustChangePassword,
            DateTime? expiresAtUtc,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedCredentialHash = TextNormalizer.Required(
                credentialHash,
                nameof(credentialHash),
                2048);
            var normalizedSecurityStamp = TextNormalizer.Required(
                newSecurityStamp,
                nameof(newSecurityStamp),
                128);
            if (expiresAtUtc.HasValue)
            {
                DomainGuard.RequireUtc(expiresAtUtc.Value, nameof(expiresAtUtc));
            }

            RecordChange(changedAtUtc, changedByUserId);
            CredentialHash = normalizedCredentialHash;
            SecurityStamp = normalizedSecurityStamp;
            MustChangePassword = mustChangePassword;
            CredentialExpiresAtUtc = expiresAtUtc;
        }

        public void SetStatus(
            UserAccountStatus status,
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (!Enum.IsDefined(typeof(UserAccountStatus), status)
                || status == UserAccountStatus.Unspecified)
            {
                throw new DomainValidationException("User account status is invalid.");
            }

            var normalizedSecurityStamp = TextNormalizer.Required(
                newSecurityStamp,
                nameof(newSecurityStamp),
                128);

            RecordChange(changedAtUtc, changedByUserId);
            Status = status;
            SecurityStamp = normalizedSecurityStamp;
        }

        private void ValidateRoleCompatibility()
        {
            if (PrimaryRole == PrimaryRole.Student
                && PersonRecordKind != PersonRecordKind.Student)
            {
                throw new DomainValidationException(
                    "Student accounts must reference a Student record.");
            }

            if (PrimaryRole == PrimaryRole.EmployeeFaculty
                && PersonRecordKind != PersonRecordKind.EmployeeFaculty)
            {
                throw new DomainValidationException(
                    "Employee/Faculty accounts must reference an Employee/Faculty record.");
            }

            if (PrimaryRole == PrimaryRole.Administrator
                && PersonRecordKind == PersonRecordKind.Student)
            {
                throw new DomainValidationException(
                    "Administrator accounts cannot reference Student records.");
            }
        }

        private static string NormalizeLoginId(string value)
        {
            var normalized = TextNormalizer.Required(
                value,
                nameof(value),
                100).ToLowerInvariant();
            if (normalized.Contains(" "))
            {
                throw new DomainValidationException("Login ID cannot contain spaces.");
            }

            return normalized;
        }
    }
}
