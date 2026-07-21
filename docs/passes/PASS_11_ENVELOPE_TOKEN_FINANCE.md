# Pass 11 — Canonical Repository Envelopes, Session-Token Digests, and Enrollment/Finance Persistence

## Objective

Correct the inherited repository-envelope and session-token contracts before activating additional production repositories, then add validated specialized persistence, Student financial read models, and controlled Enrollment and Finance orchestration without weakening the seven-project dependency boundary or exposing JSON and filesystem access to UI-layer source.

## Construction and promotion history

- synchronized Pass 10 baseline: `686aa29a3c9827975d711c6713838f5cc2d22918`;
- prerequisite checkpoint: PR #51; `develop` commit `5d17748a550c79ac11b341b73acc1256d3b6d144`;
- complete implementation continuation: PR #52; exact head `fded5a9ac1f00176a9aeaae4c70a3f972c26ec84`;
- implementation integration commit: `69e69bb621cea80f135f073b79739f4833054eb0`;
- premature mainline promotion: PR #53; `main` commit `925696ca6dd75fc8be513e818835cde0ad85c812`;
- abandoned corrective draft: PR #54; not merged;
- Corrective Unit 1: PR #55; exact validated head `517af90bd2218dea471747b2da5c92e49526e3ac`; merge commit `849396b3a00158b154cd9086cf0229cafd0868de`;
- Corrective Unit 2: PR #56; exact validated code head `eac074c23da378e1cdae9a1abdf76090851b89a9`.

PR #53 promoted Pass 11 before independent closure evidence existed. Corrective closure preserves this history and validates the actual promoted tree rather than rewriting PR #53 as closure-gated.

## Canonical repository-envelope contract

The runtime, all 49 production templates, and source-tree validation use exactly:

1. `repositoryName`;
2. `schemaVersion`;
3. `revision`;
4. `updatedAtUtc`;
5. `updatedByUserId`;
6. `records`.

Canonical writes do not emit legacy `repository` or `createdAtUtc` fields. Compatibility reads accept those fields only as migration input and fail closed on conflicting names, invalid schema versions, revisions, timestamps, actors, or record collections.

## One-way journaled migration and corrective recovery

`RepositoryEnvelopeMigrationService`:

- scans the authoritative 49-repository catalog;
- identifies legacy envelopes without modifying canonical repositories;
- preserves repository revisions and record payloads;
- stages canonical replacements through the journaled transaction coordinator;
- revalidates expected revisions while canonical locks are held;
- uses revision-preserving transaction mutations;
- canonicalizes the transaction journal through the journal path;
- is idempotent after successful migration.

Corrective Unit 2 adds explicit migration audit states:

- `NotRequired`;
- `Registered`;
- `RecoveryRequired`.

If repository migration commits but operational audit registration fails, `RepositoryEnvelopeMigrationAuditRegistrationException` retains the committed migration result. `RecoverAuditRegistration` registers the missing audit record idempotently by audit ID or transaction ID.

The transaction coordinator now exposes an optional no-op-by-default deterministic failure-injection seam. Journal evidence records applied mutation count, last applied repository, failure type, and failure message. Successful rollback records `RolledBack`; unsuccessful rollback retains the incomplete journal and backups for `RecoverIncompleteTransaction`.

Independent tests convert all 49 repositories to legacy input, inject failure after mutation 17, prove byte-for-byte restoration of all 48 non-journal repositories, verify rollback journal evidence and backup cleanup, retry migration, and prove all 49 repositories are canonical.

## Cryptographic session-token verifier

Authentication:

- generates a 32-byte cryptographically random raw bearer token;
- returns the raw token only in the authentication response;
- persists only a versioned SHA-256 digest;
- verifies the presented token by recomputing the digest;
- compares digests through a fixed-time byte loop;
- rejects missing, unsupported, or legacy digest records.

`SessionSecurityMigrationService` revokes active legacy sessions, clears legacy bearer material and digest placeholders, records the actor, and advances the sessions repository revision. Legacy bearer values are not converted into valid new sessions because the original raw token cannot be recovered securely.

## Canonical persisted schemas and specialized mappers

Pass 11 defines explicit persisted record schema version `1` and validated rehydration for:

1. `Enrollment`;
2. `TuitionAssessment`;
3. `Payment`;
4. `FinancialAdjustment`;
5. `ScholarshipAward`.

Corrective Unit 1 removes the obsolete version-`0` compatibility path. Version `0`, negative versions, and future versions fail closed. Specialized mappers always write version `1`.

The records preserve identifiers, entity versions, archive metadata, timestamps, actors, lifecycle state, immutable snapshots, Money values and currencies, Subject or charge collections, receipt and allocation data, posting and void metadata, approval and application metadata, source links, and decision history.

Corrective Unit 2 verifies the complete persisted property set and byte-identical serialize–rehydrate–serialize output for all five aggregates, including nested Enrollment subject lines, Tuition Assessment charge lines, and Payment allocations.

## Typed repository activation

The following adapters are `SpecializedMapperCompleted` and active through the Infrastructure composition root:

- StudentRecordRepositoryAdapter;
- EmployeeRecordRepositoryAdapter;
- CourseRepositoryAdapter;
- SubjectRepositoryAdapter;
- AcademicPeriodRepositoryAdapter;
- AssessmentChargeRuleRepositoryAdapter;
- EnrollmentRepositoryAdapter;
- TuitionAssessmentRepositoryAdapter;
- PaymentRepositoryAdapter;
- FinancialAdjustmentRepositoryAdapter;
- ScholarshipAwardRepositoryAdapter.

