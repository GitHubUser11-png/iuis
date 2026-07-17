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
| C# projects | 7 created and compiled | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and executed | `.github/workflows/windows-build.yml` |
| Build scripts | Created and executed | source-tree validation and Release build/test scripts |
| Test framework integration | Created and executed | MSTest framework and adapter 3.6.4 |
| Initial automated tests | 3 passed | Domain, Application, and Infrastructure canonical marker tests |
| Production JSON templates | 0 created | Scheduled after repository contracts |
| Release compilation | Successful | GitHub Actions run `29549975855`; 0 warnings, 0 errors |
| Automated test execution | Successful | `IUIS.Tests.trx`; 3 executed, 3 passed, 0 failed |
| Build evidence artifact | Verified | `iuis-windows-build-evidence-6`, SHA-256 `759b84fdf04a7b9f929d60b6c1eacc4fb5d6bed1714992c80116c0bebd1f4b1c` |
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
| 1 | Seven-project Visual Studio solution foundation | Created, compiled through stacked Pass 2 evidence, and awaiting ordered merge |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Created, committed, compiled, and tested; PR #2 open |
| 3+ | Domain, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

The seven-project source foundation compiles successfully in Release configuration on the Windows GitHub Actions runner. The structural validator, NuGet restoration, MSBuild compilation, MSTest discovery, three initial tests, TRX generation, and evidence-artifact publication have completed successfully. No production Domain model, Application service layer, JSON persistence engine, repository template set, authentication workflow, business module, backup/restore implementation, or release-certified executable exists yet.
