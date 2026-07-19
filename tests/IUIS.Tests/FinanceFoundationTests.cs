using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Domain.Common;
using IUIS.Domain.Finance;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class FinanceFoundationTests
    {
        private static readonly DateTime CreatedAtUtc =
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const string ActorId = "USR-2026-999999";

        [TestMethod]
        public void ActivePerUnitChargeRuleCalculatesRoundedAmount()
        {
            var rule = CreateChargeRule(ChargeCalculationKind.PerAcademicUnit, 1500m);
            rule.Activate(CreatedAtUtc.AddMinutes(1), ActorId);

            var amount = rule.Calculate(3.50m);

            Assert.AreEqual(5250.00m, amount.Amount);
            Assert.AreEqual("PHP", amount.CurrencyCode);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void InactiveChargeRuleCannotCalculateAmount()
        {
            CreateChargeRule(ChargeCalculationKind.FixedAmount, 500m).Calculate(1m);
        }

        [TestMethod]
        public void TuitionAssessmentPostsChargeSnapshots()
        {
            var assessment = CreateDraftAssessment();
            assessment.AddChargeLine(
                CreateChargeLine(1, 3000m),
                CreatedAtUtc.AddMinutes(1),
                ActorId);

            assessment.Post(CreatedAtUtc.AddMinutes(2), ActorId);

            Assert.AreEqual(TuitionAssessmentStatus.Posted, assessment.Status);
            Assert.AreEqual(3000.00m, assessment.GrossAmount.Amount);
            Assert.AreEqual("TUITION-PER-UNIT", assessment.ChargeLines[0].RuleCodeSnapshot);
            Assert.AreEqual(3L, assessment.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PostedAssessmentCannotAcceptAdditionalCharge()
        {
            var assessment = CreatePostedAssessment();
            assessment.AddChargeLine(
                CreateChargeLine(2, 500m),
                CreatedAtUtc.AddMinutes(3),
                ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PostedAssessmentCannotBeArchived()
        {
            CreatePostedAssessment().Archive(CreatedAtUtc.AddMinutes(3), ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void AssessmentRejectsDuplicateChargeLineIdentifier()
        {
            var assessment = CreateDraftAssessment();
            assessment.AddChargeLine(
                CreateChargeLine(1, 3000m),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            assessment.AddChargeLine(
                CreateChargeLine(1, 500m),
                CreatedAtUtc.AddMinutes(2),
                ActorId);
        }

        [TestMethod]
        public void FixedScholarshipEffectIsCappedAtEligibleAmount()
        {
            var award = CreateFixedScholarshipAward(5000m);
            award.Approve(CreatedAtUtc.AddMinutes(1), ActorId);

            var effect = award.PreviewEffect(
                "ASM-2026-000001",
                Money.PhilippinePeso(3000m));

            Assert.AreEqual(3000.00m, effect.CreditAmount.Amount);
            Assert.AreEqual(ScholarshipAwardStatus.Approved, award.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void AppliedScholarshipAwardCannotBeAppliedTwice()
        {
            var award = CreateFixedScholarshipAward(1000m);
            award.Approve(CreatedAtUtc.AddMinutes(1), ActorId);
            award.MarkApplied(
                "ASM-2026-000001",
                "FAD-2026-000001",
                CreatedAtUtc.AddMinutes(2),
                ActorId);
            award.MarkApplied(
                "ASM-2026-000001",
                "FAD-2026-000002",
                CreatedAtUtc.AddMinutes(3),
                ActorId);
        }

        [TestMethod]
        public void ScholarshipEffectCreatesCreditAdjustment()
        {
            var award = CreateFixedScholarshipAward(1000m);
            award.Approve(CreatedAtUtc.AddMinutes(1), ActorId);
            var effect = award.PreviewEffect(
                "ASM-2026-000001",
                Money.PhilippinePeso(3000m));
            var adjustment = FinancialAdjustment.CreateScholarshipCredit(
                "FAD-2026-000001",
                effect,
                CreatedAtUtc.AddMinutes(2),
                ActorId);

            adjustment.Post(CreatedAtUtc.AddMinutes(3), ActorId);
            award.MarkApplied(
                effect.AssessmentId,
                adjustment.Id,
                CreatedAtUtc.AddMinutes(4),
                ActorId);

            Assert.AreEqual(FinancialAdjustmentDirection.Credit, adjustment.Direction);
            Assert.AreEqual(FinancialAdjustmentStatus.Posted, adjustment.Status);
            Assert.AreEqual(ScholarshipAwardStatus.Applied, award.Status);
            Assert.AreEqual(adjustment.Id, award.AppliedAdjustmentId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PostedFinancialAdjustmentCannotBeArchived()
        {
            var adjustment = CreatePostedScholarshipAdjustment();
            adjustment.Archive(CreatedAtUtc.AddMinutes(4), ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PaymentMustBeFullyAllocatedBeforePosting()
        {
            var payment = CreateDraftPayment(3000m);
            payment.AddAllocation(
                new PaymentAllocation(
                    "PAL-2026-000001",
                    "ASM-2026-000001",
                    Money.PhilippinePeso(2000m)),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            payment.Post(
                "RCT-2026-000001",
                CreatedAtUtc.AddMinutes(2),
                ActorId);
        }

        [TestMethod]
        public void FullyAllocatedPaymentPostsWithReceipt()
        {
            var payment = CreatePostedPayment(3000m);

            Assert.AreEqual(PaymentStatus.Posted, payment.Status);
            Assert.AreEqual("RCT-2026-000001", payment.ReceiptNumber);
            Assert.AreEqual(3000.00m, payment.AllocatedAmount.Amount);
            Assert.AreEqual(0.00m, payment.UnallocatedAmount.Amount);
            Assert.AreEqual(1, payment.Allocations.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PostedPaymentCannotAcceptAnotherAllocation()
        {
            var payment = CreatePostedPayment(3000m);
            payment.AddAllocation(
                new PaymentAllocation(
                    "PAL-2026-000002",
                    "ASM-2026-000002",
                    Money.PhilippinePeso(100m)),
                CreatedAtUtc.AddMinutes(3),
                ActorId);
        }

        [TestMethod]
        public void VoidedPaymentRetainsReceiptAndAllocations()
        {
            var payment = CreatePostedPayment(3000m);
            payment.Void(
                "Duplicate collection was confirmed.",
                CreatedAtUtc.AddMinutes(3),
                ActorId);

            Assert.AreEqual(PaymentStatus.Voided, payment.Status);
            Assert.AreEqual("RCT-2026-000001", payment.ReceiptNumber);
            Assert.AreEqual(1, payment.Allocations.Count);
            Assert.AreEqual(3000.00m, payment.Amount.Amount);
            Assert.AreEqual("Duplicate collection was confirmed.", payment.VoidReason);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PostedPaymentCannotBeArchived()
        {
            CreatePostedPayment(3000m).Archive(CreatedAtUtc.AddMinutes(3), ActorId);
        }

        [TestMethod]
        public void StudentLedgerDerivesAssessmentPaymentAndScholarshipBalance()
        {
            var assessment = CreatePostedAssessment();
            var adjustment = CreatePostedScholarshipAdjustment();
            var payment = CreatePostedPayment(1500m);

            var ledger = StudentLedgerDeriver.Derive(
                "STU-2026-000001",
                "PHP",
                new[] { assessment },
                new[] { adjustment },
                new[] { payment });

            Assert.AreEqual(3, ledger.Entries.Count);
            Assert.AreEqual(3000.00m, ledger.TotalDebits.Amount);
            Assert.AreEqual(2500.00m, ledger.TotalCredits.Amount);
            Assert.AreEqual(500.00m, ledger.Balance.Amount);
        }

        [TestMethod]
        public void StudentLedgerIncludesPaymentVoidReversal()
        {
            var payment = CreatePostedPayment(1500m);
            payment.Void(
                "Collection was voided by an approved Finance workflow.",
                CreatedAtUtc.AddMinutes(3),
                ActorId);

            var ledger = StudentLedgerDeriver.Derive(
                "STU-2026-000001",
                "PHP",
                new TuitionAssessment[0],
                new FinancialAdjustment[0],
                new[] { payment });

            Assert.AreEqual(2, ledger.Entries.Count);
            Assert.AreEqual(1500.00m, ledger.TotalDebits.Amount);
            Assert.AreEqual(1500.00m, ledger.TotalCredits.Amount);
            Assert.AreEqual(0.00m, ledger.Balance.Amount);
            Assert.AreEqual(
                StudentLedgerEntryKind.PaymentVoidDebit,
                ledger.Entries[1].Kind);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void StudentLedgerRejectsSourceCurrencyMismatch()
        {
            var assessment = CreatePostedAssessment();
            StudentLedgerDeriver.Derive(
                "STU-2026-000001",
                "USD",
                new[] { assessment },
                new FinancialAdjustment[0],
                new Payment[0]);
        }

        private static AssessmentChargeRule CreateChargeRule(
            ChargeCalculationKind calculationKind,
            decimal amount)
        {
            return new AssessmentChargeRule(
                "ACR-2026-000001",
                "TUITION-PER-UNIT",
                "Tuition charge rule",
                AssessmentChargeCategory.Tuition,
                calculationKind,
                Money.PhilippinePeso(amount),
                CreatedAtUtc,
                ActorId);
        }

        private static AssessmentChargeLine CreateChargeLine(int sequence, decimal amount)
        {
            return new AssessmentChargeLine(
                "ACL-2026-" + sequence.ToString("000000"),
                "ACR-2026-000001",
                "TUITION-PER-UNIT",
                "Tuition charge snapshot",
                AssessmentChargeCategory.Tuition,
                Money.PhilippinePeso(amount));
        }

        private static TuitionAssessment CreateDraftAssessment()
        {
            return new TuitionAssessment(
                "ASM-2026-000001",
                "STU-2026-000001",
                "ENR-2026-000001",
                "APD-2026-000001",
                "PHP",
                CreatedAtUtc,
                ActorId);
        }

        private static TuitionAssessment CreatePostedAssessment()
        {
            var assessment = CreateDraftAssessment();
            assessment.AddChargeLine(
                CreateChargeLine(1, 3000m),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            assessment.Post(CreatedAtUtc.AddMinutes(2), ActorId);
            return assessment;
        }

        private static ScholarshipAward CreateFixedScholarshipAward(decimal amount)
        {
            return new ScholarshipAward(
                "SAW-2026-000001",
                "STU-2026-000001",
                "SCP-2026-000001",
                "APD-2026-000001",
                ScholarshipEffectKind.FixedAmount,
                "PHP",
                Money.PhilippinePeso(amount),
                0m,
                CreatedAtUtc,
                ActorId);
        }

        private static FinancialAdjustment CreatePostedScholarshipAdjustment()
        {
            var award = CreateFixedScholarshipAward(1000m);
            award.Approve(CreatedAtUtc.AddMinutes(1), ActorId);
            var effect = award.PreviewEffect(
                "ASM-2026-000001",
                Money.PhilippinePeso(3000m));
            var adjustment = FinancialAdjustment.CreateScholarshipCredit(
                "FAD-2026-000001",
                effect,
                CreatedAtUtc.AddMinutes(2),
                ActorId);
            adjustment.Post(CreatedAtUtc.AddMinutes(3), ActorId);
            return adjustment;
        }

        private static Payment CreateDraftPayment(decimal amount)
        {
            return new Payment(
                "PAY-2026-000001",
                "STU-2026-000001",
                "APD-2026-000001",
                Money.PhilippinePeso(amount),
                PaymentMethod.Cash,
                CreatedAtUtc,
                null,
                CreatedAtUtc,
                ActorId);
        }

        private static Payment CreatePostedPayment(decimal amount)
        {
            var payment = CreateDraftPayment(amount);
            payment.AddAllocation(
                new PaymentAllocation(
                    "PAL-2026-000001",
                    "ASM-2026-000001",
                    Money.PhilippinePeso(amount)),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            payment.Post(
                "RCT-2026-000001",
                CreatedAtUtc.AddMinutes(2),
                ActorId);
            return payment;
        }
    }
}