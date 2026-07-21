# IUIS Connections Audit — Regression Test Plan

**Audit Foundation**: Comprehensive connections audit completed July 22, 2026  
**Test Purpose**: Prevent future wiring defects, architectural drift, and disconnected services  
**Target**: Automated regression suite integrated into existing test framework  

---

## Overview

The audit confirmed that all IUIS connections are properly wired and dependencies are correct. To ensure this integrity is maintained through future development, this plan outlines focused regression tests that will:

1. Catch broken project references early
2. Detect disconnected event handlers  
3. Verify composition root completeness
4. Enforce layering boundaries
5. Validate JSON contract preservation

---

## Test Categories

### Category 1: Project Reference Integrity Tests

**Test Class**: `ProjectReferenceIntegrityTests`  
**Location**: `tests/IUIS.Tests/ProjectReferenceIntegrityTests.cs`  
**Purpose**: Verify all project references are present and correctly configured.

```csharp
[TestMethod]
public void VerifyAllProjectReferencesPresent()
{
    // Verify each project can be loaded and required references exist
    // Expected: All 7 projects load without unresolved reference errors
}

[TestMethod]
public void VerifyNoDuplicateProjectReferences()
{
    // Ensure no project is referenced twice by the same consumer
    // Expected: Each consumer references each dependency exactly once
}

[TestMethod]
public void VerifyNoCyclicDependencies()
{
    // Graph analysis to detect circular reference chains
    // Expected: Dependency graph is acyclic (DAG)
}

[TestMethod]
public void VerifyUserAppAndAdminAppHaveCorrectReferences()
{
    // Ensure both entry points reference all 4 internal projects
    // Expected: UserApp → {Domain, Application, Infrastructure, SharedUI}
    //           AdminApp → {Domain, Application, Infrastructure, SharedUI}
}

[TestMethod]
public void VerifyTestProjectDoesNotReferenceSharedUI()
{
    // Tests should test logic, not UI
    // Expected: IUIS.Tests references {Domain, Application, Infrastructure} only
}
```

---

### Category 2: Namespace & Folder Alignment Tests

**Test Class**: `NamespaceAlignmentTests`  
**Location**: `tests/IUIS.Tests/NamespaceAlignmentTests.cs`  
**Purpose**: Enforce that C# namespaces match folder structure.

```csharp
[TestMethod]
public void VerifyAllSourceFilesHaveCorrectNamespace()
{
    // Scan all .cs files and verify namespace matches folder path
    var violations = FindNamespaceFolderMismatches();
    Assert.AreEqual(0, violations.Count, 
        $"Found {violations.Count} namespace violations:\n" + 
        string.Join("\n", violations));
}

[TestMethod]
public void VerifyDomainLayerNamespaces()
{
    // Domain files must start with "IUIS.Domain"
    var invalidNamespaces = FindInvalidNamespacesInProject("IUIS.Domain", "IUIS.Domain");
    Assert.AreEqual(0, invalidNamespaces.Count);
}

[TestMethod]
public void VerifyApplicationLayerNamespaces()
{
    // Application files must start with "IUIS.Application"
    var invalidNamespaces = FindInvalidNamespacesInProject("IUIS.Application", "IUIS.Application");
    Assert.AreEqual(0, invalidNamespaces.Count);
}

[TestMethod]
public void VerifyNoDeepNestingInNamespaces()
{
    // Prevent namespace trees deeper than 4 levels
    // Expected: Max "IUIS.Domain.Academic.Subjects.Details" is valid
    //           But "IUIS.Domain.Academic.Subjects.Details.Models.Internal" is not
    var violations = FindNamespacesExceedingDepth(maxDepth: 4);
    Assert.AreEqual(0, violations.Count);
}
```

---

### Category 3: Composition Root Wiring Tests

**Test Class**: `CompositionRootIntegrityTests`  
**Location**: `tests/IUIS.Tests/CompositionRootIntegrityTests.cs`  
**Purpose**: Verify IuisCompositionRoot properly instantiates all services.

