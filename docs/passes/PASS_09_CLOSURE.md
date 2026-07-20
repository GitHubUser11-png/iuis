# Pass 9 Closure — Integrated-Tree Validation and Mainline Promotion

## Objective

Close Pass 9 from the actual merged integration commit, eliminate the remaining transaction revision race, classify every aggregate adapter honestly, validate the complete Windows tree, promote the result to `main`, synchronize `develop`, and establish the next construction boundary only after synchronization.

## Starting point

- Pass 9 integration commit: `2b7b629889523a00d54d8e699f705e1ecc4f8358`
- closure branch: `build/pass-09-closure`
- source hardening commit: `f8e9df4abaef995c9fd65225e9d135dbaec37551`
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

## Required validation gate

The closure pull request must prove:

- exactly 49 production templates;
- all seven project boundaries;
- NuGet restoration;
- .NET Framework 4.8 Release compilation;
- zero compiler warnings;
- zero compiler errors;
- 127 of 127 tests passing;
- valid TRX output; and
- closure artifact publication.

## Promotion sequence

1. Validate the exact closure PR head.
2. Merge the closure into `develop`.
3. Open and validate a `develop` to `main` promotion pull request.
4. Merge the validated promotion.
5. Validate the exact resulting mainline tree.
6. Synchronize `develop` to the final `main` commit.
7. Verify ahead `0` and behind `0`.
8. Define and register the exact Pass 10 boundary.

## Evidence status

Pending closure workflow, artifact registration, merge, promotion, exact-mainline validation, and final synchronization.

## Evidence boundary

Pass 9 closure does not claim that the 18 production aggregate adapters can all hydrate current Domain aggregates. The readiness catalog is an enforcement record, not an implementation substitute. Specialized mappers, production Forms, trusted Administrator environment enforcement, operational backup/restore execution, deployment packaging, and release certification remain outside Pass 9 closure.
