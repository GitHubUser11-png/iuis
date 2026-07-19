# Pass 4 — Core Identity and Person Aggregates

## Objective

Implement the first production aggregate group for identity and person master records while preserving the Domain-only boundary established in Pass 3.

## Starting point

- repository: `GitHubUser11-png/iuis`
- base branch: `develop`
- base includes Pass 3 closure integration
- pass branch: `build/pass-04-core-identity-person`

## Created

- `InstitutionIdentifier`
- `UserAccount`
- `UserSession`
- `StudentStatus`
- `EmploymentStatus`
- `StudentRecord`
- `EmployeeRecord`
- `CoreIdentityPersonAggregateTests`

## Implemented contracts

### Stable institution identifiers

Identifiers use `PREFIX-YYYY-NNNNNN`, with uppercase prefixes, a four-digit institution-local year, a six-digit positive sequence, deterministic parsing, and value equality. Sequence allocation remains deferred to Infrastructure and the central `id_sequences.json` transaction.

### User accounts

User accounts remain separate from Student and Employee records. The aggregate stores a stable person-record reference, canonical role, account status, credential hash, Security Stamp, temporary-credential state, and credential expiration. Login ID, credential, and status changes require a Security Stamp replacement and advance the entity version.

### Sessions

Sessions store the public Session ID separately from the token hash. Raw bearer tokens are not part of the Domain model. The aggregate enforces inactivity and absolute expiration, Security Stamp matching, activity-window updates, revocation metadata, and active-session-only revocation.

### Student and Employee records

Student and Employee records remain independent from User accounts. Their institutional number is identical to the aggregate ID and cannot be changed. Contact, assignment, course, and lifecycle status mutations advance entity version and preserve audit metadata.

## Architecture boundary

All new code belongs to `IUIS.Domain`. No new file references WinForms, file-system APIs, JSON serialization, Application, Infrastructure, SharedUI, UserApp, or AdminApp. Sequence allocation, repository uniqueness, authentication cryptography, session-token generation, and persistence remain outside Domain.

## Test coverage

Eight tests were added for identifier formatting, identifier rejection, account normalization, role/person compatibility, session validity, session revocation, Student number invariance, and Employee assignment versioning. The pre-existing 25 tests remain.

## Evidence boundary

Pass 4 is not considered compiled, tested, or merge-ready until the Windows GitHub Actions workflow completes successfully on the final documented branch head and its TRX and artifact evidence are inspected.
