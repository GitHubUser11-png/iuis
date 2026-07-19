# Pass 5 — Academic Foundation Aggregates

## Objective

Implement the first coherent academic Domain aggregate group: Courses, Curriculum versions, Subjects and prerequisite graphs, Academic Period lifecycle, Enrollment review, and immutable Enrollment snapshots. Preserve the locked rule that Enrollment approval does not create a tuition Assessment; Finance performs that later transaction.

## Starting point

- repository: `GitHubUser11-png/iuis`
- original starting commit: `54633e7585b5b2341f3a615cc886ac6bc018afa0`
- implementation branch: `build/pass-05-academic-foundations`
- implementation pull request: `#11`
- final validation-fix pull request: `#14`

## Preflight integration repair

Before the new academic sources could be evaluated, PR #11 workflow run `29684436704` exposed syntax corruption in three pre-existing Pass 4 identity files after parallel histories were synchronized. The repair was isolated in PR #12 and validated independently before merge:

- restored one canonical `InstitutionIdentifier`;
- restored one canonical `UserAccount`;
- restored one canonical `UserSession`;
- removed duplicate uncompiled Student/Employee alternatives;
- Windows run `29684773571`: 0 warnings, 0 errors, 33 of 33 tests passed;
- repair integration commit: `0c400df71bbf9e956834af1afad09f28c64d6e0e`.

The preflight failure was not caused by Pass 5 academic behavior.

## Created Domain sources

- `Academic/AcademicEnums.cs`
- `Academic/AcademicUnitRules.cs`
- `Academic/Course.cs`
- `Academic/Curriculum.cs`
- `Academic/Subject.cs`
- `Academic/SubjectPrerequisiteGraph.cs`
- `Academic/AcademicPeriod.cs`
- `Academic/Enrollment.cs`

## Course and Curriculum contracts

- Course IDs use `CRS-YYYY-NNNNNN`.
- Course code is a normalized immutable business code; name, Department, duration, and lifecycle are versioned mutations.
- Curriculum IDs use `CUR-YYYY-NNNNNN`.
- Each Curriculum belongs to one Course and has an immutable version code and effective year.
- Curriculum Subject lines store Subject ID, year level, term, units, and required/optional state.
- Duplicate Subjects are rejected within one Curriculum version.
- Subject membership is editable only while Draft.
- Approval requires at least one Subject.
- Approved Curricula use explicit activation, supersession, and retirement transitions.

## Subject and prerequisite contracts

- Subject IDs use `SUB-YYYY-NNNNNN`.
- Subject code is immutable.
- Title and units are editable only while Draft or Inactive.
- Units use decimal values with at most two fractional digits.
- A Subject cannot require itself or contain duplicate prerequisite references.
- `SubjectPrerequisiteGraph.ValidateAcyclic` rejects duplicate nodes, missing references, and directed cycles using deterministic traversal.
- Completion-eligibility enforcement remains deferred until authoritative grade/result records exist.

## Academic Period lifecycle

One canonical `AcademicPeriodStatus` is used:

1. Draft
2. Scheduled
3. EnrollmentOpen
4. EnrollmentClosed
5. InProgress
6. Completed
7. Cancelled

The aggregate validates internal schedule ordering and permits only explicit lifecycle transitions. Cross-period overlap and parallel-period policy remain deferred to a future typed Application policy and system setting.

## Enrollment aggregate and snapshots

- Enrollment IDs use `ENR-YYYY-NNNNNN`.
- Student, Academic Period, Course, Curriculum, and Subject relationships use stable IDs.
- Enrollment captures immutable Course ID/code/name and Curriculum ID/version snapshots.
- Subject lines capture immutable Subject ID/code/title, units, year level, term, required/optional state, and optional Section code.
- Duplicate Subject lines are rejected.
- Subject lines are editable only while Draft or Returned for Correction.
- Submission requires at least one Subject line.
- Review and decision transitions are explicit: Draft, Submitted, UnderReview, ReturnedForCorrection, Approved, Rejected, Withdrawn, Cancelled, and Completed.
- Approved Enrollment contains no Assessment, charge, Payment, Scholarship, or ledger state. Finance creates Assessment later through a separate transaction.

## Existing aggregate correction

`StudentRecord.CourseId` now requires a canonical `CRS-YYYY-NNNNNN` identifier. The human-facing Course code remains separate as `Course.Code`.

## Test coverage

`AcademicFoundationTests.cs` covers:

- Course identifier, code normalization, and lifecycle;
- Curriculum unit totals, approval, duplicate rejection, and post-approval immutability;
- Subject self-prerequisite rejection;
- acyclic, cyclic, and missing-reference prerequisite graphs;
- Academic Period schedule and lifecycle validation;
- Enrollment submission, duplicate lines, immutable snapshots, review requirements, correction/resubmission, and approval;
- canonical Course linkage from Student records;
- academic-unit fractional precision.

The complete suite contains 54 tests after Pass 5.

## Compiler and test corrections

The first renewed merge-candidate run, `29684890491`, failed because `StudentRecord` referred to `StudentStatus.Unspecified`, while the canonical status enum uses `Applicant = 0`. The guard was corrected to accept every defined canonical status.

The next run, `29685010892`, compiled successfully but exposed one incorrect test expectation. A newly created Enrollment starts at version 1 and advances through Subject-line addition, submission, review, and approval, producing version 5. The test incorrectly expected version 4.

PR #14 corrected the assertion without changing production Domain behavior.

## Final Windows evidence

GitHub Actions run `29685193452` validated correction head `e7eabb3cd63d3345658a9c62972a81512575b1cf` against the merged Pass 5 source.

- source-tree and architecture validation: passed;
- NuGet restoration: passed;
- Release MSBuild: passed;
- warnings: `0`;
- errors: `0`;
- MSTest: passed;
- tests executed: `54`;
- tests passed: `54`;
- tests failed: `0`;
- TRX verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-47`;
- artifact ID: `8441849007`;
- SHA-256: `0ca4c821cc8383c63536d5e6e4d1335131082cdacc66793c20a5d69a67754fcf`;
- expiration: 2026-08-02.

## Integration result

- PR #11 merged the academic aggregate implementation.
- Pass 5 merge commit: `7dffa7498bf1efece00cffe417cc76b86c285547`.
- PR #14 merged the validated test correction.
- final validated integration commit: `6b5db90fce2691f7b76fbd5eed2731aa01179b82`.
- `develop` was recreated from the final validated `main` baseline after branch synchronization had removed it.

## Architecture boundary

All production additions belong to `IUIS.Domain`. Domain continues to reference only `System` and `System.Core`. It does not reference WinForms, file-system APIs, JSON serialization, Application, Infrastructure, SharedUI, UserApp, or AdminApp.

## Supporting diagram

An editable FigJam entity-relationship diagram exists at:

- `https://www.figma.com/board/0mEh0ffU2DFnixqZqYWSjD`

The diagram is explanatory only. GitHub source and automated tests are authoritative.

## Explicitly deferred

- repository-level uniqueness for Course, Subject, Curriculum-version, and Academic Period codes;
- cross-period overlap and parallel-period policy;
- grade/result records and completed-prerequisite eligibility;
- Section and Faculty Assignment aggregates;
- Application DTOs and use cases;
- JSON envelopes, repositories, and templates;
- sequence allocation, locks, transactions, audit, and Notifications;
- tuition Assessment generation and Finance integration;
- WinForms pages and DataGridView workflows.

## Closure result

Pass 5 is implemented, compile-verified, test-verified, and integrated. This is not release certification.
