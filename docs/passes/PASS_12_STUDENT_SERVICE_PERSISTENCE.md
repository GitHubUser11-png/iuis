# Pass 12 — Remaining Student-Service Specialized Persistence, Confidential Record Segregation, Controlled Lifecycle Orchestration, and Released Service Projections

## Starting point

- final synchronized Pass 11 commit: `b546152ab6d284b6d142311ebf941f94484aa1b3`;
- `main` and `develop`: identical, ahead `0`, behind `0`;
- implementation branch: `build/pass-12-student-service-persistence`;
- exactly seven projects;
- exactly 49 authoritative production JSON repositories;
- eleven specialized-complete aggregate adapters;
- seven explicitly deferred fail-closed adapters.

## Objective

Complete specialized persistence and controlled Application vertical slices for the remaining Student-service domains without weakening confidentiality, ownership, executable separation, repository concurrency, or transaction boundaries.

The seven target aggregates are:

1. `LibraryBook`;
2. `LibraryBorrowing`;
3. `CounselingCase`;
4. `DisciplineCase`;
5. `ClinicAppointment`;
6. `MedicalRecord`;
7. `MedicalClearance`.

## Locked architecture

- C# 7.3 and .NET Framework 4.8;
- Domain remains independent of Infrastructure, JSON, filesystem APIs, and WinForms;
- Application depends on Domain contracts only;
- Infrastructure owns JSON records, specialized mappers, repository adapters, and composition registration;
- Forms and every other UI-layer C# file remain free of direct JSON and filesystem dependencies;
- `IUIS.UserApp.exe` and `IUIS.AdminApp.exe` remain separate;
- Administrator status does not bypass confidentiality restrictions;
- Student ownership is derived from the authenticated session;
- repository revisions and aggregate entity versions are enforced before writes;
- related repository mutations use the Pass 8 journaled transaction coordinator;
- an adapter remains fail-closed until its schema, rehydration factory, mapper, restart round trip, and migration behavior pass production tests.

## Confidentiality model

Canonical persistence may retain authoritative restricted state, but released Application projections must use separate shapes and explicit allowlists.

- Counseling confidential sessions are distinct from released counseling summaries.
- Discipline evidence, findings, internal rationale, and incident narrative are distinct from released notices and released decisions.
- Medical consultations, clinical notes, assessments, and treatment plans are distinct from released medical summaries.
- Medical Clearance history is retained as restricted state; released status, validity, number, and approved summary are projected separately.
- Credential, bearer-token, password, digest, and Security Stamp material is not part of any Student-service aggregate record.

## Construction units

### Unit 1 — Canonical schema and segregation contracts

Define explicit canonical record schema shapes for all seven aggregates and their nested records. Lock confidential/released separation through automated tests. Keep all seven adapters deferred and fail-closed.

### Unit 2 — Library reconstruction and persistence

Add invariant-valid `LibraryBook` and `LibraryBorrowing` rehydration factories, specialized mappers, adapter activation, composition registration, restart round trips, inventory integrity, circulation concurrency, and journaled issue/return/lost coordination.

### Unit 3 — Clinic appointment and medical reconstruction

Add `ClinicAppointment`, `MedicalRecord`, and `MedicalClearance` rehydration, specialized mappers, adapter activation, restricted/released medical projections, session-aware appointment and clearance workflows, and confidentiality tests.

### Unit 4 — Counseling and discipline reconstruction

Add `CounselingCase` and `DisciplineCase` rehydration, specialized mappers, adapter activation, counselor/discipline permission boundaries, released-only Student projections, response ownership, and restricted administrator tests.

### Unit 5 — Integrated orchestration and closure preparation

Exercise journaled multi-repository commands, stale repository and entity revisions, exception atomicity, restart recovery, all-seven schema migration behavior, exact 18-of-18 mapper readiness, Windows Release compilation, complete MSTest/TRX evidence, documentation reconciliation, and Figma persistence-to-Application-to-UI evidence.

## Unit 1 implementation record

`StudentServicePersistedRecords.cs` defines explicit canonical persisted shapes for:

- Library Book and embedded copies;
- Library Borrowing lifecycle metadata;
- Counseling confidential sessions and released summaries;
- Discipline restricted evidence, findings, rationale, released notice, and released decision;
- Clinic Appointment workflow metadata;
- Medical confidential consultations and released summaries;
- Medical Clearance restricted history and released current state.

Unit 1 tests require:

- exactly seven top-level aggregate record types derived from `PersistedEntityRecord`;
- parameterless serialization construction;
- non-null empty collection initialization;
- separate confidential and released record types;
- absence of credential and bearer material;
- canonical camel-case JSON names for restricted/released collections;
- all seven repository adapters remaining `DeferredWithExplicitReason` until specialized mappers pass.

## Current evidence boundary

Unit 1 schema contracts and tests are committed on the Pass 12 branch. They are not yet described as compiled or tested until the authoritative Windows workflow produces machine-generated evidence.

No Pass 12 adapter has been activated. No production Application command, query, or UI has been added. Pass 12 is not merged, closure-validated, promoted, synchronized, deployed, or release-certified.

## Exact next gate

Open a draft pull request to `develop`, validate the exact Unit 1 head through the authoritative Windows workflow, record Release/MSBuild/MSTest/TRX/artifact evidence, and proceed to Library reconstruction only after the schema boundary passes.