# Integrated University Information System (IUIS)

This repository contains the authoritative C# implementation of the Integrated University Information System specified for the IT332 final project.

## Current implementation state

Passes 1 through 8 establish the seven-project .NET Framework 4.8 solution, Domain foundations, the exact 49-file production JSON catalog, central ID allocation, cross-process locks, hardened atomic writes, journaled transactions, login lockout, forced password change, and one-time production bootstrap.

Pass 9 adds and closes Application authorization, session-aware command/query execution, Student own-record enforcement, Employee and Administrator permission boundaries, restricted DTOs, typed repository seams, in-lock expected-revision revalidation, and explicit readiness classification for all 18 aggregate adapters.

Pass 10 adds the first operational typed-persistence activation:

- canonical persisted-record schema v1 for StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule;
- validated Domain rehydration factories that restore value objects, lifecycle state, archive state, timestamps, actor IDs, and entity versions without reflection or transition replay;
- six specialized `System.Text.Json` mappers and six activated typed repository adapters;
- twelve remaining adapters that stay fail-closed with explicit deferral reasons;
- an Infrastructure composition root for repositories, authorization, projections, and Application services;
- real JSON-backed Student own-profile and Employee self-service read models with concurrency tokens;
- controlled Student and Employee contact updates with session-derived ownership, expected repository/entity versions, journaled transactions, and audit-ready metadata;
- canonical Administrator Employee master-record bootstrap; and
- mapper, migration, restart, stale-token, confidentiality, and vertical-slice tests.

Pass 10 implementation was validated with zero compiler warnings, zero compiler errors, and 142 of 142 tests, then merged through PR #43. PR #44 subsequently placed that implementation tree on `main` before the formal independent closure audit.

The Pass 10 closure branch starts from exact mainline merge commit `1405b507e9e8e0d7b24031c09cedeef500c5ac2f`. It adds six independent integrated-tree tests and corrects an `AssessmentChargeRule` exception-atomicity defect discovered during audit. The expected closure suite is 148 tests. Closure evidence, promotion confirmation, exact-mainline reverification, final branch synchronization, and the Pass 11 boundary are not complete until the closure sequence finishes.

The audit also reconfirmed a pre-existing repository-envelope discrepancy: the authoritative contract requires `repositoryName`, `schemaVersion`, `revision`, `updatedAtUtc`, `updatedByUserId`, and `records`, while the existing templates and runtime still use legacy `repository` and persist `createdAtUtc`. Controlled compatibility migration of all 49 templates is reserved as the first Pass 11 requirement.

Progress and evidence are recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md). Pass-specific records are in [`docs/passes/PASS_10_CANONICAL_SCHEMAS_SPECIALIZED_MAPPERS.md`](docs/passes/PASS_10_CANONICAL_SCHEMAS_SPECIALIZED_MAPPERS.md) and [`docs/passes/PASS_10_CLOSURE.md`](docs/passes/PASS_10_CLOSURE.md).

## Exact next gate

Validate and merge `build/pass-10-closure`, promote the closure corrections and evidence records to `main`, validate the exact promoted and final documentation-inclusive mainline trees, synchronize `develop` and `main` with zero divergence, and record the exact Pass 11 construction boundary.

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
