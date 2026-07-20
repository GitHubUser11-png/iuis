# IUIS Implementation State

Last updated: 2026-07-20

## Status vocabulary

The project distinguishes Specified, Created, Committed, Verified in GitHub, Compiled, Tested, Merged, and Release-certified states. These states are not interchangeable.

## Authoritative baseline

| Item | State | Evidence |
|---|---|---|
| Private GitHub repository | Created | `GitHubUser11-png/iuis` |
| Passes 0–7 | Completed, compiled, tested, merged, closure-validated, and synchronized | PRs #1–#26; Pass 7 final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 final baseline | Promoted, exact-tree validated, and synchronized | `main` and `develop` at `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f` |
| Pass 8 final evidence | Successful | run `29693980046`; artifact `iuis-windows-build-evidence-102`; ID `8444441580`; SHA-256 `8d082a7a293ae9bc1bc5ada5badee1414ab8f4689de2ab772c04e99118d960c9` |
| Production repository catalog and templates | Created, validated, merged, and promoted | exactly 49 descriptors and 49 JSON files |
| Persistence and security foundation | Created, validated, merged, and promoted | locks, atomic writes, journaled transactions, recovery, lockout, PBKDF2, restricted sessions, bootstrap |
| Pass 8 test count | 110 passed | 96 existing plus 14 Infrastructure tests |
| Pass 9 branch and PR | Created; integration pending | `build/pass-09-application-authorization-repositories`; PR #35 |
| Pass 9 authorization | Created and implementation-validated | active profiles, grants, restrictions, role, application, session purpose, ownership, confidentiality |
| Pass 9 repositories | Created and implementation-validated | typed contracts, mapped adapters, stable-ID lookup, optimistic revisions, journaled Application transactions |
| Pass 9 projections | Created and implementation-validated | own-record, self-service, released and internal Counseling, Discipline, Medical DTOs |
| Pass 9 implementation validation | Successful | run `29715030204`; head `898e1c54187c1e2ed5feec9f0085c59b525c7efd`; 0 warnings; 0 errors; 127 tests passed |
| Pass 9 artifact | Verified | `iuis-windows-build-evidence-105`; ID `8450258190`; SHA-256 `9fe43133d8c3c3bdb30ba5d51367233d3c474e13b3a4219d95a2e0364d9da318` |
| Pass 9 Figma model | Created | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Executable certification | Not achieved | final release gate only |

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration | Source and 17 tests created; implementation validation successful; final-head validation, PR merge, and closure pending |
| 10+ | Business UI, operations, deployment, certification | Not started |

## Current truthful completion statement

Passes 0–8 are synchronized at `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f`. Pass 9 establishes Application authorization, session-aware execution, session-derived Student ownership, Employee and Administrator boundaries, released/internal DTO separation, mapped typed repository seams, and journal-coordinated Application transactions. The implementation head compiled with zero warnings and zero errors, and all 127 tests passed. Pass 9 is not merged or closure-complete yet.

Complete specialized JSON mappers for every evolved Domain aggregate shape, production Forms, trusted-device/network enforcement, backup/restore execution, deployment, and release certification remain incomplete.

## Exact next gate

Validate the evidence-updated final PR #35 head, merge it into `develop`, independently validate the merged Pass 9 tree, promote the completed baseline to `main`, synchronize `develop`, and define the Pass 10 boundary.
