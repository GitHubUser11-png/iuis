# Pass 9 Closure — Integrated-Tree Validation and Mainline Promotion

## Objective

Close Pass 9 from the actual merged integration commit, eliminate the remaining transaction revision race, classify every aggregate adapter honestly, validate the complete Windows tree, promote the result to `main`, synchronize `develop`, and establish the next construction boundary only after synchronization.

## Starting point

- Pass 9 integration commit: `2b7b629889523a00d54d8e699f705e1ecc4f8358`
- closure branch: `build/pass-09-closure`
- source hardening commit: `f8e9df4abaef995c9fd65225e9d135dbaec37551`
- first closure validation head: `85a075d79f86d8596bd950136ab4e2ba9a81b5e2`
- closure pull request: `#38`
- target branch: `develop`

## Closure changes

### In-lock expected revision revalidation

Application-staged transaction mutations now carry their expected repository revision. After canonical repository and journal locks are acquired, `JournaledTransactionCoordinator` verifies:

1. the authoritative target exists;
2. its envelope is structurally valid;
3. its current revision equals the staged expected revision;
4. the staged envelope repository name matches the target; and
5. the staged revision equals expected revision plus one.

Any mismatch aborts before backup creation, journal preparation, or target replacement.

### Deterministic stale-stage test

The Application transaction test pauses a mapper after its initial unlocked revision read. A second writer commits a newer revision before the transaction proceeds. The transaction then reaches the locked revision gate, fails with a revision conflict, and leaves the concurrently committed value and revision intact.

### Aggregate mapper readiness

All 18 production adapters are recorded in `AggregateMapperReadinessCatalog` as either `RequiresSpecializedMapper` or `DeferredWithExplicitReason`. No production adapter is claimed as generic-compatible or specialized-complete without a production hydration implementation.

## Independent closure validation

GitHub Actions run `29717369189` validated closure head `85a075d79f86d8596bd950136ab4e2ba9a81b5e2`.

- source-tree and architecture validation: passed;
- exactly 49 production templates: passed;
- seven-project boundary validation: passed;
- NuGet restoration: passed;
- .NET Framework 4.8 Release compilation: passed;
- compiler warnings: `0`;
- compiler errors: `0`;
- MSTest: `127` executed, `127` passed, `0` failed;
- TRX verification: passed;
- artifact publication: passed.

Closure artifact:

- name: `iuis-windows-build-evidence-114`;
- artifact ID: `8451078816`;
- SHA-256: `110e821dece6f998fabedfffda21df7d558fa309decf55c31addea05fccef721`;
- expiration: 2026-08-03.

The artifact contains `build-summary.json`, the Release build log and binary log, the MSTest log, the TRX result, and source-tree validation evidence. The Release log reports `0 Warning(s)` and `0 Error(s)`. The TRX counters are total `127`, executed `127`, passed `127`, failed `0`.

## Promotion sequence

1. Validate the evidence-updated final closure PR head.
2. Merge the closure into `develop`.
3. Open and validate a `develop` to `main` promotion pull request.
4. Merge the validated promotion.
5. Validate the exact resulting mainline tree.
6. Synchronize `develop` to the final `main` commit.
7. Verify ahead `0` and behind `0`.
8. Define and register the exact Pass 10 boundary.

## Evidence status

The first independent closure workflow is successful. Final evidence-updated-head validation, closure merge, promotion, exact-mainline validation, final synchronization, and Pass 10 boundary registration remain pending.

## Evidence boundary

Pass 9 closure does not claim that the 18 production aggregate adapters can all hydrate current Domain aggregates. The readiness catalog is an enforcement record, not an implementation substitute. Specialized mappers, production Forms, trusted Administrator environment enforcement, operational backup/restore execution, deployment packaging, and release certification remain outside Pass 9 closure.
