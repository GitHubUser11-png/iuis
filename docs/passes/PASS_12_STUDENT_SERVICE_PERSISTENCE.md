# Pass 12 — Remaining Student-Service Specialized Persistence, Confidential Record Segregation, Controlled Lifecycle Orchestration, and Released Service Projections

## Starting point

- final synchronized Pass 11 commit: `b546152ab6d284b6d142311ebf941f94484aa1b3`;
- `main` and `develop`: identical at construction start;
- implementation branch: `build/pass-12-student-service-persistence`;
- draft pull request: PR #61;
- exactly seven projects;
- exactly 49 authoritative production JSON repositories;
- eleven specialized-complete adapters and seven fail-closed adapters at Pass 12 construction start.

## Locked architecture

- C# 7.3 and .NET Framework 4.8;
- Domain remains independent of Infrastructure, JSON, filesystem APIs, and WinForms;
- Application depends on Domain contracts only;
- Infrastructure owns persisted records, mappers, adapters, and composition;
- all UI-layer C# files remain free of direct JSON and filesystem dependencies;
- `IUIS.UserApp.exe` and `IUIS.AdminApp.exe` remain separate;
- Administrator status does not bypass confidentiality restrictions;
- Student ownership is derived from the authenticated session;
- repository revisions and entity versions are enforced before writes;
- related writes use the journaled transaction coordinator;
- incomplete adapters remain fail-closed.

## Confidentiality model

Canonical persistence may retain authoritative restricted state, but Student-facing and generally released Application projections use separate DTOs and explicit allowlists.

- Counseling confidential sessions remain separate from released summaries.
- Discipline evidence, findings, internal rationale, and incident narrative remain separate from released notices and decisions.
- Medical consultations, notes, assessments, and treatment plans remain separate from released medical summaries.
- Medical Clearance history remains restricted; released status, number, validity, request reason, and approved summary are projected separately.
- Library projections exclude internal actors and internal Prepared/Cancelled workflow state.
- Credential, bearer-token, password, digest, and Security Stamp material is excluded from Student-service aggregate schemas.

# Unit 1 — Canonical Student-service schemas

Unit 1 defined canonical schema version 1 persisted records for:

1. LibraryBook;
2. LibraryBorrowing;
3. CounselingCase;
4. DisciplineCase;
5. ClinicAppointment;
6. MedicalRecord;
7. MedicalClearance.

It established parameterless serialization construction, initialized collections, camel-case field contracts, confidential/restricted versus released segregation, and fail-closed adapter status.

Draft PR #60 was closed without merge after run `29797409211` exposed two MSTest generic-inference errors. Replacement draft PR #61 corrected those assertions.

Final Unit 1 evidence:

- exact head: `bd2db780cb89dd0b6419cd08c8cb36d236a4cb11`;
- run: `29798083094` / run number `217`;
- tests: `178 / 178` passed;
- warnings/errors: `0 / 0`;
- artifact: `iuis-windows-build-evidence-217`;
- artifact ID: `8482766300`;
- SHA-256: `c85a75c30e50159eed0ed47be26d55811e8fe8b8ed55057e8e2776a7b4ac0c11`.

# Unit 2 — Library reconstruction and persistence

## Domain and persistence

`LibraryBookCopy.Rehydrate`, `LibraryBook.Rehydrate`, and `LibraryBorrowing.Rehydrate` reconstruct invariant-valid inventory and circulation state without reflection or transition replay.

`LibraryBookJsonMapper` and `LibraryBorrowingJsonMapper` enforce record schema version 1, explicit enum parsing, institution-local dates, archive state, metadata, and restart-safe round trips.

Activated adapters:

- `LibraryBookRepositoryAdapter` -> `books`;
- `LibraryBorrowingRepositoryAdapter` -> `borrowings`.

## Application and composition

`IuisCompositionRoot` registers `LibraryBooks`, `LibraryBorrowings`, `StudentLibraryCirculation`, and `LibraryCirculation`.

The Student circulation projection derives Student identity from the session and exposes released circulation state without internal actor identifiers. Librarian Issue, Return, and Mark Lost commands enforce permissions, repository revisions, entity versions, relationships, lifecycle state, and journaled two-repository mutation.

