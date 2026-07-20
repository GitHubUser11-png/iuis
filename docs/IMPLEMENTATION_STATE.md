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
| Pass 10 branch and PR | Created; final-head validation and merge pending | `build/pass-10-canonical-schemas-specialized-mappers`; PR #43 |
| Pass 10 canonical schemas | Created and implementation-validated | Student, Employee, Course, Subject, Academic Period, and Assessment Charge Rule record schema v1 |
| Pass 10 specialized mappers | Created and implementation-validated | six completed and activated; twelve fail-closed with explicit deferral reasons |
| Pass 10 composition and vertical slices | Created and implementation-validated | composition root, real Student own-profile and Employee self-service reads, controlled contact updates |
| Pass 10 implementation validation | Successful | run `29723680915`; head `0c0f660471f3bc1606bf0a50a0a45f8548fefd9c`; 0 warnings; 0 errors; 142 tests passed |
| Pass 10 artifact | Verified | `iuis-windows-build-evidence-128`; ID `8453381929`; SHA-256 `fccb5e7b2c236a7827e13965b296b87a440f089d397734e6088bea5062047751` |
| Production repository catalog and templates | Preserved and validated | exactly 49 descriptors and 49 JSON files |
| Pass 10 Figma model | Completed | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Executable certification | Not achieved | final release gate only |

## Pass 10 implementation evidence

| Stage | Run | Artifact | ID | SHA-256 |
|---|---:|---|---:|---|
| Validated implementation head | `29723680915` | `iuis-windows-build-evidence-128` | `8453381929` | `fccb5e7b2c236a7827e13965b296b87a440f089d397734e6088bea5062047751` |

The successful implementation run passed source-tree and architecture validation, exact 49-template validation, seven-project boundary validation, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero warnings, zero errors, all 142 MSTest cases, TRX verification, and artifact publication.

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration, transaction revision hardening, mapper readiness | Completed, validated, closure-validated, promoted, exact-mainline validated, synchronized |
| 10 | Canonical persisted schemas, specialized mappers, typed activation, composition wiring, Student/Employee vertical slices | Source created; implementation validation successful; final PR-head validation and merge pending |
| 11+ | Remaining specialized mappers, production UI, operations, deployment, certification | Not started |

## Current truthful completion statement

Passes 0 through 9 are synchronized. Pass 10 now defines canonical record schema v1 and validated specialized mappers for StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule. The six adapters are activated through the Infrastructure composition root. Real JSON-backed Student own-profile and Employee self-service reads are session-derived, and controlled contact updates use authorization, repository and entity concurrency tokens, journaled transactions, and audit-ready mutation metadata. The implementation head compiled with zero warnings and zero errors, and 142 of 142 tests passed.

Pass 10 is not yet merged or closure-complete. The remaining twelve aggregate adapters, production Forms, trusted-device/network enforcement, operational backup/restore execution, deployment, and release certification remain incomplete.

## Exact next gate

Validate the evidence-updated final PR #43 head, merge it into `develop`, independently closure-validate the integrated Pass 10 tree, harden any defects found in integrated validation, promote the completed baseline to `main`, synchronize branches, and define the Pass 11 construction boundary.
