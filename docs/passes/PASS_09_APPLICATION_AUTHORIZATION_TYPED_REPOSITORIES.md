# Pass 9 — Application Authorization, Typed Repository Adapters, and Restricted Projections

## Objective

Build the first production Application-layer authorization and typed repository boundary on top of the validated Pass 8 persistence foundation. Pass 9 enforces role, permission, session, ownership, and confidentiality rules before commands or queries reach repositories or Domain aggregates.

## Final integration baseline

- synchronized Pass 8 baseline: `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f`
- implementation pull request: `#35`
- implementation integration commit: `2b7b629889523a00d54d8e699f705e1ecc4f8358`
- closure pull request: `#38`
- closure merge commit: `065018e8b643667f29eb4b6dd00af2d67e56dd8f`
- promotion pull request: `#39`
- exact-mainline evidence pull request: `#40`
- final synchronized commit: `559811d39f37a5fb4c6be62e71e87f3c366749cf`

## Authorization model

Pass 9 resolves access from Primary Role, required application kind, session purpose, active profile permissions, direct grants, direct restrictions, ownership, confidentiality, and explicit role compatibility. Effective permissions are active profile permissions union direct grants, subject to direct restrictions. Restrictions win. Administrator status is not a universal confidentiality bypass: Restricted data requires `confidentiality.restricted`, and Highly Restricted data requires `confidentiality.high`, in addition to the operation permission.

## Session-aware execution

`SessionAwareRequestExecutor` loads the authoritative principal for every command or query through `IAuthorizationPrincipalProvider`, resolves the request, and invokes the handler only after authorization succeeds. `JsonAuthorizationPrincipalProvider` reads `users.json`, `sessions.json`, and `permission_profiles.json` under deterministic locks and validates account/session status, token, expiration, Security Stamp, linkage, application kind, session purpose, role compatibility, and assigned-profile existence.

## Ownership and confidentiality

`StudentOwnRecordQueryService` does not accept a Student ID. The Student ID is derived from the validated session principal. Employee queries require explicit permission and compatible roles. Counseling, Discipline, and Medical queries use separate released and internal DTO types; released DTOs contain no internal notes, evidence, findings, rationale, clinical assessment, or treatment data.

## Typed repository boundary

Application defines revision-aware repository contracts for Student, Employee, Academic, Finance, Library, Counseling, Discipline, and Clinic aggregates. Infrastructure provides `MappedJsonRepository<T>`, mapper contracts, authoritative-name adapters, stable-ID lookup, optimistic expected-revision writes, and `JournaledApplicationTransactionCoordinator` for related writes. The mapper boundary keeps JSON out of Application and supports aggregate-specific hydration.

## In-lock revision hardening

Application-staged transaction mutations carry expected revision metadata. `JournaledTransactionCoordinator` reopens and validates every revision-checked authoritative repository after all canonical transaction locks have been acquired and before backups, journal preparation, or replacement. The staged envelope name and next revision are checked under the same locks. A concurrent committed revision causes a controlled conflict before any transaction target is replaced.

The existing 127-test suite remains at 127 tests. Its journaled Application transaction test includes a deterministic concurrency scenario in which the staged mapper pauses after the pre-lock read, another writer commits revision 2, and the stale revision-1 transaction is rejected without overwriting the concurrent value.

## Aggregate mapper readiness classification

`AggregateMapperReadinessCatalog` classifies all 18 production aggregate adapters. StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule adapters require specialized mappers. Enrollment, TuitionAssessment, Payment, FinancialAdjustment, ScholarshipAward, LibraryBook, LibraryBorrowing, CounselingCase, DisciplineCase, ClinicAppointment, MedicalRecord, and MedicalClearance adapters are deferred with explicit reasons tied to embedded snapshots, collections, confidential records, immutable history, or workflow state.

No production Domain adapter is claimed as generic-mapper compatible or specialized-mapper complete in Pass 9.

## Final validation evidence

| Stage | Run | Artifact | ID | SHA-256 |
|---|---:|---|---:|---|
| Implementation head | `29715030204` | `iuis-windows-build-evidence-105` | `8450258190` | `9fe43133d8c3c3bdb30ba5d51367233d3c474e13b3a4219d95a2e0364d9da318` |
| Final implementation PR head | `29715210764` | `iuis-windows-build-evidence-107` | `8450333060` | `64525aa1f3c7b91362dbb91a1a4fb872248cbe95376663cc02ef0fa1f79f1a7c` |
| Independent closure head | `29717369189` | `iuis-windows-build-evidence-114` | `8451078816` | `110e821dece6f998fabedfffda21df7d558fa309decf55c31addea05fccef721` |
| Final closure PR head | `29717509492` | `iuis-windows-build-evidence-115` | `8451127726` | `7949403fb57ecabe5dbc8cc6e1904fa4c17d031ea6fe6759cd94b43501f9a762` |
| Mainline promotion | `29717634787` | `iuis-windows-build-evidence-117` | `8451167506` | `c84d402c8bcba0b01ca65219f282f8c8140795bf0b3113439f120cca455d887f` |
| Exact mainline tree | `29717728053` | `iuis-windows-build-evidence-119` | `8451202733` | `ee1c098bf11457e46e362c0196eb1d8aff0271f75e1e8408afbbf87eff8776aa` |

Every stage passed exact 49-template validation, all seven project boundaries, Release compilation with zero warnings and zero errors, all 127 tests, TRX verification, and artifact publication.

## Figma Application-service and closure model

`https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

## Evidence boundary

The generic mapper and aggregate-specific repository-name adapters establish the typed persistence seam, but complete specialized mappers for every evolved Domain aggregate shape remain a later construction requirement. Production Forms, trusted-device/network enforcement, notification dispatch, operational backup/restore execution, deployment packaging, and release certification also remain deferred.

## Final status

Pass 9 is implemented, closure-hardened, independently validated, promoted, exact-mainline validated, and synchronized. `main` and `develop` are identical at `559811d39f37a5fb4c6be62e71e87f3c366749cf`.

## Exact next construction boundary

# Integrated University Information System Build Execution — Pass 10: Canonical Persisted Record Schemas, Specialized Aggregate Mappers, Typed Repository Activation, Composition-Root Registration, Student and Employee Read-Model Vertical Slices, Controlled Application Writes, Mapper Compatibility and Repository Migration Tests, Windows Release Compilation, Figma Persistence-to-UI Wiring Model, and Pull Request Integration
