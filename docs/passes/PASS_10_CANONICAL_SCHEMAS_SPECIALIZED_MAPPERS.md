# Pass 10 — Canonical Persisted Schemas, Specialized Mappers, and Vertical Slices

## Objective

Activate the first production typed-repository set without allowing reflection-based private-state mutation, transition replay during hydration, Application-to-Infrastructure coupling, or direct Forms access to JSON and the file system.

## Starting point

- synchronized Pass 9 baseline: `81c4b78acb149cef5a9feef6f8c71ae8b9b3037e`;
- implementation branch: `build/pass-10-canonical-schemas-specialized-mappers`;
- pull request: `#43`;
- validated implementation head: `0c0f660471f3bc1606bf0a50a0a45f8548fefd9c`.

## Canonical record schema v1

Pass 10 defines explicit persisted-record types for:

1. `StudentRecord`;
2. `EmployeeRecord`;
3. `Course`;
4. `Subject`;
5. `AcademicPeriod`;
6. `AssessmentChargeRule`.

Every canonical persisted entity includes:

- `recordSchemaVersion`;
- stable `id`;
- entity `version`;
- archive state;
- created and updated UTC timestamps;
- created and updated actor IDs;
- archive timestamp and actor when archived;
- aggregate-specific primitive and nested value-object fields.

Schema version `0` is accepted only as the unversioned canonical migration input. Every subsequent write emits schema version `1`. Unsupported schema versions fail closed.

## Domain rehydration boundary

The six activated Domain aggregates expose explicit rehydration factories. They invoke normal constructors to validate identity and value-object invariants, then restore validated persistence metadata through `EntityBase.RestorePersistenceState`. Lifecycle state and collections are assigned by the aggregate's own factory rather than by reflection or by replaying state-changing methods.

Restoration validates:

- entity version greater than or equal to one;
- UTC and chronological timestamps;
- required actor identifiers;
- active records without archive residue;
- archived records with archive timestamp and actor matching the latest change;
- defined lifecycle states;
- Subject prerequisite uniqueness and non-self-reference;
- Course, Subject, Academic Period, Student, Employee, and charge-rule invariants.

## Specialized mapper activation

The following adapters are classified `SpecializedMapperCompleted` and instantiate their specialized mapper internally:

- `StudentRecordRepositoryAdapter`;
- `EmployeeRecordRepositoryAdapter`;
- `CourseRepositoryAdapter`;
- `SubjectRepositoryAdapter`;
- `AcademicPeriodRepositoryAdapter`;
- `AssessmentChargeRuleRepositoryAdapter`.

The remaining twelve adapters remain `DeferredWithExplicitReason`. No production adapter is classified as generic-mapper compatible or left in an ambiguous readiness state.

## Composition root and real vertical slices

`IuisCompositionRoot` constructs the repository catalog, JSON store, six activated typed repositories, journaled Application transaction coordinator, authoritative JSON principal provider, permission resolver, session-aware executor, projection service, Student own-profile query, Employee self-service query, and Student/Employee contact-update services.

The real read path is:

`Windows Forms → session-aware Application query → authorization → typed Application repository contract → specialized Infrastructure adapter → canonical JSON record → released DTO`.

Student and Employee read DTOs expose repository revision and entity version as concurrency tokens without exposing internal persistence objects.

## Controlled contact updates

Student and Employee contact updates:

- derive record ownership from the authenticated session;
- require the appropriate own-record permission;
- require the User application and full-access session;
- validate caller-supplied expected repository revision and entity version;
- update Domain value objects and entity audit metadata;
- stage the complete typed repository snapshot through the Pass 8/9 journaled transaction coordinator;
- return transaction ID, new repository revision, new entity version, update timestamp, and update actor.

A stale repository or entity token is rejected before mutation and cannot overwrite a committed update.

## Bootstrap compatibility

Production bootstrap now seeds the initial Administrator's separate Employee master record through the canonical Employee schema and specialized mapper. The user account remains a separate record linked by stable Employee ID. The caller still supplies the initial Administrator credential; no fixed production credential is introduced.

## Tests

Fifteen Pass 10 tests were added, increasing the suite from 127 to 142. Coverage includes:

- canonical Administrator Employee bootstrap;
- activated-adapter empty/bootstrap reads;
- Student and Employee value-object and metadata round trips;
- Academic lifecycle, date, and prerequisite reconstruction;
- Money and charge-rule lifecycle reconstruction;
- unversioned canonical migration to schema v1;
- unsupported schema rejection;
- six-completed/twelve-deferred readiness classification;
- real JSON-backed Student and Employee session-derived read models;
- journaled Student and Employee contact updates;
- stale concurrency-token rejection;
- released DTO confidentiality regression protection.

## Successful Windows evidence

GitHub Actions run `29723680915` validated implementation head `0c0f660471f3bc1606bf0a50a0a45f8548fefd9c`.

- source-tree and architecture validation: passed;
- production templates: exactly `49`;
- project boundaries: exactly `7`;
- NuGet restoration: passed;
- .NET Framework 4.8 Release MSBuild: passed;
- compiler warnings: `0`;
- compiler errors: `0`;
- MSTest: `142` executed, `142` passed, `0` failed;
- TRX verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-128`;
- artifact ID: `8453381929`;
- SHA-256: `fccb5e7b2c236a7827e13965b296b87a440f089d397734e6088bea5062047751`;
- expiration: 2026-08-03.

## Figma model

The editable FigJam board includes the Pass 10 persistence-to-UI wiring model:

- `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

## Evidence boundary

Pass 10 does not activate the remaining twelve aggregate adapters. It also does not complete production Forms, trusted-device/network enforcement, notification delivery, operational backup/restore execution, deployment packaging, or final release certification.

## Exact next gate

Validate the final evidence-updated PR #43 head, merge it into `develop`, independently validate the integrated Pass 10 tree during closure, promote only after closure, synchronize branches, and define the Pass 11 boundary.
