# Pass 8 — Integrated-Tree Closure

## Objective

Independently validate the actual Pass 8 tree merged into `develop`, register closure evidence, promote the validated baseline to `main`, synchronize `main` and `develop`, and establish the exact Pass 9 construction boundary.

## Integration and promotion history

- Pass 8 predecessor PR: `#27` — closed without merge
- replacement implementation PR: `#28`
- integrated implementation commit: `4920f3dec56e584bb3d4aa620b194b5dd31c36ce`
- closure PR: `#30`
- closure integration commit: `60dffbba76509a658f0ca87ca3c2862ec5d1b641`
- mainline promotion PR: `#31`
- mainline promotion commit: `a4db81b8c02297bc1ff7707c4b2023d2787bf8f4`
- evidence-only mainline validation PR: `#32`, closed without merge

## Successful validation history

| Stage | Run | Head/tree | Artifact | ID | SHA-256 |
|---|---:|---|---|---:|---|
| Original implementation | `29691593386` | `c622287893a2e13011eeae9674fda785c8525f44` | `iuis-windows-build-evidence-88` | `8443750845` | `d79a4789460820da807e3147ede3edef6a754a490af14f158fa4c9b45f84d0ca` |
| Replacement PR | `29692698528` | `28c797f7a0c2d453852728fc27d12214cbfdd84e` | `iuis-windows-build-evidence-90` | `8444064263` | `8955298d4693623d585e62bdae83750106d74a269d5aa0c488b8d6436bc931d3` |
| Integrated-tree closure | `29693351179` | `525c4db77f9aec30b9d586c898a2acb0a9d62140` | `iuis-windows-build-evidence-93` | `8444253055` | `18b78f3e2d44592e9f9b73dd96522ada56a728afacea76fd4d5faafc21aa9e11` |
| Final closure head | `29693457219` | `5112765594d37957e11683ac01de40e6c3d98f30` | `iuis-windows-build-evidence-95` | `8444287182` | `9627ed324cb1fada54a0f104d494df5fc8a9f4cf1bf287b9367128179c10b818` |
| Promotion PR | `29693549723` | `60dffbba76509a658f0ca87ca3c2862ec5d1b641` | `iuis-windows-build-evidence-97` | `8444309381` | `6213f0fe180be2b23a7f52750ec4f33c98d1e79180c6a2a10d9a367a4c20823d` |
| Exact mainline tree | `29693704636` | `a4db81b8c02297bc1ff7707c4b2023d2787bf8f4` | `iuis-windows-build-evidence-99` | `8444355210` | `73b586dc6486cf0e55fbde4dccb6c3590dd0b6a001ab26eebc8897939ba4e1ce` |

Every successful run passed source-tree and architecture validation, exact 49-template validation, NuGet restoration, .NET Framework 4.8 Release MSBuild, zero compiler warnings, zero compiler errors, all 110 MSTest cases, TRX verification, and artifact publication.

## Validated Pass 8 baseline

The promoted and validated tree contains:

- exactly 49 production repository descriptors: 14 principal and 35 supporting;
- exactly 49 initial JSON templates;
- schema-versioned envelopes and optimistic revision checks;
- central ID allocation through `id_sequences.json`;
- deterministic cross-process locks;
- hardened atomic writes with SHA-256 staging verification;
- journaled multi-file transaction coordination, rollback, and recovery;
- login-attempt recording and five-failure temporary lockout;
- PBKDF2-HMAC-SHA256 credential hashing;
- restricted first-login sessions and forced password replacement;
- one-time production bootstrap without a built-in credential;
- 110 passing tests.

## Figma closure model

- `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

The diagram is explanatory. GitHub source and machine-generated validation evidence remain authoritative.

## Evidence boundary

Pass 8 closure does not constitute release certification. Complete typed repositories, Application authorization and permission orchestration, restricted DTO projection, business-module Forms, operational backup and restore execution, deployment packaging, and final security/recovery certification remain incomplete.

## Exact Pass 9 starting point

# Integrated University Information System Build Execution — Pass 9: Application Authorization and Permission Resolution, Typed Repository Adapters, Restricted DTO Projection, Session-Aware Command and Query Orchestration, Student Own-Record Enforcement, Employee Permission Boundaries, Expanded Application and Infrastructure Tests, Windows Release Compilation, Figma Application-Service Model, and Pull Request Integration

## Pass 9 objectives

1. Implement Application-layer authorization using role, active permission profiles, direct grants, direct restrictions, session application kind, session purpose, ownership, and confidentiality classification.
2. Enforce restrictions over grants and prevent Administrator status from becoming a universal confidentiality bypass.
3. Add typed repository contracts and Infrastructure adapters for Domain aggregates implemented through Pass 7.
4. Create separate internal, released, and Student-own-record DTO projections.
5. Derive Student ownership from the authenticated session.
6. Validate session status, expiration, Security Stamp, purpose, permission, ownership, and expected revision for every command/query.
7. Route related mutations through the Pass 8 transaction coordinator.
8. Add tests for permission resolution, ownership denial, DTO exclusion, session restrictions, repository round trips, optimistic conflicts, and multi-repository orchestration.

## Closure result

Pass 8 implementation, replacement integration, integrated-tree closure, mainline promotion, and exact mainline-tree validation are complete. Final documentation integration and final `main`/`develop` synchronization remain the administrative finishing steps. This is not release certification.