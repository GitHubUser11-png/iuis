# Pass 11 — Canonical Repository Envelopes, Session-Token Digests, and Enrollment/Finance Persistence

## Objective

Correct the inherited repository-envelope and session-token contracts before activating additional production repositories, then add validated specialized persistence, Student financial read models, and controlled Enrollment and Finance orchestration without weakening the seven-project dependency boundary or exposing JSON and filesystem access to Forms.

## Starting point

- synchronized Pass 10 baseline: `686aa29a3c9827975d711c6713838f5cc2d22918`;
- implementation branch: `build/pass-11-envelope-token-finance`;
- prerequisite checkpoint integration: PR #51; `develop` commit `5d17748a550c79ac11b341b73acc1256d3b6d144`;
- continuation integration pull request: PR #52;
- validated continuation head: `df6e5997b83b999cab459f3baa4cf11245ebffc9`.

PR #51 integrated the prerequisite checkpoint before the full Pass 11 boundary was complete. PR #52 is the controlled continuation containing the remaining specialized persistence, Application vertical slices, tests, documentation, and final integration evidence.

## Canonical repository-envelope contract

The runtime, all 49 production templates, and source-tree validation now use exactly:

1. `repositoryName`;
2. `schemaVersion`;
3. `revision`;
4. `updatedAtUtc`;
5. `updatedByUserId`;
6. `records`.

Canonical writes do not emit legacy `repository` or `createdAtUtc` fields.

A compatibility converter accepts the legacy fields only as migration input. It rejects conflicting canonical and legacy repository names, validates catalog name, schema version, revision, UTC update timestamp, update actor, and records collection, then emits only the canonical six-field envelope.

## One-way journaled migration

`RepositoryEnvelopeMigrationService`:

- scans the authoritative 49-repository catalog;
- identifies legacy envelopes without changing canonical repositories;
- preserves repository revisions and record payloads;
- stages canonical replacements through the transaction coordinator;
- revalidates expected revisions while canonical locks are held;
- uses revision-preserving transaction mutations;
- records migrated repository names, transaction ID, actor, timestamp, and committed result;
- canonicalizes the transaction journal through the same journal path;
- is idempotent and reports a no-op after successful migration.

Rollback remains available through the retained transaction journal and pre-replacement backups if application fails before commit.

## Cryptographic session-token verifier

Authentication now:

- generates a 32-byte cryptographically random raw bearer token;
- returns the raw token only in the authentication response;
- persists only a versioned SHA-256 digest;
- verifies the presented raw token by recomputing the digest;
- compares digests through a fixed-time byte loop;
- rejects sessions with missing or unsupported digest versions;
- rejects sessions that still contain legacy bearer material.

`SessionSecurityMigrationService` identifies legacy sessions, revokes any active legacy session, clears persisted legacy bearer values and digest placeholders, records the update actor, and advances the sessions repository revision. Active bearer material is not migrated into a new valid session because the original raw token cannot be recovered securely.

## Canonical persisted schemas and specialized mappers

Pass 11 defines explicit record schema v1 and validated rehydration for:

1. `Enrollment`;
2. `TuitionAssessment`;
3. `Payment`;
4. `FinancialAdjustment`;
5. `ScholarshipAward`.

The records preserve stable identifiers, entity versions, archive metadata, timestamps, actors, lifecycle state, immutable snapshots, Money values and currencies, Subject or charge collections, receipt and allocation data, posting and void metadata, approval and application metadata, source links, and decision history.

The Domain factories reconstruct invariant-valid state without reflection-based private mutation and without replaying business transitions merely to hydrate stored records. Unsupported record-schema versions fail closed.

## Typed repository activation

The following adapters are newly classified `SpecializedMapperCompleted` and activated through the Infrastructure composition root:

- `EnrollmentRepositoryAdapter`;
- `TuitionAssessmentRepositoryAdapter`;
- `PaymentRepositoryAdapter`;
- `FinancialAdjustmentRepositoryAdapter`;
- `ScholarshipAwardRepositoryAdapter`.

