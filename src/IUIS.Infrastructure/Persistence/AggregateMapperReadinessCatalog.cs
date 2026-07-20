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
                Requires("StudentRecordRepositoryAdapter", "StudentRecord", "students",
                    "PersonName, ContactInformation, PostalAddress, private setters, and entity metadata require explicit reconstruction."),
                Requires("EmployeeRecordRepositoryAdapter", "EmployeeRecord", "employees",
                    "Workforce identity, value objects, employment state, and entity metadata require explicit reconstruction."),
                Requires("CourseRepositoryAdapter", "Course", "courses",
                    "Private state and lifecycle status require a mapper that restores persisted state without replaying transitions."),
                Requires("SubjectRepositoryAdapter", "Subject", "subjects",
                    "Prerequisite and subject lifecycle state require explicit hydration and invariant validation."),
                Requires("AcademicPeriodRepositoryAdapter", "AcademicPeriod", "academic_periods",
                    "Institution-local dates and period lifecycle state require explicit hydration."),
                Deferred("EnrollmentRepositoryAdapter", "Enrollment", "enrollments",
                    "Enrollment snapshots and workflow history require a dedicated persisted shape before activation."),
                Deferred("TuitionAssessmentRepositoryAdapter", "TuitionAssessment", "assessments",
                    "Assessment lines, snapshots, finalization, supersession, and immutable finance history require a dedicated mapper."),
                Requires("AssessmentChargeRuleRepositoryAdapter", "AssessmentChargeRule", "assessment_charge_rules",
                    "Money values, applicability state, and lifecycle metadata require explicit hydration."),
                Deferred("PaymentRepositoryAdapter", "Payment", "payments",
                    "Receipt, allocation, posting, void, and reversal state must be reconstructed without replaying financial actions."),
                Deferred("FinancialAdjustmentRepositoryAdapter", "FinancialAdjustment", "financial_adjustments",
                    "Approval and posting history require a dedicated immutable finance persistence mapper."),
                Deferred("ScholarshipAwardRepositoryAdapter", "ScholarshipAward", "scholarship_awards",
                    "Award effect, suspension, revocation, and eligibility snapshots require a dedicated mapper."),
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

        private static AggregateMapperReadinessRecord Requires(
            string adapter,
            string aggregate,
            string repository,
            string reason)
        {
            return new AggregateMapperReadinessRecord(
                adapter,
                aggregate,
                repository,
                AggregateMapperReadiness.RequiresSpecializedMapper,
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
