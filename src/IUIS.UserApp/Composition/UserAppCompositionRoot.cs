using System;
using Microsoft.Extensions.DependencyInjection;

using IUIS.Application.Repositories;
using IUIS.Application.Services.Student;
using IUIS.Application.Services.Library;
using IUIS.Application.Services.Counseling;
using IUIS.Application.Services.Medical;
using IUIS.Application.Services.Discipline;
using IUIS.Application.Navigation;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;
using IUIS.Infrastructure.Identity;
using IUIS.Infrastructure.Presentation;
using IUIS.SharedUI.Application;
using IUIS.SharedUI.Navigation;
using IUIS.UserApp.Configuration;

namespace IUIS.UserApp.Composition
{
    internal static class UserAppCompositionRoot
    {
        public static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            
            var dataRoot = UserAppSettings.ResolveDataRoot();
            
            // Infrastructure - Core Persistence
            services.AddSingleton<IRepositoryCatalog>(sp => 
                new RepositoryCatalog(dataRoot));
            services.AddSingleton<JsonRepositoryStore>();
            services.AddSingleton<MappedJsonRepository>();
            
            // Infrastructure - Security
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<PasswordHasher>();
            services.AddSingleton<LoginAttemptService>();
            services.AddSingleton<SessionTokenProtector>();
            services.AddSingleton<IInfrastructure.Security.IAuthenticationService>(sp => sp.GetRequiredService<AuthenticationService>());
            
            // Infrastructure - Identity
            services.AddSingleton<ApplicationIdentifierAllocator>();
            
            // Infrastructure - Presentation
            services.AddSingleton<ApplicationRuntime>();
            
            // Application - Repositories (via Infrastructure)
            services.AddScoped<IStudentRecordRepository>(sp => 
                new AggregateRepositoryAdapters.StudentRecordAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<IEmployeeRecordRepository>(sp =>
                new AggregateRepositoryAdapters.EmployeeRecordAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<ILibraryBookRepository>(sp =>
                new AggregateRepositoryAdapters.LibraryBookAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<ILibraryBorrowingRepository>(sp =>
                new AggregateRepositoryAdapters.LibraryBorrowingAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<ICounselingCaseRepository>(sp =>
                new AggregateRepositoryAdapters.CounselingCaseAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<IDisciplineCaseRepository>(sp =>
                new AggregateRepositoryAdapters.DisciplineCaseAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<IClinicAppointmentRepository>(sp =>
                new AggregateRepositoryAdapters.ClinicAppointmentAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            services.AddScoped<IMedicalRecordRepository>(sp =>
                new AggregateRepositoryAdapters.MedicalRecordAdapter(
                    sp.GetRequiredService<MappedJsonRepository>()));
            
            // Application - Services - Student
            services.AddScoped<IStudentDashboardService, StudentDashboardService>();
            services.AddScoped<IStudentProfileService, StudentProfileService>();
            services.AddScoped<IStudentEnrollmentService, StudentEnrollmentService>();
            services.AddScoped<IStudentFinanceService, StudentFinanceService>();
            services.AddScoped<IStudentScholarshipService, StudentScholarshipService>();
            services.AddScoped<IStudentNotificationService, StudentNotificationService>();
            services.AddScoped<IStudentAccessGuard, StudentAccessGuard>();
            
            // Application - Services - Library
            services.AddScoped<IApplication.Services.Library.ILibraryCirculationService, LibraryCirculationService>();
            
            // Application - Services - Counseling & Discipline
            services.AddScoped<IApplication.Services.Counseling.ICounselingService, CounselingServices>();
            services.AddScoped<IApplication.Services.Discipline.IDisciplineService, DisciplineServices>();
            
            // Application - Services - Medical
            services.AddScoped<IApplication.Services.Medical.IMedicalService, MedicalServices>();
            
            // Application - Navigation
            services.AddSingleton<INavigationService, FormNavigationService>();
            
            // SharedUI - Application Runtime Context
            services.AddSingleton(sp => sp.GetRequiredService<ApplicationRuntime>());
            
            return services.BuildServiceProvider();
        }
    }
}
