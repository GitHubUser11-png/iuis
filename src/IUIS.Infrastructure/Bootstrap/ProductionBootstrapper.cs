using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using IUIS.Domain.People;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Infrastructure.Bootstrap
{
    public sealed class ProductionBootstrapRequest
    {
        public string AdministratorLoginId { get; set; }
        public string AdministratorInitialPassword { get; set; }
        public string AdministratorGivenName { get; set; }
        public string AdministratorMiddleName { get; set; }
        public string AdministratorFamilyName { get; set; }
        public string AdministratorSuffix { get; set; }
        public string AdministratorEmailAddress { get; set; }
        public string AdministratorMobileNumber { get; set; }
        public string AdministratorAlternatePhoneNumber { get; set; }
        public string AdministratorAddressLine1 { get; set; }
        public string AdministratorAddressLine2 { get; set; }
        public string AdministratorBarangay { get; set; }
        public string AdministratorCityMunicipality { get; set; }
        public string AdministratorProvince { get; set; }
        public string AdministratorPostalCode { get; set; }
        public string AdministratorCountryCode { get; set; }
        public string AdministratorBirthDate { get; set; }
        public string DepartmentId { get; set; }
        public string PositionTitle { get; set; }
        public DateTime BootstrapAtUtc { get; set; }
    }

    public sealed class ProductionBootstrapResult
    {
        public string AdministratorUserId { get; set; }
        public string AdministratorEmployeeId { get; set; }
        public int RepositoryFileCount { get; set; }
        public bool MustChangePassword { get; set; }
    }

    public sealed class ProductionBootstrapper
    {
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly AtomicFileWriter _writer = new AtomicFileWriter();
        private readonly PasswordHasher _passwords = new PasswordHasher();
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ProductionBootstrapper(ProductionRepositoryCatalog catalog, JsonInfrastructureOptions options)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public ProductionBootstrapResult Initialize(ProductionBootstrapRequest request)
        {
            Validate(request);
            Directory.CreateDirectory(_options.DataRoot);
            var existing = Directory.GetFiles(_options.DataRoot, "*.json", SearchOption.TopDirectoryOnly);
            if (existing.Length != 0) throw new InvalidOperationException("Production bootstrap requires an empty data directory.");

            var employeeId = "EMP-" + request.BootstrapAtUtc.Year + "-000001";
            var userId = "USR-" + request.BootstrapAtUtc.Year + "-000001";
            foreach (var descriptor in _catalog.All)
                _writer.WriteUtf8(_catalog.ResolvePath(_options.DataRoot, descriptor.Name), EmptyEnvelope(descriptor, request.BootstrapAtUtc));

            SeedSecurityPolicy(request.BootstrapAtUtc);
            SeedAdministrator(request, employeeId, userId);
            SeedSequences(request.BootstrapAtUtc, userId);
            SeedManifest(request.BootstrapAtUtc);

            var count = Directory.GetFiles(_options.DataRoot, "*.json", SearchOption.TopDirectoryOnly).Length;
            if (count != 49) throw new InvalidOperationException("Production bootstrap did not create exactly 49 JSON repositories.");
            return new ProductionBootstrapResult
            {
                AdministratorEmployeeId = employeeId,
                AdministratorUserId = userId,
                RepositoryFileCount = count,
                MustChangePassword = true
            };
        }

        private void SeedAdministrator(ProductionBootstrapRequest request, string employeeId, string userId)
        {
            var employee = new EmployeeRecord(
                employeeId,
                employeeId,
                new PersonName(
                    request.AdministratorGivenName,
                    request.AdministratorMiddleName,
                    request.AdministratorFamilyName,
                    request.AdministratorSuffix),
                new ContactInformation(
                    request.AdministratorEmailAddress,
                    request.AdministratorMobileNumber,
                    request.AdministratorAlternatePhoneNumber),
                new PostalAddress(
                    request.AdministratorAddressLine1,
                    request.AdministratorAddressLine2,
                    request.AdministratorBarangay,
                    request.AdministratorCityMunicipality,
                    request.AdministratorProvince,
                    request.AdministratorPostalCode,
                    request.AdministratorCountryCode),
                InstitutionLocalDate.Parse(request.AdministratorBirthDate),
                request.DepartmentId,
                request.PositionTitle,
                EmploymentStatus.Active,
                false,
                request.BootstrapAtUtc,
                userId);
            var employeeMapper = new EmployeeRecordJsonMapper();
            var employees = new RepositoryEnvelope<JsonElement>
            {
                Repository = "employees", SchemaVersion = 1, Revision = 1,
                CreatedAtUtc = request.BootstrapAtUtc, UpdatedAtUtc = request.BootstrapAtUtc,
                UpdatedByUserId = userId,
                Records = new List<JsonElement>
                {
                    employeeMapper.ToJson(employee, _json)
                }
            };
            var users = new RepositoryEnvelope<PersistedUserAccount>
            {
                Repository = "users", SchemaVersion = 1, Revision = 1,
                CreatedAtUtc = request.BootstrapAtUtc, UpdatedAtUtc = request.BootstrapAtUtc,
                UpdatedByUserId = userId,
                Records = new List<PersistedUserAccount>
                {
                    new PersistedUserAccount
                    {
                        Id = userId, LoginId = request.AdministratorLoginId.Trim().ToLowerInvariant(),
                        PrimaryRole = "Administrator", PersonRecordKind = "EmployeeFaculty", PersonRecordId = employeeId,
                        CredentialHash = _passwords.Hash(request.AdministratorInitialPassword, 210000),
                        SecurityStamp = Guid.NewGuid().ToString("N"), Status = "Active", MustChangePassword = true,
                        CreatedAtUtc = request.BootstrapAtUtc, UpdatedAtUtc = request.BootstrapAtUtc, Version = 1,
                        PermissionProfileIds = new List<string>(),
                        DirectPermissionGrants = new List<string>(),
                        DirectPermissionRestrictions = new List<string>()
                    }
                }
            };
            _writer.WriteUtf8(_catalog.ResolvePath(_options.DataRoot, "employees"), JsonSerializer.Serialize(employees, _json));
            _writer.WriteUtf8(_catalog.ResolvePath(_options.DataRoot, "users"), JsonSerializer.Serialize(users, _json));
        }

        private void SeedSecurityPolicy(DateTime now)
        {
            var envelope = new RepositoryEnvelope<SecurityPolicyRecord>
            {
                Repository = "security_policy", SchemaVersion = 1, Revision = 1,
                CreatedAtUtc = now, UpdatedAtUtc = now, UpdatedByUserId = "SYSTEM",
                Records = new List<SecurityPolicyRecord>
                {
                    new SecurityPolicyRecord
                    {
                        Id = "SECURITY-POLICY-1", MaximumFailedAttempts = 5,
                        ObservationWindowMinutes = 15, LockoutMinutes = 15,
                        MinimumPasswordLength = 12, Pbkdf2Iterations = 210000, UpdatedAtUtc = now
                    }
                }
            };
            _writer.WriteUtf8(_catalog.ResolvePath(_options.DataRoot, "security_policy"), JsonSerializer.Serialize(envelope, _json));
        }

        private void SeedSequences(DateTime now, string actor)
        {
            var envelope = new RepositoryEnvelope<IdSequenceRecord>
            {
                Repository = "id_sequences", SchemaVersion = 1, Revision = 1,
                CreatedAtUtc = now, UpdatedAtUtc = now, UpdatedByUserId = actor,
                Records = new List<IdSequenceRecord>
                {
                    new IdSequenceRecord { Prefix = "EMP", Year = now.Year, LastAllocatedSequence = 1, UpdatedAtUtc = now, UpdatedByUserId = actor },
                    new IdSequenceRecord { Prefix = "USR", Year = now.Year, LastAllocatedSequence = 1, UpdatedAtUtc = now, UpdatedByUserId = actor }
                }
            };
            _writer.WriteUtf8(_catalog.ResolvePath(_options.DataRoot, "id_sequences"), JsonSerializer.Serialize(envelope, _json));
        }

        private void SeedManifest(DateTime now)
        {
            var records = _catalog.All.Select(x => new RepositoryManifestRecord
            {
                Repository = x.Name, FileName = x.FileName, SchemaVersion = x.SchemaVersion,
                Revision = ReadRevision(_catalog.ResolvePath(_options.DataRoot, x.Name)),
                Sha256 = AtomicFileWriter.ComputeSha256(_catalog.ResolvePath(_options.DataRoot, x.Name)),
                VerifiedAtUtc = now
            }).ToList();
            var envelope = new RepositoryEnvelope<RepositoryManifestRecord>
            {
                Repository = "repository_manifest", SchemaVersion = 1, Revision = 1,
                CreatedAtUtc = now, UpdatedAtUtc = now, UpdatedByUserId = "SYSTEM", Records = records
            };
            _writer.WriteUtf8(_catalog.ResolvePath(_options.DataRoot, "repository_manifest"), JsonSerializer.Serialize(envelope, _json));
        }

        private long ReadRevision(string path)
        {
            using (var document = JsonDocument.Parse(File.ReadAllText(path)))
                return document.RootElement.GetProperty("revision").GetInt64();
        }

        private string EmptyEnvelope(ProductionRepositoryDescriptor descriptor, DateTime now)
        {
            var envelope = new RepositoryEnvelope<object>
            {
                Repository = descriptor.Name, SchemaVersion = descriptor.SchemaVersion, Revision = 0,
                CreatedAtUtc = now, UpdatedAtUtc = now, UpdatedByUserId = "SYSTEM", Records = new List<object>()
            };
            return JsonSerializer.Serialize(envelope, _json);
        }

        private static void Validate(ProductionBootstrapRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.BootstrapAtUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("Bootstrap timestamp must be UTC.");
            if (string.IsNullOrWhiteSpace(request.AdministratorLoginId) || request.AdministratorLoginId.Contains(" "))
                throw new ArgumentException("Administrator login ID is required and cannot contain spaces.");
            if (string.IsNullOrEmpty(request.AdministratorInitialPassword) || request.AdministratorInitialPassword.Length < 12)
                throw new ArgumentException("Administrator initial password must contain at least 12 characters.");
            if (string.IsNullOrWhiteSpace(request.AdministratorGivenName)
                || string.IsNullOrWhiteSpace(request.AdministratorFamilyName)
                || string.IsNullOrWhiteSpace(request.AdministratorEmailAddress)
                || string.IsNullOrWhiteSpace(request.AdministratorAddressLine1)
                || string.IsNullOrWhiteSpace(request.AdministratorCityMunicipality)
                || string.IsNullOrWhiteSpace(request.AdministratorProvince)
                || string.IsNullOrWhiteSpace(request.AdministratorCountryCode)
                || string.IsNullOrWhiteSpace(request.AdministratorBirthDate)
                || string.IsNullOrWhiteSpace(request.DepartmentId)
                || string.IsNullOrWhiteSpace(request.PositionTitle))
                throw new ArgumentException("Complete administrator employee information is required.");
            InstitutionLocalDate.Parse(request.AdministratorBirthDate);
        }
    }
}
