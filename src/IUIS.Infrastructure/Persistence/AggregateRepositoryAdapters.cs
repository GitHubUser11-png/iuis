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
    public sealed class StudentRecordRepositoryAdapter : MappedJsonRepository<StudentRecord>, IStudentRecordRepository
    { public StudentRecordRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<StudentRecord> mapper) : base("students", store, mapper) { } }
    public sealed class EmployeeRecordRepositoryAdapter : MappedJsonRepository<EmployeeRecord>, IEmployeeRecordRepository
    { public EmployeeRecordRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<EmployeeRecord> mapper) : base("employees", store, mapper) { } }
    public sealed class CourseRepositoryAdapter : MappedJsonRepository<Course>, ICourseRepository
    { public CourseRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<Course> mapper) : base("courses", store, mapper) { } }
    public sealed class SubjectRepositoryAdapter : MappedJsonRepository<Subject>, ISubjectRepository
    { public SubjectRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<Subject> mapper) : base("subjects", store, mapper) { } }
    public sealed class AcademicPeriodRepositoryAdapter : MappedJsonRepository<AcademicPeriod>, IAcademicPeriodRepository
    { public AcademicPeriodRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<AcademicPeriod> mapper) : base("academic_periods", store, mapper) { } }
    public sealed class EnrollmentRepositoryAdapter : MappedJsonRepository<Enrollment>, IEnrollmentRepository
    { public EnrollmentRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<Enrollment> mapper) : base("enrollments", store, mapper) { } }
    public sealed class TuitionAssessmentRepositoryAdapter : MappedJsonRepository<TuitionAssessment>, ITuitionAssessmentRepository
    { public TuitionAssessmentRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<TuitionAssessment> mapper) : base("assessments", store, mapper) { } }
    public sealed class AssessmentChargeRuleRepositoryAdapter : MappedJsonRepository<AssessmentChargeRule>, IAssessmentChargeRuleRepository
    { public AssessmentChargeRuleRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<AssessmentChargeRule> mapper) : base("assessment_charge_rules", store, mapper) { } }
    public sealed class PaymentRepositoryAdapter : MappedJsonRepository<Payment>, IPaymentRepository
    { public PaymentRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<Payment> mapper) : base("payments", store, mapper) { } }
    public sealed class FinancialAdjustmentRepositoryAdapter : MappedJsonRepository<FinancialAdjustment>, IFinancialAdjustmentRepository
    { public FinancialAdjustmentRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<FinancialAdjustment> mapper) : base("financial_adjustments", store, mapper) { } }
    public sealed class ScholarshipAwardRepositoryAdapter : MappedJsonRepository<ScholarshipAward>, IScholarshipAwardRepository
    { public ScholarshipAwardRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<ScholarshipAward> mapper) : base("scholarship_awards", store, mapper) { } }
    public sealed class LibraryBookRepositoryAdapter : MappedJsonRepository<LibraryBook>, ILibraryBookRepository
    { public LibraryBookRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<LibraryBook> mapper) : base("books", store, mapper) { } }
    public sealed class LibraryBorrowingRepositoryAdapter : MappedJsonRepository<LibraryBorrowing>, ILibraryBorrowingRepository
    { public LibraryBorrowingRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<LibraryBorrowing> mapper) : base("borrowings", store, mapper) { } }
    public sealed class CounselingCaseRepositoryAdapter : MappedJsonRepository<CounselingCase>, ICounselingCaseRepository
    { public CounselingCaseRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<CounselingCase> mapper) : base("counseling", store, mapper) { } }
    public sealed class DisciplineCaseRepositoryAdapter : MappedJsonRepository<DisciplineCase>, IDisciplineCaseRepository
    { public DisciplineCaseRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<DisciplineCase> mapper) : base("discipline_incidents", store, mapper) { } }
    public sealed class ClinicAppointmentRepositoryAdapter : MappedJsonRepository<ClinicAppointment>, IClinicAppointmentRepository
    { public ClinicAppointmentRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<ClinicAppointment> mapper) : base("appointments", store, mapper) { } }
    public sealed class MedicalRecordRepositoryAdapter : MappedJsonRepository<MedicalRecord>, IMedicalRecordRepository
    { public MedicalRecordRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<MedicalRecord> mapper) : base("medical_records", store, mapper) { } }
    public sealed class MedicalClearanceRepositoryAdapter : MappedJsonRepository<MedicalClearance>, IMedicalClearanceRepository
    { public MedicalClearanceRepositoryAdapter(JsonRepositoryStore store, IJsonRecordMapper<MedicalClearance> mapper) : base("clearances", store, mapper) { } }
}