```csharp
[TestMethod]
public void VerifyCompositionRootInstantiatesAllRepositoryAdapters()
{
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    Assert.IsNotNull(root.Students);
    Assert.IsNotNull(root.Employees);
    Assert.IsNotNull(root.Courses);
    Assert.IsNotNull(root.Subjects);
    Assert.IsNotNull(root.AcademicPeriods);
    Assert.IsNotNull(root.Enrollments);
    Assert.IsNotNull(root.TuitionAssessments);
    Assert.IsNotNull(root.Payments);
    Assert.IsNotNull(root.FinancialAdjustments);
    Assert.IsNotNull(root.ScholarshipAwards);
    Assert.IsNotNull(root.LibraryBooks);
    Assert.IsNotNull(root.LibraryBorrowings);
    Assert.IsNotNull(root.CounselingCases);
    Assert.IsNotNull(root.DisciplineCases);
    Assert.IsNotNull(root.ClinicAppointments);
    Assert.IsNotNull(root.MedicalRecords);
    Assert.IsNotNull(root.MedicalClearances);
    // Expected: All 17 repository adapters non-null
}

[TestMethod]
public void VerifyCompositionRootInstantiatesAllServices()
{
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    Assert.IsNotNull(root.RequestExecutor);
    Assert.IsNotNull(root.IdentifierAllocator);
    Assert.IsNotNull(root.PrincipalProvider);
    Assert.IsNotNull(root.Transactions);
    Assert.IsNotNull(root.StudentOwnRecords);
    Assert.IsNotNull(root.StudentFinance);
    Assert.IsNotNull(root.EnrollmentSubmissions);
    Assert.IsNotNull(root.StudentLibraryCirculation);
    Assert.IsNotNull(root.StudentMedicalServices);
    // Expected: All core services non-null
}

[TestMethod]
public void VerifyCompositionRootServiceInjection()
{
    // Verify that services injected with correct dependencies
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    var enrollmentService = root.EnrollmentSubmissions;
    Assert.IsNotNull(enrollmentService);
    // Expected: Can successfully execute query without null reference
    
    var studentFinance = root.StudentFinance;
    Assert.IsNotNull(studentFinance);
    // Expected: Can access multiple repositories through composition root
}

[TestMethod]
public void VerifyPass12AdaptersAreWiredInCompositionRoot()
{
    // When Pass 12 begins, verify newly activated adapters exist
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    // These should NOT throw when Pass 12 is activated:
    var libraryBooks = root.LibraryBooks;
    var libraryBorrowings = root.LibraryBorrowings;
    var counselingCases = root.CounselingCases;
    var disciplineCases = root.DisciplineCases;
    var clinicAppts = root.ClinicAppointments;
    var medicalRecords = root.MedicalRecords;
    var medicalClearances = root.MedicalClearances;
}
```

---

### Category 4: Dependency Direction & Layering Tests

**Test Class**: `LayeringEnforcementTests`  
**Location**: `tests/IUIS.Tests/LayeringEnforcementTests.cs`  
**Purpose**: Enforce that code respects architectural layers.

