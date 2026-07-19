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
| Default branch | Finalized through Pass 7 | `main` at `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Integration branch | Pass 8 implementation integrated | `develop` at `4920f3dec56e584bb3d4aa620b194b5dd31c36ce` before closure |
| Passes 1 and 2 | Completed, compiled, tested, and merged | PRs #1–#4 |
| Pass 3 Domain foundations | Completed, compiled, tested, and merged | PRs #5 and #6 |
| Pass 4 core identity/person aggregates | Completed, compiled, tested, and merged | PR #8 and synchronized follow-up history |
| Pass 5 academic aggregates | Completed, compiled, tested, and merged | PRs #11, #14, and #16 |
| Pass 6 Finance foundations | Completed, compiled, tested, merged, and closure-validated | PRs #17 and #18 |
| Pass 7 Student Service Operations | Completed, compiled, tested, merged, closure-validated, and synchronized | PRs #23–#26; final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 predecessor PR | Closed without merge | PR #27 |
| Pass 8 implementation | Recovered, validated, and merged into `develop` | PR #28; integration commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce` |
| Pass 8 original validation | Successful | run `29691593386`; 0 warnings, 0 errors; 110 tests passed |
| Pass 8 original artifact | Verified | `iuis-windows-build-evidence-88`, ID `8443750845`, SHA-256 `d79a4789460820da807e3147ede3edef6a754a490af14f158fa4c9b45f84d0ca` |
| Pass 8 replacement validation | Successful | run `29692698528`; 0 warnings, 0 errors; 110 tests passed |
| Pass 8 replacement artifact | Verified | `iuis-windows-build-evidence-90`, ID `8444064263`, SHA-256 `8955298d4693623d585e62bdae83750106d74a269d5aa0c488b8d6436bc931d3` |
| Pass 8 integrated-tree closure validation | Successful | PR #30; run `29693351179`; head `525c4db77f9aec30b9d586c898a2acb0a9d62140`; 0 warnings, 0 errors; 110 tests passed |
| Pass 8 closure artifact | Verified | `iuis-windows-build-evidence-93`, ID `8444253055`, SHA-256 `18b78f3e2d44592e9f9b73dd96522ada56a728afacea76fd4d5faafc21aa9e11` |
| Visual Studio solution | Created and compiled through integrated Pass 8 | `IUIS.sln` |
| C# projects | 7 created and compiled through integrated Pass 8 | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Production repository catalog | Created and validated | exactly 49 descriptors: 14 principal and 35 supporting |
| Production JSON templates | 49 created and validated | `templates/production-data/*.json` |
| Persistence coordination foundation | Created, validated, and integrated | envelopes, revision checks, locks, atomic writes, journaled transactions, recovery |
| Security bootstrap foundation | Created, validated, and integrated | login attempts, lockout, PBKDF2, restricted sessions, forced password change, one-time bootstrap |
| Pass 8 test count | 110 passed | 96 existing plus 14 Infrastructure tests |
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
- exactly 49 authoritative production JSON files
- centralized identifiers and journaled multi-file mutations
- no Forms that read or write JSON directly

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, and Student Service foundations | Completed, compiled, tested, merged, and synchronized |
| 8 | Production repository and security bootstrap foundation | Implementation integrated; integrated-tree closure validation successful; final closure-head validation and mainline promotion pending |
| 9+ | Application orchestration, complete typed repositories, UI, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 7 are integrated and mainline-synchronized. Pass 8 is recovered, implementation-validated, merged into `develop`, and independently validated from the actual integrated tree. The baseline contains the exact 49-repository catalog and templates, central ID allocation, cross-process locks, hardened atomic writes, journaled transactions, login-attempt lockout, forced password change, and production bootstrap. Three successful Pass 8 runs have completed with zero warnings, zero errors, and all 110 tests passing.

Pass 8 is not yet promoted to `main` or finally synchronized. Application authorization orchestration, complete typed repositories, restricted DTO projection, business-module Forms, backup/restore execution, deployment, and release certification remain incomplete.

## Exact next gate

Validate the final closure documentation head, merge PR #30 into `develop`, promote the resulting complete Pass 8 baseline to `main`, validate the exact mainline integration commit, synchronize `develop` to final `main`, verify zero divergence, record final evidence, and begin Pass 9 from that synchronized commit.