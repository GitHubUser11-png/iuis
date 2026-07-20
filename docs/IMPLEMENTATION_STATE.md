# IUIS Implementation State

Last updated: 2026-07-20

## Status vocabulary

The project distinguishes Specified, Created, Committed, Verified in GitHub, Compiled, Tested, Merged, and Release-certified states. These states are not interchangeable.

## Authoritative baseline

| Item | State | Evidence |
|---|---|---|
| Private GitHub repository | Created | `GitHubUser11-png/iuis` |
| Passes 0–7 | Completed, compiled, tested, merged, closure-validated, and synchronized | PRs #1–#26; Pass 7 final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 implementation | Recovered, validated, and merged | PR #28; integration commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce` |
| Pass 8 closure | Independently validated and merged | PR #30; closure commit `60dffbba76509a658f0ca87ca3c2862ec5d1b641` |
| Pass 8 final baseline | Promoted, exact-tree validated, and synchronized | `main` and `develop` at `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f` |
| Pass 8 final exact-tree evidence | Successful | run `29693980046`; artifact `iuis-windows-build-evidence-102`; ID `8444441580`; SHA-256 `8d082a7a293ae9bc1bc5ada5badee1414ab8f4689de2ab772c04e99118d960c9` |
| Production repository catalog and templates | Created, validated, merged, and promoted | exactly 49 descriptors and 49 JSON files |
| Persistence and security foundation | Created, validated, merged, and promoted | locks, atomic writes, journaled transactions, recovery, lockout, PBKDF2, restricted sessions, bootstrap |
| Pass 8 test count | 110 passed | 96 existing plus 14 Infrastructure tests |
| Pass 9 branch | Created; compilation and tests pending | `build/pass-09-application-authorization-repositories` |
| Pass 9 authorization | Created on branch | active profiles, grants, restrictions, role, application, session purpose, ownership, confidentiality |
| Pass 9 repositories | Created on branch | typed contracts, mapped adapters, stable-ID lookup, optimistic revisions, journaled Application transactions |
| Pass 9 projections | Created on branch | own-record, self-service, released and internal Counseling, Discipline, Medical DTOs |
| Pass 9 tests | Created; execution pending | 17 new tests; expected suite total 127 |
| Executable certification | Not achieved | final release gate only |

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration | Source and tests created; Windows validation and PR integration pending |
| 10+ | Business UI, operations, deployment, certification | Not started |

## Current truthful completion statement

Passes 0–8 are synchronized at `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f`. Pass 9 source establishes Application authorization, session-aware execution, session-derived Student ownership, Employee and Administrator boundaries, released/internal DTO separation, mapped typed repositories, and journal-coordinated Application transactions. These Pass 9 changes are not yet compiled, tested, reviewed, or merged.

## Exact next gate

Commit the Pass 9 source and 17 tests, open the pull request against `develop`, complete Windows Release compilation and all 127 tests, correct defects, merge the validated head, and closure-validate the merged tree before mainline promotion.
