using System;
using System.Collections.Generic;

namespace IUIS.Infrastructure.Persistence
{
    public enum AggregateMapperReadiness
    {
        GenericMapperCompatible = 0,
        RequiresSpecializedMapper = 1,
        SpecializedMapperCompleted = 2,
        DeferredWithExplicitReason = 3
    }

    public sealed class AggregateMapperReadinessRecord
    {
        public AggregateMapperReadinessRecord(
            string adapterName,
            string aggregateTypeName,
            string repositoryName,
            AggregateMapperReadiness readiness,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(adapterName))
                throw new ArgumentException("Adapter name is required.", nameof(adapterName));
            if (string.IsNullOrWhiteSpace(aggregateTypeName))
                throw new ArgumentException("Aggregate type name is required.", nameof(aggregateTypeName));
            if (string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("Repository name is required.", nameof(repositoryName));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Readiness reason is required.", nameof(reason));

            AdapterName = adapterName.Trim();
            AggregateTypeName = aggregateTypeName.Trim();
            RepositoryName = repositoryName.Trim().ToLowerInvariant();
            Readiness = readiness;
            Reason = reason.Trim();
        }

        public string AdapterName { get; private set; }
        public string AggregateTypeName { get; private set; }
        public string RepositoryName { get; private set; }
        public AggregateMapperReadiness Readiness { get; private set; }
        public string Reason { get; private set; }
    }

    public static class AggregateMapperReadinessCatalog
    {
        private static readonly IReadOnlyList<AggregateMapperReadinessRecord> Records =
            new List<AggregateMapperReadinessRecord>
            {
                Completed("StudentRecordRepositoryAdapter", "StudentRecord", "students",
                    "Canonical schema v1 reconstructs identity, people value objects, lifecycle state, archive metadata, timestamps, actors, and entity version."),
                Completed("EmployeeRecordRepositoryAdapter", "EmployeeRecord", "employees",
                    "Canonical schema v1 reconstructs workforce identity, assignment state, people value objects, archive metadata, timestamps, actors, and version."),
                Completed("CourseRepositoryAdapter", "Course", "courses",
                    "Canonical schema v1 restores Course lifecycle and metadata through the validated Domain rehydration factory."),
                Completed("SubjectRepositoryAdapter", "Subject", "subjects",
                    "Canonical schema v1 restores prerequisite collections, lifecycle state, and entity metadata."),
                Completed("AcademicPeriodRepositoryAdapter", "AcademicPeriod", "academic_periods",
                    "Canonical schema v1 reconstructs institution-local dates, schedule invariants, lifecycle state, and metadata."),
                Completed("EnrollmentRepositoryAdapter", "Enrollment", "enrollments",
                    "Pass 11 schema v1 reconstructs immutable course and curriculum snapshots, Subject lines, workflow history, lifecycle state, and metadata."),
                Completed("TuitionAssessmentRepositoryAdapter", "TuitionAssessment", "assessments",
                    "Pass 11 schema v1 reconstructs charge snapshots, Money values, posting or cancellation state, immutable finance metadata, and versions."),
                Completed("AssessmentChargeRuleRepositoryAdapter", "AssessmentChargeRule", "assessment_charge_rules",
                    "Canonical schema v1 preserves Money, calculation policy, lifecycle state, archive state, timestamps, actors, and entity version."),
                Completed("PaymentRepositoryAdapter", "Payment", "payments",
                    "Pass 11 schema v1 reconstructs immutable receipt data, allocations, Money values, posting and void state, and entity metadata."),
                Completed("FinancialAdjustmentRepositoryAdapter", "FinancialAdjustment", "financial_adjustments",
                    "Pass 11 schema v1 reconstructs source links, Money, direction, posting or cancellation metadata, and immutable finance state."),
                Completed("ScholarshipAwardRepositoryAdapter", "ScholarshipAward", "scholarship_awards",
                    "Pass 11 schema v1 reconstructs effect policy, approval and application links, Money, lifecycle state, and entity metadata."),
                Deferred("LibraryBookRepositoryAdapter", "LibraryBook", "books",
                    "Embedded copy inventory and copy-state invariants require explicit collection reconstruction."),
                Deferred("LibraryBorrowingRepositoryAdapter", "LibraryBorrowing", "borrowings",
                    "Issue, renewal, return, overdue, lost, and cancellation history require explicit workflow hydration."),
                Deferred("CounselingCaseRepositoryAdapter", "CounselingCase", "counseling",
                    "Confidential sessions and released summaries require separate persisted projections and strict reconstruction."),
                Deferred("DisciplineCaseRepositoryAdapter", "DisciplineCase", "discipline_incidents",
                    "Evidence, findings, responses, notices, and decisions require restricted persisted shapes and explicit hydration."),
                Deferred("ClinicAppointmentRepositoryAdapter", "ClinicAppointment", "appointments",
                    "Appointment workflow and consultation linkage require explicit lifecycle hydration."),
                Deferred("MedicalRecordRepositoryAdapter", "MedicalRecord", "medical_records",
                    "Confidential consultations and released summaries require separate persisted projections and explicit hydration."),
                Deferred("MedicalClearanceRepositoryAdapter", "MedicalClearance", "clearances",
                    "Clearance history, numbering, validity, issue, denial, and revocation require explicit reconstruction.")
            }.AsReadOnly();

        public static IReadOnlyList<AggregateMapperReadinessRecord> All
        {
            get { return Records; }
        }

        private static AggregateMapperReadinessRecord Completed(
            string adapter,
            string aggregate,
            string repository,
            string reason)
        {
            return new AggregateMapperReadinessRecord(
                adapter,
                aggregate,
                repository,
                AggregateMapperReadiness.SpecializedMapperCompleted,
                reason);
        }

        private static AggregateMapperReadinessRecord Deferred(
            string adapter,
            string aggregate,
            string repository,
            string reason)
        {
            return new AggregateMapperReadinessRecord(
                adapter,
                aggregate,
                repository,
                AggregateMapperReadiness.DeferredWithExplicitReason,
                reason);
        }
    }
}
