# Pass 8 Recovery and Replacement Pull-Request Integration

## Objective

Recover the exact Windows-validated Pass 8 implementation after PR #27 was closed without merge, integrate it through a replacement pull request, and preserve a verifiable path to post-merge closure validation.

## Recovery source

- repository: `GitHubUser11-png/iuis`
- Pass 7 authoritative baseline: `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd`
- closed predecessor pull request: `#27`
- validated Pass 8 implementation commit: `c622287893a2e13011eeae9674fda785c8525f44`
- recovered branch: `build/pass-08-recovery-integration-final`
- replacement pull request: `#28`
- replacement validated head: `28c797f7a0c2d453852728fc27d12214cbfdd84e`
- integrated `develop` commit: `4920f3dec56e584bb3d4aa620b194b5dd31c36ce`

PR #27 was closed without merge after repository synchronization removed the implementation branch. The validated commit remained reachable and was used directly for recovery. The production implementation was not reconstructed or rewritten.

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

Three failed Windows runs were used to correct the implementation before successful validation:

1. run `29691283953` identified an identifier-sequence integer type mismatch;
2. run `29691378777` identified incompatible test-project restore configuration;
3. run `29691481045` identified missing `System.Text.Json` runtime dependencies in the test output.

## Original implementation validation

Run `29691593386` passed against commit `c622287893a2e13011eeae9674fda785c8525f44`:

- source-tree, architecture, and exact 49-template validation: passed;
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
- SHA-256: `d79a4789460820da807e3147ede3edef6a754a490af14f158fa4c9b45f84d0ca`.

## Replacement pull-request validation

Run `29692698528` passed against recovered head `28c797f7a0c2d453852728fc27d12214cbfdd84e`:

- source-tree, architecture, and exact 49-template validation: passed;
- NuGet restoration: passed;
- Release MSBuild: passed;
- compiler warnings: `0`;
- compiler errors: `0`;
- MSTest: `110` executed, `110` passed, `0` failed;
- TRX verification: passed;
- artifact publication: passed.

Artifact:

- name: `iuis-windows-build-evidence-90`;
- artifact ID: `8444064263`;
- SHA-256: `8955298d4693623d585e62bdae83750106d74a269d5aa0c488b8d6436bc931d3`;
- expiration: 2026-08-02.

## Integration result

PR #28 merged the recovered implementation into `develop` as commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce`.

Recovery and replacement-pull-request integration are complete. Pass 8 is integrated into `develop`, but closure and mainline promotion require independent validation of the actual merged tree.

## Evidence boundary

This recovery does not itself claim Pass 8 closure or release certification. Complete typed repositories for every aggregate, Application authorization orchestration, business-module Forms, operational backup/restore execution, deployment, and release certification remain deferred.

## Exact next gate

Validate `build/pass-08-closure` created directly from integration commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce`, merge the validated closure record into `develop`, promote the resulting baseline to `main`, validate the exact mainline integration commit, and synchronize `main` and `develop`.