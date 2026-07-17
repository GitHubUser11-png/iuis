# Pass 3 — Production Domain Foundations

## Objective

Create the first production Domain code for IUIS while preserving the locked architecture: Domain remains independent from WinForms, file-system APIs, JSON serialization, Infrastructure, credentials, and fixed development seed behavior.

## Starting point

- repository: `GitHubUser11-png/iuis`
- base branch: `develop`
- starting `develop` commit: `ff5a2ced7dfbfc85078dc7800f94b698ff6de007`
- pass branch: `build/pass-03-domain-foundations`

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

## Test coverage created

`DomainFoundationTests.cs` covers:

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

The pre-existing three solution-foundation tests remain in place.

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

## Validation gate

Pass 3 is not complete until the pull-request Windows workflow completes source-tree validation, NuGet restoration, Release MSBuild, MSTest execution, TRX verification, and evidence artifact publication on the final documented head.
