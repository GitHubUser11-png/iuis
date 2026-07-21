# Pass 12 — Remaining Student-Service Specialized Persistence, Confidential Record Segregation, Controlled Lifecycle Orchestration, and Released Service Projections

## Starting point

- final synchronized Pass 11 commit: `b546152ab6d284b6d142311ebf941f94484aa1b3`;
- `main` and `develop`: identical, ahead `0`, behind `0` at construction start;
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

Unit 1 tests prove:

- exactly seven top-level aggregate record types derived from `PersistedEntityRecord`;
- parameterless serialization construction;
- non-null empty collection initialization;
- separate confidential and released record types;
- absence of credential and bearer material;
- canonical camel-case JSON names for restricted/released collections;
- all seven repository adapters remaining `DeferredWithExplicitReason` until specialized mappers pass.

## Unit 1 GitHub history

Draft PR #60 was closed without merge after run `29797409211` exposed two MSTest compile-time generic-inference errors at head `67edcf083e0caeda7fc63e36d59bb7ab05dad100`. Source validation and restore had passed; the production schema implementation did not produce a compiler error. The failed artifact was `iuis-windows-build-evidence-214`, ID `8482477436`, SHA-256 `62935a54acd845211426c7c84ae130916a21e502d223dd537764a1ceda78b6de`.

The assertions were corrected without changing production behavior. Replacement draft PR #61 validates corrected exact head `f97bb72ce8e9e33a4b9539c96be07a1ad1bed996`.

## Successful Unit 1 Windows evidence

GitHub Actions run `29797763890` (run 216) validated exact corrected head `f97bb72ce8e9e33a4b9539c96be07a1ad1bed996`.

- source-tree and architecture validation: passed;
- project boundaries: exactly `7 / 7`;
- production templates: exactly `49 / 49`;
- canonical repository-envelope fields: exactly `6`;
- UI C# files scanned: `8`;
- prohibited UI dependency findings: `0`;
- NuGet restoration: passed;
- .NET Framework 4.8 Release MSBuild: passed;
- compiler warnings: `0`;
- compiler errors: `0`;
- MSTest: `178 / 178` passed;
- TRX production and verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-216`;
- artifact ID: `8482666736`;
- SHA-256: `af49f327d5dcf05d020d3210f1853a017ca22f6b9979c9b6c8832b65401f102c`;
- expiration: `2026-08-04T03:13:17Z`.

The locally calculated ZIP digest matched the GitHub artifact digest.

## Current evidence boundary

Unit 1 canonical schemas and confidentiality-segregation tests are implemented and independently Windows-validated. All seven target adapters remain fail-closed. No Pass 12 adapter has been activated, no composition-root registration has been added, and no production Application command, query, or UI has been introduced.

Pass 12 remains under construction in draft PR #61. It is not merged, closure-validated, promoted, synchronized, deployed, or release-certified.

## Exact next gate

Validate this documentation-inclusive Unit 1 head through the authoritative Windows workflow. After that exact head passes, begin Unit 2 from the same branch:

**LibraryBook and LibraryBorrowing Invariant-Preserving Rehydration, Specialized Mappers, Repository Activation, Composition Registration, Restart Round Trips, Concurrency Enforcement, and Journaled Circulation Coordination.**
