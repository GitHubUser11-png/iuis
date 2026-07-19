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
| Default branch | Created and updated | `main` |
| Integration branch | Temporarily absent after repository synchronization; restoration pending Pass 7 integration | `develop` |
| Passes 1 and 2 | Completed, compiled, tested, and merged | PRs #1–#4 |
| Pass 3 Domain foundations | Completed, compiled, tested, and merged | PRs #5 and #6 |
| Pass 4 core identity/person aggregates | Completed, compiled, tested, and merged | PR #8 and synchronized follow-up history |
| Pass 4 preflight repair | Completed, compiled, tested, and merged | PR #12; run `29684773571`; 33 tests passed |
| Pass 5 academic aggregates | Completed and merged | PR #11; implementation merge `7dffa7498bf1efece00cffe417cc76b86c285547` |
| Pass 5 validation correction | Completed and merged | PR #14; final integration commit `6b5db90fce2691f7b76fbd5eed2731aa01179b82` |
| Pass 5 closure | Completed and merged | PR #16; closure commit `3fe05ae42f076a4c9ca7ff9e3f197ca8c8d4a9dd` |
| Pass 6 Finance foundations | Completed, compiled, tested, and merged | PR #17; integration commit `d5b24245009bfc8b6639a5bbdc7fa1e6d7af59eb` |
| Pass 6 closure | Completed, validated, and merged | PR #18; closure commit `9dcff9616dc8afb19af6d5bcf0497db77b31caa6` |
| Pass 7 Student Service Operations | Created, compiled, and tested; integration pending | recovered branch `build/pass-07-student-service-operations-v2` |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created and compiled through Pass 7 | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and operational | `.github/workflows/windows-build.yml` |
| Production Domain foundations | Created, compiled, tested, and merged | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Core identity/person aggregates | Created, compiled, tested, and merged | InstitutionIdentifier, UserAccount, UserSession, StudentRecord, EmployeeRecord |
| Academic foundation aggregates | Created, compiled, tested, and merged | Course, Curriculum, Subject, prerequisite graph, AcademicPeriod, Enrollment, snapshots |
| Finance foundation aggregates | Created, compiled, tested, and merged | charge rules, Tuition Assessment, Scholarship Award effects, Financial Adjustment, Payment, derived Student Ledger |
| Student Service Domain aggregates | Created, compiled, and tested on recovered Pass 7 branch | Library, Counseling, Discipline, Clinic, Medical Record, Medical Clearance |
| Pass 7 implementation validation | Successful | run `29688650226`; 0 warnings, 0 errors; 96 tests passed |
| Pass 7 implementation artifact | Verified | `iuis-windows-build-evidence-67`, artifact `8442878636`, SHA-256 `c4d3090e74b3686125bee1ff82b117606b3e0a556a7225a7b0a4cb8b4f11e432` |
| Production JSON templates | 0 created | scheduled after repository contracts |
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
| 7 | Student Service Operations Domain foundations | Created, compiled, and tested; final recovered-head validation and integration pending |
| 8+ | Remaining Domain aggregates, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 6 are integrated into the authoritative repository history. Pass 7 source and 24 new tests compile successfully on the recovered implementation branch, covering Library inventory and Borrowings, Counseling confidential sessions and controlled releases, Discipline incident and Violation workflows, Clinic appointments, append-only Medical Records, and Clinic Medical Clearance history. The Windows Release workflow completed with zero warnings and zero errors, and all 96 tests passed with TRX and artifact evidence. Final recovered-head validation and pull-request integration remain pending. No Application service layer, production JSON persistence engine, 49-file repository template set, authentication implementation, business-module UI, backup/restore implementation, or release-certified executable exists yet.

## Exact next gate

Validate the recovered final documentation head, open the replacement Pass 7 integration pull request against current `main`, merge after all Windows gates pass, recreate `develop` from the integrated mainline, and independently closure-validate the merged Student Service Domain baseline.
