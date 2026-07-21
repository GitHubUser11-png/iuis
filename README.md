# Integrated University Information System (IUIS)

This repository contains the authoritative C# implementation of the Integrated University Information System specified for the IT332 final project.

## Current implementation state

Passes 1 through 8 establish the seven-project .NET Framework 4.8 solution, Domain foundations, the exact 49-file production JSON catalog, central ID allocation, cross-process locks, hardened atomic writes, journaled transactions, login lockout, forced password change, and one-time production bootstrap.

Pass 9 closes Application authorization, session-aware command/query execution, Student own-record enforcement, Employee and Administrator permission boundaries, restricted DTOs, typed repository seams, in-lock expected-revision revalidation, and explicit readiness classification for all 18 aggregate adapters.

Pass 10 completes the first typed-persistence activation for StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule. It includes six specialized mappers, real Student and Employee JSON-backed vertical slices, controlled contact updates, independent closure hardening, exact-mainline validation, and 148 tests.

Pass 11 implements and correctively closes the next persistence and security boundary:

- the authoritative six-field repository envelope: `repositoryName`, `schemaVersion`, `revision`, `updatedAtUtc`, `updatedByUserId`, and `records`;
- compatibility reads for legacy `repository` and `createdAtUtc` input with canonical-only writes;
- a revision-preserving, journaled one-way rewrite across all 49 repositories;
- raw session tokens returned once, SHA-256 digests persisted, fixed-time verification, and legacy-session rejection and revocation;
- exact record schema version `1` enforcement for specialized persisted aggregates;
- specialized persistence for Enrollment, TuitionAssessment, Payment, FinancialAdjustment, and ScholarshipAward;
- eleven activated specialized adapters and seven remaining fail-closed adapters;
- released-state Student Enrollment and Finance projections;
- controlled Enrollment submission, Assessment posting, Payment allocation and posting, Financial Adjustment posting, and journaled Scholarship Award application;
- deterministic transaction failure evidence, byte-for-byte rollback verification, migration audit-recovery classification, exhaustive five-mapper integrity, Scholarship coordination rollback, and all-UI dependency scanning.

## Pass 11 corrective-closure history

Pass 11 implementation was completed through PR #52 and promoted to `main` through PR #53 at commit `925696ca6dd75fc8be513e818835cde0ad85c812` before independent closure evidence existed. Corrective closure preserves that historical fact rather than describing PR #53 as closure-gated.

Corrective Unit 1 was independently validated and merged through PR #55 at `849396b3a00158b154cd9086cf0229cafd0868de`.

Corrective Unit 2 and pre-promotion documentation reconciliation were validated at head `ac7adb00c106c4752b12bfaa3e6df5070b39e5c0` and merged through PR #56 to `main` commit `60536382383d9300000c9d042ad01ab6083a21e1`.

Validation-only PR #57 independently validated that exact merged-main commit without adding a trigger commit:

- Windows run `29795300068`;
- artifact `iuis-windows-build-evidence-209`;
- artifact ID `8481826369`;
- SHA-256 `288c4cb168b36701533cf9a219dd8622836ed3bd1bc5d21dd70fe8e738984800`;
- zero compiler warnings and zero compiler errors;
- 172 of 172 tests passed;
- TRX verification and artifact publication passed.

The final evidence-registration record is intentionally separated from the corrective code merge. It is validated as documentation-only, merged with an exact-head guard, and followed by an exact-final-main validation before `develop` is restored to zero divergence.

Progress and evidence are recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md). The Pass 11 implementation record is [`docs/passes/PASS_11_ENVELOPE_TOKEN_FINANCE.md`](docs/passes/PASS_11_ENVELOPE_TOKEN_FINANCE.md). Corrective-closure evidence is recorded in [`docs/passes/PASS_11_CORRECTIVE_CLOSURE.md`](docs/passes/PASS_11_CORRECTIVE_CLOSURE.md).

## Locked Pass 12 boundary

Pass 12 is defined, but construction is not authorized until Pass 11 reaches exact-final-main validation and `main`/`develop` zero divergence.

**Pass 12: Remaining Student-Service Specialized Persistence, Confidential Record Segregation, Controlled Lifecycle Orchestration, and Released Service Projections.**

The intended boundary is:

- canonical persisted schema v1 and validated specialized mappers for LibraryBook, LibraryBorrowing, CounselingCase, DisciplineCase, ClinicAppointment, MedicalRecord, and MedicalClearance;
- activation of the seven currently fail-closed adapters through the composition root;
- separate restricted persisted state and released projections for counseling, discipline, medical, and clearance information;
- session-derived ownership, role, application-kind, purpose, permission, confidentiality, repository-revision, and entity-version enforcement;
- controlled library circulation, appointment, consultation/medical-record, clearance, counseling, and discipline lifecycle orchestration;
- journaled multi-repository mutations where one command changes related authoritative repositories;
- exhaustive mapper, authorization, confidentiality, rollback, concurrency, and exception-atomicity tests;
- Windows Release validation and Figma architecture evidence.

Pass 12 excludes production-form completion, notification dispatch, operational backup scheduling, deployment packaging, and release certification.

## Locked technical target

- C# 7.3
- Windows Forms
- .NET Framework 4.8
- `System.Text.Json`
- separate `IUIS.UserApp.exe` and `IUIS.AdminApp.exe`
- layered seven-project Visual Studio solution
- shared synchronized JSON persistence
- exactly 49 authoritative production JSON files
- centralized identifiers and journaled related mutations
- no direct JSON or file-system access from any UI-layer C# source

## Branch model

- `main`: reviewed, validated baseline
- `develop`: synchronized integration branch
- `build/pass-*`: controlled implementation and closure passes created from exact integration commits

No component is described as compiled or tested unless machine-generated build or test evidence exists. Pass 11 is not release-certified, and Pass 12 has not started.