## Unit 2 failed-run history

Initial Unit 2 head `e4f476824c3d0db84cb54d43a5930be4fd66c78f` ran as `29800360307` / run number `235`.

- production compilation: passed with zero warnings/errors;
- all thirteen new Library tests: passed;
- suite: `187 / 190` because three older tests retained the pre-Unit-2 readiness state.

Failed artifact:

- `iuis-windows-build-evidence-235`;
- ID `8483550446`;
- SHA-256 `f0e7eca785ee9dec64621b28476755c1092461a4e6cc1bb0e505dbcec937ac38`.

The stale compatibility assertions were reconciled, temporary maintenance workflows were removed, and the authoritative Windows workflow was restored.

## Final Unit 2 evidence

- implementation/documentation head: `ca2288c597debd6c2de0657103d7141ca60853d5`;
- run: `29802968984` / run number `244`;
- final evidence-registration head: `96e0ddb5a6c0dea970c6c3138520bde2c7ed901a`;
- final run: `29803227374` / run number `245`;
- project boundaries: `7 / 7`;
- production templates: `49 / 49`;
- canonical envelope fields: `6`;
- UI C# files scanned: `8`, prohibited findings `0`;
- Release warnings/errors: `0 / 0`;
- tests: `190 / 190` passed;
- artifact: `iuis-windows-build-evidence-245`;
- artifact ID: `8484538149`;
- SHA-256: `8c5c63a9c94f6a876df05eb1b0b83e92e03a64683ce2fc12496c8bef24041ab7`.

# Unit 3 — Clinic Appointment and Medical persistence

## Domain rehydration

Unit 3 adds explicit persisted-state reconstruction for:

- `ClinicAppointment`;
- `MedicalRecord`;
- `MedicalClearance`;
- nested Medical consultations;
- released Medical summaries;
- Medical Clearance history entries.

Rehydration validates stable identifiers, defined lifecycle states, required timestamps and actors, chronology, version and archive metadata, Appointment schedule/check-in/completion consistency, retained Medical Record rules, released-summary Consultation references, Clearance validity, numbering, review state, and chronological continuous history.

No reflection-based private-state mutation or business-transition replay is used.

## Confidential and released segregation

`MedicalRecord` reconstructs confidential consultations and released summaries as separate collections. Released Student DTOs expose only released summary text and timestamp, Appointment released fields, and released Clearance fields.

Released DTOs exclude:

- internal clinical notes;
- internal assessments;
- treatment plans;
- restricted Clearance history;
- clinician and administrative actor identifiers;
- credential and session material.

`RestrictedMedicalRecordQueryService` requires an Employee or Administrator role, the correct application kind, explicit `clinic.medical.restricted.read`, and explicit `confidentiality.restricted`. Administrator status alone is insufficient.

## Specialized persistence and readiness

Specialized mappers:

- `ClinicAppointmentJsonMapper`;
- `MedicalRecordJsonMapper`;
- `MedicalClearanceJsonMapper`.

Activated adapters:

- `ClinicAppointmentRepositoryAdapter` -> `appointments`;
- `MedicalRecordRepositoryAdapter` -> `medical_records`;
- `MedicalClearanceRepositoryAdapter` -> `clearances`.

Mapper readiness after Unit 3:

- `16` `SpecializedMapperCompleted`;
- `2` `DeferredWithExplicitReason`;
- `0` generic-compatible placeholders;
- `0` ambiguous requirements.

The remaining fail-closed adapters are:

- `CounselingCaseRepositoryAdapter`;
- `DisciplineCaseRepositoryAdapter`.

## Composition and Application services

`IuisCompositionRoot` registers:

- `ClinicAppointments`;
- `MedicalRecords`;
- `MedicalClearances`;
- `StudentMedicalServices`;
- `RestrictedMedicalRecords`;
- `ClinicAppointmentCommands`;
- `MedicalRecordCommands`;
- `MedicalClearanceCommands`.

Session-aware services implement:

