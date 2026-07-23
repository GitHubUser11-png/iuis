using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using IUIS.Domain.Counseling;
using IUIS.Domain.Discipline;
using IUIS.Domain.Time;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class CounselingCaseJsonMapper :
        IJsonRecordMapper<CounselingCase>
    {
        public CounselingCase FromJson(
            JsonElement element,
            JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard
                .Read<PersistedCounselingCaseRecord>(
                    element,
                    options,
                    "CounselingCase");
            var sessions = (record.ConfidentialSessions
                    ?? new List<PersistedCounselingConfidentialSessionRecord>())
                .Select(item => CounselingSessionRecord.Rehydrate(
                    item.SessionId,
                    item.OccurredAtUtc,
                    item.CounselorEmployeeId,
                    PersistedRecordMapperGuard
                        .ParseEnum<CounselingRiskLevel>(
                            item.RiskLevel,
                            nameof(item.RiskLevel),
                            true),
                    item.InternalNotes))
                .ToList();
            var summaries = (record.ReleasedSummaries
                    ?? new List<PersistedCounselingReleasedSummaryRecord>())
                .Select(item => CounselingReleasedSummary.Rehydrate(
                    item.SummaryId,
                    item.SessionId,
                    item.ReleaseAuthorizationId,
                    item.Summary,
                    item.ReleasedAtUtc,
                    item.ReleasedByUserId))
                .ToList();

            return CounselingCase.Rehydrate(
                record.Id,
                record.StudentId,
                record.RequestedAppointmentAtUtc,
                record.RequestReason,
                PersistedRecordMapperGuard
                    .ParseEnum<CounselingCaseStatus>(
                        record.Status,
                        nameof(record.Status),
                        true),
                record.ConfirmedAppointmentAtUtc,
                record.AssignedCounselorEmployeeId,
                record.ClosedAtUtc,
                record.ClosureSummary,
                sessions,
                summaries,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(
            CounselingCase value,
            JsonSerializerOptions options)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return PersistedRecordMapperGuard.Write(
                new PersistedCounselingCaseRecord
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
                    RequestedAppointmentAtUtc =
                        value.RequestedAppointmentAtUtc,
                    RequestReason = value.RequestReason,
                    Status = value.Status.ToString(),
                    ConfirmedAppointmentAtUtc =
                        value.ConfirmedAppointmentAtUtc,
                    AssignedCounselorEmployeeId =
                        value.AssignedCounselorEmployeeId,
                    ClosedAtUtc = value.ClosedAtUtc,
                    ClosureSummary = value.ClosureSummary,
                    ConfidentialSessions = value.ConfidentialSessions
                        .Select(item =>
                            new PersistedCounselingConfidentialSessionRecord
                            {
                                SessionId = item.SessionId,
                                OccurredAtUtc = item.OccurredAtUtc,
                                CounselorEmployeeId =
                                    item.CounselorEmployeeId,
                                RiskLevel = item.RiskLevel.ToString(),
                                InternalNotes = item.InternalNotes
                            })
                        .ToList(),
                    ReleasedSummaries = value.ReleasedSummaries
                        .Select(item =>
                            new PersistedCounselingReleasedSummaryRecord
                            {
                                SummaryId = item.SummaryId,
                                SessionId = item.SessionId,
                                ReleaseAuthorizationId =
                                    item.ReleaseAuthorizationId,
                                Summary = item.Summary,
                                ReleasedAtUtc = item.ReleasedAtUtc,
                                ReleasedByUserId =
                                    item.ReleasedByUserId
                            })
                        .ToList()
                },
                options);
        }
    }

    public sealed class DisciplineCaseJsonMapper :
        IJsonRecordMapper<DisciplineCase>
    {
        public DisciplineCase FromJson(
            JsonElement element,
            JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard
                .Read<PersistedDisciplineCaseRecord>(
                    element,
                    options,
                    "DisciplineCase");

            var violation = record.Violation == null
                ? null
                : DisciplineViolation.Rehydrate(
                    record.Violation.ViolationId,
                    record.Violation.Code,
                    record.Violation.Description,
                    PersistedRecordMapperGuard
                        .ParseEnum<DisciplineSeverity>(
                            record.Violation.Severity,
                            nameof(record.Violation.Severity),
                            true));

            var notice = record.ReleasedNotice == null
                ? null
                : DisciplineReleasedNotice.Rehydrate(
                    record.ReleasedNotice.NoticeId,
                    record.ReleasedNotice.ReleasedSummary,
                    InstitutionLocalDate.Parse(
                        record.ReleasedNotice.ResponseDueDate),
                    record.ReleasedNotice.ReleasedAtUtc,
                    record.ReleasedNotice.ReleasedByUserId);

            var decision = ReadDecision(record);

            var evidence = (record.RestrictedEvidence
                    ?? new List<PersistedDisciplineEvidenceReferenceRecord>())
                .Select(item => DisciplineEvidenceReference.Rehydrate(
                    item.EvidenceId,
                    item.Reference,
                    item.Description,
                    item.AddedAtUtc,
                    item.AddedByUserId))
                .ToList();
            var responses = (record.StudentResponses
                    ?? new List<PersistedDisciplineStudentResponseRecord>())
                .Select(item => DisciplineStudentResponse.Rehydrate(
                    item.ResponseId,
                    item.ResponseText,
                    item.EvidenceReference,
                    item.SubmittedAtUtc))
                .ToList();
            var findings = (record.RestrictedFindings
                    ?? new List<PersistedDisciplineFindingRecord>())
                .Select(item => DisciplineFinding.Rehydrate(
                    item.FindingId,
                    item.Substantiated,
                    item.InternalFinding,
                    item.RecordedAtUtc,
                    item.RecordedByUserId))
                .ToList();

            return DisciplineCase.Rehydrate(
                record.Id,
                record.StudentId,
                record.OccurredAtUtc,
                record.Location,
                record.InternalIncidentNarrative,
                record.ReporterUserId,
                PersistedRecordMapperGuard
                    .ParseEnum<DisciplineCaseStatus>(
                        record.Status,
                        nameof(record.Status),
                        true),
                violation,
                notice,
                decision,
                evidence,
                responses,
                findings,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(
            DisciplineCase value,
            JsonSerializerOptions options)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return PersistedRecordMapperGuard.Write(
                new PersistedDisciplineCaseRecord
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
                    OccurredAtUtc = value.OccurredAtUtc,
                    Location = value.Location,
                    InternalIncidentNarrative =
                        value.InternalIncidentNarrative,
                    ReporterUserId = value.ReporterUserId,
                    Status = value.Status.ToString(),
                    Violation = WriteViolation(value),
                    ReleasedNotice = WriteNotice(value),
                    RestrictedDecision = WriteRestrictedDecision(value),
                    ReleasedDecision = WriteReleasedDecision(value),
                    RestrictedEvidence = value.RestrictedEvidence
                        .Select(item =>
                            new PersistedDisciplineEvidenceReferenceRecord
                            {
                                EvidenceId = item.EvidenceId,
                                Reference = item.Reference,
                                Description = item.Description,
                                AddedAtUtc = item.AddedAtUtc,
                                AddedByUserId = item.AddedByUserId
                            })
                        .ToList(),
                    StudentResponses = value.StudentResponses
                        .Select(item =>
                            new PersistedDisciplineStudentResponseRecord
                            {
                                ResponseId = item.ResponseId,
                                ResponseText = item.ResponseText,
                                EvidenceReference =
                                    item.EvidenceReference,
                                SubmittedAtUtc =
                                    item.SubmittedAtUtc
                            })
                        .ToList(),
                    RestrictedFindings = value.RestrictedFindings
                        .Select(item =>
                            new PersistedDisciplineFindingRecord
                            {
                                FindingId = item.FindingId,
                                Substantiated = item.Substantiated,
                                InternalFinding =
                                    item.InternalFinding,
                                RecordedAtUtc =
                                    item.RecordedAtUtc,
                                RecordedByUserId =
                                    item.RecordedByUserId
                            })
                        .ToList()
                },
                options);
        }

        private static DisciplineDecision ReadDecision(
            PersistedDisciplineCaseRecord record)
        {
            if (record.RestrictedDecision == null)
            {
                if (record.ReleasedDecision != null)
                    throw new InvalidOperationException(
                        "A released Discipline Decision requires its restricted source record.");
                return null;
            }

            if (record.ReleasedDecision != null
                && !StringComparer.Ordinal.Equals(
                    record.RestrictedDecision.DecisionId,
                    record.ReleasedDecision.DecisionId))
            {
                throw new InvalidOperationException(
                    "Restricted and released Discipline Decisions must share the same identifier.");
            }

            return DisciplineDecision.Rehydrate(
                record.RestrictedDecision.DecisionId,
                PersistedRecordMapperGuard
                    .ParseEnum<DisciplineDecisionOutcome>(
                        record.RestrictedDecision.Outcome,
                        nameof(record.RestrictedDecision.Outcome),
                        true),
                record.RestrictedDecision.InternalRationale,
                record.RestrictedDecision.SanctionSummary,
                record.RestrictedDecision.PreparedAtUtc,
                record.RestrictedDecision.PreparedByUserId,
                record.ReleasedDecision == null
                    ? null
                    : record.ReleasedDecision
                        .ReleasedDecisionSummary,
                record.ReleasedDecision == null
                    ? (DateTime?)null
                    : record.ReleasedDecision.ReleasedAtUtc,
                record.ReleasedDecision == null
                    ? null
                    : record.ReleasedDecision.ReleasedByUserId);
        }

        private static PersistedDisciplineViolationRecord
            WriteViolation(DisciplineCase value)
        {
            return value.Violation == null
                ? null
                : new PersistedDisciplineViolationRecord
                {
                    ViolationId = value.Violation.ViolationId,
                    Code = value.Violation.Code,
                    Description = value.Violation.Description,
                    Severity = value.Violation.Severity.ToString()
                };
        }

        private static PersistedDisciplineReleasedNoticeRecord
            WriteNotice(DisciplineCase value)
        {
            return value.ReleasedNotice == null
                ? null
                : new PersistedDisciplineReleasedNoticeRecord
                {
                    NoticeId = value.ReleasedNotice.NoticeId,
                    ReleasedSummary =
                        value.ReleasedNotice.ReleasedSummary,
                    ResponseDueDate =
                        value.ReleasedNotice.ResponseDueDate.ToString(),
                    ReleasedAtUtc =
                        value.ReleasedNotice.ReleasedAtUtc,
                    ReleasedByUserId =
                        value.ReleasedNotice.ReleasedByUserId
                };
        }

        private static PersistedDisciplineRestrictedDecisionRecord
            WriteRestrictedDecision(DisciplineCase value)
        {
            return value.Decision == null
                ? null
                : new PersistedDisciplineRestrictedDecisionRecord
                {
                    DecisionId = value.Decision.DecisionId,
                    Outcome = value.Decision.Outcome.ToString(),
                    InternalRationale =
                        value.Decision.InternalRationale,
                    SanctionSummary =
                        value.Decision.SanctionSummary,
                    PreparedAtUtc =
                        value.Decision.PreparedAtUtc,
                    PreparedByUserId =
                        value.Decision.PreparedByUserId
                };
        }

        private static PersistedDisciplineReleasedDecisionRecord
            WriteReleasedDecision(DisciplineCase value)
        {
            if (value.Decision == null
                || !value.Decision.ReleasedAtUtc.HasValue)
                return null;

            return new PersistedDisciplineReleasedDecisionRecord
            {
                DecisionId = value.Decision.DecisionId,
                ReleasedDecisionSummary =
                    value.Decision.ReleasedDecisionSummary,
                ReleasedAtUtc =
                    value.Decision.ReleasedAtUtc.Value,
                ReleasedByUserId =
                    value.Decision.ReleasedByUserId
            };
        }
    }
}