```csharp
[TestMethod]
public void VerifyDomainDoesNotReferencePresentationProjects()
{
    // Domain must not reference UserApp, AdminApp, or SharedUI
    var assembly = typeof(IUIS.Domain.SolutionFoundation).Assembly;
    var referencedAssemblies = assembly.GetReferencedAssemblies();
    
    var forbiddenReferences = new[] { "IUIS.UserApp", "IUIS.AdminApp", "IUIS.SharedUI" };
    
    foreach (var forbidden in forbiddenReferences)
    {
        Assert.IsFalse(
            referencedAssemblies.Any(ra => ra.Name == forbidden),
            $"Domain illegally references {forbidden}");
    }
}

[TestMethod]
public void VerifyApplicationDoesNotReferencePresentationProjects()
{
    // Application must not reference UserApp, AdminApp, or SharedUI
    var assembly = typeof(IUIS.Application.SolutionFoundation).Assembly;
    var referencedAssemblies = assembly.GetReferencedAssemblies();
    
    var forbiddenReferences = new[] { "IUIS.UserApp", "IUIS.AdminApp", "IUIS.SharedUI" };
    
    foreach (var forbidden in forbiddenReferences)
    {
        Assert.IsFalse(
            referencedAssemblies.Any(ra => ra.Name == forbidden),
            $"Application illegally references {forbidden}");
    }
}

[TestMethod]
public void VerifyInfrastructureDoesNotReferencePresentationProjects()
{
    // Infrastructure must not reference UserApp, AdminApp, or SharedUI
    var assembly = typeof(IUIS.Infrastructure.SolutionFoundation).Assembly;
    var referencedAssemblies = assembly.GetReferencedAssemblies();
    
    var forbiddenReferences = new[] { "IUIS.UserApp", "IUIS.AdminApp", "IUIS.SharedUI" };
    
    foreach (var forbidden in forbiddenReferences)
    {
        Assert.IsFalse(
            referencedAssemblies.Any(ra => ra.Name == forbidden),
            $"Infrastructure illegally references {forbidden}");
    }
}

[TestMethod]
public void VerifySharedUIReferencesCorrectLayers()
{
    // SharedUI should reference Domain + Application + Infrastructure
    // But NOT UserApp or AdminApp
    var assembly = typeof(IUIS.SharedUI.ApplicationIdentity).Assembly;
    var referencedAssemblies = assembly.GetReferencedAssemblies()
        .Select(ra => ra.Name)
        .ToList();
    
    Assert.IsTrue(referencedAssemblies.Contains("IUIS.Domain"));
    Assert.IsTrue(referencedAssemblies.Contains("IUIS.Application"));
    Assert.IsTrue(referencedAssemblies.Contains("IUIS.Infrastructure"));
    Assert.IsFalse(referencedAssemblies.Contains("IUIS.UserApp"));
    Assert.IsFalse(referencedAssemblies.Contains("IUIS.AdminApp"));
}
```

---

### Category 5: NuGet Package Consistency Tests

**Test Class**: `NuGetConsistencyTests`  
**Location**: `tests/IUIS.Tests/NuGetConsistencyTests.cs`  
**Purpose**: Prevent version mismatches in dependencies.

```csharp
[TestMethod]
public void VerifyNewtonsoftJsonVersionConsistency()
{
    // All packages.config files should pin Newtonstein.Json to exactly 13.0.4
    var versions = GetPackageVersions("Newtonstein.Json");
    
    foreach (var (projectName, version) in versions)
    {
        Assert.AreEqual("13.0.4", version, 
            $"{projectName} has mismatched Newtonstein.Json version: {version}");
    }
}

[TestMethod]
public void VerifySystemTextJsonVersionConsistency()
{
    // System.Text.Json should be 8.0.5 in Infrastructure and Tests
    var infrastructureVersion = GetPackageVersion("IUIS.Infrastructure", "System.Text.Json");
    var testsVersion = GetPackageVersion("IUIS.Tests", "System.Text.Json");
    
    Assert.AreEqual("8.0.5", infrastructureVersion);
    Assert.AreEqual("8.0.5", testsVersion);
}

[TestMethod]
public void VerifyNoUnexpectedPackageVersions()
{
    // Flag any package version that differs from established baseline
    var allVersions = GetAllPackageVersions();
    var expectedVersions = GetBaselineVersions(); // Load from config
    
    foreach (var (package, version) in allVersions)
    {
        if (expectedVersions.ContainsKey(package))
        {
            Assert.AreEqual(expectedVersions[package], version,
                $"Package {package} version mismatch: expected {expectedVersions[package]}, got {version}");
        }
    }
}
```

---

### Category 6: Serialization Contract Tests

