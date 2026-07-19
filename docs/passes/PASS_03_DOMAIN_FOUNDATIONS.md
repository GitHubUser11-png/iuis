# Pass 3 — Production Domain Foundations

## Objective

Create the first production Domain code for IUIS while preserving the locked architecture: Domain remains independent from WinForms, file-system APIs, JSON serialization, Infrastructure, credentials, and fixed development seed behavior.

## Starting point

- repository: `GitHubUser11-png/iuis`
- base branch: `develop`
- starting `develop` commit: `ff5a2ced7dfbfc85078dc7800f94b698ff6de007`
- pass branch: `build/pass-03-domain-foundations`
- pull request: `#5`

## Created Domain foundations

### Common entity contracts

- `IEntity`
- `IVersionedEntity`
- `IArchivableEntity`
- `EntityBase`
- `DomainValidationException`
- deterministic text normalization and invariant guards

`EntityBase` establishes stable IDs, version numbers, UTC creation/update audit metadata, archive metadata, version advancement, chronological timestamp enforcement, archive behavior, and restore behavior.

### People value objects

- `PersonName`
- `ContactInformation`
- `PostalAddress`

These types normalize user-entered text, preserve explicit value semantics, reject structurally invalid values, and avoid persistence or UI dependencies.

### Institution date value type

- `InstitutionLocalDate`

This type provides date-only semantics for .NET Framework 4.8, canonical `yyyy-MM-dd` parsing and formatting, calendar validation, and deterministic comparison without introducing a time-zone service into Domain.

### Monetary value rules

- `MoneyRules`
- `Money`

The monetary contract uses `decimal`, two fractional digits, `MidpointRounding.AwayFromZero`, explicit three-letter currency codes, immutable arithmetic, and currency-match validation. `PHP` is exposed as the initial institution currency convenience factory without preventing later explicit currency validation.

### Canonical identity contracts

- `PrimaryRole`
- `PersonRecordKind`
- `UserAccountStatus`
- `SessionApplicationKind`
- `SessionPurpose`
- `UserSessionStatus`
- `IdentityPolicy`

The compatibility policy preserves the separate executable boundary: Student and Employee/Faculty roles use the general User application, while Administrator uses the restricted Administrator application.

## Test coverage

`DomainFoundationTests.cs` contains 22 tests covering:

- initial entity version and audit metadata;
- UTC enforcement;
- deterministic version advancement;
- archive and restore history;
- chronological mutation enforcement;
- person-name normalization and equality;
- email and phone normalization and rejection;
- postal-address normalization;
- date parsing, leap-day validation, and ordering;
- monetary rounding, arithmetic, currency mismatch, and nonnegative guards;
- role and application compatibility.

The pre-existing three solution-foundation tests remain in place, bringing the suite to 25 tests.

## Compiler correction

The first Windows run, `29565314840`, compiled and passed all 25 tests but reported one CS1718 warning in a test that compared a date variable to itself. The test was corrected to compare two distinct equal `InstitutionLocalDate` values. No production behavior changed.

## Final pull-request evidence

GitHub Actions run `29565661053` validated final documented head `c25c9f93118b08d933650c1d8b66997237671c46`.

- source-tree and architecture validation: passed
- NuGet restoration: passed
- Release MSBuild: passed
- warnings: `0`
- errors: `0`
- MSTest execution: passed
- tests executed: `25`
- tests passed: `25`
- tests failed: `0`
- TRX verification: passed
- artifact publication: passed

Evidence artifact:

- name: `iuis-windows-build-evidence-23`
- artifact ID: `8401004874`
- SHA-256: `d9adc62863d26fcf5e5079b47f7e79888d2bd2886324651ea269fa0ab4bb3596`
- expiration: 2026-07-31

## Integration result

PR #5 was squash-merged into `develop`.

- merge commit: `0ce0595df6142d3a2f75a292dada161141fdcd5d`
- merged source: production Domain foundations and all 25 tests
- source PR state: closed and merged

A closure branch was created from the merged `develop` commit to perform a final post-merge Windows validation and correct the authoritative implementation-state record.

## Compatibility boundary

All added source is written for C# 7.3 and .NET Framework 4.8. Domain references only `System` and `System.Core`. No Domain file references WinForms, `System.IO`, `System.Text.Json`, Application, Infrastructure, SharedUI, UserApp, or AdminApp.

## Explicitly deferred

- Student, Employee, User, Session, Course, Subject, Enrollment, Finance, Library, Counseling, Discipline, Clinic, Attendance, and Operations aggregates;
- repository envelopes and catalog;
- JSON templates;
- identifiers and sequence allocation;
- persistence, locks, transactions, and recovery;
- authentication and Administrator bootstrap;
- UI and module services.

## Evidence boundary

Pass 3 establishes compile-verified and test-verified Domain foundations only. It does not establish complete production aggregates, persistence, authentication, business modules, operational recovery, deployment readiness, or release certification.

## Closure gate

The Pass 3 closure pull request must validate the merged Domain tree and closure-only documentation before the next construction pass begins.
