# IUIS Implementation State

Last updated: 2026-07-19

## Status vocabulary

The project uses the following evidence levels. These terms are not interchangeable.

| State | Meaning |
|---|---|
| Specified | A requirement or contract has been documented. |
| Created | Source or configuration exists in the authoritative Git repository. |
| Committed | The created material belongs to a named Git commit. |
| Verified in GitHub | The connector refetched the committed material from the expected repository and branch. |
| Compiled | A compatible compiler completed successfully and build evidence exists. |
| Tested | A test runner completed and machine-readable results exist. |
| Merged | A reviewed implementation pass was merged into its target branch. |
| Release-certified | Every mandatory build, test, security, backup, restore, and recovery gate passed. |

## Authoritative baseline

| Item | State | Evidence |
|---|---|---|
| Private GitHub repository | Created | `GitHubUser11-png/iuis` |
| Default branch | Updated through Pass 7 integration | `main` at `b4cb980d3989160969a02b4b5a51a162a088d695` before closure |
| Integration branch | Recreated from integrated Pass 7 mainline | `develop` |
| Passes 1 and 2 | Completed, compiled, tested, and merged | PRs #1–#4 |
| Pass 3 Domain foundations | Completed, compiled, tested, and merged | PRs #5 and #6 |
| Pass 4 core identity/person aggregates | Completed, compiled, tested, and merged | PR #8 and synchronized follow-up history |
| Pass 4 preflight repair | Completed, compiled, tested, and merged | PR #12; run `29684773571`; 33 tests passed |
| Pass 5 academic aggregates | Completed and merged | PR #11; implementation merge `7dffa7498bf1efece00cffe417cc76b86c285547` |
| Pass 5 validation correction | Completed and merged | PR #14; final integration commit `6b5db90fce2691f7b76fbd5eed2731aa01179b82` |
| Pass 5 closure | Completed and merged | PR #16; closure commit `3fe05ae42f076a4c9ca7ff9e3f197ca8c8d4a9dd` |
| Pass 6 Finance foundations | Completed, compiled, tested, and merged | PR #17; integration commit `d5b24245009bfc8b6639a5bbdc7fa1e6d7af59eb` |
| Pass 6 closure | Completed, validated, and merged | PR #18; closure commit `9dcff9616dc8afb19af6d5bcf0497db77b31caa6` |
| Pass 7 Student Service Operations | Completed, compiled, tested, and merged | PR #23; integration commit `b4cb980d3989160969a02b4b5a51a162a088d695` |
| Pass 7 closure | Independently validated; merge pending | PR #24; run `29689019318` |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created, compiled, and tested through Pass 7 | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and operational | `.github/workflows/windows-build.yml` |
| Production Domain foundations | Created, compiled, tested, and merged | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Core identity/person aggregates | Created, compiled, tested, and merged | InstitutionIdentifier, UserAccount, UserSession, StudentRecord, EmployeeRecord |
| Academic foundation aggregates | Created, compiled, tested, and merged | Course, Curriculum, Subject, prerequisite graph, AcademicPeriod, Enrollment, snapshots |
| Finance foundation aggregates | Created, compiled, tested, and merged | charge rules, Tuition Assessment, Scholarship Award effects, Financial Adjustment, Payment, derived Student Ledger |
| Student Service Domain aggregates | Created, compiled, tested, and merged | Library, Counseling, Discipline, Clinic, Medical Record, Medical Clearance |
| Pass 7 implementation validations | Successful | runs `29688650226` and `29688849862`; 0 warnings, 0 errors; 96 tests passed |
| Pass 7 closure validation | Successful | run `29689019318`; 0 warnings, 0 errors; 96 tests passed |
| Pass 7 closure artifact | Verified | `iuis-windows-build-evidence-72`, artifact `8442982829`, SHA-256 `cc8e7f68426e93a77328cbdc634a13a1c3e01160385ce499106bf9b835704239` |
| Production JSON templates | 0 created | next locked construction boundary |
| Executable certification | Not achieved | final release gate only |

## Locked implementation target

- C# 7.3
- Windows Forms
- .NET Framework 4.8
- `System.Text.Json`
- seven-project solution
- separate User and Administrator executables
- layered architecture
- shared synchronized JSON persistence
- exactly 49 authoritative production JSON files after template implementation
- centralized identifiers and journaled multi-file mutations
- no Forms that read or write JSON directly

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0 | Repository access, initial baseline, governance, and integration branch | Completed and merged |
| 1 | Seven-project Visual Studio solution foundation | Completed, compiled, tested, and merged |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Completed, compiled, tested, and merged |
| 3 | Production Domain foundations | Completed, compiled, tested, and merged |
| 4 | Core identity and person aggregates | Completed, compiled, tested, and merged |
| 5 | Academic foundation aggregates | Completed, compiled, tested, and merged through PR #11, correction PR #14, and closure PR #16 |
| 6 | Finance Domain foundations | Completed, compiled, tested, merged, and closure-validated through PRs #17 and #18 |
| 7 | Student Service Operations Domain foundations | Completed, compiled, tested, merged through PR #23, and closure-validated on PR #24 |
| 8+ | Repository, security bootstrap, remaining modules, UI, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 7 are integrated into the authoritative repository history. The Student Service Domain baseline covers Library inventory and Borrowings, Counseling confidential sessions and controlled releases, Discipline incident and Violation workflows, Clinic appointments, append-only Medical Records, and Clinic Medical Clearance history. The implementation and independent closure workflows completed Windows Release builds with zero warnings and zero errors, and all 96 tests passed with TRX and artifact evidence. Closure-document integration remains pending. No Application service layer, production JSON persistence engine, 49-file repository template set, authentication implementation, business-module UI, backup/restore implementation, or release-certified executable exists yet.

## Exact next gate

Merge the validated Pass 7 closure documentation, synchronize `main` and `develop`, and begin the locked production repository and security-bootstrap foundation.