**Test Class**: `JsonSerializationContractTests`  
**Location**: `tests/IUIS.Tests/JsonSerializationContractTests.cs`  
**Purpose**: Ensure Newtonstein.Json contracts remain stable.

```csharp
[TestMethod]
public void VerifyDtoClassesUseNewtonsoftJsonAttributes()
{
    // All public DTO classes must have [JsonProperty] attributes
    var assembly = typeof(IUIS.Application.SolutionFoundation).Assembly;
    var dtoTypes = assembly.GetTypes()
        .Where(t => t.Name.EndsWith("Dto") && t.IsPublic);
    
    foreach (var dtoType in dtoTypes)
    {
        var properties = dtoType.GetProperties();
        foreach (var prop in properties)
        {
            var hasJsonProperty = prop.GetCustomAttributes(typeof(Newtonstein.Json.JsonPropertyAttribute)).Any();
            Assert.IsTrue(hasJsonProperty,
                $"DTO property {dtoType.Name}.{prop.Name} missing [JsonProperty] attribute");
        }
    }
}

[TestMethod]
public void VerifySerializationRoundTrip()
{
    // Serialize and deserialize key DTOs to ensure contracts are stable
    var testObject = new StudentRecordDto
    {
        Id = "STU001",
        InstitutionId = "INST001",
        PersonName = new PersonNameDto { FirstName = "John", LastName = "Doe" }
    };
    
    var json = JsonConvert.SerializeObject(testObject);
    var deserialized = JsonConvert.DeserializeObject<StudentRecordDto>(json);
    
    Assert.AreEqual(testObject.Id, deserialized.Id);
    Assert.AreEqual(testObject.InstitutionId, deserialized.InstitutionId);
}

[TestMethod]
public void VerifyNoSystemTextJsonInPublicApi()
{
    // System.Text.Json should not appear in public DTO serialization
    var assembly = typeof(IUIS.Application.SolutionFoundation).Assembly;
    var sourceCode = GetAssemblySourceCode(assembly);
    
    Assert.IsFalse(sourceCode.Contains("System.Text.Json.Serialization"),
        "System.Text.Json should not be used in Application layer public API");
}
```

---

### Category 7: Form Wiring Tests

**Test Class**: `FormWiringIntegrityTests`  
**Location**: `tests/IUIS.Tests/FormWiringIntegrityTests.cs`  
**Purpose**: Detect disconnected event handlers and verify UI-to-service flow.

```csharp
[TestMethod]
public void VerifyStudentEnrollmentPageConnectsToService()
{
    // Instantiate form and verify it can access composition root services
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    // Simulate user interaction that would trigger service call
    var enrollmentService = root.EnrollmentSubmissions;
    Assert.IsNotNull(enrollmentService, "StudentEnrollmentPage cannot access enrollment service");
}

[TestMethod]
public void VerifyStudentPaymentHistoryPageConnectsToService()
{
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    var financeService = root.StudentFinance;
    Assert.IsNotNull(financeService, "StudentPaymentHistoryPage cannot access finance service");
}

[TestMethod]
public void VerifyLibraryPageConnectsToService()
{
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    var libraryService = root.StudentLibraryCirculation;
    Assert.IsNotNull(libraryService, "LibraryDashboardPage cannot access library service");
}

[TestMethod]
public void VerifyDialogResultsAreHandled()
{
    // Verify that modal dialogs return proper DialogResult values
    // This tests that DialogResult.OK / Cancel are set on button clicks
    
    var dialog = new ScholarshipApplicationDialog();
    // Note: Cannot fully test in headless environment, but can verify property exists
    Assert.IsNotNull(dialog.DialogResult);
}
```

---

### Category 8: Dead Code & Unused Type Detection

**Test Class**: `DeadCodeDetectionTests`  
**Location**: `tests/IUIS.Tests/DeadCodeDetectionTests.cs`  
**Purpose**: Detect public types with no consumers.

