# Pass 8 — Integrated-Tree Closure

## Objective

Independently validate the actual Pass 8 tree merged into `develop`, register closure evidence, promote the validated baseline to `main`, synchronize `main` and `develop`, and establish the exact Pass 9 construction boundary.

## Starting point

- repository: `GitHubUser11-png/iuis`
- Pass 8 replacement integration PR: `#28`
- integrated `develop` commit: `4920f3dec56e584bb3d4aa620b194b5dd31c36ce`
- closure branch: `build/pass-08-closure`
- predecessor PR #27: closed without merge
- validated implementation commit: `c622287893a2e13011eeae9674fda785c8525f44`
- replacement-PR validated head: `28c797f7a0c2d453852728fc27d12214cbfdd84e`

## Implementation evidence already established

### Original implementation validation

- run: `29691593386`
- validated commit: `c622287893a2e13011eeae9674fda785c8525f44`
- result: successful
- warnings: `0`
- errors: `0`
- tests: `110` executed, `110` passed, `0` failed
- artifact: `iuis-windows-build-evidence-88`
- artifact ID: `8443750845`
- SHA-256: `d79a4789460820da807e3147ede3edef6a754a490af14f158fa4c9b45f84d0ca`

### Replacement pull-request validation

- pull request: `#28`
- run: `29692698528`
- validated head: `28c797f7a0c2d453852728fc27d12214cbfdd84e`
- result: successful
- warnings: `0`
- errors: `0`
- tests: `110` executed, `110` passed, `0` failed
- artifact: `iuis-windows-build-evidence-90`
- artifact ID: `8444064263`
- SHA-256: `8955298d4693623d585e62bdae83750106d74a269d5aa0c488b8d6436bc931d3`
- expiration: 2026-08-02

## Integrated baseline under closure validation

The merged tree contains:

- exactly 49 production repository descriptors: 14 principal and 35 supporting;
- exactly 49 initial JSON templates;
- schema-versioned repository envelopes and optimistic revision checks;
- central identifier allocation through `id_sequences.json`;
- deterministic cross-process locks;
- hardened same-directory atomic writes with SHA-256 verification;
- journaled multi-file transaction coordination, rollback, and recovery;
- login-attempt recording and five-failure temporary lockout;
- PBKDF2-HMAC-SHA256 password hashing;
- restricted first-login sessions and forced password replacement;
- one-time production bootstrap without a built-in credential;
- 14 Infrastructure tests and 96 pre-existing tests.

## Closure validation gate

The closure head must pass:

1. source-tree and architecture validation;
2. exact 49-template validation;
3. NuGet restoration;
4. .NET Framework 4.8 Release MSBuild;
5. zero compiler warnings and zero compiler errors;
6. all 110 MSTest cases;
7. TRX production and verification;
8. closure artifact publication.

After successful closure validation, this record must be merged into `develop`. The resulting `develop` baseline must then be promoted to `main`, and the exact mainline integration commit must receive Windows validation before final branch synchronization.

## Figma closure model

Editable closure and promotion diagram:

- `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

The diagram is explanatory. GitHub source and machine-generated validation evidence remain authoritative.

## Evidence boundary

Pass 8 closure does not constitute release certification. Complete typed repositories, Application authorization and permission orchestration, restricted DTO projection, business-module Forms, operational backup and restore execution, deployment packaging, and final security/recovery certification remain incomplete.

## Exact Pass 9 starting point

# Integrated University Information System Build Execution — Pass 9: Application Authorization and Permission Resolution, Typed Repository Adapters, Restricted DTO Projection, Session-Aware Command and Query Orchestration, Student Own-Record Enforcement, Employee Permission Boundaries, Expanded Application and Infrastructure Tests, Windows Release Compilation, Figma Application-Service Model, and Pull Request Integration

## Pass 9 objectives

1. Implement Application-layer authorization using authenticated role, active permission profiles, direct grants, direct restrictions, session application kind, session purpose, ownership, and confidentiality classification.
2. Enforce restrictions over grants and prevent Administrator status from becoming a universal confidentiality bypass.
3. Add typed repository contracts and Infrastructure adapters for the Domain aggregates implemented through Pass 7.
4. Create separate internal, released, and Student-own-record DTO projections.
5. Derive Student ownership from the authenticated session rather than editable identifiers.
6. Validate session status, expiration, Security Stamp, purpose, permission, ownership, and expected repository revision for every command and query.
7. Route related mutations through the Pass 8 journaled transaction coordinator.
8. Add tests for permission resolution, ownership denial, restricted DTO exclusion, session restrictions, repository round trips, optimistic conflicts, and multi-repository orchestration.

## Current closure state

The closure branch and documentation exist. Independent integrated-tree Windows validation, closure merge, mainline promotion, exact mainline validation, and final branch synchronization remain pending.