namespace IUIS.Domain.Academic
{
    public enum CourseStatus
    {
        Unspecified = 0,
        Draft = 1,
        Active = 2,
        Inactive = 3,
        Retired = 4
    }

    public enum CurriculumStatus
    {
        Unspecified = 0,
        Draft = 1,
        Approved = 2,
        Active = 3,
        Superseded = 4,
        Retired = 5
    }

    public enum SubjectStatus
    {
        Unspecified = 0,
        Draft = 1,
        Active = 2,
        Inactive = 3,
        Retired = 4
    }

    public enum AcademicPeriodStatus
    {
        Unspecified = 0,
        Draft = 1,
        Scheduled = 2,
        EnrollmentOpen = 3,
        EnrollmentClosed = 4,
        InProgress = 5,
        Completed = 6,
        Cancelled = 7
    }

    public enum EnrollmentStatus
    {
        Unspecified = 0,
        Draft = 1,
        Submitted = 2,
        UnderReview = 3,
        ReturnedForCorrection = 4,
        Approved = 5,
        Rejected = 6,
        Withdrawn = 7,
        Cancelled = 8,
        Completed = 9
    }
}
