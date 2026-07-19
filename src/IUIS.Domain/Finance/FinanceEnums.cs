namespace IUIS.Domain.Finance
{
    public enum AssessmentChargeCategory
    {
        Unspecified = 0,
        Tuition = 1,
        Laboratory = 2,
        Miscellaneous = 3,
        Other = 4
    }

    public enum ChargeCalculationKind
    {
        Unspecified = 0,
        FixedAmount = 1,
        PerAcademicUnit = 2
    }

    public enum ChargeRuleStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2,
        Retired = 3
    }

    public enum TuitionAssessmentStatus
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2
    }

    public enum ScholarshipEffectKind
    {
        Unspecified = 0,
        FixedAmount = 1,
        PercentageOfEligibleCharges = 2,
        FullEligibleCharges = 3
    }

    public enum ScholarshipAwardStatus
    {
        Prepared = 0,
        Approved = 1,
        Applied = 2,
        Cancelled = 3
    }

    public enum FinancialAdjustmentDirection
    {
        Unspecified = 0,
        Debit = 1,
        Credit = 2
    }

    public enum FinancialAdjustmentSourceKind
    {
        Unspecified = 0,
        ScholarshipAward = 1,
        AdministrativeCorrection = 2,
        PaymentCorrection = 3,
        Other = 4
    }

    public enum FinancialAdjustmentStatus
    {
        Prepared = 0,
        Posted = 1,
        Cancelled = 2
    }

    public enum PaymentMethod
    {
        Unspecified = 0,
        Cash = 1,
        BankTransfer = 2,
        OnlineGateway = 3,
        Check = 4,
        Other = 5
    }

    public enum PaymentStatus
    {
        Draft = 0,
        Posted = 1,
        Voided = 2
    }

    public enum StudentLedgerEntryKind
    {
        Unspecified = 0,
        AssessmentDebit = 1,
        AdjustmentDebit = 2,
        AdjustmentCredit = 3,
        PaymentCredit = 4,
        PaymentVoidDebit = 5
    }
}