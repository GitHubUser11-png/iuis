using System;

namespace IUIS.Domain.Clinic
{
    public static class ClinicPersistenceFactory
    {
        public static MedicalConsultationRecord RehydrateConsultation(
            string consultationId,
            string appointmentId,
            string clinicianEmployeeId,
            DateTime occurredAtUtc,
            string internalClinicalNotes,
            string internalAssessment,
            string internalTreatmentPlan)
        {
            return new MedicalConsultationRecord(
                consultationId,
                appointmentId,
                clinicianEmployeeId,
                occurredAtUtc,
                internalClinicalNotes,
                internalAssessment,
                internalTreatmentPlan);
        }

        public static MedicalReleasedSummary RehydrateReleasedSummary(
            string summaryId,
            string consultationId,
            string releasedSummary,
            DateTime releasedAtUtc,
            string releasedByUserId)
        {
            return new MedicalReleasedSummary(
                summaryId,
                consultationId,
                releasedSummary,
                releasedAtUtc,
                releasedByUserId);
        }

        public static MedicalClearanceHistoryEntry RehydrateClearanceHistory(
            string historyId,
            MedicalClearanceHistoryAction action,
            MedicalClearanceStatus fromStatus,
            MedicalClearanceStatus toStatus,
            string reason,
            DateTime occurredAtUtc,
            string actorUserId)
        {
            return new MedicalClearanceHistoryEntry(
                historyId,
                action,
                fromStatus,
                toStatus,
                reason,
                occurredAtUtc,
                actorUserId);
        }
    }
}
