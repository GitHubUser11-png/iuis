# Pass 6 — Finance Domain Foundations

## Objective

Implement a coherent Finance Domain foundation without introducing persistence, file-system access, WinForms, or cross-process transaction code. The scope covers Assessment charge rules, Tuition Assessment construction and posting, Scholarship Award credit effects, Financial Adjustments, Payment allocation and void lifecycle, immutable posted-finance invariants, and derived Student Ledger contracts.

## Starting point

- repository: `GitHubUser11-png/iuis`
- base branch: `develop`
- starting commit: `3fe05ae42f076a4c9ca7ff9e3f197ca8c8d4a9dd`
- implementation branch: `build/pass-06-finance-foundations`
- Passes 1 through 5 were integrated before branch creation

## Created Domain sources

- `Finance/FinanceEnums.cs`
- `Finance/AssessmentChargeRule.cs`
- `Finance/TuitionAssessment.cs`
- `Finance/ScholarshipAward.cs`
- `Finance/FinancialAdjustment.cs`
- `Finance/Payment.cs`
- `Finance/StudentLedger.cs`

## Modified Domain foundation

`EntityBase.Archive` and `EntityBase.Restore` are now virtual so finance aggregates can enforce stronger retention rules. Posted Assessments, posted Adjustments, and posted or voided Payments override these operations and reject archival or restoration that would erase or obscure posted financial history.

## Assessment charge rules

- Charge-rule IDs use `ACR-YYYY-NNNNNN`.
- Codes are normalized immutable business identifiers.
- Supported calculations are fixed amount and per-academic-unit.
- Monetary rates use `Money` and therefore decimal, two-digit rounding, and explicit currency.
- Only Active rules calculate charges.
- Draft rules may be edited; activation, deactivation, and retirement are explicit.

## Tuition Assessment

- Assessment IDs use `ASM-YYYY-NNNNNN`.
- Student, Enrollment, and Academic Period links use stable IDs.
- Nested charge-line IDs use `ACL-YYYY-NNNNNN`.
- Each line snapshots the charge-rule ID, code, description, category, and amount.
- Draft Assessments may add or remove charge lines.
- Posting requires at least one positive charge.
- Posted Assessments cannot be edited, cancelled, archived, or restored.
- Scholarship and Payment state are not embedded in the Assessment.

## Scholarship Award effects

- Award IDs use `SAW-YYYY-NNNNNN`.
- Awards link Student, Scholarship Program, and Academic Period IDs.
- Supported effects are fixed amount, percentage of eligible charges, and full eligible charges.
- Fixed awards are capped at the eligible charge amount.
- `PreviewEffect` creates an immutable credit-effect contract without mutating Payment history.
- Application of an Award is recorded once through Assessment and Financial Adjustment IDs.
- Reapplication is rejected.

## Financial Adjustment

- Adjustment IDs use `FAD-YYYY-NNNNNN`.
- Adjustments contain explicit debit or credit direction, positive amount, source kind, source record, and reason.
- Scholarship effects create Credit Adjustments.
- Prepared Adjustments may be posted or cancelled.
- Posted Adjustments are immutable and cannot be archived or restored.

## Payment and allocation lifecycle

- Payment IDs use `PAY-YYYY-NNNNNN`.
- Allocation IDs use `PAL-YYYY-NNNNNN`.
- Receipt numbers use `RCT-YYYY-NNNNNN`.
- Payments link a Student and Academic Period and contain immutable amount, method, receipt time, and optional external reference.
- Draft allocations cannot exceed the Payment amount and cannot duplicate an Assessment.
- Posting requires at least one allocation and complete allocation of the Payment amount.
- Posted Payments cannot be edited or deleted.
- Voiding preserves the original Payment, Receipt Number, amount, and allocations and adds explicit void metadata.
- Receipt-number uniqueness and external-reference duplicate policy remain repository responsibilities.

## Derived Student Ledger

`StudentLedgerDeriver` constructs a transient `StudentLedgerSnapshot` from authoritative posted Finance records:

- posted Tuition Assessments produce debits;
- posted debit or credit Adjustments produce corresponding entries;
- posted Payments produce credits;
- voided Payments retain the original credit and add a reversal debit.

The Ledger is derived and is not a new repository. No `student_ledgers.json` file is introduced.

## Test coverage

`FinanceFoundationTests.cs` covers:

- active fixed and per-unit charge calculations;
- inactive-rule rejection;
- Assessment posting and snapshot preservation;
- posted Assessment immutability;
- duplicate charge-line rejection;
- Scholarship capping and single-use application;
- Scholarship Credit Adjustment creation;
- posted Adjustment retention;
- complete Payment-allocation requirements;
- Receipt Number and allocation preservation;
- posted Payment immutability;
- Payment void retention;
- derived Ledger debit, credit, balance, and void reversal;
- Ledger currency consistency.

## Supporting FigJam model

An editable Finance Domain entity-relationship diagram was generated through Figma:

- `https://www.figma.com/board/PVtLH7VIUDQIPed8X6rIWV`

The diagram is explanatory only. GitHub source and automated test evidence remain authoritative.

## Explicitly deferred

- Application DTOs, commands, authorization, and orchestration;
- repository-level code and Receipt Number uniqueness;
- external Payment reference duplicate policy;
- `System.Text.Json` envelopes and the 49 production templates;
- sequence allocation through `id_sequences.json`;
- persistent locks and multi-file transaction coordination;
- assessment generation from approved Enrollment;
- atomic Scholarship Adjustment and Award application orchestration;
- Payment receipt-print dispatch semantics;
- Payment void approval requests;
- Finance WinForms and reporting screens;
- backup, restore, recovery, and release certification.

## Current validation state

The Finance source, tests, project-file updates, documentation, and FigJam model are committed on the Pass 6 branch. No compilation or test claim is made until the final branch head passes the Windows workflow and produces TRX and artifact evidence.