- Student Appointment request;
- Clinic schedule, confirm, check-in, no-show, and completion;
- confidential Consultation recording;
- released Medical summary publication;
- Student Medical Clearance request;
- Clearance review, issue, denial, and revocation;
- released Student medical overview;
- explicitly authorized restricted Medical Record access.

Commands validate session purpose, application kind, role, explicit permissions, confidentiality classification, session-derived ownership, repository revisions, entity versions, relationships, and lifecycle state before mutation.

Appointment completion plus confidential Consultation creation is staged across `appointments` and `medical_records` through the journaled transaction coordinator. Deterministic failure testing proves byte-for-byte rollback and restart-safe recovery.

## Unit 3 tests

Thirteen focused Unit 3 tests cover:

1. Clinic Appointment mapper round trip;
2. Medical Record confidential/released round trip;
3. Medical Clearance history and validity round trip;
4. unsupported schema and contradictory lifecycle rejection;
5. exact readiness `16 / 2`;
6. composition-root restart round trips;
7. session-derived Student projection and confidential-field exclusion;
8. Administrator denial without explicit restricted permission;
9. authorized restricted Medical access;
10. stale repository and entity token rejection before transaction;
11. journaled Appointment and Medical Record coordination;
12. cross-Student Clearance denial;
13. Clearance request/review/issue lifecycle;
14. deterministic related-write rollback and restart verification.

The list includes one combined test that proves both Administrator denial and authorized access, so the physical Unit 3 test count is thirteen.

## Unit 3 failed-run and reconciliation history

Initial full Unit 3 test head `688bd15a339681fd7111e7c8b3bde5db925ae21e` ran as GitHub Actions run `29810751435` / run number `258`.

- source-tree validation: passed;
- NuGet restoration: passed;
- Release compilation: passed with zero warnings and zero errors;
- all thirteen new Unit 3 tests: passed;
- suite result: `197 / 203` because six earlier readiness assertions still expected Unit 2’s `13 completed / 5 deferred` state.

Failed artifact:

- `iuis-windows-build-evidence-258`;
- artifact ID: `8487361768`;
- SHA-256: `6dee3fb1ae7b866c19eaa26655ef7899488760538c30ebc8d6897503285d9d9c`.

Only the six stale compatibility assertions were reconciled. No Unit 3 production behavior was changed during this test-compatibility pass.

## Unit 3 implementation-head evidence

Exact implementation head `619e6596e13e68ec478bccd814c554ac29756ec2` passed:

- GitHub Actions run: `29812684868`;
- run number: `264`;
- source-tree and architecture validation: passed;
- project boundaries: exactly `7 / 7`;
- production templates: exactly `49 / 49`;
- canonical repository-envelope fields: exactly `6`;
- UI C# files scanned: `8`;
- prohibited UI dependency findings: `0`;
- NuGet restoration: passed;
- .NET Framework 4.8 Release compilation: passed;
- compiler warnings: `0`;
- compiler errors: `0`;
- MSTest: `203 / 203` passed;
- TRX production and verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-264`;
- artifact ID: `8488124999`;
- SHA-256: `85f9bf3f8717a25674df0fddbb8b2e9dd9c354672b045cb2c89bf0bc735f167d`;
- expiration: `2026-08-04T08:04:34Z`.

The independently calculated ZIP digest matches the GitHub artifact digest.

# Current evidence boundary

Units 1, 2, and the Unit 3 implementation head are machine-validated. PR #61 remains draft and unmerged. Pass 12 is not closure-validated, promoted, synchronized, deployed, or release-certified.

This evidence-registration commit must receive a separate successful Windows run before Unit 3 is described as independently validated.

# Exact next gate

Validate the documentation-inclusive Unit 3 evidence head. After that exact head succeeds, extend the existing FigJam board and proceed only to:

**Pass 12 — Construction Unit 4: CounselingCase and DisciplineCase Invariant-Preserving Rehydration, Confidential Session and Restricted Investigation Segregation, Specialized Mappers, Final Adapter Activation, Session-Aware Counseling and Discipline Orchestration, Released Student Projections, Journaled Related Mutations, and Windows Validation.**
