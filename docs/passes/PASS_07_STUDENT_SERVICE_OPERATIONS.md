# Pass 7 — Student Service Operations Domain Foundations

## Objective

Implement the coherent Student Service Operations Domain group without introducing persistence, file-system access, WinForms, authorization orchestration, or cross-process transaction code. The scope covers Library inventory and Borrowings, Counseling appointments and confidential sessions with controlled releases, Discipline incident and Violation workflows, Clinic appointments and append-only Medical Records, and Clinic Medical Clearance issuance and revocation history.

## Starting point

- repository: `GitHubUser11-png/iuis`
- original starting commit: `4ec4036452bb89d9b919d25aac8c20e844294ef8`
- original implementation branch: `build/pass-07-student-service-operations`
- recovered integration branch: `build/pass-07-student-service-operations-v2`
- original implementation pull request: `#21`
- synchronization pull request: `#22`
- Passes 1 through 6 were integrated and closure-validated before branch creation

## Branch synchronization note

The repository automation removed the original implementation branch and closed PR #21 after a formatting-only mainline commit was added. The validated implementation commit `6d55b5d86856627839c5e3560f70fa9b2df7a770` remained available. The implementation was recovered on `build/pass-07-student-service-operations-v2`, and PR #22 merged current mainline commit `e0f514888570b5b7d82a963592a8ede0b5d2e106` into that branch. The synchronization changed formatting only and did not alter Pass 7 Domain behavior.

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
- Librarian actor identifiers are recorded for issue, return, renewal, cancellation, and lost-recording operations.
- Librarian permission enforcement remains an Application authorization responsibility.

## Counseling cases and controlled releases

- Counseling Case IDs use `CNS-YYYY-NNNNNN`.
- Session IDs use `CSN-YYYY-NNNNNN`.
- Released Summary IDs use `CSR-YYYY-NNNNNN`.
- Release Authorization IDs use `CRL-YYYY-NNNNNN`.
- Case lifecycle is Requested, Confirmed, Assigned, Active, Closed, or Cancelled.
- A counselor must be assigned before a confidential session can be recorded.
- Confidential session records and released summaries are separate Domain types and collections.
- Release authorizations permit only named released-artifact scopes; no scope grants internal-note access.
- Authorizations use explicit validity windows and revocation.
- Student-facing and external DTO filtering remains an Application responsibility.

## Discipline incidents and responses

- Discipline Case IDs use `DIN-YYYY-NNNNNN`.
- Violation IDs use `VIO-YYYY-NNNNNN`.
- Evidence, Notice, Response, Finding, and Decision records use `DEV`, `DNT`, `DSR`, `DFN`, and `DDC` prefixes.
- Lifecycle covers intake, review, conversion to Violation, notice release, Student response, findings, decision preparation, decision release, closure, and dismissal.
- Investigation narrative, evidence, findings, and decision rationale remain distinct from released notice and decision summaries.
- A decision cannot be prepared without at least one finding.
- Closed and dismissed cases cannot re-enter investigation workflows.

## Clinic appointments and Medical Records

- Clinic Appointment IDs use `CAP-YYYY-NNNNNN`.
- Consultation IDs use `CON-YYYY-NNNNNN`.
- Medical Record IDs use `MDR-YYYY-NNNNNN`.
- Released Medical Summary IDs use `MRS-YYYY-NNNNNN`.
- Appointment lifecycle is Requested, Scheduled, Confirmed, Checked In, Completed, Cancelled, or No Show.
- Appointment records contain operational and released reason information only; confidential clinical content is stored in Medical Records.
- Medical Records separate confidential Consultation records from released summaries.
- Medical Records are append-only at the Domain boundary and cannot be archived.
- The one-active-Medical-Record-per-Student invariant requires repository-level uniqueness and remains deferred to persistence.

## Clinic Medical Clearance workflow

- Medical Clearance IDs use `MCL-YYYY-NNNNNN`.
- unique Clearance Numbers use `MCN-YYYY-NNNNNN`.
- history records use `MCH-YYYY-NNNNNN`.
- lifecycle is Requested, Under Review, Issued, Denied, or Revoked.
- issuance requires prior clinical review.
- issued Clearances contain released summaries rather than confidential Medical notes.
- revocation retains the original Clearance Number and complete transition history.
- Clearance Number uniqueness across records remains a repository responsibility.
- This is a Clinic Medical Clearance workflow, not a university-wide multi-office clearance chain.

## Test coverage

`StudentServiceDomainTests.cs` adds 24 tests covering:

- Library inventory accounting and unavailable-copy rejection;
- issue, overdue, renewal, return, and lost Borrowing behavior;
- Counseling assignment, confidential/released separation, authorization scope, revocation, and closure;
- Discipline intake, review, Violation conversion, released notices, responses, findings, decisions, dismissal, and duplicate evidence rejection;
- Clinic Appointment progression;
- Medical Record confidentiality separation, closure, and retention;
- Medical Clearance review, issue, validity, revocation history, and identifier validation.

The complete suite contains 96 tests after Pass 7.

## Supporting FigJam model

Editable Student Service Domain entity-relationship model:

- `https://www.figma.com/board/2DiMHY2V9ZlTMh3TUFYvXW`

The diagram is explanatory only. GitHub source and automated test evidence are authoritative.

## Windows implementation validation

GitHub Actions run `29688650226` validated implementation head `6d55b5d86856627839c5e3560f70fa9b2df7a770` before branch recovery.

- source-tree and architecture validation: passed;
- NuGet restoration: passed;
- Release MSBuild: passed;
- warnings: `0`;
- errors: `0`;
- MSTest: passed;
- tests executed: `96`;
- tests passed: `96`;
- tests failed: `0`;
- TRX verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-67`;
- artifact ID: `8442878636`;
- SHA-256: `c4d3090e74b3686125bee1ff82b117606b3e0a556a7225a7b0a4cb8b4f11e432`;
- expiration: 2026-08-02.

The recovered final documentation head must be revalidated before integration.

## Architecture boundary

All production additions belong to `IUIS.Domain`. Domain continues to reference only `System` and `System.Core`. It does not reference WinForms, file-system APIs, JSON serialization, Application, Infrastructure, SharedUI, UserApp, or AdminApp.

## Explicitly deferred

- Librarian, Counselor, Discipline Officer, and Clinician permission enforcement;
- restricted DTO construction and own-record filtering;
- repository uniqueness for ISBNs, barcodes, active Medical Records, and Clearance Numbers;
- `books.json`, `borrowings.json`, `counseling.json`, `violations.json`, `medical_records.json`, and `clearances.json` repositories;
- `System.Text.Json` envelopes and the 49 production templates;
- sequence allocation through `id_sequences.json`;
- cross-process locking and journaled multi-file transactions;
- notification, email, SMS, and document dispatch;
- Student and employee WinForms pages;
- backup, restore, recovery, deployment, and release certification.

## Current integration state

The Pass 7 implementation compiled successfully with zero warnings and zero errors, and all 96 tests passed. Final recovered-head validation and pull-request integration remain pending. This is not release certification.
