using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using IUIS.Domain.Academic;
using IUIS.Domain.Finance;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class EnrollmentJsonMapper : IJsonRecordMapper<Enrollment>
    {
        public Enrollment FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedEnrollmentRecord>(
                element,
                options,
                "Enrollment");
            var lines = (record.SubjectLines ?? new List<PersistedEnrollmentSubjectLine>())
                .Select(item => new EnrollmentSubjectLine(
                    item.SubjectId,
                    item.SubjectCodeSnapshot,
                    item.SubjectTitleSnapshot,
                    item.UnitsSnapshot,
                    item.YearLevelSnapshot,
                    item.TermNumberSnapshot,
                    item.IsRequiredSnapshot,
                    item.SectionCode))
                .ToList();
            return Enrollment.Rehydrate(
                record.Id,
                record.StudentId,
                record.AcademicPeriodId,
                record.CourseIdSnapshot,
                record.CourseCodeSnapshot,
                record.CourseNameSnapshot,
                record.CurriculumIdSnapshot,
                record.CurriculumVersionSnapshot,
                lines,
                PersistedRecordMapperGuard.ParseEnum<EnrollmentStatus>(
                    record.Status,
                    nameof(record.Status),
                    false),
                record.SubmittedAtUtc,
                record.SubmittedByUserId,
                record.ReviewStartedAtUtc,
                record.ReviewStartedByUserId,
                record.DecisionAtUtc,
                record.DecisionByUserId,
                record.DecisionReason,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(Enrollment value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedEnrollmentRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    StudentId = value.StudentId,
                    AcademicPeriodId = value.AcademicPeriodId,
                    CourseIdSnapshot = value.CourseIdSnapshot,
                    CourseCodeSnapshot = value.CourseCodeSnapshot,
                    CourseNameSnapshot = value.CourseNameSnapshot,
                    CurriculumIdSnapshot = value.CurriculumIdSnapshot,
                    CurriculumVersionSnapshot = value.CurriculumVersionSnapshot,
                    Status = value.Status.ToString(),
                    SubmittedAtUtc = value.SubmittedAtUtc,
                    SubmittedByUserId = value.SubmittedByUserId,
                    ReviewStartedAtUtc = value.ReviewStartedAtUtc,
                    ReviewStartedByUserId = value.ReviewStartedByUserId,
                    DecisionAtUtc = value.DecisionAtUtc,
                    DecisionByUserId = value.DecisionByUserId,
                    DecisionReason = value.DecisionReason,
                    SubjectLines = value.SubjectLines.Select(item =>
                        new PersistedEnrollmentSubjectLine
                        {
                            SubjectId = item.SubjectId,
                            SubjectCodeSnapshot = item.SubjectCodeSnapshot,
                            SubjectTitleSnapshot = item.SubjectTitleSnapshot,
                            UnitsSnapshot = item.UnitsSnapshot,
                            YearLevelSnapshot = item.YearLevelSnapshot,
                            TermNumberSnapshot = item.TermNumberSnapshot,
                            IsRequiredSnapshot = item.IsRequiredSnapshot,
                            SectionCode = item.SectionCode
                        }).ToList()
                },
                options);
        }
    }

    public sealed class TuitionAssessmentJsonMapper : IJsonRecordMapper<TuitionAssessment>
    {
        public TuitionAssessment FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedTuitionAssessmentRecord>(
                element,
                options,
                "TuitionAssessment");
            var lines = (record.ChargeLines ?? new List<PersistedAssessmentChargeLine>())
                .Select(item => new AssessmentChargeLine(
                    item.LineId,
                    item.ChargeRuleId,
                    item.RuleCodeSnapshot,
                    item.DescriptionSnapshot,
                    PersistedRecordMapperGuard.ParseEnum<AssessmentChargeCategory>(
                        item.Category,
                        nameof(item.Category),
                        false),
                    new Money(item.Amount, item.CurrencyCode)))
                .ToList();
            return TuitionAssessment.Rehydrate(
                record.Id,
                record.StudentId,
                record.EnrollmentId,
                record.AcademicPeriodId,
                record.CurrencyCode,
                lines,
                PersistedRecordMapperGuard.ParseEnum<TuitionAssessmentStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.PostedAtUtc,
                record.PostedByUserId,
                record.CancelledAtUtc,
                record.CancelledByUserId,
                record.CancellationReason,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(TuitionAssessment value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedTuitionAssessmentRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    StudentId = value.StudentId,
                    EnrollmentId = value.EnrollmentId,
                    AcademicPeriodId = value.AcademicPeriodId,
                    CurrencyCode = value.CurrencyCode,
                    Status = value.Status.ToString(),
                    PostedAtUtc = value.PostedAtUtc,
                    PostedByUserId = value.PostedByUserId,
                    CancelledAtUtc = value.CancelledAtUtc,
                    CancelledByUserId = value.CancelledByUserId,
                    CancellationReason = value.CancellationReason,
                    ChargeLines = value.ChargeLines.Select(item =>
                        new PersistedAssessmentChargeLine
                        {
                            LineId = item.LineId,
                            ChargeRuleId = item.ChargeRuleId,
                            RuleCodeSnapshot = item.RuleCodeSnapshot,
                            DescriptionSnapshot = item.DescriptionSnapshot,
                            Category = item.Category.ToString(),
                            Amount = item.Amount.Amount,
                            CurrencyCode = item.Amount.CurrencyCode
                        }).ToList()
                },
                options);
        }
    }

    public sealed class PaymentJsonMapper : IJsonRecordMapper<Payment>
    {
        public Payment FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedPaymentRecord>(
                element,
                options,
                "Payment");
            var allocations = (record.Allocations ?? new List<PersistedPaymentAllocation>())
                .Select(item => new PaymentAllocation(
                    item.AllocationId,
                    item.AssessmentId,
                    new Money(item.Amount, item.CurrencyCode)))
                .ToList();
            return Payment.Rehydrate(
                record.Id,
                record.StudentId,
                record.AcademicPeriodId,
                new Money(record.Amount, record.CurrencyCode),
                PersistedRecordMapperGuard.ParseEnum<PaymentMethod>(
                    record.Method,
                    nameof(record.Method),
                    false),
                record.ReceivedAtUtc,
                record.ExternalReference,
                allocations,
                PersistedRecordMapperGuard.ParseEnum<PaymentStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.ReceiptNumber,
                record.PostedAtUtc,
                record.PostedByUserId,
                record.VoidedAtUtc,
                record.VoidedByUserId,
                record.VoidReason,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(Payment value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedPaymentRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    StudentId = value.StudentId,
                    AcademicPeriodId = value.AcademicPeriodId,
                    Amount = value.Amount.Amount,
                    CurrencyCode = value.Amount.CurrencyCode,
                    Method = value.Method.ToString(),
                    ReceivedAtUtc = value.ReceivedAtUtc,
                    ExternalReference = value.ExternalReference,
                    Status = value.Status.ToString(),
                    ReceiptNumber = value.ReceiptNumber,
                    PostedAtUtc = value.PostedAtUtc,
                    PostedByUserId = value.PostedByUserId,
                    VoidedAtUtc = value.VoidedAtUtc,
                    VoidedByUserId = value.VoidedByUserId,
                    VoidReason = value.VoidReason,
                    Allocations = value.Allocations.Select(item =>
                        new PersistedPaymentAllocation
                        {
                            AllocationId = item.AllocationId,
                            AssessmentId = item.AssessmentId,
                            Amount = item.Amount.Amount,
                            CurrencyCode = item.Amount.CurrencyCode
                        }).ToList()
                },
                options);
        }
    }

    public sealed class FinancialAdjustmentJsonMapper : IJsonRecordMapper<FinancialAdjustment>
    {
        public FinancialAdjustment FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedFinancialAdjustmentRecord>(
                element,
                options,
                "FinancialAdjustment");
            return FinancialAdjustment.Rehydrate(
                record.Id,
                record.StudentId,
                record.AssessmentId,
                PersistedRecordMapperGuard.ParseEnum<FinancialAdjustmentDirection>(
                    record.Direction,
                    nameof(record.Direction),
                    false),
                new Money(record.Amount, record.CurrencyCode),
                PersistedRecordMapperGuard.ParseEnum<FinancialAdjustmentSourceKind>(
                    record.SourceKind,
                    nameof(record.SourceKind),
                    false),
                record.SourceRecordId,
                record.Reason,
                PersistedRecordMapperGuard.ParseEnum<FinancialAdjustmentStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.PostedAtUtc,
                record.PostedByUserId,
                record.CancellationReason,
                record.CancelledAtUtc,
                record.CancelledByUserId,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(FinancialAdjustment value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedFinancialAdjustmentRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    StudentId = value.StudentId,
                    AssessmentId = value.AssessmentId,
                    Direction = value.Direction.ToString(),
                    Amount = value.Amount.Amount,
                    CurrencyCode = value.Amount.CurrencyCode,
                    SourceKind = value.SourceKind.ToString(),
                    SourceRecordId = value.SourceRecordId,
                    Reason = value.Reason,
                    Status = value.Status.ToString(),
                    PostedAtUtc = value.PostedAtUtc,
                    PostedByUserId = value.PostedByUserId,
                    CancellationReason = value.CancellationReason,
                    CancelledAtUtc = value.CancelledAtUtc,
                    CancelledByUserId = value.CancelledByUserId
                },
                options);
        }
    }

    public sealed class ScholarshipAwardJsonMapper : IJsonRecordMapper<ScholarshipAward>
    {
        public ScholarshipAward FromJson(JsonElement element, JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedScholarshipAwardRecord>(
                element,
                options,
                "ScholarshipAward");
            var fixedAmount = record.FixedAmount.HasValue
                ? new Money(record.FixedAmount.Value, record.CurrencyCode)
                : null;
            return ScholarshipAward.Rehydrate(
                record.Id,
                record.StudentId,
                record.ScholarshipProgramId,
                record.AcademicPeriodId,
                PersistedRecordMapperGuard.ParseEnum<ScholarshipEffectKind>(
                    record.EffectKind,
                    nameof(record.EffectKind),
                    false),
                record.CurrencyCode,
                fixedAmount,
                record.Percentage,
                PersistedRecordMapperGuard.ParseEnum<ScholarshipAwardStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.ApprovedAtUtc,
                record.ApprovedByUserId,
                record.AppliedAssessmentId,
                record.AppliedAdjustmentId,
                record.AppliedAtUtc,
                record.AppliedByUserId,
                record.CancellationReason,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(ScholarshipAward value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedScholarshipAwardRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    StudentId = value.StudentId,
                    ScholarshipProgramId = value.ScholarshipProgramId,
                    AcademicPeriodId = value.AcademicPeriodId,
                    EffectKind = value.EffectKind.ToString(),
                    CurrencyCode = value.CurrencyCode,
                    FixedAmount = value.FixedAmount == null
                        ? (decimal?)null
                        : value.FixedAmount.Amount,
                    Percentage = value.Percentage,
                    Status = value.Status.ToString(),
                    ApprovedAtUtc = value.ApprovedAtUtc,
                    ApprovedByUserId = value.ApprovedByUserId,
                    AppliedAssessmentId = value.AppliedAssessmentId,
                    AppliedAdjustmentId = value.AppliedAdjustmentId,
                    AppliedAtUtc = value.AppliedAtUtc,
                    AppliedByUserId = value.AppliedByUserId,
                    CancellationReason = value.CancellationReason
                },
                options);
        }
    }
}
