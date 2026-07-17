namespace IUIS.Domain.People
{
    public enum StudentStatus
    {
        Unspecified = 0,
        PendingAdmission = 1,
        Active = 2,
        OnLeave = 3,
        Inactive = 4,
        Graduated = 5,
        Withdrawn = 6,
        Dismissed = 7
    }

    public enum EmploymentStatus
    {
        Unspecified = 0,
        PendingActivation = 1,
        Active = 2,
        OnLeave = 3,
        Suspended = 4,
        Separated = 5,
        Retired = 6
    }

    public enum EmploymentCategory
    {
        Unspecified = 0,
        Regular = 1,
        Contractual = 2,
        PartTime = 3,
        Visiting = 4
    }
}
