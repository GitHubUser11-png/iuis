using IUIS.Infrastructure.Projections.Student;

namespace IUIS.Application.Abstractions.StudentSelfService
{
    public interface IStudentProjectionDataSource
    {
        StudentProjectionSnapshot ReadStudentSources(
            string studentId);
    }
}
