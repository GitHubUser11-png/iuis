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
| Default branch | Updated through finalized Pass 7 closure | `main` |
| Integration branch | Synchronized to finalized Pass 7 mainline | `develop` |
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
| Pass 7 closure | Completed, validated, and promoted to mainline | PRs #24 and #25; finalized commit `6fdf2479af4494924b7249df4896f6d170ae0f49` before documentation finalization |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created, compiled, and tested through Pass 7 | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and operational | `.github/workflows/windows-build.yml` |
| Production Domain foundations | Created, compiled, tested, and merged | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Core identity/person aggregates | Created, compiled, tested, and merged | InstitutionIdentifier, UserAccount, UserSession, StudentRecord, EmployeeRecord |
| Academic foundation aggregates | Created, compiled, tested, and merged | Course, Curriculum, Subject, prerequisite graph, AcademicPeriod, Enrollment, snapshots |
| Finance foundation aggregates | Created, compiled, tested, and merged | charge rules, Tuition Assessment, Scholarship Award effects, Financial Adjustment, Payment, derived Student Ledger |
| Student Service Domain aggregates | Created, compiled, tested, merged, and closure-validated | Library, Counseling, Discipline, Clinic, Medical Record, Medical Clearance |
| Pass 7 Windows validations | Successful | runs `29688650226`, `29688849862`, `29689019318`, `29689147617`, and `29689242236`; 0 warnings, 0 errors; 96 tests passed |
| Final Pass 7 integration artifact | Verified | `iuis-windows-build-evidence-76`, artifact `8443050329`, SHA-256 `5e3fa2f01d91dcf3d1bb1b2295f6e9faf7866e1f6f33af25246c5d5d9e955ad6` |
| Production JSON templates | 0 created | Pass 8 construction boundary |
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
| 7 | Student Service Operations Domain foundations | Completed, compiled, tested, merged, closure-validated, and mainline-synchronized through PRs #23–#25 |
| 8+ | Repository, security bootstrap, remaining modules, UI, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 7 are integrated into the authoritative repository history. The Student Service Domain baseline covers Library inventory and Borrowings, Counseling confidential sessions and controlled releases, Discipline incident and Violation workflows, Clinic appointments, append-only Medical Records, and Clinic Medical Clearance history. Five Windows validations completed with zero warnings and zero errors, and all 96 tests passed with TRX and artifact evidence. No production JSON persistence engine, 49-file repository template set, authentication implementation, Application orchestration, business-module UI, backup/restore implementation, or release-certified executable exists yet.

## Exact next starting point

The next implementation branch must begin from the finalized Pass 7 `develop` baseline. Pass 8 must create the production repository and security-bootstrap foundation: authoritative repository catalog, initial JSON files, central ID sequence allocation, cross-process file locks, hardened atomic writes, journaled multi-file transactions, Login attempt tracking, lockout, forced password change, and production bootstrap. Domain and Forms must not access JSON or the file system directly.
