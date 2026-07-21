# Pass 12 — Remaining Student-Service Specialized Persistence, Confidential Record Segregation, Controlled Lifecycle Orchestration, and Released Service Projections

## Starting point

- final synchronized Pass 11 commit: `b546152ab6d284b6d142311ebf941f94484aa1b3`;
- `main` and `develop`: identical, ahead `0`, behind `0` at construction start;
- implementation branch: `build/pass-12-student-service-persistence`;
- exactly seven projects;
- exactly 49 authoritative production JSON repositories;
- eleven specialized-complete aggregate adapters at Pass 12 construction start;
- seven explicitly deferred fail-closed adapters at Pass 12 construction start.

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
- Library Student projections expose circulation state and bibliographic context but exclude internal actor identifiers and prepared or cancelled internal workflow state.
- Credential, bearer-token, password, digest, and Security Stamp material is not part of any Student-service aggregate record.

## Construction units

### Unit 1 — Canonical schema and segregation contracts

Define explicit canonical record schema shapes for all seven aggregates and their nested records. Lock confidential/released separation through automated tests. Keep all seven adapters deferred and fail-closed.

### Unit 2 — Library reconstruction and persistence

Add invariant-valid `LibraryBook` and `LibraryBorrowing` rehydration factories, specialized mappers, adapter activation, composition registration, restart round trips, inventory integrity, circulation concurrency, journaled issue/return/lost coordination, and a released own-record Student circulation projection.

### Unit 3 — Clinic appointment and medical reconstruction

Add `ClinicAppointment`, `MedicalRecord`, and `MedicalClearance` rehydration, specialized mappers, adapter activation, restricted/released medical projections, session-aware appointment and clearance workflows, and confidentiality tests.

### Unit 4 — Counseling and discipline reconstruction

Add `CounselingCase` and `DisciplineCase` rehydration, specialized mappers, adapter activation, counselor/discipline permission boundaries, released-only Student projections, response ownership, and restricted administrator tests.

### Unit 5 — Integrated orchestration and closure preparation

Exercise journaled multi-repository commands, stale repository and entity revisions, exception atomicity, restart recovery, all-seven schema migration behavior, exact 18-of-18 mapper readiness, Windows Release compilation, complete MSTest/TRX evidence, documentation reconciliation, and Figma persistence-to-Application-to-UI evidence.

## Unit 1 implementation and evidence

`StudentServicePersistedRecords.cs` defines explicit canonical persisted shapes for Library Book and copies, Library Borrowing lifecycle metadata, Counseling confidential sessions and released summaries, Discipline restricted and released shapes, Clinic Appointment workflow metadata, Medical confidential consultations and released summaries, and Medical Clearance restricted history and released state.

Unit 1 tests prove exactly seven top-level persisted aggregate records, parameterless serialization construction, non-null collection initialization, confidential/released type separation, absence of credential or bearer material, canonical camel-case names, and fail-closed adapter readiness.

Draft PR #60 was closed without merge after run `29797409211` exposed two MSTest compile-time generic-inference errors. Replacement draft PR #61 corrected those assertions.

Successful Unit 1 evidence:

- corrected code head: `f97bb72ce8e9e33a4b9539c96be07a1ad1bed996`;
- run: `29797763890` / run number `216`;
- documentation-inclusive head: `bd2db780cb89dd0b6419cd08c8cb36d236a4cb11`;
- run: `29798083094` / run number `217`;
- Release compilation: passed with `0` warnings and `0` errors;
- tests: `178 / 178` passed;
- final Unit 1 artifact: `iuis-windows-build-evidence-217`;
- artifact ID: `8482766300`;
- SHA-256: `c85a75c30e50159eed0ed47be26d55811e8fe8b8ed55057e8e2776a7b4ac0c11`.

## Unit 2 Domain reconstruction

`LibraryBookCopy.Rehydrate` reconstructs copy identity, barcode, condition, and state while rejecting unsupported condition/state combinations. Lost copies must carry Lost condition, maintenance copies must carry Damaged condition, and non-lost/non-maintenance copies cannot retain those conditions.

`LibraryBook.Rehydrate` reconstructs bibliographic state, lifecycle state, embedded copies, metadata, archive state, and version. It rejects null copies, duplicate copy IDs, duplicate barcodes, inconsistent inventory totals, and retired books containing on-loan copies.

`LibraryBorrowing.Rehydrate` reconstructs Student, Book, Copy, due date, issue state, renewal count, return state, lost state, timestamps, actors, archive state, and version. It rejects negative renewal counts and workflow states with missing, contradictory, or chronologically invalid metadata.

## Unit 2 specialized persistence and activation

`LibraryBookJsonMapper` and `LibraryBorrowingJsonMapper` enforce canonical record schema version `1`, explicit enum parsing, institution-local date reconstruction, complete entity metadata, and restart-safe round trips.

The following adapters are active and parameterless with respect to mapper selection:

- `LibraryBookRepositoryAdapter` -> `books` -> `LibraryBookJsonMapper`;
- `LibraryBorrowingRepositoryAdapter` -> `borrowings` -> `LibraryBorrowingJsonMapper`.

Mapper readiness is now:

- `13` `SpecializedMapperCompleted`;
- `5` `DeferredWithExplicitReason`;
- `0` generic-compatible placeholders;
- `0` unclassified specialized requirements.

The five remaining fail-closed adapters are Counseling Case, Discipline Case, Clinic Appointment, Medical Record, and Medical Clearance.

## Unit 2 composition and Application services

`IuisCompositionRoot` now registers `LibraryBooks`, `LibraryBorrowings`, `StudentLibraryCirculation`, and `LibraryCirculation`.

`StudentLibraryCirculationQueryService.GetOwnOverview` requires `student.library.read`, `UserApplication`, Student role, and `OwnRecord` confidentiality. Student identity is derived from the authenticated principal rather than supplied by the caller. The released projection includes Issued, Overdue, Returned, and Lost borrowings and excludes Prepared and Cancelled records. It exposes Book title, author, category, copy ID, due date, status, issue/return/lost timestamps, renewal count, and entity/repository revisions without internal actor IDs.

`LibraryCirculationCommandService` implements controlled Issue, Return, and Mark Lost commands. Each command validates session purpose, application kind, role, explicit permission, repository revisions, aggregate entity versions, Book/Borrowing/Copy relationships, and Domain lifecycle state before staging related writes.

Permissions are:

- `library.circulation.issue`;
- `library.circulation.return`;
- `library.circulation.lost.record`.

Issue creates and issues a new Borrowing and places the selected Book Copy on loan. Return completes the Borrowing and restores the Book Copy to Available or Maintenance according to returned condition. Mark Lost terminates the Borrowing and marks the Book Copy Lost. Every related mutation is staged through the journaled Application transaction coordinator across `books` and `borrowings`.

## Unit 2 tests

Thirteen new Unit 2 tests cover mapper round trips, invalid persisted state, readiness activation, journaled issue/return/lost coordination, stale repository and entity revisions, own-record projection, released field exclusion, Student command denial, composition-root restart, deterministic byte-for-byte rollback, and real-repository stale-token rejection.

## Unit 2 failed-run and reconciliation history

Initial Unit 2 head `e4f476824c3d0db84cb54d43a5930be4fd66c78f` ran as GitHub Actions run `29800360307` / run number `235`.

The source-tree gate, package restore, and Release compilation completed with zero compiler errors and zero warnings. All thirteen new Library tests passed. The suite result was `187 / 190` because three older tests still hard-coded the pre-Unit-2 readiness state and omitted `books` and `borrowings` from the composition activation list.

Failed-run artifact:

- `iuis-windows-build-evidence-235`;
- artifact ID `8483550446`;
- SHA-256 `f0e7eca785ee9dec64621b28476755c1092461a4e6cc1bb0e505dbcec937ac38`.

Compatibility assertions were reconciled through controlled test-only commits:

- `cf296537bc81eebd6f8f588bc1423d4ee61b6187` — Pass 10 canonical count and method name;
- `20323326235e9f6cd92a5b35bb90e3d9492595f2` — Pass 9 typed-adapter count;
- `c20969035260b8e9ad91ba6e55398ff8063d7fcf` — composition activation list and removal of temporary maintenance workflows.

The authoritative `.github/workflows/windows-build.yml` was restored byte-for-byte from `develop`, with `contents: read` and the original Release build job only.

## Successful Unit 2 Windows evidence

GitHub Actions run `29802968984` / run number `244` validated exact documentation-inclusive head `ca2288c597debd6c2de0657103d7141ca60853d5`.

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
- MSTest: `190 / 190` passed;
- TRX production and verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-244`;
- artifact ID: `8484440239`;
- SHA-256: `3ddba8c7080314f8f66555e19ab8a74923f2dd6a579adf053e6fba5f7197e30c`;
- expiration: `2026-08-04T05:07:18Z`.

The locally calculated ZIP digest matched the GitHub artifact digest.

## Current evidence boundary

Unit 1 and Unit 2 are implemented and independently Windows-validated. Library Book and Library Borrowing are now active specialized adapters, bringing mapper readiness to `13 / 18` complete with five remaining fail-closed adapters.

This evidence-registration commit is the final Unit 2 exact-head validation target. Pass 12 remains under construction in draft PR #61. It is not merged, closure-validated, promoted, synchronized, deployed, or release-certified.

## Exact next gate

Validate this evidence-registration head through the authoritative Windows workflow. After that exact head passes, proceed to:

**Pass 12 — Construction Unit 3: ClinicAppointment, MedicalRecord, and MedicalClearance Invariant-Preserving Rehydration, Confidential Consultation Segregation, Specialized Mappers, Repository Activation, Session-Aware Appointment and Clearance Orchestration, Released Medical Projections, Journaled Related Mutations, and Windows Validation.**