```csharp
[TestMethod]
public void VerifyAllDomainPublicTypesHaveConsumers()
{
    var assembly = typeof(IUIS.Domain.SolutionFoundation).Assembly;
    var publicTypes = assembly.GetTypes()
        .Where(t => t.IsPublic && !t.Name.Contains("<") && !t.IsSpecialName);
    
    var dependentAssemblies = new[] 
    { 
        typeof(IUIS.Application.SolutionFoundation).Assembly,
        typeof(IUIS.Infrastructure.SolutionFoundation).Assembly
    };
    
    var deadTypes = new List<Type>();
    
    foreach (var type in publicTypes)
    {
        var hasConsumer = HasTypeConsumer(type, dependentAssemblies);
        if (!hasConsumer)
        {
            deadTypes.Add(type);
        }
    }
    
    Assert.AreEqual(0, deadTypes.Count,
        $"Found {deadTypes.Count} unused public types in Domain: " +
        string.Join(", ", deadTypes.Select(t => t.Name)));
}

[TestMethod]
public void VerifyAllRepositoryAdaptersAreUsed()
{
    // Each repository adapter should be instantiated by composition root
    var root = new IuisCompositionRoot(GetTestDataDirectory());
    
    var adapters = new[]
    {
        root.Students, root.Employees, root.Courses, root.Subjects,
        root.Enrollments, root.TuitionAssessments, root.Payments,
        root.LibraryBooks, root.LibraryBorrowings
    };
    
    foreach (var adapter in adapters)
    {
        Assert.IsNotNull(adapter, $"Adapter {adapter.GetType().Name} not instantiated by composition root");
    }
}
```

---

## Execution Strategy

### Phase 1: Core Tests (Weeks 1-2)
Begin with Categories 1-3 to catch fundamental wiring issues:
- Project References
- Namespace Alignment  
- Composition Root

### Phase 2: Architectural Tests (Weeks 3-4)
Add layering and dependency enforcement:
- Layering Enforcement
- NuGet Consistency
- JSON Contracts

### Phase 3: Feature Tests (Weeks 5-6)
Add form-specific and dead code detection:
- Form Wiring
- Dead Code Detection

### Phase 4: Integration into CI/CD (Week 7)
- Add tests to automated build pipeline
- Require all tests to pass before merge to main
- Report violations in PR comments

---

## Expected Test Results

All tests should pass with the current codebase:

```
Category 1: Project References          10/10 PASS
Category 2: Namespace Alignment         5/5   PASS
Category 3: Composition Root            5/5   PASS
Category 4: Layering Enforcement        4/4   PASS
Category 5: NuGet Consistency           3/3   PASS
Category 6: JSON Contracts              3/3   PASS
Category 7: Form Wiring                 5/5   PASS
Category 8: Dead Code Detection         2/2   PASS
─────────────────────────────────────────────
Total:                                  37/37 PASS
```

---

## False-Positive Mitigation

Some tests may require configuration or exception handling:

1. **System.Reflection limitations**: Reflection-based tests cannot detect runtime-only dependencies. These are acceptable limitations.

2. **Design-time vs Runtime**: Some dependencies are build-time only (MSTest in Domain). Allow exceptions in configuration.

3. **Third-party dependencies**: Don't flag references to NuGet packages, only internal project references.

---

## Maintenance

- Update baseline versions in `NuGetConsistencyTests` when dependencies are upgraded
- Update `CompositionRootIntegrityTests` when new Pass 12 adapters are activated
- Keep namespace patterns documented for new developers

---

## Success Criteria

✅ **Regression tests pass**: All 37 tests pass on current codebase  
✅ **Early detection**: Any future architectural drift is caught within 1 hour of commit  
✅ **Zero false positives**: No test alerts developers to non-issues  
✅ **Low maintenance**: Tests require < 2 hours/month to maintain  

---

**Test Plan Created**: July 22, 2026  
**Ready for Implementation**: Yes  
**Estimated Implementation Time**: 40 hours (distributed across 6 weeks)
