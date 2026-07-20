# IUIS Implementation State

Last updated: 2026-07-20

## Status vocabulary

The project distinguishes Specified, Created, Committed, Verified in GitHub, Compiled, Tested, Merged, and Release-certified states. These states are not interchangeable.

## Authoritative baseline

| Item | State | Evidence |
|---|---|---|
| Private GitHub repository | Created | `GitHubUser11-png/iuis` |
| Passes 0–7 | Completed, compiled, tested, merged, closure-validated, and synchronized | PRs #1–#26; Pass 7 final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 final baseline | Completed and synchronized | `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f` |
| Pass 9 implementation and closure | Completed, promoted, exact-mainline validated, and synchronized | PRs #35, #38–#42; final synchronized commit `81c4b78acb149cef5a9feef6f8c71ae8b9b3037e` |
| Pass 10 implementation and closure | Completed, promoted, exact-final-mainline validated, and synchronized | PRs #43–#49; final synchronized commit `686aa29a3c9827975d711c6713838f5cc2d22918`; 148 tests passed |
| Pass 11 prerequisite checkpoint | Integrated into `develop` before full pass completion | PR #51; integration commit `5d17748a550c79ac11b341b73acc1256d3b6d144` |
| Pass 11 continuation | Implementation-validated; final evidence-updated PR head and merge pending | branch `build/pass-11-envelope-token-finance`; PR #52 |
| Canonical repository envelopes | Implemented and validated | exact six-field canonical contract across runtime, validator, bootstrap outputs, and 49 templates |
| Legacy envelope migration | Implemented and validated | compatibility reader; revision-preserving journaled one-way rewrite; idempotent all-49 migration; audit record |
| Session-token digest security | Implemented and validated | raw token returned once; SHA-256 digest persisted; fixed-time verification; legacy sessions rejected and revoked |
| Pass 11 canonical aggregate schemas | Implemented and validated | Enrollment, Tuition Assessment, Payment, Financial Adjustment, and Scholarship Award record schema v1 |
| Aggregate mapper readiness | Updated and validated | 11 specialized-complete; 7 fail-closed with explicit reasons |
| Student enrollment and finance reads | Implemented and validated | session-derived ownership; released Enrollment, Assessment, Payment, Scholarship, and balance projections |
| Controlled Enrollment and Finance writes | Implemented and validated | Enrollment submission; Assessment posting; Payment allocation/posting; Adjustment posting; journaled Scholarship application |
| Pass 11 implementation validation | Successful | run `29753253012`; head `df6e5997b83b999cab459f3baa4cf11245ebffc9`; 0 warnings; 0 errors; 160 tests passed |
| Pass 11 artifact | Verified | `iuis-windows-build-evidence-158`; ID `8465588173`; SHA-256 `9fd73b4619feb125daeb3ee0ced0d9e3dd11a052f44a1c3754c985400eb189c5` |
| Production repository catalog and templates | Preserved and validated | exactly 49 descriptors and 49 canonical JSON files |
| Pass 11 Figma model | Completed | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Executable certification | Not achieved | final release gate only |

## Pass 11 evidence

| Stage | Run | Artifact | ID | SHA-256 |
|---|---:|---|---:|---|
| Prerequisite checkpoint | `29748882312` | checkpoint evidence | — | — |
| Complete validated continuation head | `29753253012` | `iuis-windows-build-evidence-158` | `8465588173` | `9fd73b4619feb125daeb3ee0ced0d9e3dd11a052f44a1c3754c985400eb189c5` |

Run `29753253012` passed source-tree and architecture validation, exactly 49 canonical templates, exactly six envelope fields, all seven project boundaries, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero warnings, zero errors, all 160 MSTest cases, TRX verification, and artifact publication.

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration, transaction revision hardening, mapper readiness | Completed, validated, closure-validated, promoted, exact-mainline validated, synchronized |
| 10 | Canonical persisted schemas, specialized mappers, typed activation, composition wiring, Student/Employee vertical slices | Completed, independently closure-validated, promoted, exact-final-mainline validated, synchronized |
| 11 | Canonical envelopes, session-token digests, Enrollment and Finance specialized persistence, Student finance reads, controlled writes | Implementation complete and validated; exact final PR-head validation and merge pending |
| 12+ | Remaining specialized mappers, production UI, operations, deployment, certification | Not started |

## Current truthful completion statement

Passes 0 through 10 are synchronized. Pass 11 has corrected the repository-envelope contract and session-token verifier, normalized all 49 templates and source validation, added journaled legacy-envelope migration, activated specialized persistence for Enrollment and four Finance aggregates, and connected Student financial read models and controlled Enrollment and Finance commands through the real composition root.

The complete Pass 11 continuation head compiled with zero warnings and zero errors and passed 160 of 160 tests. PR #52 still requires validation of the evidence-updated final head and merge into `develop`. Pass 11 independent integrated-tree closure, mainline promotion, final branch synchronization, production Forms, operational deployment, and release certification are not complete.

## Exact next gate

Validate the evidence-updated final PR #52 head, merge it into `develop`, then begin Pass 11 Closure from the exact integration commit. Closure must independently validate the merged tree, register closure evidence, promote the completed baseline to `main`, validate exact mainline integration, synchronize `develop` and `main` with ahead `0` and behind `0`, and define Pass 12 only after synchronization.
