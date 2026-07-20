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
| Pass 10 implementation | Validated and merged | PR #43; integration commit `149b42c86bc10fa7bef3b71e89083ca0d0ba35cb`; final implementation run `29724102942`; 142 tests passed |
| Pass 10 early mainline merge | Completed before formal closure | PR #44; merge commit `1405b507e9e8e0d7b24031c09cedeef500c5ac2f` |
| Pass 10 closure branch | Created from exact integrated mainline tree | `build/pass-10-closure` from `1405b507e9e8e0d7b24031c09cedeef500c5ac2f` |
| Pass 10 closure hardening | Implemented; validation pending | exception-atomic `AssessmentChargeRule` mutations and six independent closure tests |
| Pass 10 canonical schemas | Implementation-validated; closure audit in progress | Student, Employee, Course, Subject, Academic Period, and Assessment Charge Rule record schema v1 |
| Pass 10 specialized mappers | Implementation-validated; closure audit in progress | six completed and activated; twelve fail-closed with explicit deferral reasons |
| Pass 10 composition and vertical slices | Implementation-validated; closure reverification in progress | composition root, real Student own-profile and Employee self-service reads, controlled contact updates |
| Pass 10 implementation artifact | Verified | `iuis-windows-build-evidence-131`; ID `8453549131`; SHA-256 `d8ef889101dfa28b6085b6bed934b1cfc8bb98cf64868502302a4dadf6b49932` |
| Production repository catalog and templates | Preserved and validated | exactly 49 descriptors and 49 JSON files |
| Pass 10 Figma model | Completed | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Canonical repository-envelope migration | Required in Pass 11 | current legacy envelope still uses `repository` and `createdAtUtc`; authoritative contract requires `repositoryName` and no `createdAtUtc` field |
| Executable certification | Not achieved | final release gate only |

## Pass 10 implementation evidence

| Stage | Run | Artifact | ID | SHA-256 |
|---|---:|---|---:|---|
| Validated implementation head | `29723680915` | `iuis-windows-build-evidence-128` | `8453381929` | `fccb5e7b2c236a7827e13965b296b87a440f089d397734e6088bea5062047751` |
| Final implementation PR head | `29724102942` | `iuis-windows-build-evidence-131` | `8453549131` | `d8ef889101dfa28b6085b6bed934b1cfc8bb98cf64868502302a4dadf6b49932` |

The final implementation run passed source-tree and architecture validation, exact 49-template validation, seven-project boundary validation, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero warnings, zero errors, all 142 MSTest cases, TRX verification, and artifact publication.

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration, transaction revision hardening, mapper readiness | Completed, validated, closure-validated, promoted, exact-mainline validated, synchronized |
| 10 | Canonical persisted schemas, specialized mappers, typed activation, composition wiring, Student/Employee vertical slices | Implementation integrated and present on main; independent closure validation in progress |
| 11+ | Envelope migration, remaining specialized mappers, production UI, operations, deployment, certification | Not started |

## Current truthful completion statement

Passes 0 through 9 are synchronized. Pass 10 implementation is integrated through PR #43 and was promoted to `main` through PR #44 before the formal independent closure pass. The implementation defines canonical record schema v1 and specialized mappers for StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule; activates six repositories through the Infrastructure composition root; and provides real JSON-backed Student and Employee read/update vertical slices. The final implementation head compiled with zero warnings and zero errors, and 142 of 142 tests passed.

Pass 10 closure is now auditing the exact integrated tree, adding six independent restart, archive, schema-version, mapper-readiness, and exception-atomicity checks, and correcting the discovered `AssessmentChargeRule` partial-mutation defect. The expected closure test count is 148. Closure validation, final evidence registration, promotion confirmation, exact-mainline validation, and final branch synchronization remain pending.

The remaining twelve aggregate adapters, authoritative repository-envelope migration, production Forms, trusted-device/network enforcement, operational backup/restore execution, deployment, and release certification remain incomplete.

## Exact next gate

Open and validate the Pass 10 closure PR against reconstructed `develop`, merge the independently validated closure baseline, promote the closure corrections and records to `main`, validate the exact promoted and final documentation-inclusive mainline trees, synchronize `develop` to the final `main` commit with ahead `0` and behind `0`, and then record the exact Pass 11 boundary.
