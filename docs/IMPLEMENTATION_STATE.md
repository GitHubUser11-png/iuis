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
| Passes 0–7 | Completed, compiled, tested, merged, closure-validated, and synchronized | PRs #1–#26; Pass 7 final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 predecessor PR | Closed without merge | PR #27 |
| Pass 8 implementation | Recovered, validated, and merged | PR #28; integration commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce` |
| Pass 8 closure | Independently validated and merged | PR #30; closure integration commit `60dffbba76509a658f0ca87ca3c2862ec5d1b641` |
| Pass 8 mainline promotion | Completed | PR #31; mainline commit `a4db81b8c02297bc1ff7707c4b2023d2787bf8f4` |
| Exact mainline-tree validation | Successful | PR #32; run `29693704636`; 0 warnings, 0 errors; 110 tests passed |
| Production repository catalog | Created, validated, merged, and promoted | exactly 49 descriptors: 14 principal and 35 supporting |
| Production JSON templates | Created, validated, merged, and promoted | exactly 49 files under `templates/production-data` |
| Persistence coordination | Created, validated, merged, and promoted | envelopes, revisions, cross-process locks, atomic writes, journaled transactions, recovery |
| Security bootstrap | Created, validated, merged, and promoted | login attempts, lockout, PBKDF2, restricted sessions, forced password change, one-time bootstrap |
| Pass 8 test count | 110 passed | 96 existing plus 14 Infrastructure tests |
| Figma closure model | Created | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Executable certification | Not achieved | final release gate only |

## Pass 8 validation evidence

| Stage | Run | Artifact | ID | SHA-256 |
|---|---:|---|---:|---|
| Original implementation | `29691593386` | `iuis-windows-build-evidence-88` | `8443750845` | `d79a4789460820da807e3147ede3edef6a754a490af14f158fa4c9b45f84d0ca` |
| Replacement PR | `29692698528` | `iuis-windows-build-evidence-90` | `8444064263` | `8955298d4693623d585e62bdae83750106d74a269d5aa0c488b8d6436bc931d3` |
| Integrated-tree closure | `29693351179` | `iuis-windows-build-evidence-93` | `8444253055` | `18b78f3e2d44592e9f9b73dd96522ada56a728afacea76fd4d5faafc21aa9e11` |
| Final closure head | `29693457219` | `iuis-windows-build-evidence-95` | `8444287182` | `9627ed324cb1fada54a0f104d494df5fc8a9f4cf1bf287b9367128179c10b818` |
| Mainline promotion PR | `29693549723` | `iuis-windows-build-evidence-97` | `8444309381` | `6213f0fe180be2b23a7f52750ec4f33c98d1e79180c6a2a10d9a367a4c20823d` |
| Exact mainline tree | `29693704636` | `iuis-windows-build-evidence-99` | `8444355210` | `73b586dc6486cf0e55fbde4dccb6c3590dd0b6a001ab26eebc8897939ba4e1ce` |

Every successful run passed source-tree and architecture validation, exact 49-template validation, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero compiler warnings, zero compiler errors, all 110 MSTest cases, TRX verification, and artifact publication.

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
| 8 | Production repository and security bootstrap foundation | Completed, validated, integrated, closure-validated, promoted to `main`, and exact-mainline-tree validated |
| 9 | Application authorization, typed repository adapters, restricted DTO projection, and session-aware orchestration | Exact next construction boundary |
| 10+ | Business UI, operations, deployment, and certification | Not started |

## Current truthful completion statement

Passes 0 through 8 are implemented in the authoritative repository history. Pass 8 establishes the exact 49-file production repository catalog and templates, central ID allocation, cross-process locking, hardened atomic writes, journaled multi-file transactions and recovery, login-attempt tracking and lockout, PBKDF2-HMAC-SHA256 password hashing, restricted first-login sessions, forced password replacement, and one-time production bootstrap without a fixed credential. Six successful Windows validation stages completed with zero warnings, zero errors, and all 110 tests passing.

Pass 8 is not release certification. Application authorization orchestration, complete typed repositories, restricted DTO projection, business-module Forms, backup/restore execution, deployment, and final security/recovery certification remain incomplete.

## Exact next starting point

# Integrated University Information System Build Execution — Pass 9: Application Authorization and Permission Resolution, Typed Repository Adapters, Restricted DTO Projection, Session-Aware Command and Query Orchestration, Student Own-Record Enforcement, Employee Permission Boundaries, Expanded Application and Infrastructure Tests, Windows Release Compilation, Figma Application-Service Model, and Pull Request Integration

Pass 9 must begin from the final synchronized Pass 8 `develop` baseline after closure-finalization documentation is integrated. Its objectives are permission resolution, role and confidentiality enforcement, typed persistence adapters, restricted DTOs, session-derived ownership, session-aware command/query validation, transaction-coordinated mutations, and expanded Application/Infrastructure tests.