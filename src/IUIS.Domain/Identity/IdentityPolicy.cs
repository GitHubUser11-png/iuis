namespace IUIS.Domain.Identity
{
    public static class IdentityPolicy
    {
        public static bool IsCompatible(
            PrimaryRole primaryRole,
            SessionApplicationKind applicationKind)
        {
            if (primaryRole == PrimaryRole.Administrator)
            {
                return applicationKind == SessionApplicationKind.AdministratorApplication;
            }

            if (primaryRole == PrimaryRole.Student
                || primaryRole == PrimaryRole.EmployeeFaculty)
            {
                return applicationKind == SessionApplicationKind.UserApplication;
            }

            return false;
        }

        public static PersonRecordKind GetRequiredPersonRecordKind(PrimaryRole primaryRole)
        {
            switch (primaryRole)
            {
                case PrimaryRole.Student:
                    return PersonRecordKind.Student;
                case PrimaryRole.EmployeeFaculty:
                    return PersonRecordKind.EmployeeFaculty;
                default:
                    return PersonRecordKind.Unspecified;
            }
        }
    }
}
