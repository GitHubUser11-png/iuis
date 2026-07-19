# Pass 6 — Finance Domain Foundations

## Objective

Implement a coherent Finance Domain foundation without introducing persistence, file-system access, WinForms, or cross-process transaction code. The scope covers Assessment charge rules, Tuition Assessment construction and posting, Scholarship Award credit effects, Financial Adjustments, Payment allocation and void lifecycle, immutable posted-finance invariants, and derived Student Ledger contracts.

## Starting point

- repository: `GitHubUser11-png/iuis`
- base branch: `develop`
- starting commit: `3fe05ae42f076a4c9ca7ff9e3f197ca8c8d4a9dd`
- implementation branch: `build/pass-06-finance-foundations`
- implementation pull request: `#17`
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

`FinanceFoundationTests.cs` adds 18 Finance tests covering:

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

The complete suite contains 72 tests after Pass 6.

## Supporting FigJam model

An editable Finance Domain entity-relationship diagram was generated through Figma:

- `https://www.figma.com/board/PVtLH7VIUDQIPed8X6rIWV`

The diagram is explanatory only. GitHub source and automated test evidence remain authoritative.

## Windows implementation validation

GitHub Actions run `29686053729` validated implementation head `6a6ad194edc9b2e9eab42846172bcfa6fa73600b`.

- source-tree and architecture validation: passed;
- NuGet restoration: passed;
- Release MSBuild: passed;
- warnings: `0`;
- errors: `0`;
- MSTest: passed;
- tests executed: `72`;
- tests passed: `72`;
- tests failed: `0`;
- TRX verification: passed;
- artifact publication: passed.

Evidence artifact:

- name: `iuis-windows-build-evidence-53`;
- artifact ID: `8442097786`;
- SHA-256: `035fafb86642ab0f0b48e85ef8bdef085fc31393f3af7be7a6b91e1f2f9ab72c`;
- expiration: 2026-08-02.

## Final implementation-head validation

GitHub Actions run `29686160166` validated final PR head `2e8093cbb13e1f0a6dfd0ba5987c924030c2f1f3` after evidence documentation was added.

- source-tree and architecture validation: passed;
- NuGet restoration: passed;
- Release MSBuild: passed;
- MSTest: passed;
- TRX verification: passed;
- artifact publication: passed.

Final-head artifact:

- name: `iuis-windows-build-evidence-55`;
- artifact ID: `8442130601`;
- SHA-256: `1cee4df9922bde548090ef487b787943dee89cbbd53621d63f1557af0f6543fd`;
- expiration: 2026-08-02.

## Integration result

- PR #17 was squash-merged into `develop`.
- integration commit: `d5b24245009bfc8b6639a5bbdc7fa1e6d7af59eb`.
- closure branch: `build/pass-06-closure`.
- independent post-merge validation is recorded in `PASS_06_CLOSURE.md`.

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

## Integration state

Pass 6 is implemented, compile-verified, test-verified, and merged into `develop`. Independent closure validation remains the final administrative gate. This is not release certification.