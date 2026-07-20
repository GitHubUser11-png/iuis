# Pass 9 — Application Authorization, Typed Repository Adapters, and Restricted Projections

## Objective

Build the first production Application-layer authorization and typed repository boundary on top of the validated Pass 8 persistence foundation. Pass 9 enforces role, permission, session, ownership, and confidentiality rules before commands or queries reach repositories or Domain aggregates.

## Starting point

- synchronized Pass 8 baseline: `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f`
- Pass 8 final run: `29693980046`
- Pass 8 final artifact: `iuis-windows-build-evidence-102`
- artifact ID: `8444441580`
- SHA-256: `8d082a7a293ae9bc1bc5ada5badee1414ab8f4689de2ab772c04e99118d960c9`
- implementation branch: `build/pass-09-application-authorization-repositories`

## Authorization model

Pass 9 resolves access from Primary Role, required application kind, session purpose, active profile permissions, direct grants, direct restrictions, ownership, confidentiality, and explicit role compatibility. Effective permissions are active profile permissions union direct grants, subject to direct restrictions. Restrictions win. Administrator status is not a universal confidentiality bypass: Restricted data requires `confidentiality.restricted`, and Highly Restricted data requires `confidentiality.high`, in addition to the operation permission.

## Session-aware execution

`SessionAwareRequestExecutor` loads the authoritative principal for every command or query through `IAuthorizationPrincipalProvider`, resolves the request, and invokes the handler only after authorization succeeds. `JsonAuthorizationPrincipalProvider` reads `users.json`, `sessions.json`, and `permission_profiles.json` under deterministic locks and validates account/session status, token, expiration, Security Stamp, linkage, application kind, session purpose, role compatibility, and assigned profile existence.

## Ownership and confidentiality

`StudentOwnRecordQueryService` does not accept a Student ID. The Student ID is derived from the validated session principal. Employee queries require explicit permission and compatible roles. Counseling, Discipline, and Medical queries use separate released and internal DTO types; released DTOs contain no internal notes, evidence, findings, rationale, clinical assessment, or treatment data.

## Typed repository boundary

Application defines revision-aware repository contracts for Student, Employee, Academic, Finance, Library, Counseling, Discipline, and Clinic aggregates. Infrastructure provides `MappedJsonRepository<T>`, mapper contracts, authoritative-name adapters, stable-ID lookup, optimistic expected-revision writes, and `JournaledApplicationTransactionCoordinator` for related writes. The mapper boundary keeps JSON out of Application and supports aggregate-specific hydration.

## Expanded tests

Seventeen tests cover profile resolution, restriction precedence, Administrator confidentiality, Student ownership and cross-record denial, restricted session purposes, DTO exclusion, session-derived queries, Employee role boundaries, typed repository round trips, optimistic conflicts, journaled transactions, profile loading, expiration, and Security Stamp mismatch. Expected total: 127 tests.

## Validation gate

The branch must pass source-tree and architecture validation, exact 49-template validation, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero warnings/errors, all 127 tests, TRX verification, and artifact publication. No success claim is made before machine evidence exists.

## Deferred

Production Forms, complete aggregate-specific mappers for evolved shapes, trusted-device/network enforcement, notification dispatch, backup/restore execution, deployment packaging, and release certification remain deferred.

## Exact next gate

Commit the Pass 9 source and tests, generate the editable Figma Application-service model, open a pull request against `develop`, complete Windows validation, correct defects, merge the validated head, and closure-validate the merged tree before mainline promotion.