Together with the six Pass 10 adapters, the readiness catalog now contains:

- `11` specialized-complete adapters;
- `7` adapters deferred with explicit reasons;
- `0` production adapters classified as generic-mapper compatible;
- `0` production adapters left in an ambiguous requires-mapper state.

Library, Borrowing, Counseling, Discipline, Clinic Appointment, Medical Record, and Medical Clearance adapters remain fail-closed.

## Student financial read model

`StudentFinanceQueryService` derives Student ownership from the authenticated session and returns released projections for:

- Enrollment and selected Subject lines;
- Tuition Assessments;
- Posted Payments and receipt numbers;
- Scholarship Award effects;
- posted assessment, debit-adjustment, credit-adjustment, payment, and balance totals;
- repository revisions and entity versions for concurrency-aware clients.

The projection excludes internal approval rationale, adjustment reasons and source records, administrative actor identifiers, credential material, and unreleased notes.

## Controlled Application commands

Pass 11 adds session-aware services for:

1. Student Enrollment submission;
2. Tuition Assessment creation and posting;
3. Payment creation, allocation, receipt assignment, and posting;
4. Financial Adjustment creation and posting;
5. Scholarship Award application with a posted credit adjustment.

Commands validate role, application kind, session purpose, permission, confidentiality marker, Student ownership where applicable, repository revisions, entity versions, lifecycle status, currency consistency, allocation totals, immutable IDs, and audit actors. Writes are routed through the journaled transaction coordinator. Scholarship application stages the updated Award and created Financial Adjustment in one multi-repository transaction.

## Regression and security tests

Twelve Pass 11 tests increased the complete suite from `148` to `160`. They cover:

- 49 canonical six-field production envelopes;
- legacy compatibility reads and canonical-only writes;
- all-49 journaled migration and idempotence;
- revision and record preservation;
- raw-token-once issuance and digest-only persistence;
- fixed-time-compatible digest verification and tamper rejection;
- legacy-session rejection, revocation, and bearer-material removal;
- five specialized mapper round trips;
- unsupported finance record-schema rejection;
- readiness classification at 11 completed and 7 deferred;
- session-derived Student financial ownership;
- stale-revision denial before Payment writes;
- journaled Scholarship Award and Financial Adjustment coordination.

## Successful Windows implementation evidence

GitHub Actions run `29753253012` validated continuation head `df6e5997b83b999cab459f3baa4cf11245ebffc9`.

- source-tree and architecture validation: passed;
- production templates: exactly `49`;
- canonical envelope fields: exactly `6`;
- project boundaries: exactly `7`;
- NuGet restoration: passed;
- .NET Framework 4.8 Release MSBuild: passed;
- compiler warnings: `0`;
- compiler errors: `0`;
- MSTest: `160` executed, `160` passed, `0` failed;
- TRX verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-158`;
- artifact ID: `8465588173`;
- SHA-256: `9fd73b4619feb125daeb3ee0ced0d9e3dd11a052f44a1c3754c985400eb189c5`;
- expiration: 2026-08-03.

## Figma model

The editable FigJam board contains `IUIS Pass 11 Envelope-to-Finance Wiring`:

- `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

The model connects legacy-envelope migration, canonical repositories, raw-token issuance, digest verification, session-aware Application execution, Student financial projections, five specialized adapters, and journaled controlled writes.

## Evidence boundary

Pass 11 does not constitute independent integrated-tree closure or final release certification. It does not activate the remaining seven specialized adapters and does not complete production Forms, notification dispatch, trusted-device/network enforcement, operational backup scheduling, deployment packaging, or final recovery certification.

## Exact next gate

Validate the evidence-updated final PR #52 head, merge it into `develop`, independently validate the actual merged Pass 11 tree during Pass 11 Closure, register closure evidence, promote only after successful closure, synchronize branches, and define the Pass 12 construction boundary.
