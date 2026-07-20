using System;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Orchestration;
using IUIS.Application.Repositories;
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
            AssessmentChargeRules = new AssessmentChargeRuleRepositoryAdapter(Store);

            Transactions = new JournaledApplicationTransactionCoordinator(
                new JournaledTransactionCoordinator(Catalog, Options));
            PrincipalProvider = new JsonAuthorizationPrincipalProvider(Catalog, Options);
            RequestExecutor = new SessionAwareRequestExecutor(
                PrincipalProvider,
                new PermissionResolver());
            Projections = new RestrictedProjectionService();

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
        }

        public ProductionRepositoryCatalog Catalog { get; private set; }
        public JsonInfrastructureOptions Options { get; private set; }
        public JsonRepositoryStore Store { get; private set; }

        public IStudentRecordRepository Students { get; private set; }
        public IEmployeeRecordRepository Employees { get; private set; }
        public ICourseRepository Courses { get; private set; }
        public ISubjectRepository Subjects { get; private set; }
        public IAcademicPeriodRepository AcademicPeriods { get; private set; }
        public IAssessmentChargeRuleRepository AssessmentChargeRules { get; private set; }

        public IApplicationTransactionCoordinator Transactions { get; private set; }
        public IAuthorizationPrincipalProvider PrincipalProvider { get; private set; }
        public SessionAwareRequestExecutor RequestExecutor { get; private set; }
        public RestrictedProjectionService Projections { get; private set; }

        public StudentOwnRecordQueryService StudentOwnRecords { get; private set; }
        public EmployeeRecordQueryService EmployeeSelfService { get; private set; }
        public StudentContactUpdateService StudentContactUpdates { get; private set; }
        public EmployeeContactUpdateService EmployeeContactUpdates { get; private set; }
    }
}
