using IUIS.Domain.Projections.Student;

namespace IUIS.Infrastructure.Projections.Student
{
    public sealed class StudentProjectionDataSource : IStudentProjectionDataSource
    {
        public StudentProjectionSnapshot ReadStudentSources(string studentId)
        {
            // TODO: Implement actual data loading from JSON repositories
            // This is a stub to allow the build to succeed
            return new StudentProjectionSnapshot
            {
                CapturedAtUtc = System.DateTime.UtcNow
            };
        }
    }
}
