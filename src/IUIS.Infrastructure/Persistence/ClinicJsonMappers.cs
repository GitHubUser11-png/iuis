using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using IUIS.Domain.Clinic;
using IUIS.Domain.Time;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class ClinicAppointmentJsonMapper :
        IJsonRecordMapper<ClinicAppointment>
    {
        public ClinicAppointment FromJson(
            JsonElement element,
            JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedClinicAppointmentRecord>(
                element,
                options,
                "ClinicAppointment");
            return ClinicAppointment.Rehydrate(
                record.Id,
                record.StudentId,
                record.RequestedAppointmentAtUtc,
                record.ReleasedReasonSummary,
                PersistedRecordMapperGuard.ParseEnum<ClinicAppointmentStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.ScheduledAtUtc,
                record.ClinicianEmployeeId,
                record.CheckedInAtUtc,
                record.CompletedAtUtc,
                record.ConsultationId,
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

        public JsonElement ToJson(
            ClinicAppointment value,
            JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedClinicAppointmentRecord
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
                    RequestedAppointmentAtUtc = value.RequestedAppointmentAtUtc,
                    ReleasedReasonSummary = value.ReleasedReasonSummary,
                    Status = value.Status.ToString(),
                    ScheduledAtUtc = value.ScheduledAtUtc,
                    ClinicianEmployeeId = value.ClinicianEmployeeId,
                    CheckedInAtUtc = value.CheckedInAtUtc,
                    CompletedAtUtc = value.CompletedAtUtc,
                    ConsultationId = value.ConsultationId,
                    CancellationReason = value.CancellationReason
                },
                options);
        }
    }

    public sealed class MedicalRecordJsonMapper :
        IJsonRecordMapper<MedicalRecord>
    {
        public MedicalRecord FromJson(
            JsonElement element,
            JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedMedicalRecord>(
                element,
                options,
                "MedicalRecord");
            var consultations = (record.ConfidentialConsultations
                ?? new List<PersistedMedicalConfidentialConsultationRecord>())
                .Select(item =>
                {
                    if (item == null)
                    {
                        throw new InvalidOperationException(
                            "MedicalRecord persisted consultation is invalid.");
                    }
                    return ClinicPersistenceFactory.RehydrateConsultation(
                        item.ConsultationId,
                        item.AppointmentId,
                        item.ClinicianEmployeeId,
                        item.OccurredAtUtc,
                        item.InternalClinicalNotes,
                        item.InternalAssessment,
                        item.InternalTreatmentPlan);
                })
                .ToList();
            var summaries = (record.ReleasedSummaries
                ?? new List<PersistedMedicalReleasedSummaryRecord>())
                .Select(item =>
                {
                    if (item == null)
                    {
                        throw new InvalidOperationException(
                            "MedicalRecord persisted released summary is invalid.");
                    }
                    return ClinicPersistenceFactory.RehydrateReleasedSummary(
                        item.SummaryId,
                        item.ConsultationId,
                        item.ReleasedSummary,
                        item.ReleasedAtUtc,
                        item.ReleasedByUserId);
                })
                .ToList();

            return MedicalRecord.Rehydrate(
                record.Id,
                record.StudentId,
                PersistedRecordMapperGuard.ParseEnum<MedicalRecordStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.ClosedAtUtc,
                consultations,
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
            MedicalRecord value,
            JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedMedicalRecord
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
                    Status = value.Status.ToString(),
                    ClosedAtUtc = value.ClosedAtUtc,
                    ConfidentialConsultations = value.ConfidentialConsultations
                        .Select(item => new PersistedMedicalConfidentialConsultationRecord
                        {
                            ConsultationId = item.ConsultationId,
                            AppointmentId = item.AppointmentId,
                            ClinicianEmployeeId = item.ClinicianEmployeeId,
                            OccurredAtUtc = item.OccurredAtUtc,
                            InternalClinicalNotes = item.InternalClinicalNotes,
                            InternalAssessment = item.InternalAssessment,
                            InternalTreatmentPlan = item.InternalTreatmentPlan
                        }).ToList(),
                    ReleasedSummaries = value.ReleasedSummaries
                        .Select(item => new PersistedMedicalReleasedSummaryRecord
                        {
                            SummaryId = item.SummaryId,
                            ConsultationId = item.ConsultationId,
                            ReleasedSummary = item.ReleasedSummary,
                            ReleasedAtUtc = item.ReleasedAtUtc,
                            ReleasedByUserId = item.ReleasedByUserId
                        }).ToList()
                },
                options);
        }
    }

    public sealed class MedicalClearanceJsonMapper :
        IJsonRecordMapper<MedicalClearance>
    {
        public MedicalClearance FromJson(
            JsonElement element,
            JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedMedicalClearanceRecord>(
                element,
                options,
                "MedicalClearance");
            var history = (record.RestrictedHistory
                ?? new List<PersistedMedicalClearanceHistoryRecord>())
                .Select(item =>
                {
                    if (item == null)
                    {
                        throw new InvalidOperationException(
                            "MedicalClearance persisted history entry is invalid.");
                    }
                    return ClinicPersistenceFactory.RehydrateClearanceHistory(
                        item.HistoryId,
                        PersistedRecordMapperGuard.ParseEnum<MedicalClearanceHistoryAction>(
                            item.Action,
                            nameof(item.Action),
                            true),
                        PersistedRecordMapperGuard.ParseEnum<MedicalClearanceStatus>(
                            item.FromStatus,
                            nameof(item.FromStatus),
                            true),
                        PersistedRecordMapperGuard.ParseEnum<MedicalClearanceStatus>(
                            item.ToStatus,
                            nameof(item.ToStatus),
                            true),
                        item.Reason,
                        item.OccurredAtUtc,
                        item.ActorUserId);
                })
                .ToList();

            return MedicalClearance.Rehydrate(
                record.Id,
                record.StudentId,
                record.MedicalRecordId,
                record.RequestReason,
                PersistedRecordMapperGuard.ParseEnum<MedicalClearanceStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.ReviewingClinicianEmployeeId,
                record.ClearanceNumber,
                ParseOptionalDate(record.ValidFrom),
                ParseOptionalDate(record.ValidUntil),
                record.ReleasedSummary,
                record.RevocationReason,
                history,
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
            MedicalClearance value,
            JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedMedicalClearanceRecord
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
                    MedicalRecordId = value.MedicalRecordId,
                    RequestReason = value.RequestReason,
                    Status = value.Status.ToString(),
                    ReviewingClinicianEmployeeId = value.ReviewingClinicianEmployeeId,
                    ClearanceNumber = value.ClearanceNumber,
                    ValidFrom = value.ValidFrom.HasValue
                        ? value.ValidFrom.Value.ToString()
                        : null,
                    ValidUntil = value.ValidUntil.HasValue
                        ? value.ValidUntil.Value.ToString()
                        : null,
                    ReleasedSummary = value.ReleasedSummary,
                    RevocationReason = value.RevocationReason,
                    RestrictedHistory = value.History
                        .Select(item => new PersistedMedicalClearanceHistoryRecord
                        {
                            HistoryId = item.HistoryId,
                            Action = item.Action.ToString(),
                            FromStatus = item.FromStatus.ToString(),
                            ToStatus = item.ToStatus.ToString(),
                            Reason = item.Reason,
                            OccurredAtUtc = item.OccurredAtUtc,
                            ActorUserId = item.ActorUserId
                        }).ToList()
                },
                options);
        }

        private static InstitutionLocalDate? ParseOptionalDate(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? (InstitutionLocalDate?)null
                : InstitutionLocalDate.Parse(value);
        }
    }
}
