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
| Release compilation | Successful on final Pass 2 documentation head | Run `29550063410`; commit `7279793ab12adea13e899ca81f5980cd9b68d5b9`; 0 warnings, 0 errors |
| Automated test execution | Successful | `IUIS.Tests.trx`; 3 executed, 3 passed, 0 failed |
| Build evidence artifact | Verified | `iuis-windows-build-evidence-7`, artifact `8395407334`, SHA-256 `35ab9b87b7158d90f4eb59499045ab40d48f53859d371fedf7a6ea05060bad46` |
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
| 1 | Seven-project Visual Studio solution foundation | Created and compile-verified through stacked Pass 2 evidence; PR #1 awaiting ordered integration |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Created, committed, compiled, tested, and documented; PR #2 awaiting ordered integration |
| 3+ | Domain, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

The seven-project source foundation compiles successfully in Release configuration on the Windows GitHub Actions runner. The structural validator, NuGet restoration, MSBuild compilation, MSTest discovery, three initial tests, TRX generation, and evidence-artifact publication have completed successfully on the final documented Pass 2 branch head. Passes 1 and 2 are not yet integrated into `develop`. No production Domain model, Application service layer, JSON persistence engine, repository template set, authentication workflow, business module, backup/restore implementation, or release-certified executable exists yet.