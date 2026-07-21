using IUIS.Application.Repositories;
using IUIS.Domain.Academic;
using IUIS.Domain.Clinic;
using IUIS.Domain.Counseling;
using IUIS.Domain.Discipline;
using IUIS.Domain.Finance;
using IUIS.Domain.Library;
using IUIS.Domain.People;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class StudentRecordRepositoryAdapter
        : MappedJsonRepository<StudentRecord>, IStudentRecordRepository
    {
        public StudentRecordRepositoryAdapter(JsonRepositoryStore store)
            : base("students", store, new StudentRecordJsonMapper()) { }
    }

    public sealed class EmployeeRecordRepositoryAdapter
        : MappedJsonRepository<EmployeeRecord>, IEmployeeRecordRepository
    {
        public EmployeeRecordRepositoryAdapter(JsonRepositoryStore store)
            : base("employees", store, new EmployeeRecordJsonMapper()) { }
    }

    public sealed class CourseRepositoryAdapter
        : MappedJsonRepository<Course>, ICourseRepository
    {
        public CourseRepositoryAdapter(JsonRepositoryStore store)
            : base("courses", store, new CourseJsonMapper()) { }
    }

    public sealed class SubjectRepositoryAdapter
        : MappedJsonRepository<Subject>, ISubjectRepository
    {
        public SubjectRepositoryAdapter(JsonRepositoryStore store)
            : base("subjects", store, new SubjectJsonMapper()) { }
    }

    public sealed class AcademicPeriodRepositoryAdapter
        : MappedJsonRepository<AcademicPeriod>, IAcademicPeriodRepository
    {
        public AcademicPeriodRepositoryAdapter(JsonRepositoryStore store)
            : base("academic_periods", store, new AcademicPeriodJsonMapper()) { }
    }

    public sealed class EnrollmentRepositoryAdapter
        : MappedJsonRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepositoryAdapter(JsonRepositoryStore store)
            : base("enrollments", store, new EnrollmentJsonMapper()) { }
    }

    public sealed class TuitionAssessmentRepositoryAdapter
        : MappedJsonRepository<TuitionAssessment>, ITuitionAssessmentRepository
    {
        public TuitionAssessmentRepositoryAdapter(JsonRepositoryStore store)
            : base("assessments", store, new TuitionAssessmentJsonMapper()) { }
    }

    public sealed class AssessmentChargeRuleRepositoryAdapter
        : MappedJsonRepository<AssessmentChargeRule>, IAssessmentChargeRuleRepository
    {
        public AssessmentChargeRuleRepositoryAdapter(JsonRepositoryStore store)
            : base("assessment_charge_rules", store, new AssessmentChargeRuleJsonMapper()) { }
    }

    public sealed class PaymentRepositoryAdapter
        : MappedJsonRepository<Payment>, IPaymentRepository
    {
        public PaymentRepositoryAdapter(JsonRepositoryStore store)
            : base("payments", store, new PaymentJsonMapper()) { }
    }

    public sealed class FinancialAdjustmentRepositoryAdapter
        : MappedJsonRepository<FinancialAdjustment>, IFinancialAdjustmentRepository
    {
        public FinancialAdjustmentRepositoryAdapter(JsonRepositoryStore store)
            : base("financial_adjustments", store, new FinancialAdjustmentJsonMapper()) { }
    }

    public sealed class ScholarshipAwardRepositoryAdapter
        : MappedJsonRepository<ScholarshipAward>, IScholarshipAwardRepository
    {
        public ScholarshipAwardRepositoryAdapter(JsonRepositoryStore store)
            : base("scholarship_awards", store, new ScholarshipAwardJsonMapper()) { }
    }

    public sealed class LibraryBookRepositoryAdapter
        : MappedJsonRepository<LibraryBook>, ILibraryBookRepository
    {
        public LibraryBookRepositoryAdapter(JsonRepositoryStore store)
            : base("books", store, new LibraryBookJsonMapper()) { }
    }

    public sealed class LibraryBorrowingRepositoryAdapter
        : MappedJsonRepository<LibraryBorrowing>, ILibraryBorrowingRepository
    {
        public LibraryBorrowingRepositoryAdapter(JsonRepositoryStore store)
            : base("borrowings", store, new LibraryBorrowingJsonMapper()) { }
    }

    public sealed class CounselingCaseRepositoryAdapter
        : MappedJsonRepository<CounselingCase>, ICounselingCaseRepository
    {
        public CounselingCaseRepositoryAdapter(
            JsonRepositoryStore store,
            IJsonRecordMapper<CounselingCase> mapper)
            : base("counseling", store, mapper) { }
    }

    public sealed class DisciplineCaseRepositoryAdapter
        : MappedJsonRepository<DisciplineCase>, IDisciplineCaseRepository
    {
        public DisciplineCaseRepositoryAdapter(
            JsonRepositoryStore store,
            IJsonRecordMapper<DisciplineCase> mapper)
            : base("discipline_incidents", store, mapper) { }
    }

    public sealed class ClinicAppointmentRepositoryAdapter
        : MappedJsonRepository<ClinicAppointment>, IClinicAppointmentRepository
    {
        public ClinicAppointmentRepositoryAdapter(JsonRepositoryStore store)
            : base("appointments", store, new ClinicAppointmentJsonMapper()) { }
    }

    public sealed class MedicalRecordRepositoryAdapter
        : MappedJsonRepository<MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepositoryAdapter(JsonRepositoryStore store)
            : base("medical_records", store, new MedicalRecordJsonMapper()) { }
    }

    public sealed class MedicalClearanceRepositoryAdapter
        : MappedJsonRepository<MedicalClearance>, IMedicalClearanceRepository
    {
        public MedicalClearanceRepositoryAdapter(JsonRepositoryStore store)
            : base("clearances", store, new MedicalClearanceJsonMapper()) { }
    }
}
