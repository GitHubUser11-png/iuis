using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace IUIS.Infrastructure.Persistence
{
    public enum ProductionRepositoryKind
    {
        Principal = 0,
        Supporting = 1
    }

    public sealed class ProductionRepositoryDescriptor
    {
        public ProductionRepositoryDescriptor(string name, ProductionRepositoryKind kind, bool sensitive)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Repository name is required.", nameof(name));
            Name = name.Trim().ToLowerInvariant();
            FileName = Name + ".json";
            Kind = kind;
            SchemaVersion = 1;
            ContainsSensitiveData = sensitive;
        }

        public string Name { get; private set; }
        public string FileName { get; private set; }
        public ProductionRepositoryKind Kind { get; private set; }
        public int SchemaVersion { get; private set; }
        public bool ContainsSensitiveData { get; private set; }
    }

    public sealed class ProductionRepositoryCatalog
    {
        private static readonly ReadOnlyCollection<ProductionRepositoryDescriptor> Items =
            new ReadOnlyCollection<ProductionRepositoryDescriptor>(new List<ProductionRepositoryDescriptor>
            {
                P("students", true), P("courses", false), P("subjects", false), P("enrollments", true),
                P("payments", true), P("books", false), P("borrowings", true), P("counseling", true),
                P("violations", true), P("medical_records", true), P("employees", true), P("attendance", true),
                P("clearances", true), P("users", true),

                S("academic_periods", false), S("assessments", true), S("assessment_charge_rules", false),
                S("scholarship_programs", false), S("scholarship_applications", true), S("scholarship_awards", true),
                S("appointments", true), S("consultations", true), S("subject_assignments", true),
                S("notifications", true), S("account_applications", true), S("permission_profiles", true),
                S("login_attempts", true), S("sessions", true), S("security_policy", true),
                S("password_assistance_requests", true), S("admin_access_rules", true),
                S("administrative_approvals", true), S("discipline_incidents", true), S("violation_responses", true),
                S("work_schedules", true), S("attendance_corrections", true),
                S("employee_profile_corrections", true), S("student_profile_corrections", true),
                S("payment_void_requests", true), S("financial_adjustments", true), S("audit_logs", true),
                S("id_sequences", true), S("transaction_journal", true), S("repository_manifest", false),
                S("system_settings", true), S("backup_catalog", true), S("repository_health_history", true),
                S("operational_report_runs", true), S("restore_history", true)
            });

        private readonly ReadOnlyDictionary<string, ProductionRepositoryDescriptor> _byName;

        public ProductionRepositoryCatalog()
        {
            if (Items.Count != 49) throw new InvalidOperationException("Exactly 49 production repositories are required.");
            if (Items.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 49)
                throw new InvalidOperationException("Production repository names must be unique.");
            _byName = new ReadOnlyDictionary<string, ProductionRepositoryDescriptor>(
                Items.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase));
        }

        public IReadOnlyList<ProductionRepositoryDescriptor> All { get { return Items; } }
        public IReadOnlyList<ProductionRepositoryDescriptor> PrincipalRepositories
        {
            get { return new ReadOnlyCollection<ProductionRepositoryDescriptor>(Items.Where(x => x.Kind == ProductionRepositoryKind.Principal).ToList()); }
        }
        public IReadOnlyList<ProductionRepositoryDescriptor> SupportingRepositories
        {
            get { return new ReadOnlyCollection<ProductionRepositoryDescriptor>(Items.Where(x => x.Kind == ProductionRepositoryKind.Supporting).ToList()); }
        }

        public ProductionRepositoryDescriptor Get(string name)
        {
            ProductionRepositoryDescriptor value;
            if (string.IsNullOrWhiteSpace(name) || !_byName.TryGetValue(name.Trim(), out value))
                throw new KeyNotFoundException("Unknown production repository: " + name);
            return value;
        }

        public string ResolvePath(string dataRoot, string name)
        {
            if (string.IsNullOrWhiteSpace(dataRoot)) throw new ArgumentException("Data root is required.", nameof(dataRoot));
            return Path.Combine(Path.GetFullPath(dataRoot), Get(name).FileName);
        }

        private static ProductionRepositoryDescriptor P(string name, bool sensitive)
        { return new ProductionRepositoryDescriptor(name, ProductionRepositoryKind.Principal, sensitive); }
        private static ProductionRepositoryDescriptor S(string name, bool sensitive)
        { return new ProductionRepositoryDescriptor(name, ProductionRepositoryKind.Supporting, sensitive); }
    }
}
