using System;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Orchestration;
using IUIS.Application.Repositories;
using IUIS.Infrastructure.Identity;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Infrastructure.Composition
{
    public sealed class IuisCompositionRoot
    {
        public IuisCompositionRoot(string dataRoot)
            : this(
                new ProductionRepositoryCatalog(),
                new JsonInfrastructureOptions(dataRoot))
        {
        }

        public IuisCompositionRoot(
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Store = new JsonRepositoryStore(Catalog, Options);

            Students = new StudentRecordRepositoryAdapter(Store);
            Employees = new EmployeeRecordRepositoryAdapter(Store);
            Courses = new CourseRepositoryAdapter(Store);
            Subjects = new SubjectRepositoryAdapter(Store);
            AcademicPeriods = new AcademicPeriodRepositoryAdapter(Store);
            Enrollments = new EnrollmentRepositoryAdapter(Store);
            TuitionAssessments = new TuitionAssessmentRepositoryAdapter(Store);
            AssessmentChargeRules = new AssessmentChargeRuleRepositoryAdapter(Store);
            Payments = new PaymentRepositoryAdapter(Store);
            FinancialAdjustments = new FinancialAdjustmentRepositoryAdapter(Store);
            ScholarshipAwards = new ScholarshipAwardRepositoryAdapter(Store);
            LibraryBooks = new LibraryBookRepositoryAdapter(Store);
            LibraryBorrowings = new LibraryBorrowingRepositoryAdapter(Store);
            ClinicAppointments = new ClinicAppointmentRepositoryAdapter(Store);
            MedicalRecords = new MedicalRecordRepositoryAdapter(Store);
            MedicalClearances = new MedicalClearanceRepositoryAdapter(Store);

            Transactions = new JournaledApplicationTransactionCoordinator(
                new JournaledTransactionCoordinator(Catalog, Options));
            IdentifierAllocator = new ApplicationIdentifierAllocator(Catalog, Options);
            PrincipalProvider = new JsonAuthorizationPrincipalProvider(Catalog, Options);
            RequestExecutor = new SessionAwareRequestExecutor(
                PrincipalProvider,
                new PermissionResolver());
            Projections = new RestrictedProjectionService();
            EnvelopeMigrations = new RepositoryEnvelopeMigrationService(
                Catalog,
                Options);
            SessionSecurityMigrations = new SessionSecurityMigrationService(
                Catalog,
                Options);

            StudentOwnRecords = new StudentOwnRecordQueryService(
                RequestExecutor,
                Students,
                Projections);
            EmployeeSelfService = new EmployeeRecordQueryService(
                RequestExecutor,
                Employees,
                Projections);
            StudentContactUpdates = new StudentContactUpdateService(
                RequestExecutor,
                Students,
                Transactions);
            EmployeeContactUpdates = new EmployeeContactUpdateService(
                RequestExecutor,
                Employees,
                Transactions);
            StudentFinance = new StudentFinanceQueryService(
                RequestExecutor,
                Enrollments,
                TuitionAssessments,
                Payments,
                FinancialAdjustments,
                ScholarshipAwards);
            EnrollmentSubmissions = new StudentEnrollmentSubmissionService(
                RequestExecutor,
                Enrollments,
                Transactions,
                IdentifierAllocator);
            AssessmentPostings = new TuitionAssessmentPostingService(
                RequestExecutor,
                Enrollments,
                TuitionAssessments,
                Transactions,
                IdentifierAllocator);
            PaymentPostings = new PaymentPostingService(
                RequestExecutor,
                TuitionAssessments,
                Payments,
                Transactions,
                IdentifierAllocator);
            AdjustmentPostings = new FinancialAdjustmentPostingService(
                RequestExecutor,
                TuitionAssessments,
                FinancialAdjustments,
                Transactions,
                IdentifierAllocator);
            ScholarshipApplications = new ScholarshipAwardApplicationService(
                RequestExecutor,
                ScholarshipAwards,
                TuitionAssessments,
                FinancialAdjustments,
                Transactions,
                IdentifierAllocator);
            StudentLibraryCirculation = new StudentLibraryCirculationQueryService(
                RequestExecutor,
                LibraryBooks,
                LibraryBorrowings);
            LibraryCirculation = new LibraryCirculationCommandService(
                RequestExecutor,
                LibraryBooks,
                LibraryBorrowings,
                Transactions,
                IdentifierAllocator);
            StudentMedicalServices = new StudentMedicalServicesQueryService(
                RequestExecutor,
                ClinicAppointments,
                MedicalRecords,
                MedicalClearances);
            RestrictedMedicalRecords = new RestrictedMedicalRecordQueryService(
                RequestExecutor,
                MedicalRecords);
            ClinicAppointmentCommands = new ClinicAppointmentCommandService(
                RequestExecutor,
                ClinicAppointments,
                MedicalRecords,
                Transactions,
                IdentifierAllocator);
            MedicalRecordCommands = new MedicalRecordCommandService(
                RequestExecutor,
                MedicalRecords,
                Transactions,
                IdentifierAllocator);
            MedicalClearanceCommands = new MedicalClearanceCommandService(
                RequestExecutor,
                MedicalRecords,
                MedicalClearances,
                Transactions,
                IdentifierAllocator);
        }

        public ProductionRepositoryCatalog Catalog { get; private set; }
        public JsonInfrastructureOptions Options { get; private set; }
        public JsonRepositoryStore Store { get; private set; }

        public IStudentRecordRepository Students { get; private set; }
        public IEmployeeRecordRepository Employees { get; private set; }
        public ICourseRepository Courses { get; private set; }
        public ISubjectRepository Subjects { get; private set; }
        public IAcademicPeriodRepository AcademicPeriods { get; private set; }
        public IEnrollmentRepository Enrollments { get; private set; }
        public ITuitionAssessmentRepository TuitionAssessments { get; private set; }
        public IAssessmentChargeRuleRepository AssessmentChargeRules { get; private set; }
        public IPaymentRepository Payments { get; private set; }
        public IFinancialAdjustmentRepository FinancialAdjustments { get; private set; }
        public IScholarshipAwardRepository ScholarshipAwards { get; private set; }
        public ILibraryBookRepository LibraryBooks { get; private set; }
        public ILibraryBorrowingRepository LibraryBorrowings { get; private set; }
        public IClinicAppointmentRepository ClinicAppointments { get; private set; }
        public IMedicalRecordRepository MedicalRecords { get; private set; }
        public IMedicalClearanceRepository MedicalClearances { get; private set; }

        public IApplicationTransactionCoordinator Transactions { get; private set; }
        public IApplicationIdentifierAllocator IdentifierAllocator { get; private set; }
        public IAuthorizationPrincipalProvider PrincipalProvider { get; private set; }
        public SessionAwareRequestExecutor RequestExecutor { get; private set; }
        public RestrictedProjectionService Projections { get; private set; }
        public RepositoryEnvelopeMigrationService EnvelopeMigrations { get; private set; }
        public SessionSecurityMigrationService SessionSecurityMigrations { get; private set; }

        public StudentOwnRecordQueryService StudentOwnRecords { get; private set; }
        public EmployeeRecordQueryService EmployeeSelfService { get; private set; }
        public StudentContactUpdateService StudentContactUpdates { get; private set; }
        public EmployeeContactUpdateService EmployeeContactUpdates { get; private set; }
        public StudentFinanceQueryService StudentFinance { get; private set; }
        public StudentEnrollmentSubmissionService EnrollmentSubmissions { get; private set; }
        public TuitionAssessmentPostingService AssessmentPostings { get; private set; }
        public PaymentPostingService PaymentPostings { get; private set; }
        public FinancialAdjustmentPostingService AdjustmentPostings { get; private set; }
        public ScholarshipAwardApplicationService ScholarshipApplications { get; private set; }
        public StudentLibraryCirculationQueryService StudentLibraryCirculation { get; private set; }
        public LibraryCirculationCommandService LibraryCirculation { get; private set; }
        public StudentMedicalServicesQueryService StudentMedicalServices { get; private set; }
        public RestrictedMedicalRecordQueryService RestrictedMedicalRecords { get; private set; }
        public ClinicAppointmentCommandService ClinicAppointmentCommands { get; private set; }
        public MedicalRecordCommandService MedicalRecordCommands { get; private set; }
        public MedicalClearanceCommandService MedicalClearanceCommands { get; private set; }
    }
}
