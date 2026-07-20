# Integrated University Information System (IUIS)

This repository contains the authoritative C# implementation of the Integrated University Information System specified for the IT332 final project.

## Current implementation state

Passes 1 through 8 establish the seven-project .NET Framework 4.8 solution, Domain foundations, the exact 49-file production JSON catalog, central ID allocation, cross-process locks, hardened atomic writes, journaled transactions, login lockout, forced password change, and one-time production bootstrap.

Pass 9 closes Application authorization, session-aware command/query execution, Student own-record enforcement, Employee and Administrator permission boundaries, restricted DTOs, typed repository seams, in-lock expected-revision revalidation, and explicit readiness classification for all 18 aggregate adapters.

Pass 10 completes the first typed-persistence activation for StudentRecord, EmployeeRecord, Course, Subject, AcademicPeriod, and AssessmentChargeRule. It includes six specialized mappers, real Student and Employee JSON-backed vertical slices, controlled contact updates, independent closure hardening, exact-mainline validation, and 148 tests.

Pass 11 implements the next persistence and security boundary:

- the authoritative six-field repository envelope: `repositoryName`, `schemaVersion`, `revision`, `updatedAtUtc`, `updatedByUserId`, and `records`;
- compatibility reads for legacy `repository` and `createdAtUtc` input with canonical-only writes;
- an idempotent, revision-preserving, journaled one-way rewrite across all 49 repositories;
- all 49 production templates and source-tree validation normalized to the canonical contract;
- raw session tokens returned once, SHA-256 digests persisted, fixed-time verification, and legacy-session rejection and revocation;
- canonical record schema v1 and specialized mappers for Enrollment, TuitionAssessment, Payment, FinancialAdjustment, and ScholarshipAward;
- eleven activated specialized adapters and seven remaining fail-closed adapters;
- real Student Enrollment, Assessment, balance, Payment, and Scholarship read models;
- controlled Enrollment submission, Assessment posting, Payment allocation and posting, Financial Adjustment posting, and journaled Scholarship Award application.

Pass 11 adds twelve regression and security tests, bringing the suite to 160 tests. GitHub Actions run `29753253012` validated continuation head `df6e5997b83b999cab459f3baa4cf11245ebffc9`: exactly 49 canonical templates, six canonical envelope fields, seven project boundaries, zero compiler warnings, zero compiler errors, 160 of 160 tests passed, valid TRX output, and artifact publication.

PR #51 integrated the prerequisite checkpoint before the complete Pass 11 boundary was finished. Draft PR #52 is the controlled continuation carrying the complete implementation, expanded tests, documentation, Figma model, and final integration gate. Pass 11 is not closure-complete or release-certified until the exact final PR head is validated and merged, followed by independent integrated-tree closure.

Progress and evidence are recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md). The Pass 11 implementation record is [`docs/passes/PASS_11_ENVELOPE_TOKEN_FINANCE.md`](docs/passes/PASS_11_ENVELOPE_TOKEN_FINANCE.md).

## Exact next gate

Validate the evidence-updated final PR #52 head, merge it into `develop`, then independently closure-validate the exact merged Pass 11 tree before any mainline promotion or Pass 12 construction.

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
