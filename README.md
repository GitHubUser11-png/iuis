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

Pass 10 implementation validation succeeded through GitHub Actions run `29723680915`: exactly 49 production templates, seven valid project boundaries, zero compiler warnings, zero compiler errors, and 142 of 142 tests passed. PR #43 remains the integration gate; Pass 10 is not closure-complete or promoted to `main` yet.

Progress and evidence are recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md). Pass-specific evidence is recorded in [`docs/passes/PASS_10_CANONICAL_SCHEMAS_SPECIALIZED_MAPPERS.md`](docs/passes/PASS_10_CANONICAL_SCHEMAS_SPECIALIZED_MAPPERS.md).

## Exact next gate

Validate the final evidence-updated PR #43 head, merge it into `develop`, independently closure-validate the merged Pass 10 tree, promote only after successful closure, synchronize branches, and define the Pass 11 construction boundary.

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