Seven adapters remain fail-closed with explicit reasons:

- LibraryBookRepositoryAdapter;
- LibraryBorrowingRepositoryAdapter;
- CounselingCaseRepositoryAdapter;
- DisciplineCaseRepositoryAdapter;
- ClinicAppointmentRepositoryAdapter;
- MedicalRecordRepositoryAdapter;
- MedicalClearanceRepositoryAdapter.

## Student financial read model

`StudentFinanceQueryService` derives Student ownership from the authenticated session. Corrective Unit 1 applies explicit released-state allowlists:

- Enrollment excludes Draft and Unspecified;
- Tuition Assessment exposes Posted and Cancelled;
- Payment exposes Posted and Voided;
- Financial Adjustment exposes Posted only;
- Scholarship Award exposes Approved, Applied, and Cancelled.

The projection excludes internal approval rationale, adjustment reasons and source records, administrative actor identifiers, credential material, and unreleased records. It returns repository revisions and entity versions for concurrency-aware clients.

## Controlled Application commands

Pass 11 supplies session-aware services for:

1. Student Enrollment submission;
2. Tuition Assessment creation and posting;
3. Payment creation, allocation, receipt assignment, and posting;
4. Financial Adjustment creation and posting;
5. Scholarship Award application with a posted credit adjustment.

Commands validate role, application kind, session purpose, permission, confidentiality, Student ownership where applicable, repository revisions, entity versions, lifecycle state, currency consistency, allocation totals, identifiers, and audit actors.

Corrective Unit 1 requires each Payment allocation to carry the expected Tuition Assessment entity version. All allocation inputs, amounts, Assessment identities, statuses, ownership, period, currency, uniqueness, and versions are validated before Payment, allocation, or receipt identifiers are consumed.

Corrective Unit 2 injects failure after the first Scholarship transaction mutation and proves `scholarship_awards.json` and `financial_adjustments.json` both return to their exact pre-command bytes. The Award remains Approved, no Adjustment remains, rollback evidence is recorded, and the same request succeeds on retry.

## UI dependency boundary

The source validator scans every `*.cs` file under:

- `src/IUIS.SharedUI`;
- `src/IUIS.UserApp`;
- `src/IUIS.AdminApp`.

It rejects direct or fully qualified references to `System.IO` and `System.Text.Json`. Audit Unit 2 validated eight UI C# files with zero prohibited findings.

## Test progression

- Pass 10 baseline: 148 tests;
- original Pass 11 implementation: 160 tests;
- Corrective Unit 1: 167 tests;
- Corrective Unit 2: 172 tests.

Corrective evidence covers:

- exact schema version `1` enforcement;
- Payment allocation entity-concurrency before identifier consumption;
- released-state projection allowlists;
- posted Payment and Financial Adjustment exception atomicity;
- all-49 deterministic migration rollback and retry;
- transaction-journal failure evidence;
- migration audit-registration recovery classification and idempotence;
- exhaustive five-mapper field preservation;
- Scholarship multi-repository rollback and retry;
- all-UI dependency scanning.

## Windows evidence

| Stage | Commit | Run | Artifact | ID | SHA-256 |
|---|---|---:|---|---:|---|
| Exact implementation head | `fded5a9ac1f00176a9aeaae4c70a3f972c26ec84` | `29754011862` | `iuis-windows-build-evidence-161` | `8465908242` | `5e44960a8654022cdbc559d1c4b74e7af2566e7bbcb2dfda13c0921df9f05dbd` |
| Corrective Unit 1 | `517af90bd2218dea471747b2da5c92e49526e3ac` | `29792336074` | `iuis-windows-build-evidence-201` | `8480770463` | `77d60d81918f2df2adbc2f642fe92c5d8b0c9bcc4eb93e82646e20f235db8b2c` |
| Corrective Unit 2 code | `eac074c23da378e1cdae9a1abdf76090851b89a9` | `29793780496` | `iuis-windows-build-evidence-203` | `8481293446` | `26a2b77fad29ceef6e695f30f7aeef7077b65b01652d152667142e89c63acc8b` |

Run `29793780496` passed exactly seven project boundaries, 49 canonical templates, six envelope fields, eight UI C# files with zero prohibited findings, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero warnings, zero errors, 172 of 172 MSTest cases, TRX verification, and artifact publication.

## Figma model

The editable FigJam board contains the implementation and corrective-closure flows:

- `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

## Locked Pass 12 boundary

Pass 12 is defined as:

**Remaining Student-Service Specialized Persistence, Confidential Record Segregation, Controlled Lifecycle Orchestration, and Released Service Projections.**

It will address the seven deferred adapters, restricted persisted state and released projections, session-aware permission and ownership enforcement, concurrency-aware lifecycle orchestration, journaled related mutations, exhaustive security and rollback tests, Windows validation, and Figma evidence.

Pass 12 excludes production-form completion, notification dispatch, backup scheduling, deployment packaging, and release certification.

## Evidence boundary and exact final gate

Pass 11 corrective code is independently validated but is not final until the documentation-inclusive PR #56 head is validated and merged, the exact resulting `main` commit is independently Windows-validated, final evidence is registered, `develop` is restored to that final validated commit, and branch comparison reports ahead `0` and behind `0`.
