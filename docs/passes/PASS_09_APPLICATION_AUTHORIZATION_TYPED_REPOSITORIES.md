# Pass 9 — Application Authorization, Typed Repository Adapters, and Restricted Projections

## Objective

Build the first production Application-layer authorization and typed repository boundary on top of the validated Pass 8 persistence foundation. Pass 9 enforces role, permission, session, ownership, and confidentiality rules before commands or queries reach repositories or Domain aggregates.

## Starting point

- synchronized Pass 8 baseline: `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f`
- implementation branch: `build/pass-09-application-authorization-repositories`
- implementation pull request: `#35`
- integration commit: `2b7b629889523a00d54d8e699f705e1ecc4f8358`
- closure branch: `build/pass-09-closure`
- closure pull request: `#38`

## Authorization model

Pass 9 resolves access from Primary Role, required application kind, session purpose, active profile permissions, direct grants, direct restrictions, ownership, confidentiality, and explicit role compatibility. Effective permissions are active profile permissions union direct grants, subject to direct restrictions. Restrictions win. Administrator status is not a universal confidentiality bypass: Restricted data requires `confidentiality.restricted`, and Highly Restricted data requires `confidentiality.high`, in addition to the operation permission.

## Session-aware execution

`SessionAwareRequestExecutor` loads the authoritative principal for every command or query through `IAuthorizationPrincipalProvider`, resolves the request, and invokes the handler only after authorization succeeds. `JsonAuthorizationPrincipalProvider` reads `users.json`, `sessions.json`, and `permission_profiles.json` under deterministic locks and validates account/session status, token, expiration, Security Stamp, linkage, application kind, session purpose, role compatibility, and assigned-profile existence.

## Ownership and confidentiality

`StudentOwnRecordQueryService` does not accept a Student ID. The Student ID is derived from the validated session principal. Employee queries require explicit permission and compatible roles. Counseling, Discipline, and Medical queries use separate released and internal DTO types; released DTOs contain no internal notes, evidence, findings, rationale, clinical assessment, or treatment data.

## Typed repository boundary

Application defines revision-aware repository contracts for Student, Employee, Academic, Finance, Library, Counseling, Discipline, and Clinic aggregates. Infrastructure provides `MappedJsonRepository<T>`, mapper contracts, authoritative-name adapters, stable-ID lookup, optimistic expected-revision writes, and `JournaledApplicationTransactionCoordinator` for related writes. The mapper boundary keeps JSON out of Application and supports aggregate-specific hydration.

## In-lock revision hardening

The closure pass adds expected revision metadata to Application-staged transaction mutations. `JournaledTransactionCoordinator` now reopens and validates every revision-checked authoritative repository after all canonical transaction locks have been acquired and before backups, journal preparation, or replacement. The staged envelope name and next revision are also checked under the same locks. A concurrent committed revision therefore causes a controlled conflict before any transaction target is replaced.

The existing 127-test suite remains at 127 tests. Its journaled Application transaction test now includes a deterministic concurrency scenario in which the staged mapper pauses after the pre-lock read, another writer commits revision 2, and the stale revision-1 transaction is rejected without overwriting the concurrent value.

## Aggregate mapper readiness classification

`AggregateMapperReadinessCatalog` classifies all 18 production aggregate adapters. No production Domain aggregate is represented as generic-mapper compatible or specialized-mapper complete without an explicit hydration implementation.

### Requires specialized mapper

StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule adapters require explicit reconstruction of value objects, private lifecycle state, and entity metadata.

### Deferred with explicit reason

Enrollment, TuitionAssessment, Payment, FinancialAdjustment, ScholarshipAward, LibraryBook, LibraryBorrowing, CounselingCase, DisciplineCase, ClinicAppointment, MedicalRecord, and MedicalClearance adapters contain embedded snapshots, collections, confidential records, immutable financial history, or workflow transitions that require dedicated persisted shapes before operational activation.

### Generic-mapper compatible

None of the production Domain adapters. The generic mapper remains valid for simple public DTO-like records and the test probe aggregate only.

### Specialized mapper completed

None in Pass 9 closure. Completion is deferred rather than falsely represented.

## Test baseline

Seventeen Pass 9 tests cover profile resolution, restriction precedence, Administrator confidentiality, Student ownership and cross-record denial, restricted session purposes, DTO exclusion, session-derived queries, Employee role boundaries, typed repository round trips, optimistic conflicts, journaled transactions, profile loading, expiration, and Security Stamp mismatch. The closure hardens two existing tests without increasing the suite count: mapper readiness is asserted during the typed round-trip test, and stale pre-lock staging is asserted during the Application transaction test.

## Validation evidence

### Implementation head

Run `29715030204` validated implementation head `898e1c54187c1e2ed5feec9f0085c59b525c7efd` with zero warnings, zero errors, and 127 of 127 tests passing. Artifact `iuis-windows-build-evidence-105`, ID `8450258190`, SHA-256 `9fe43133d8c3c3bdb30ba5d51367233d3c474e13b3a4219d95a2e0364d9da318`.

### Final implementation PR head

Run `29715210764` validated final PR #35 head `edfe94f85bac10ce47777f7c8a0d55f8b27d0c68` with zero warnings, zero errors, and 127 of 127 tests passing. Artifact `iuis-windows-build-evidence-107`, ID `8450333060`, SHA-256 `64525aa1f3c7b91362dbb91a1a4fb872248cbe95376663cc02ef0fa1f79f1a7c`.

### Independent closure head

Run `29717369189` validated closure head `85a075d79f86d8596bd950136ab4e2ba9a81b5e2` with exactly 49 templates, all seven project boundaries, zero warnings, zero errors, 127 of 127 tests passing, valid TRX output, and artifact publication. Artifact `iuis-windows-build-evidence-114`, ID `8451078816`, SHA-256 `110e821dece6f998fabedfffda21df7d558fa309decf55c31addea05fccef721`.

## Figma Application-service model

- `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

The editable FigJam model shows session validation, permission resolution, ownership and confidentiality checks, typed repository adapters, journaled transactions, released/internal projection boundaries, in-lock revision validation, mapper readiness, and the closure promotion sequence.

## Evidence boundary

The generic mapper and aggregate-specific repository-name adapters establish the typed persistence seam, but complete specialized mappers for every evolved Domain aggregate shape remain a later construction requirement. Production Forms, trusted-device/network enforcement, notification dispatch, operational backup/restore execution, deployment packaging, and release certification also remain deferred.

## Exact next gate

Validate the evidence-updated final closure PR head, merge it into `develop`, promote the validated baseline to `main`, validate the exact mainline tree, synchronize `develop`, and only then define the Pass 10 construction boundary.
