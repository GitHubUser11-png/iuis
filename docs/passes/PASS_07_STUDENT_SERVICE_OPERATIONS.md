# Pass 7 — Student Service Operations Domain Foundations

## Objective

Implement the coherent Student Service Operations Domain group without introducing persistence, file-system access, WinForms, authorization orchestration, or cross-process transaction code. The scope covers Library inventory and Borrowings, Counseling appointments and confidential sessions with controlled releases, Discipline incident and Violation workflows, Clinic appointments and append-only Medical Records, and Clinic Medical Clearance issuance and revocation history.

## Starting point and integration history

- repository: `GitHubUser11-png/iuis`
- original starting commit: `4ec4036452bb89d9b919d25aac8c20e844294ef8`
- original implementation branch: `build/pass-07-student-service-operations`
- original implementation pull request: `#21`, closed without merge after branch synchronization
- recovered implementation branch: `build/pass-07-student-service-operations-v2`
- synchronization pull request: `#22`
- replacement integration pull request: `#23`
- integration commit: `b4cb980d3989160969a02b4b5a51a162a088d695`
- recreated integration branch: `develop`

The repository automation removed the original implementation branch after a formatting-only mainline commit was added. The validated implementation commit remained available, was recovered on the replacement branch, synchronized with current `main`, revalidated, and integrated through PR #23.

## Created Domain sources

- `Services/ServiceDomainGuard.cs`
- `Library/LibraryModels.cs`
- `Counseling/CounselingModels.cs`
- `Discipline/DisciplineModels.cs`
- `Clinic/ClinicModels.cs`

## Library inventory and Borrowing

- Book IDs use `LBK-YYYY-NNNNNN`.
- Copy IDs use `LCP-YYYY-NNNNNN`.
- Borrowing IDs use `BRW-YYYY-NNNNNN`.
- Library inventory tracks Available, On Loan, Maintenance, and Lost copies.
- The aggregate enforces `TotalCopies = AvailableCopies + ActiveBorrowedCopies + MaintenanceCopies + LostCopies`.
- Copies move through explicit issue, return, maintenance, and lost operations.
- Borrowings support Prepared, Issued, Overdue, Returned, Lost, and Cancelled states.
- Renewal requires a later due date and returns an overdue Borrowing to Issued state.
- Librarian actor identifiers are recorded; Librarian role enforcement remains an Application responsibility.

## Counseling cases and controlled releases

- Counseling Case IDs use `CNS-YYYY-NNNNNN`.
- Session, Released Summary, and Release Authorization IDs use `CSN`, `CSR`, and `CRL` prefixes.
- Case lifecycle is Requested, Confirmed, Assigned, Active, Closed, or Cancelled.
- A counselor must be assigned before a confidential session is recorded.
- Confidential session records and released summaries are separate Domain types and collections.
- Release authorizations permit only named released-artifact scopes, use explicit validity windows, and may be revoked.
- No release scope grants internal-note access.

## Discipline incidents and responses

- Discipline Case and Violation IDs use `DIN` and `VIO` prefixes.
- Evidence, Notice, Response, Finding, and Decision records use `DEV`, `DNT`, `DSR`, `DFN`, and `DDC` prefixes.
- Lifecycle covers intake, review, Violation conversion, notice release, Student response, findings, decision preparation, decision release, closure, and dismissal.
- Investigation narrative, evidence, findings, and decision rationale remain distinct from released notice and decision summaries.
- A decision cannot be prepared without a recorded finding.

## Clinic appointments and Medical Records

- Clinic Appointment, Consultation, Medical Record, and Released Summary IDs use `CAP`, `CON`, `MDR`, and `MRS` prefixes.
- Appointment lifecycle is Requested, Scheduled, Confirmed, Checked In, Completed, Cancelled, or No Show.
- Appointment records contain operational and released reason information only.
- Medical Records separate confidential Consultations from released summaries.
- Medical Records are retained and cannot be archived.
- One active Medical Record per Student remains a repository-level uniqueness invariant.

## Clinic Medical Clearance workflow

- Medical Clearance, Clearance Number, and history IDs use `MCL`, `MCN`, and `MCH` prefixes.
- Lifecycle is Requested, Under Review, Issued, Denied, or Revoked.
- Issuance requires prior clinical review.
- Issued Clearances contain released summaries rather than confidential Medical notes.
- Revocation retains the original Clearance Number and complete history.
- Clearance Number uniqueness remains a repository responsibility.
- This is a Clinic Medical Clearance workflow, not a university-wide multi-office clearance chain.

## Test coverage

`StudentServiceDomainTests.cs` adds 24 tests. Combined with the existing suite, Pass 7 contains 96 tests covering Library inventory and Borrowings, Counseling confidentiality and release controls, Discipline workflows, Clinic Appointments, Medical Record retention, and Medical Clearance issuance and revocation.

## Supporting FigJam model

- `https://www.figma.com/board/2DiMHY2V9ZlTMh3TUFYvXW`

The diagram is explanatory only. GitHub source and automated test evidence are authoritative.

## Windows validation evidence

### Production implementation validation

Run `29688650226` validated implementation head `6d55b5d86856627839c5e3560f70fa9b2df7a770`.

- warnings: `0`;
- errors: `0`;
- tests: `96` executed, `96` passed, `0` failed;
- TRX verification and artifact publication: passed.

Artifact:

- `iuis-windows-build-evidence-67`;
- artifact ID `8442878636`;
- SHA-256 `c4d3090e74b3686125bee1ff82b117606b3e0a556a7225a7b0a4cb8b4f11e432`.

### Final replacement-PR validation

Run `29688849862` validated final PR head `3b363a0b627c27a6c236c4ab800f694ce09c3209`.

- source-tree and architecture validation: passed;
- NuGet restoration: passed;
- Release MSBuild: passed;
- warnings: `0`;
- errors: `0`;
- MSTest: `96` executed, `96` passed, `0` failed;
- TRX verification and artifact publication: passed.

Artifact:

- `iuis-windows-build-evidence-69`;
- artifact ID `8442932025`;
- SHA-256 `2521ad970f4fd181b64bbaee012d507de14aa813b2d872056479b35585f02ab2`.

## Architecture boundary

All production additions belong to `IUIS.Domain`. Domain continues to reference only `System` and `System.Core`. It does not reference WinForms, file-system APIs, JSON serialization, Application, Infrastructure, SharedUI, UserApp, or AdminApp.

## Explicitly deferred

- role and permission enforcement;
- restricted DTO construction and Student own-record filtering;
- repository uniqueness for ISBNs, barcodes, active Medical Records, and Clearance Numbers;
- service-operation JSON repositories and production templates;
- central sequence allocation, cross-process locks, atomic writes, and transaction journals;
- notification and document dispatch;
- Student and employee WinForms pages;
- backup, restore, recovery, deployment, and release certification.

## Integration state

Pass 7 is implemented, compile-verified, test-verified, and integrated through PR #23. Independent closure validation remains pending. This is not release certification.
