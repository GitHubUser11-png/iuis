# Pass 8 Recovery and Replacement Pull-Request Integration

## Objective

Recover the exact Windows-validated Pass 8 implementation after PR #27 was closed without merge, integrate it through a replacement pull request, and preserve a verifiable path to post-merge closure validation.

## Recovery source

- repository: `GitHubUser11-png/iuis`
- Pass 7 authoritative baseline: `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd`
- closed predecessor pull request: `#27`
- validated Pass 8 implementation commit: `c622287893a2e13011eeae9674fda785c8525f44`
- recovered branch: `build/pass-08-recovery-integration-final`
- target branch: `develop`

PR #27 was closed without merge after the implementation branch was removed by repository synchronization. The validated commit remained reachable and was used directly for recovery. The production implementation was not reconstructed or rewritten.

## Validated implementation recovered

The recovered commit contains:

- exactly 49 production repository descriptors: 14 principal and 35 supporting;
- all 49 initial JSON envelope templates;
- schema and revision validation;
- central ID sequence allocation;
- deterministic cross-process locks;
- hardened same-directory atomic writes;
- journaled multi-file transaction coordination and recovery;
- login-attempt persistence and temporary lockout;
- PBKDF2-HMAC-SHA256 password hashing;
- restricted first-login sessions and forced password change;
- one-time production bootstrap without a built-in credential;
- 14 Infrastructure tests, producing 110 total tests;
- source-tree enforcement for the exact 49-template contract.

## Diagnostic history

Three failed Windows runs were used to correct the implementation before the successful validation:

1. run `29691283953` identified an identifier-sequence integer type mismatch;
2. run `29691378777` identified incompatible test-project restore configuration;
3. run `29691481045` identified missing `System.Text.Json` runtime dependencies in the test output.

The corrected final implementation then passed run `29691593386` against commit `c622287893a2e13011eeae9674fda785c8525f44`.

## Successful implementation evidence

Run `29691593386` completed successfully:

- source-tree and architecture validation: passed;
- 49-template validation: passed;
- NuGet restoration: passed;
- Release MSBuild: passed;
- compiler warnings: `0`;
- compiler errors: `0`;
- MSTest: `110` executed, `110` passed, `0` failed;
- TRX verification: passed;
- artifact publication: passed.

Artifact:

- name: `iuis-windows-build-evidence-88`;
- artifact ID: `8443750845`;
- SHA-256: `d79a4789460820da807e3147ede3edef6a754a490af14f158fa4c9b45f84d0ca`;
- expiration: 2026-08-02.

## Replacement pull-request gate

The recovered documentation head must independently pass the complete Windows workflow before merge. After merge into `develop`, an independently created closure branch must validate the actual merged tree before promotion to `main`.

## Evidence boundary

This recovery does not claim Pass 8 closure or release certification. Complete typed repositories for every aggregate, Application authorization orchestration, business-module Forms, operational backup/restore execution, deployment, and release certification remain deferred.

## Exact next gate

Open the replacement pull request against `develop`, complete final-head Windows validation, merge the validated implementation, then create and validate the Pass 8 integrated-tree closure branch.
