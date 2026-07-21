# IUIS Implementation State

Last updated: 2026-07-21

## Status vocabulary

The project distinguishes Specified, Created, Committed, Verified in GitHub, Compiled, Tested, Merged, Closure-validated, Promoted, Synchronized, and Release-certified states. These states are not interchangeable.

## Authoritative baseline

| Item | State | Evidence |
|---|---|---|
| Private GitHub repository | Created | `GitHubUser11-png/iuis` |
| Passes 0–7 | Completed, compiled, tested, merged, closure-validated, and synchronized | PRs #1–#26; Pass 7 final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 final baseline | Completed and synchronized | `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f` |
| Pass 9 implementation and closure | Completed, promoted, exact-mainline validated, and synchronized | PRs #35, #38–#42; final synchronized commit `81c4b78acb149cef5a9feef6f8c71ae8b9b3037e` |
| Pass 10 implementation and closure | Completed, promoted, exact-final-mainline validated, and synchronized | PRs #43–#49; final synchronized commit `686aa29a3c9827975d711c6713838f5cc2d22918`; 148 tests passed |
| Pass 11 prerequisite checkpoint | Integrated into `develop` before full pass completion | PR #51; integration commit `5d17748a550c79ac11b341b73acc1256d3b6d144` |
| Pass 11 implementation continuation | Completed and integrated | PR #52; exact head `fded5a9ac1f00176a9aeaae4c70a3f972c26ec84`; integration commit `69e69bb621cea80f135f073b79739f4833054eb0`; 160 tests passed |
| Pass 11 premature promotion | Merged to `main` before independent closure evidence | PR #53; promoted commit `925696ca6dd75fc8be513e818835cde0ad85c812` |
| Corrective Unit 1 | Independently validated and merged | PR #55; merge commit `849396b3a00158b154cd9086cf0229cafd0868de`; run `29792336074`; 167 tests passed |
| Corrective Unit 2 and reconciliation | Independently validated and merged | PR #56; documentation head `ac7adb00c106c4752b12bfaa3e6df5070b39e5c0`; merge commit `60536382383d9300000c9d042ad01ab6083a21e1`; run `29795001964`; 172 tests passed |
| Exact PR #56 merged-main validation | Successful | validation-only PR #57; exact head `60536382383d9300000c9d042ad01ab6083a21e1`; run `29795300068`; artifact `iuis-windows-build-evidence-209` |
| Final evidence registration | In progress | branch `build/pass-11-final-evidence-registration` created from exact validated merged-main commit `60536382383d9300000c9d042ad01ab6083a21e1` |
| Production repository catalog and templates | Preserved and validated | exactly 49 descriptors and exactly 49 canonical JSON files |
| Canonical repository envelopes | Implemented and validated | exact six-field canonical contract across runtime, validator, bootstrap outputs, and 49 templates |
| Session-token digest security | Implemented and validated | raw token returned once; SHA-256 digest persisted; fixed-time verification; legacy sessions rejected and revoked |
| Pass 11 specialized aggregate schemas | Corrected and validated | exact record schema version `1` for Enrollment, Tuition Assessment, Payment, Financial Adjustment, and Scholarship Award |
| Aggregate mapper readiness | Validated | 11 specialized-complete; 7 fail-closed with explicit reasons |
| Student enrollment and finance reads | Corrected and validated | session-derived ownership and released-state allowlists |
| Controlled Enrollment and Finance writes | Corrected and validated | entity/repository concurrency, pre-allocation validation, journaled related writes |
| Migration rollback and recovery | Independently validated | deterministic failure injection; byte-for-byte rollback; journal failure evidence; retry; audit-registration recovery |
| Broader UI dependency boundary | Independently validated | every C# file under SharedUI, UserApp, and AdminApp scanned; 0 prohibited dependency findings |
| Pass 11 Figma model | Updated | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Executable certification | Not achieved | deployment and final release gate only |

## Historical reconciliation

PR #52 completed the Pass 11 implementation and merged it into `develop`. PR #53 promoted `develop` to `main` at `925696ca6dd75fc8be513e818835cde0ad85c812` before an independent closure run existed. Corrective closure does not recast PR #53 as closure-gated.

PR #54 was an abandoned corrective draft. PR #55 merged Corrective Unit 1. PR #56 merged Corrective Unit 2 and pre-promotion documentation reconciliation. Validation-only PR #57 proved the exact PR #56 merge commit without adding a trigger commit.

## Combined corrective delta

The combined corrective delta from promoted commit `925696ca6dd75fc8be513e818835cde0ad85c812` through Corrective Unit 2 code head `eac074c23da378e1cdae9a1abdf76090851b89a9` is exactly ten implementation/test files:

