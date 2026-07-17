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
| Integration branch | Created and updated | `develop` |
| Pass 1 branch | Integrated | PR #1 |
| Pass 2 branch | Integrated into Pass 1 | PR #2 |
| Integrated baseline commit | Created on `develop` | `7f61d529380923b04a959d655320150940f3549a` |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created, compiled, tested, and merged | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created, enforced, and merged | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created, executed, and merged | `.github/workflows/windows-build.yml` |
| Build scripts | Created, executed, and merged | source-tree validation and Release build/test scripts |
| Test framework integration | Created, executed, and merged | MSTest framework and adapter 3.6.4 |
| Initial automated tests | 3 passed | Domain, Application, and Infrastructure canonical marker tests |
| Integration-candidate validation | Successful | Run `29551813015`; commit `e0987c88e497982f6b691a7f1181c0b2d0d05925`; 0 warnings, 0 errors |
| Integration-candidate artifact | Verified | `iuis-windows-build-evidence-12`, artifact `8396004598`, SHA-256 `903bac5c0127a7f31e96934465e60a0e0ca48a7711019bc274b2c845c5396db6` |
| Post-merge Windows validation | Successful | Run `29552005590`; closure head `e2d4c2e32f4a62f3575c9a085ec24ab6709f6459`; 0 warnings, 0 errors; 3 tests passed |
| Post-merge evidence artifact | Verified | `iuis-windows-build-evidence-14`, artifact `8396072032`, SHA-256 `900f7fe14d2d5a2e9f0eeda984c43c6ca0fa823143a8b1a7ffa54d4bdbb321ae` |
| Production JSON templates | 0 created | Scheduled after repository contracts |
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
| 0 | Repository access, initial baseline, governance, and `develop` branch | Completed and merged |
| 1 | Seven-project Visual Studio solution foundation | Completed, compiled, tested, and merged through PR #1 |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Completed, compiled, tested, and merged through PR #2 and PR #1 |
| 2 Closure | Final evidence, ordered integration, and post-merge validation | Post-merge Windows validation passed; closure PR awaiting final validation and merge |
| 3+ | Domain, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 and 2 are integrated into `develop` at commit `7f61d529380923b04a959d655320150940f3549a`. The integrated seven-project source foundation has been revalidated after merge through a closure branch created from that exact `develop` baseline. Release compilation completed with zero warnings and zero errors, and all three foundation tests passed. The remaining closure commits only finalize evidence documentation and are subject to the same pull-request validation before merge. No production Domain model, Application service layer, JSON persistence engine, repository template set, authentication workflow, business module, backup/restore implementation, or release-certified executable exists yet.