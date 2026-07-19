# Pass 5 — Academic Foundation Aggregates

## Objective

Implement the first coherent academic Domain aggregate group: Courses, Curriculum versions, Subjects and prerequisite graphs, Academic Period lifecycle, Enrollment review, and immutable Enrollment snapshots. Preserve the locked rule that Enrollment approval does not create a tuition Assessment; Finance performs that later transaction.

## Starting point

- repository: `GitHubUser11-png/iuis`
- base branch: `develop`
- original starting commit: `54633e7585b5b2341f3a615cc886ac6bc018afa0`
- pass branch: `build/pass-05-academic-foundations`
- Pass 4 identity and person aggregates were already present at branch creation

## Preflight integration repair

Before the new academic sources could be evaluated, PR #11 workflow run `29684436704` exposed syntax corruption in three pre-existing Pass 4 identity files on the recreated integration base. Two parallel Pass 4 histories had been synchronized through `main`, and Git auto-merged overlapping class implementations into malformed concatenated source.

The repair was isolated in PR #12 and validated independently before merge:

- restored one canonical `InstitutionIdentifier`;
- restored one canonical `UserAccount`;
- restored one canonical `UserSession`;
- removed duplicate uncompiled Student/Employee alternatives;
- Windows run `29684773571`: 0 warnings, 0 errors, 33 of 33 tests passed;
- repair integration commit: `0c400df71bbf9e956834af1afad09f28c64d6e0e`.

PR #11 is therefore evaluated against the repaired `develop` baseline. The preflight failure was not caused by Pass 5 academic behavior.

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

- Course IDs use the canonical `CRS-YYYY-NNNNNN` format.
- Course code is a normalized immutable business code; Course name, Department, duration, and lifecycle remain versioned mutations.
- Curriculum IDs use `CUR-YYYY-NNNNNN`.
- Each Curriculum belongs to one Course and has an immutable version code and effective year.
- Curriculum Subject lines store Subject ID, year-level, term, units, and required/optional state.
- Duplicate Subjects are rejected within one Curriculum version.
- Curriculum Subject membership is editable only while Draft.
- Approval requires at least one Subject; Approved Curricula can be activated, superseded, or retired through explicit transitions.

## Subject and prerequisite contracts

- Subject IDs use `SUB-YYYY-NNNNNN`.
- Subject code is immutable; title and units are editable only while Draft or Inactive.
- Subject units use decimal values with at most two fractional digits.
- A Subject cannot require itself and cannot contain duplicate prerequisite references.
- `SubjectPrerequisiteGraph.ValidateAcyclic` verifies duplicate nodes, missing references, and directed cycles with deterministic traversal.
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

The aggregate validates internal schedule ordering and permits only explicit lifecycle transitions. Cross-period overlap and parallel-period policy remain deferred to the future Application policy and typed system setting because no authoritative parallel-period rule is yet implemented.

## Enrollment aggregate and snapshots

- Enrollment IDs use `ENR-YYYY-NNNNNN`.
- Student, Academic Period, Course, Curriculum, and Subject links use stable canonical IDs.
- Enrollment captures immutable Course ID/code/name and Curriculum ID/version snapshots.
- Enrollment Subject lines capture immutable Subject ID/code/title, units, year-level, term, required/optional state, and optional Section code.
- Duplicate Subject lines are rejected.
- Subject lines are editable only while Draft or Returned for Correction.
- Submission requires at least one Subject line.
- Review and decisions use explicit transitions: Draft, Submitted, UnderReview, ReturnedForCorrection, Approved, Rejected, Withdrawn, Cancelled, and Completed.
- Approved Enrollment contains no Assessment, charge, payment, scholarship, or ledger state. Finance must create Assessment in a later multi-file transaction.

## Existing aggregate correction

`StudentRecord.CourseId` now requires a canonical `CRS-YYYY-NNNNNN` identifier. The previous arbitrary `CRS-BSIT` fixture was replaced with a stable Course ID. The human-facing Course code remains a separate `Course.Code` value.

## Tests

`AcademicFoundationTests.cs` adds coverage for:

- Course identifier, code normalization, and lifecycle;
- Curriculum unit totals, approval, duplicate rejection, and post-approval immutability;
- Subject self-prerequisite rejection;
- acyclic, cyclic, and missing-reference prerequisite graphs;
- Academic Period schedule and lifecycle validation;
- Enrollment submission, duplicate lines, immutable snapshots, review requirements, correction/resubmission, and approval;
- canonical Course linkage from Student records;
- academic-unit fractional precision.

The existing Domain and structural tests remain active.

## Architecture boundary

All production additions belong to `IUIS.Domain`. Domain continues to reference only `System` and `System.Core`. It does not reference WinForms, file-system APIs, JSON serialization, Application, Infrastructure, SharedUI, UserApp, or AdminApp.

## Supporting diagram

An editable FigJam entity-relationship diagram was generated for the Pass 5 academic model:

- `https://www.figma.com/board/0mEh0ffU2DFnixqZqYWSjD`

The diagram is explanatory only. The GitHub source and automated tests remain authoritative.

## Explicitly deferred

- repository-level uniqueness for Course, Subject, Curriculum-version, and Academic Period codes;
- cross-period overlap and parallel-period policy;
- grade/result records and completed-prerequisite eligibility;
- Section and faculty assignment aggregates;
- Application DTOs and use cases;
- JSON envelopes, repositories, and templates;
- sequence allocation, locks, transactions, audit, and notifications;
- tuition Assessment generation and Finance integration;
- WinForms pages and DataGridView workflows.

## Evidence gate

The final documented branch head must pass Windows source-tree validation, NuGet restoration, Release MSBuild, MSTest discovery and execution, TRX verification, and artifact publication against the repaired `develop` baseline before PR #11 is merged.
