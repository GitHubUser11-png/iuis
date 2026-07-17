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
| Combined foundation commit | Merged into `develop` | `7f61d529380923b04a959d655320150940f3549a` |
| Pass 2 closure | Completed and merged | PR #3 and final correction PR #4 |
| Current pre-Pass-3 `develop` head | Verified | `ff5a2ced7dfbfc85078dc7800f94b698ff6de007` |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created, compiled, tested, and merged | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created, enforced, and merged | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created, executed, and merged | `.github/workflows/windows-build.yml` |
| Build scripts | Created, executed, and merged | source-tree validation and Release build/test scripts |
| Test framework integration | Created, executed, and merged | MSTest framework and adapter 3.6.4 |
| Pass 2 automated tests | 3 passed | Domain, Application, and Infrastructure canonical marker tests |
| Pass 3 branch | Created | `build/pass-03-domain-foundations` |
| Production Domain foundations | Created on Pass 3 branch | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Pass 3 Domain tests | 22 created | awaiting Windows execution |
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
| 1 | Seven-project Visual Studio solution foundation | Completed, compiled, tested, and merged through PR #1 |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Completed, compiled, tested, and merged through PR #2 and PR #1 |
| 2 Closure | Final evidence, ordered integration, and post-merge validation | Completed, validated, and merged through PR #3 and PR #4 |
| 3 | Production Domain foundations | Created and committed on pass branch; compilation and tests pending pull-request workflow |
| 4+ | Domain aggregates, Application, Infrastructure, UI, modules, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 and 2 are integrated into `develop` and remain compile-verified and test-verified. Pass 3 production Domain foundations now exist on `build/pass-03-domain-foundations`, including entity/versioning contracts, person and contact value objects, institution-local dates, monetary rules, canonical identity enumerations, and 22 new MSTest methods. These Pass 3 sources are not yet described as compiled or tested because the pull-request Windows workflow has not completed. No production aggregate set, Application service layer, JSON persistence engine, repository template set, authentication workflow, business module, backup/restore implementation, or release-certified executable exists yet.

## Exact next gate

Open the first Domain pull request into `develop`, execute the Windows Release build and all 25 tests, correct any C# 7.3 or .NET Framework 4.8 diagnostics, record final evidence, and merge only after the final documented head passes.
