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
| Pass 10 early implementation-tree promotion | Historical fact | PR #44; merge commit `1405b507e9e8e0d7b24031c09cedeef500c5ac2f`; occurred before formal closure |
| Pass 10 independent closure | Validated and merged | PR #45; final closure head `839ea9dcc960123878d1f20e73531781e4550274`; merge commit `ec4ebba5e0a76706319cdbd94a5cd51b7f7dc13b` |
| Pass 10 formal closure promotion | Validated and merged | PR #46; exact promoted mainline commit `b3f22c5641de6842e0696268d0e4930ae034e274` |
| Exact promoted-mainline validation | Successful | PR #47; run `29744523930`; artifact `iuis-windows-build-evidence-142`; ID `8461860597`; SHA-256 `6ff00c7c9883b913198aa02a4095968e7b08c4c68043cdd4a7f7cbf49ff94ae7` |
| Pass 10 closure hardening | Completed and validated | exception-atomic `AssessmentChargeRule` mutations and six independent integrated-tree tests |
| Pass 10 canonical schemas | Closure-validated | Student, Employee, Course, Subject, Academic Period, and Assessment Charge Rule record schema v1 |
| Pass 10 specialized mappers | Closure-validated | six completed and activated; twelve fail-closed with explicit deferral reasons |
| Pass 10 composition and vertical slices | Closure-reverified | composition root, real Student own-profile and Employee self-service reads, controlled contact updates |
| Pass 10 closure test count | Successful | 148 executed, 148 passed, 0 failed |
| Production repository catalog and templates | Preserved and validated | exactly 49 descriptors and 49 JSON files |
| Pass 10 Figma model | Completed | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Canonical repository-envelope migration | Locked first Pass 11 requirement | current legacy envelope uses `repository` and `createdAtUtc`; authoritative contract requires `repositoryName` and no `createdAtUtc` field |
| Session-token digest correction | Locked Pass 11 security requirement | current bearer value is persisted under `TokenHash`; Pass 11 must store only a cryptographic digest and reject or migrate legacy sessions |
| Executable certification | Not achieved | final release gate only |

## Pass 10 evidence summary

| Stage | Run | Artifact | ID | SHA-256 |
|---|---:|---|---:|---|
| Validated implementation head | `29723680915` | `iuis-windows-build-evidence-128` | `8453381929` | `fccb5e7b2c236a7827e13965b296b87a440f089d397734e6088bea5062047751` |
| Final implementation PR head | `29724102942` | `iuis-windows-build-evidence-131` | `8453549131` | `d8ef889101dfa28b6085b6bed934b1cfc8bb98cf64868502302a4dadf6b49932` |
| Independent closure head | `29743422763` | `iuis-windows-build-evidence-136` | `8461386106` | `66c439b535cd1b7d5fb517531ecfbbd047a84140a20bfa078f6029ad0a3968b9` |
| Final evidence-updated closure head | `29743767951` | `iuis-windows-build-evidence-138` | `8461571257` | `b9ce4c7da2b68a7b89a2e749023f6b70f5558dc0dd8c2bb7f1181aba1a173ddf` |
| Closure-baseline promotion | `29744151700` | `iuis-windows-build-evidence-140` | `8461739187` | `9d42bee1c6f46b493627591ca9f32a112f2b062e85d340beab651a5882e33d73` |
| Exact promoted mainline | `29744523930` | `iuis-windows-build-evidence-142` | `8461860597` | `6ff00c7c9883b913198aa02a4095968e7b08c4c68043cdd4a7f7cbf49ff94ae7` |

Every closure-stage run passed source-tree and architecture validation, exact 49-template validation, seven-project boundary validation, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero warnings, zero errors, all 148 MSTest cases, TRX verification, and artifact publication.

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration, transaction revision hardening, mapper readiness | Completed, validated, closure-validated, promoted, exact-mainline validated, synchronized |
| 10 | Canonical persisted schemas, specialized mappers, typed activation, composition wiring, Student/Employee vertical slices | Completed, independently closure-validated, promoted, and exact-promoted-mainline validated; final closure-record validation and branch synchronization pending |
| 11 | Canonical envelope migration, session-token digest enforcement, Enrollment and Finance specialized persistence and orchestration | Exact next construction boundary after final synchronization |
| 12+ | Remaining specialized mappers, production UI, operations, deployment, certification | Not started |

## Current truthful completion statement

Passes 0 through 9 are synchronized. Pass 10 implementation, independent closure correction, closure test expansion, formal promotion, and exact promoted-mainline validation are complete. The six activated canonical record schemas and specialized mappers have been verified through actual restarted repositories and the real composition-root path. Student and Employee read/update vertical slices remain session-aware, revision-aware, journal-coordinated, and projection-restricted. The exact promoted mainline compiled with zero warnings and zero errors and passed 148 of 148 tests.

The final repository tasks for Pass 10 are validation and merge of this closure-record finalization, exact validation of the resulting documentation-inclusive main commit, and synchronization of `develop` to that final `main` commit with ahead `0` and behind `0`. Pass 10 is not release certification.

The remaining twelve aggregate adapters, authoritative repository-envelope migration, session-token digest correction, production Forms, trusted-device/network enforcement, operational backup/restore execution, deployment, and release certification remain incomplete.

## Exact next construction boundary

# Integrated University Information System Build Execution — Pass 11: Canonical Repository Envelope Contract Migration, Legacy Envelope Compatibility and One-Way Rewrite, Cryptographic Session-Token Digest Enforcement, Production Template and Validator Normalization, Enrollment and Finance Persisted Schemas, Specialized Mappers and Typed Adapter Activation, Student Financial Read Models, Controlled Assessment and Payment Orchestration, Migration and Security Regression Tests, Windows Release Compilation, Figma Envelope-to-Finance Wiring Model, and Pull Request Integration

Pass 11 begins only after final Pass 10 closure-record validation and zero-divergence branch synchronization. Its detailed objectives are recorded in `docs/passes/PASS_10_CLOSURE.md`.
