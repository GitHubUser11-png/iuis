using IUIS.Domain.Academic;
using IUIS.Domain.Clinic;
using IUIS.Domain.Counseling;
using IUIS.Domain.Discipline;
using IUIS.Domain.Finance;
using IUIS.Domain.Library;
using IUIS.Domain.People;

namespace IUIS.Application.Repositories
{
    public interface IStudentRecordRepository : IVersionedRepository<StudentRecord> { }
    public interface IEmployeeRecordRepository : IVersionedRepository<EmployeeRecord> { }
    public interface ICourseRepository : IVersionedRepository<Course> { }
    public interface ISubjectRepository : IVersionedRepository<Subject> { }
    public interface IAcademicPeriodRepository : IVersionedRepository<AcademicPeriod> { }
    public interface IEnrollmentRepository : IVersionedRepository<Enrollment> { }
    public interface ITuitionAssessmentRepository : IVersionedRepository<TuitionAssessment> { }
    public interface IAssessmentChargeRuleRepository : IVersionedRepository<AssessmentChargeRule> { }
    public interface IPaymentRepository : IVersionedRepository<Payment> { }
    public interface IFinancialAdjustmentRepository : IVersionedRepository<FinancialAdjustment> { }
    public interface IScholarshipAwardRepository : IVersionedRepository<ScholarshipAward> { }
    public interface ILibraryBookRepository : IVersionedRepository<LibraryBook> { }
    public interface ILibraryBorrowingRepository : IVersionedRepository<LibraryBorrowing> { }
    public interface ICounselingCaseRepository : IVersionedRepository<CounselingCase> { }
    public interface IDisciplineCaseRepository : IVersionedRepository<DisciplineCase> { }
    public interface IClinicAppointmentRepository : IVersionedRepository<ClinicAppointment> { }
    public interface IMedicalRecordRepository : IVersionedRepository<MedicalRecord> { }
    public interface IMedicalClearanceRepository : IVersionedRepository<MedicalClearance> { }
}
