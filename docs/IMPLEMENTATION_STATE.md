# IUIS Implementation State

Last updated: 2026-07-17

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
| Integration branch | Created | `develop` |
| Pass 1 branch | Created | `build/pass-01-solution-foundation` |
| Pass 2 branch | Created | `build/pass-02-windows-ci` |
| Visual Studio solution | Created on Pass 1 branch | `IUIS.sln` |
| C# projects | 7 created on Pass 1 branch | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created on Pass 1 branch | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created on Pass 2 branch | `.github/workflows/windows-build.yml` |
| Build scripts | Created on Pass 2 branch | source-tree validation and Release build/test scripts |
| Test framework integration | Created on Pass 2 branch | MSTest framework and adapter package contracts |
| Initial automated tests | 3 created on Pass 2 branch | canonical foundation marker tests |
| Production JSON templates | 0 created | Scheduled after repository contracts |
| Release compilation | Pending workflow execution | No successful Windows build evidence recorded yet |
| Automated test execution | Pending workflow execution | No TRX result recorded yet |
| Executable certification | Not achieved | Final release gate only |

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
| 0 | Repository access, initial baseline, governance, and `develop` branch | Completed |
| 1 | Seven-project Visual Studio solution foundation | Created and committed; PR #1 open |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Created on stacked branch; workflow result pending |
| 3+ | Domain, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

The seven-project source foundation, Windows workflow, build scripts, pinned MSTest packages, and initial structural tests exist on implementation branches. A successful Release build and automated test result are not yet claimed. No production Domain model, Application service layer, JSON persistence engine, repository template set, authentication workflow, business module, backup/restore implementation, or release-certified executable exists yet.
