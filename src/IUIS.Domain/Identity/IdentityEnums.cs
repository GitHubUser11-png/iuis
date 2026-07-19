namespace IUIS.Domain.Identity
{
    public enum PrimaryRole
    {
        Unspecified = 0,
        Student = 1,
        EmployeeFaculty = 2,
        Administrator = 3
    }

    public enum PersonRecordKind
    {
        Unspecified = 0,
        Student = 1,
        EmployeeFaculty = 2
    }

    public enum UserAccountStatus
    {
        Unspecified = 0,
        PendingActivation = 1,
        Active = 2,
        Suspended = 3,
        Deactivated = 4,
        Archived = 5
    }

    public enum SessionApplicationKind
    {
        Unspecified = 0,
        UserApplication = 1,
        AdministratorApplication = 2
    }

    public enum SessionPurpose
    {
        Unspecified = 0,
        FullAccess = 1,
        FirstLoginPasswordChange = 2,
        PasswordResetCompletion = 3
    }

    public enum UserSessionStatus
    {
        Unspecified = 0,
        Active = 1,
        Revoked = 2,
        Expired = 3
    }
}