1. `build/Test-IuisSourceTree.ps1`;
2. `src/IUIS.Application/Orchestration/EnrollmentFinanceCommandServices.cs`;
3. `src/IUIS.Application/Orchestration/StudentFinanceQueryService.cs`;
4. `src/IUIS.Infrastructure/Persistence/RepositoryEnvelopeMigrationService.cs`;
5. `src/IUIS.Infrastructure/Persistence/SpecializedAggregateJsonMappers.cs`;
6. `src/IUIS.Infrastructure/Persistence/TransactionCoordinator.cs`;
7. `tests/IUIS.Tests/IUIS.Tests.csproj`;
8. `tests/IUIS.Tests/Pass10CanonicalPersistenceTests.cs`;
9. `tests/IUIS.Tests/Pass11CorrectiveClosureTests.cs`;
10. `tests/IUIS.Tests/Pass11CorrectiveAuditUnit2Tests.cs`.

## Pass 11 evidence

| Stage | Commit | Run | Artifact | ID | SHA-256 |
|---|---|---:|---|---:|---|
| Exact implementation head | `fded5a9ac1f00176a9aeaae4c70a3f972c26ec84` | `29754011862` | `iuis-windows-build-evidence-161` | `8465908242` | `5e44960a8654022cdbc559d1c4b74e7af2566e7bbcb2dfda13c0921df9f05dbd` |
| Corrective Unit 1 head | `517af90bd2218dea471747b2da5c92e49526e3ac` | `29792336074` | `iuis-windows-build-evidence-201` | `8480770463` | `77d60d81918f2df2adbc2f642fe92c5d8b0c9bcc4eb93e82646e20f235db8b2c` |
| Corrective Unit 2 code head | `eac074c23da378e1cdae9a1abdf76090851b89a9` | `29793780496` | `iuis-windows-build-evidence-203` | `8481293446` | `26a2b77fad29ceef6e695f30f7aeef7077b65b01652d152667142e89c63acc8b` |
| Documentation-inclusive PR #56 head | `ac7adb00c106c4752b12bfaa3e6df5070b39e5c0` | `29795001964` | `iuis-windows-build-evidence-207` | `8481720544` | `8e2d0c63396d34693d98e031d6165b24b821fedf6cc4ba78125ebda184da76cb` |
| Exact PR #56 merged-main commit | `60536382383d9300000c9d042ad01ab6083a21e1` | `29795300068` | `iuis-windows-build-evidence-209` | `8481826369` | `288c4cb168b36701533cf9a219dd8622836ed3bd1bc5d21dd70fe8e738984800` |

Run `29795300068` passed all seven project boundaries, exactly 49 canonical templates, exactly six envelope fields, all-UI dependency validation, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero warnings, zero errors, all 172 MSTest cases, TRX verification, and artifact publication. The locally calculated artifact digest matched GitHub’s digest.

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from any UI-layer C# source.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration, transaction revision hardening, mapper readiness | Completed, validated, closure-validated, promoted, exact-mainline validated, synchronized |
| 10 | Canonical persisted schemas, specialized mappers, typed activation, composition wiring, Student/Employee vertical slices | Completed, independently closure-validated, promoted, exact-final-mainline validated, synchronized |
| 11 | Canonical envelopes, token digests, Enrollment/Finance persistence, released finance projections, controlled writes, corrective closure | Corrective code merged and exact merged-main validated; final evidence record integration, exact-final-main validation, and branch synchronization pending |
| 12 | Seven remaining specialized adapters, confidential/released record segregation, controlled Student-service lifecycles | Boundary defined; construction not started or authorized |
| 13+ | Production UI completion, notifications, operations, deployment, certification | Not started |

## Locked Pass 12 boundary

Pass 12 is titled:

**Remaining Student-Service Specialized Persistence, Confidential Record Segregation, Controlled Lifecycle Orchestration, and Released Service Projections.**

It is limited to:

- LibraryBook, LibraryBorrowing, CounselingCase, DisciplineCase, ClinicAppointment, MedicalRecord, and MedicalClearance canonical persisted schemas and specialized mappers;
- activation of the seven currently deferred adapters;
- restricted persisted records and released projections for confidential Student-service domains;
- session-derived ownership and permission enforcement;
- concurrency-aware commands and queries;
- journaled related mutations;
- exhaustive mapper, authorization, confidentiality, rollback, and exception-atomicity tests;
- Windows Release and Figma evidence.

It excludes production-form completion, notification dispatch, operational backup scheduling, deployment packaging, and release certification.

## Current truthful completion statement

Passes 0 through 10 are synchronized. Pass 11 corrective implementation is merged into `main`, and exact merged-main commit `60536382383d9300000c9d042ad01ab6083a21e1` is independently Windows-validated with 172 of 172 tests passed and zero warnings/errors.

Pass 11 final administrative closure still requires validation and merge of this final evidence record, exact validation of the resulting documentation-inclusive `main`, restoration of `develop` to that final commit, and proof of ahead `0`, behind `0`. Pass 12 has a defined boundary but has not started.

## Exact next gate

Validate the final evidence-registration head. If successful, merge it into `main` with an exact-head guard, independently validate the exact resulting final `main` commit, register that run in the merged evidence PR and validation-only PR metadata, restore `develop` to the exact final `main`, and verify zero divergence.
