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
| Default branch | Created | `main` |
| Integration branch | Created and updated | `develop` |
| Passes 1 and 2 | Completed, compiled, tested, and merged | PRs #1–#4 |
| Pass 3 Domain foundations | Completed, compiled, tested, and merged | PRs #5 and #6 |
| Pass 4 core identity/person aggregates | Completed, compiled, tested, and merged | PR #8; integration commit `54633e7585b5b2341f3a615cc886ac6bc018afa0` |
| Pass 5 academic branch | Created and committed | `build/pass-05-academic-foundations` |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created, compiled, tested, and merged through Pass 4 | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and operational | `.github/workflows/windows-build.yml` |
| Production Domain foundations | Created, compiled, tested, and merged | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Core identity/person aggregates | Created, compiled, tested, and merged | InstitutionIdentifier, UserAccount, UserSession, StudentRecord, EmployeeRecord |
| Academic foundation aggregates | Created on Pass 5 branch; validation pending | Course, Curriculum, Subject, prerequisite graph, AcademicPeriod, Enrollment, snapshots |
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
| 0 | Repository access, initial baseline, governance, and `develop` branch | Completed and merged |
| 1 | Seven-project Visual Studio solution foundation | Completed, compiled, tested, and merged |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Completed, compiled, tested, and merged |
| 3 | Production Domain foundations | Completed, compiled, tested, and merged |
| 4 | Core identity and person aggregates | Completed, compiled, tested, and merged through PR #8 |
| 5 | Academic foundation aggregates | Created and committed on branch; Windows validation and PR integration pending |
| 6+ | Remaining Domain aggregates, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 4 are integrated into `develop`. Pass 5 academic Domain source and tests exist on `build/pass-05-academic-foundations`, but no compilation or test claim is made until the final branch head passes the Windows workflow. No Application service layer, production JSON persistence engine, 49-file repository template set, authentication implementation, business-module UI, backup/restore implementation, or release-certified executable exists yet.

## Exact next gate

Open the Pass 5 pull request, execute the Windows validation workflow, correct compiler or test failures, record final evidence, merge into `develop`, and independently validate the merged academic Domain baseline before beginning the next Domain aggregate group.
