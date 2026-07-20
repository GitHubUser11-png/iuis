# Integrated University Information System (IUIS)

This repository contains the authoritative C# implementation of the Integrated University Information System specified for the IT332 final project.

## Current implementation state

Passes 1 through 8 establish the seven-project .NET Framework 4.8 solution, Domain foundations, the exact 49-file production JSON catalog, central ID allocation, cross-process locks, hardened atomic writes, journaled transactions, login lockout, forced password change, and one-time production bootstrap.

Pass 9 closes Application authorization, session-aware command/query execution, Student own-record enforcement, Employee and Administrator permission boundaries, restricted DTOs, typed repository seams, in-lock expected-revision revalidation, and explicit readiness classification for all 18 aggregate adapters.

Pass 10 establishes the first operational typed-persistence activation:

- canonical persisted-record schema v1 for StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule;
- validated Domain rehydration factories that reconstruct value objects, lifecycle state, archive metadata, timestamps, actor IDs, and entity versions without reflection or transition replay;
- six specialized `System.Text.Json` mappers and six activated typed repository adapters;
- twelve remaining adapters that stay fail-closed with explicit deferral reasons;
- an Infrastructure composition root for repositories, authorization, projections, and Application services;
- real JSON-backed Student own-profile and Employee self-service read models with repository and entity concurrency tokens;
- controlled Student and Employee contact updates with session-derived ownership, expected revisions, journaled transactions, and audit-ready metadata; and
- canonical Administrator Employee master-record bootstrap while preserving a separate User account.

The independent Pass 10 closure audit corrected an `AssessmentChargeRule` exception-atomicity defect and added six integrated-tree tests covering restarted repository round trips, archive metadata, unsupported record versions, incomplete archive data, mapper-readiness alignment, and rejected-mutation integrity. The suite now contains 148 tests.

Pass 10 implementation, independent closure, closure-baseline promotion, and exact promoted-mainline validation are complete through PRs #43–#47. The exact promoted mainline commit is `b3f22c5641de6842e0696268d0e4930ae034e274`. Every closure-stage Windows run validated exactly 49 production templates, seven project boundaries, zero compiler warnings, zero compiler errors, 148 of 148 tests, valid TRX output, and evidence artifact publication.

This documentation-finalization branch records the complete closure evidence and the exact Pass 11 construction boundary. Pass 10 is not final release certification. Final documentation-inclusive mainline validation and zero-divergence `main`/`develop` synchronization remain required before Pass 11 starts.

## Locked Pass 11 starting boundary

Pass 11 begins with two inherited contract corrections:

1. migrate all 49 repositories from the legacy `repository` plus `createdAtUtc` envelope to the authoritative `repositoryName`, `schemaVersion`, `revision`, `updatedAtUtc`, `updatedByUserId`, and `records` envelope through a controlled, idempotent, journaled one-way rewrite;
2. replace bearer-token persistence under `TokenHash` with raw-token-once issuance, cryptographic digest storage, fixed-time digest comparison, and legacy-session revocation or migration.

After those corrections, Pass 11 adds canonical persisted schemas and specialized adapters for Enrollment, TuitionAssessment, Payment, FinancialAdjustment, and ScholarshipAward, then connects Student financial read models and controlled enrollment, assessment, payment, adjustment, and scholarship orchestration.

Progress and evidence are recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md). Pass-specific records are in [`docs/passes/PASS_10_CANONICAL_SCHEMAS_SPECIALIZED_MAPPERS.md`](docs/passes/PASS_10_CANONICAL_SCHEMAS_SPECIALIZED_MAPPERS.md) and [`docs/passes/PASS_10_CLOSURE.md`](docs/passes/PASS_10_CLOSURE.md).

## Exact next repository gate

Validate and merge the Pass 10 closure-record finalization, validate the exact resulting documentation-inclusive `main` tree, synchronize `develop` to that exact final `main` commit with ahead `0` and behind `0`, and only then begin Pass 11.

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
- no direct JSON or file-system access from Forms

## Branch model

- `main`: reviewed release-ready baselines
- `develop`: synchronized integration branch
- `build/pass-*`: controlled implementation and closure passes created from exact integration commits

No component is described as compiled or tested unless machine-generated build or test evidence exists.
