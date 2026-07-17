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
            LoginId = NormalizeLoginId(loginId);
            PrimaryRole = primaryRole;
            PersonRecordKind = personRecordKind;
            PersonRecordId = DomainGuard.RequiredIdentifier(personRecordId, nameof(personRecordId));
            CredentialHash = TextNormalizer.Required(credentialHash, nameof(credentialHash), 2048);
            SecurityStamp = TextNormalizer.Required(securityStamp, nameof(securityStamp), 128);
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

        public void ChangeLoginId(string loginId, string newSecurityStamp, DateTime changedAtUtc, string changedByUserId)
        {
            LoginId = NormalizeLoginId(loginId);
            SecurityStamp = TextNormalizer.Required(newSecurityStamp, nameof(newSecurityStamp), 128);
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void ReplaceCredential(string credentialHash, string newSecurityStamp, bool mustChangePassword, DateTime? expiresAtUtc, DateTime changedAtUtc, string changedByUserId)
        {
            CredentialHash = TextNormalizer.Required(credentialHash, nameof(credentialHash), 2048);
            SecurityStamp = TextNormalizer.Required(newSecurityStamp, nameof(newSecurityStamp), 128);
            if (expiresAtUtc.HasValue)
            {
                DomainGuard.RequireUtc(expiresAtUtc.Value, nameof(expiresAtUtc));
            }

            MustChangePassword = mustChangePassword;
            CredentialExpiresAtUtc = expiresAtUtc;
            RecordChange(changedAtUtc, changedByUserId);
        }

        public void SetStatus(UserAccountStatus status, string newSecurityStamp, DateTime changedAtUtc, string changedByUserId)
        {
            Status = status;
            SecurityStamp = TextNormalizer.Required(newSecurityStamp, nameof(newSecurityStamp), 128);
            RecordChange(changedAtUtc, changedByUserId);
        }

        private void ValidateRoleCompatibility()
        {
            if (PrimaryRole == PrimaryRole.Student && PersonRecordKind != PersonRecordKind.Student)
            {
                throw new DomainValidationException("Student accounts must reference a Student record.");
            }

            if (PrimaryRole == PrimaryRole.EmployeeFaculty && PersonRecordKind != PersonRecordKind.Employee)
            {
                throw new DomainValidationException("Employee/Faculty accounts must reference an Employee record.");
            }
        }

        private static string NormalizeLoginId(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 100).ToLowerInvariant();
            if (normalized.Contains(" "))
            {
                throw new DomainValidationException("Login ID cannot contain spaces.");
            }

            return normalized;
        }
    }
}
