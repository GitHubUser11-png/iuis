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
            SecurityStamp = string.Empty;
            StatusChangedByUserId = string.Empty;
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
                throw new DomainValidationException("User account IDs must use the USR prefix.");
            }

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

            if (PrimaryRole == PrimaryRole.EmployeeFaculty && PersonRecordKind != PersonRecordKind.EmployeeFaculty)
            {
                throw new DomainValidationException("Employee/Faculty accounts must reference an Employee/Faculty record.");
            }

            if (PrimaryRole == PrimaryRole.Administrator && PersonRecordKind == PersonRecordKind.Student)
            {
                throw new DomainValidationException("Administrator accounts cannot reference Student records.");
            }
        }

        private static string NormalizeLoginId(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 100).ToLowerInvariant();
            if (normalized.Contains(" "))
            {
                throw new DomainValidationException("Login ID cannot contain spaces.");
            UserAccountStatus status,
            string securityStamp,
            bool mustChangePassword,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                IdentifierPolicy.Require(id, IdentifierPrefixes.User, nameof(id)),
                createdAtUtc,
                createdByUserId)
        {
            PrimaryRole = RequireRole(primaryRole, nameof(primaryRole));
            ValidatePersonLink(PrimaryRole, personRecordKind, personRecordId);

            LoginId = NormalizeLoginId(loginId, nameof(loginId));
            PersonRecordKind = personRecordKind;
            PersonRecordId = NormalizePersonRecordId(personRecordKind, personRecordId);
            Status = RequireInitialStatus(status, nameof(status));
            SecurityStamp = RequireOpaqueSecurityValue(
                securityStamp,
                nameof(securityStamp),
                16,
                256);
            MustChangePassword = mustChangePassword;
            StatusChangedAtUtc = createdAtUtc;
            StatusChangedByUserId = createdByUserId;
        }

        public string LoginId { get; private set; }

        public PrimaryRole PrimaryRole { get; private set; }

        public PersonRecordKind PersonRecordKind { get; private set; }

        public string PersonRecordId { get; private set; }

        public UserAccountStatus Status { get; private set; }

        public string StatusReason { get; private set; }

        public DateTime StatusChangedAtUtc { get; private set; }

        public string StatusChangedByUserId { get; private set; }

        public string SecurityStamp { get; private set; }

        public bool MustChangePassword { get; private set; }

        public void ChangeLoginId(
            string loginId,
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotDeactivated();

            var normalizedLoginId = NormalizeLoginId(loginId, nameof(loginId));
            if (StringComparer.Ordinal.Equals(LoginId, normalizedLoginId))
            {
                throw new DomainValidationException("The User account already has the requested Login ID.");
            }

            var normalizedSecurityStamp = RequireOpaqueSecurityValue(
                newSecurityStamp,
                nameof(newSecurityStamp),
                16,
                256);

            RecordChange(changedAtUtc, changedByUserId);
            LoginId = normalizedLoginId;
            SecurityStamp = normalizedSecurityStamp;
        }

        public void Activate(
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != UserAccountStatus.PendingActivation
                && Status != UserAccountStatus.Suspended)
            {
                throw new DomainValidationException(
                    "Only pending or suspended User accounts can be activated.");
            }

            ChangeStatus(
                UserAccountStatus.Active,
                null,
                newSecurityStamp,
                changedAtUtc,
                changedByUserId);
        }

        public void Suspend(
            string reason,
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != UserAccountStatus.Active)
            {
                throw new DomainValidationException("Only active User accounts can be suspended.");
            }

            ChangeStatus(
                UserAccountStatus.Suspended,
                TextNormalizer.Required(reason, nameof(reason), 250),
                newSecurityStamp,
                changedAtUtc,
                changedByUserId);
        }

        public void Deactivate(
            string reason,
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status == UserAccountStatus.Deactivated
                || Status == UserAccountStatus.Archived)
            {
                throw new DomainValidationException("The User account is already inactive.");
            }

            ChangeStatus(
                UserAccountStatus.Deactivated,
                TextNormalizer.Required(reason, nameof(reason), 250),
                newSecurityStamp,
                changedAtUtc,
                changedByUserId);
        }

        public void RequirePasswordChange(
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotDeactivated();

            if (MustChangePassword)
            {
                throw new DomainValidationException(
                    "The User account already requires a password change.");
            }

            var normalizedSecurityStamp = RequireOpaqueSecurityValue(
                newSecurityStamp,
                nameof(newSecurityStamp),
                16,
                256);

            RecordChange(changedAtUtc, changedByUserId);
            MustChangePassword = true;
            SecurityStamp = normalizedSecurityStamp;
        }

        public void CompletePasswordChange(
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotDeactivated();

            if (!MustChangePassword)
            {
                throw new DomainValidationException(
                    "The User account does not currently require a password change.");
            }

            var normalizedSecurityStamp = RequireOpaqueSecurityValue(
                newSecurityStamp,
                nameof(newSecurityStamp),
                16,
                256);

            RecordChange(changedAtUtc, changedByUserId);
            MustChangePassword = false;
            SecurityStamp = normalizedSecurityStamp;
        }

        public void RotateSecurityStamp(
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotDeactivated();

            var normalizedSecurityStamp = RequireOpaqueSecurityValue(
                newSecurityStamp,
                nameof(newSecurityStamp),
                16,
                256);

            if (StringComparer.Ordinal.Equals(SecurityStamp, normalizedSecurityStamp))
            {
                throw new DomainValidationException(
                    "The replacement Security Stamp must differ from the current value.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            SecurityStamp = normalizedSecurityStamp;
        }

        private static PrimaryRole RequireRole(PrimaryRole role, string parameterName)
        {
            if (role == PrimaryRole.Unspecified
                || !Enum.IsDefined(typeof(PrimaryRole), role))
            {
                throw new DomainValidationException(parameterName + " is invalid.");
            }

            return role;
        }

        private static UserAccountStatus RequireInitialStatus(
            UserAccountStatus status,
            string parameterName)
        {
            if (status != UserAccountStatus.PendingActivation
                && status != UserAccountStatus.Active
                && status != UserAccountStatus.Suspended
                && status != UserAccountStatus.Deactivated)
            {
                throw new DomainValidationException(parameterName + " is invalid for a new User account.");
            }

            return status;
        }

        private static string NormalizeLoginId(string value, string parameterName)
        {
            var normalized = TextNormalizer.Required(value, parameterName, 100).ToLowerInvariant();
            if (normalized.Length < 3)
            {
                throw new DomainValidationException(parameterName + " must contain at least 3 characters.");
            }

            for (var index = 0; index < normalized.Length; index++)
            {
                var character = normalized[index];
                var isLetter = character >= 'a' && character <= 'z';
                var isDigit = character >= '0' && character <= '9';
                var isAllowedSymbol = character == '.'
                    || character == '_'
                    || character == '-'
                    || character == '@';

                if (!isLetter && !isDigit && !isAllowedSymbol)
                {
                    throw new DomainValidationException(
                        parameterName + " contains an unsupported character.");
                }
            }

            return normalized;
        }

        private static void ValidatePersonLink(
            PrimaryRole role,
            PersonRecordKind personRecordKind,
            string personRecordId)
        {
            var requiredKind = IdentityPolicy.GetRequiredPersonRecordKind(role);
            if (requiredKind != PersonRecordKind.Unspecified)
            {
                if (personRecordKind != requiredKind)
                {
                    throw new DomainValidationException(
                        "The User account role requires a matching person-record kind.");
                }

                NormalizePersonRecordId(personRecordKind, personRecordId);
                return;
            }

            var hasPersonId = !string.IsNullOrWhiteSpace(personRecordId);
            if (personRecordKind == PersonRecordKind.Unspecified && !hasPersonId)
            {
                return;
            }

            if (role == PrimaryRole.Administrator
                && personRecordKind == PersonRecordKind.EmployeeFaculty
                && hasPersonId)
            {
                NormalizePersonRecordId(personRecordKind, personRecordId);
                return;
            }

            throw new DomainValidationException(
                "Administrator accounts may be unlinked or linked only to an Employee/Faculty record.");
        }

        private static string NormalizePersonRecordId(
            PersonRecordKind personRecordKind,
            string personRecordId)
        {
            if (personRecordKind == PersonRecordKind.Unspecified)
            {
                if (!string.IsNullOrWhiteSpace(personRecordId))
                {
                    throw new DomainValidationException(
                        nameof(personRecordId) + " must be empty when no person record kind is selected.");
                }

                return null;
            }

            if (personRecordKind == PersonRecordKind.Student)
            {
                return IdentifierPolicy.Require(
                    personRecordId,
                    IdentifierPrefixes.Student,
                    nameof(personRecordId));
            }

            if (personRecordKind == PersonRecordKind.EmployeeFaculty)
            {
                return IdentifierPolicy.Require(
                    personRecordId,
                    IdentifierPrefixes.Employee,
                    nameof(personRecordId));
            }

            throw new DomainValidationException(nameof(personRecordKind) + " is invalid.");
        }

        private static string RequireOpaqueSecurityValue(
            string value,
            string parameterName,
            int minimumLength,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainValidationException(parameterName + " is required.");
            }

            var normalized = value.Trim();
            if (normalized.Length < minimumLength || normalized.Length > maximumLength)
            {
                throw new DomainValidationException(
                    parameterName + " must contain between " + minimumLength + " and " + maximumLength + " characters.");
            }

            for (var index = 0; index < normalized.Length; index++)
            {
                if (char.IsWhiteSpace(normalized[index]))
                {
                    throw new DomainValidationException(parameterName + " cannot contain whitespace.");
                }
            }

            return normalized;
        }

        private void ChangeStatus(
            UserAccountStatus status,
            string reason,
            string newSecurityStamp,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            var normalizedSecurityStamp = RequireOpaqueSecurityValue(
                newSecurityStamp,
                nameof(newSecurityStamp),
                16,
                256);

            RecordChange(changedAtUtc, changedByUserId);
            Status = status;
            StatusReason = reason;
            StatusChangedAtUtc = changedAtUtc;
            StatusChangedByUserId = changedByUserId;
            SecurityStamp = normalizedSecurityStamp;
        }

        private void EnsureNotDeactivated()
        {
            if (Status == UserAccountStatus.Deactivated
                || Status == UserAccountStatus.Archived)
            {
                throw new DomainValidationException("The User account is inactive.");
            }
        }
    }
}
