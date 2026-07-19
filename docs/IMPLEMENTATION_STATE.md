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
| Integration branch | Recreated from validated Pass 5 baseline | `develop` |
| Passes 1 and 2 | Completed, compiled, tested, and merged | PRs #1–#4 |
| Pass 3 Domain foundations | Completed, compiled, tested, and merged | PRs #5 and #6 |
| Pass 4 core identity/person aggregates | Completed, compiled, tested, and merged | PR #8 and synchronized follow-up history |
| Pass 4 preflight repair | Completed, compiled, tested, and merged | PR #12; run `29684773571`; 33 tests passed |
| Pass 5 academic aggregates | Completed and merged | PR #11; implementation merge `7dffa7498bf1efece00cffe417cc76b86c285547` |
| Pass 5 validation correction | Completed and merged | PR #14; final integration commit `6b5db90fce2691f7b76fbd5eed2731aa01179b82` |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created, compiled, and tested through Pass 5 | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and operational | `.github/workflows/windows-build.yml` |
| Production Domain foundations | Created, compiled, tested, and merged | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Core identity/person aggregates | Created, compiled, tested, and merged | InstitutionIdentifier, UserAccount, UserSession, StudentRecord, EmployeeRecord |
| Academic foundation aggregates | Created, compiled, tested, and merged | Course, Curriculum, Subject, prerequisite graph, AcademicPeriod, Enrollment, snapshots |
| Final Pass 5 validation | Successful | run `29685193452`; 0 warnings, 0 errors; 54 tests passed |
| Final Pass 5 artifact | Verified | `iuis-windows-build-evidence-47`, artifact `8441849007`, SHA-256 `0ca4c821cc8383c63536d5e6e4d1335131082cdacc66793c20a5d69a67754fcf` |
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
| 5 | Academic foundation aggregates | Completed, compiled, tested, and merged through PR #11 and correction PR #14 |
| 6+ | Remaining Domain aggregates, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 5 are integrated into the authoritative repository history. The academic Domain baseline includes Course, Curriculum, Subject, prerequisite-graph, Academic Period, Enrollment, and immutable snapshot contracts. The final Windows Release build completed with zero warnings and zero errors, and all 54 tests passed with TRX and artifact evidence. No Application service layer, production JSON persistence engine, 49-file repository template set, authentication implementation, business-module UI, backup/restore implementation, or release-certified executable exists yet.

## Exact next starting point

The next implementation branch must begin from `develop` at the validated Pass 5 integration baseline. Pass 6 should create the Finance Domain foundation group: tuition Assessment, charge rules, Scholarship award effects, Payment and allocation lifecycle, immutable posted-finance invariants, derived ledger contracts, and expanded Domain tests. Persistence, cross-process locking, UI, and production transaction coordination remain outside Domain and must not be introduced there.
