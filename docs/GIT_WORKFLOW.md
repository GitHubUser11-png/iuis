# IUIS Git Workflow

## Protected branch intent

### `main`

Contains reviewed release-ready baselines. Direct feature development does not occur on `main` after repository bootstrap.

### `develop`

Receives completed implementation passes. Pass branches start from the current verified `develop` commit.

### `build/pass-*`

Contains one controlled implementation pass or subpass. Examples:

```text
build/pass-01-solution-foundation
build/pass-03-domain-foundations
build/pass-14-student-core
```

## Pass workflow

1. Refetch repository metadata.
2. Verify the exact repository full name.
3. Record the starting branch and commit SHA.
4. Create the pass branch from `develop`.
5. Create a coherent source set.
6. Commit with a scoped message.
7. Refetch the commit and representative files.
8. Compare the pass branch with `develop`.
9. Open a pull request into `develop`.
10. Run applicable Windows build and test checks.
11. Correct failures on the same pass branch.
12. Merge only after the pass gate is satisfied.
13. Update `docs/IMPLEMENTATION_STATE.md` with evidence-backed status.

## Commit messages

Use a short imperative subject with an implementation scope.

Examples:

```text
chore: establish repository governance baseline
build: create seven-project solution foundation
feat(domain): add canonical identity primitives
test(repository): verify production catalog mappings
fix(security): enforce token and security-stamp validation
```

Do not use messages that claim compilation, tests, or certification unless corresponding evidence exists.

## Pull-request requirements

Every implementation pull request must identify:

- pass or subpass;
- base commit;
- files created and modified;
- contracts implemented;
- contracts intentionally deferred;
- C# 7.3 and .NET Framework 4.8 considerations;
- build result;
- test result;
- security or data-integrity impact;
- exact unresolved items.

## Merge policy

- Prefer squash merge for a pass whose intermediate correction commits have no independent historical value.
- Use a merge commit when preserving multiple coherent commits is materially useful.
- Do not force-update `main` or `develop`.
- Do not merge a branch whose head moved after review without revalidation.
- Do not bypass a failing required check.

## Branch protection limitation

Repository files document the intended policy, but GitHub branch-protection settings are repository administration controls rather than source files. Until branch protection is explicitly configured, the workflow is enforced operationally and every merge must still follow this document.

## Source-of-truth rule

GitHub branch contents are authoritative for implementation state. Conversation specifications remain requirements and rationale. Local, generated, or uncommitted files are not treated as implemented until committed and refetched from the expected branch.
