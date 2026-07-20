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
| Pass 9 implementation | Validated and merged | PR #35; integration commit `2b7b629889523a00d54d8e699f705e1ecc4f8358` |
| Pass 9 closure | Validated and merged | PR #38; closure merge `065018e8b643667f29eb4b6dd00af2d67e56dd8f` |
| Pass 9 promotion | Validated and merged | PR #39; exact validated mainline code commit `559811d39f37a5fb4c6be62e71e87f3c366749cf` |
| Exact Pass 9 mainline validation | Successful | PR #40; run `29717728053`; artifact `iuis-windows-build-evidence-119`; ID `8451202733`; SHA-256 `ee1c098bf11457e46e362c0196eb1d8aff0271f75e1e8408afbbf87eff8776aa` |
| Closure-record finalization | Validated and merged | PR #41; merge `7ec8560d04bbb73725ac6df6124cc067cf6df632`; run `29717936028`; artifact `iuis-windows-build-evidence-121`; SHA-256 `e211ac1522d6bba1fd262ff0f83956e8e189f25d49cc5fdda6769819132289e4` |
| Final branch synchronization | Verified after closure-record finalization | ahead `0`; behind `0` |
| In-lock transaction revision gate | Completed and validated | expected revisions rechecked after canonical locks and before journal preparation |
| Aggregate mapper readiness catalog | Completed and validated | 18 adapters classified; no false production-ready mapper claims |
| Pass 9 test count | 127 passed | includes deterministic stale-stage concurrency and mapper-readiness assertions |
| Production repository catalog and templates | Preserved and validated | exactly 49 descriptors and 49 JSON files |
| Pass 9 Figma model | Completed | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Executable certification | Not achieved | final release gate only |

## Pass 9 evidence summary

| Stage | Run | Artifact | SHA-256 |
|---|---:|---|---|
| Implementation | `29715030204` | evidence-105 | `9fe43133d8c3c3bdb30ba5d51367233d3c474e13b3a4219d95a2e0364d9da318` |
| Final implementation head | `29715210764` | evidence-107 | `64525aa1f3c7b91362dbb91a1a4fb872248cbe95376663cc02ef0fa1f79f1a7c` |
| Independent closure | `29717369189` | evidence-114 | `110e821dece6f998fabedfffda21df7d558fa309decf55c31addea05fccef721` |
| Final closure head | `29717509492` | evidence-115 | `7949403fb57ecabe5dbc8cc6e1904fa4c17d031ea6fe6759cd94b43501f9a762` |
| Promotion | `29717634787` | evidence-117 | `c84d402c8bcba0b01ca65219f282f8c8140795bf0b3113439f120cca455d887f` |
| Exact mainline | `29717728053` | evidence-119 | `ee1c098bf11457e46e362c0196eb1d8aff0271f75e1e8408afbbf87eff8776aa` |
| Closure-record finalization | `29717936028` | evidence-121 | `e211ac1522d6bba1fd262ff0f83956e8e189f25d49cc5fdda6769819132289e4` |

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration, transaction revision hardening, mapper readiness | Completed, validated, closure-validated, promoted, exact-mainline validated, synchronized |
| 10 | Canonical persisted schemas, specialized mappers, typed activation, composition wiring, Student/Employee vertical slices | Exact next construction boundary |
| 11+ | Remaining modules, production UI, operations, deployment, certification | Not started |

## Current truthful completion statement

Passes 0 through 9 are integrated in the authoritative repository history. Pass 9 establishes role/profile/grant/restriction authorization, session-aware execution, session-derived Student ownership, restricted DTO separation, typed repository seams, in-lock expected revision revalidation, and an explicit readiness classification for all 18 production aggregate adapters. The exact mainline code tree passed with zero warnings, zero errors, and 127 of 127 tests. Closure-record finalization was separately validated, and `main` and `develop` are synchronized with ahead `0` and behind `0`.

Specialized JSON mappers for production Domain aggregates, operational activation of most typed adapters, production Forms, trusted-device/network enforcement, backup/restore execution, deployment, and release certification remain incomplete.

## Exact next starting point

# Integrated University Information System Build Execution — Pass 10: Canonical Persisted Record Schemas, Specialized Aggregate Mappers, Typed Repository Activation, Composition-Root Registration, Student and Employee Read-Model Vertical Slices, Controlled Application Writes, Mapper Compatibility and Repository Migration Tests, Windows Release Compilation, Figma Persistence-to-UI Wiring Model, and Pull Request Integration

Pass 10 objectives:

1. Define canonical versioned persisted-record schemas for the first production activation set: StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule.
2. Implement specialized mappers that reconstruct value objects, lifecycle state, archive metadata, timestamps, actor IDs, and entity versions without reflection-based private-state mutation or replaying business transitions.
3. Activate only adapters whose specialized mappers pass round-trip and invariant tests; all others remain fail-closed and explicitly deferred.
4. Register activated repositories and authorization/query services through the Infrastructure composition root without introducing Application-to-Infrastructure or Forms-to-JSON dependencies.
5. Connect real JSON-backed Student own-profile and Employee self-service read models through session-aware Application services.
6. Add controlled Student and Employee contact-update use cases with authorization, expected revisions, transaction coordination, and audit-ready mutation metadata.
7. Add schema compatibility, empty-template, restart round-trip, stable-ID, version, archive, value-object, stale-revision, migration, and confidentiality projection tests.
8. Produce Windows Release and MSTest evidence and extend the Figma persistence-to-Application-to-UI wiring model.
