using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Infrastructure.Persistence;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass12StudentServiceSchemaTests
    {
        [TestMethod]
        public void SevenDeferredAggregateRecordSchemasAreExplicitAndVersioned()
        {
            var aggregateRecordTypes = new[]
            {
                typeof(PersistedLibraryBookRecord),
                typeof(PersistedLibraryBorrowingRecord),
                typeof(PersistedCounselingCaseRecord),
                typeof(PersistedDisciplineCaseRecord),
                typeof(PersistedClinicAppointmentRecord),
                typeof(PersistedMedicalRecord),
                typeof(PersistedMedicalClearanceRecord)
            };

            Assert.AreEqual(7, aggregateRecordTypes.Length);
            foreach (var recordType in aggregateRecordTypes)
            {
                Assert.AreEqual(
                    typeof(PersistedEntityRecord),
                    recordType.BaseType,
                    recordType.Name + " must inherit the canonical persisted entity record.");
                Assert.IsNotNull(
                    recordType.GetConstructor(Type.EmptyTypes),
                    recordType.Name + " must expose a parameterless serialization constructor.");
                Assert.IsNotNull(
                    recordType.GetProperty(nameof(PersistedEntityRecord.RecordSchemaVersion)),
                    recordType.Name + " must carry recordSchemaVersion.");
            }
        }

        [TestMethod]
        public void ConfidentialAndReleasedPersistedShapesAreSegregated()
        {
            AssertHasProperty<PersistedCounselingConfidentialSessionRecord>("InternalNotes");
            AssertHasProperty<PersistedCounselingConfidentialSessionRecord>("RiskLevel");
            AssertDoesNotHaveProperty<PersistedCounselingReleasedSummaryRecord>("InternalNotes");
            AssertDoesNotHaveProperty<PersistedCounselingReleasedSummaryRecord>("RiskLevel");

            AssertHasProperty<PersistedMedicalConfidentialConsultationRecord>("InternalClinicalNotes");
            AssertHasProperty<PersistedMedicalConfidentialConsultationRecord>("InternalAssessment");
            AssertHasProperty<PersistedMedicalConfidentialConsultationRecord>("InternalTreatmentPlan");
            AssertDoesNotHaveProperty<PersistedMedicalReleasedSummaryRecord>("InternalClinicalNotes");
            AssertDoesNotHaveProperty<PersistedMedicalReleasedSummaryRecord>("InternalAssessment");
            AssertDoesNotHaveProperty<PersistedMedicalReleasedSummaryRecord>("InternalTreatmentPlan");

            AssertHasProperty<PersistedDisciplineRestrictedDecisionRecord>("InternalRationale");
            AssertDoesNotHaveProperty<PersistedDisciplineReleasedDecisionRecord>("InternalRationale");
            AssertHasProperty<PersistedDisciplineReleasedDecisionRecord>("ReleasedDecisionSummary");

            Assert.AreEqual(
                typeof(PersistedDisciplineRestrictedDecisionRecord),
                typeof(PersistedDisciplineCaseRecord)
                    .GetProperty("RestrictedDecision")
                    .PropertyType);
            Assert.AreEqual(
                typeof(PersistedDisciplineReleasedDecisionRecord),
                typeof(PersistedDisciplineCaseRecord)
                    .GetProperty("ReleasedDecision")
                    .PropertyType);
        }

        [TestMethod]
        public void PersistedCollectionsInitializeEmptyAndDistinct()
        {
            var book = new PersistedLibraryBookRecord();
            var counseling = new PersistedCounselingCaseRecord();
            var discipline = new PersistedDisciplineCaseRecord();
            var medical = new PersistedMedicalRecord();
            var clearance = new PersistedMedicalClearanceRecord();

            Assert.IsNotNull(book.Copies);
            Assert.AreEqual(0, book.Copies.Count);

            Assert.IsNotNull(counseling.ConfidentialSessions);
            Assert.IsNotNull(counseling.ReleasedSummaries);
            Assert.AreEqual(0, counseling.ConfidentialSessions.Count);
            Assert.AreEqual(0, counseling.ReleasedSummaries.Count);
            Assert.AreNotSame(counseling.ConfidentialSessions, counseling.ReleasedSummaries);

            Assert.IsNotNull(discipline.RestrictedEvidence);
            Assert.IsNotNull(discipline.StudentResponses);
            Assert.IsNotNull(discipline.RestrictedFindings);
            Assert.AreEqual(0, discipline.RestrictedEvidence.Count);
            Assert.AreEqual(0, discipline.StudentResponses.Count);
            Assert.AreEqual(0, discipline.RestrictedFindings.Count);

            Assert.IsNotNull(medical.ConfidentialConsultations);
            Assert.IsNotNull(medical.ReleasedSummaries);
            Assert.AreEqual(0, medical.ConfidentialConsultations.Count);
            Assert.AreEqual(0, medical.ReleasedSummaries.Count);
            Assert.AreNotSame(medical.ConfidentialConsultations, medical.ReleasedSummaries);

            Assert.IsNotNull(clearance.RestrictedHistory);
            Assert.AreEqual(0, clearance.RestrictedHistory.Count);
        }

        [TestMethod]
        public void StudentServiceSchemasExcludeCredentialAndBearerMaterial()
        {
            var forbiddenFragments = new[]
            {
                "password",
                "token",
                "digest",
                "securitystamp",
                "bearer",
                "credential"
            };

            var schemaTypes = typeof(PersistedLibraryBookRecord)
                .Assembly
                .GetExportedTypes()
                .Where(type => type.Namespace == typeof(PersistedLibraryBookRecord).Namespace)
                .Where(type => type.Name.StartsWith("Persisted", StringComparison.Ordinal))
                .ToList();

            foreach (var type in schemaTypes)
            {
                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var normalized = property.Name.Replace("_", string.Empty).ToLowerInvariant();
                    foreach (var fragment in forbiddenFragments)
                    {
                        Assert.IsFalse(
                            normalized.Contains(fragment),
                            type.Name + "." + property.Name
                            + " must not persist credential or bearer material.");
                    }
                }
            }
        }

        [TestMethod]
        public void SevenAdaptersRemainFailClosedUntilSpecializedMappersPass()
        {
            var records = AggregateMapperReadinessCatalog.All;
            var completed = records
                .Where(item => item.Readiness == AggregateMapperReadiness.SpecializedMapperCompleted)
                .ToList();
            var deferred = records
                .Where(item => item.Readiness == AggregateMapperReadiness.DeferredWithExplicitReason)
                .OrderBy(item => item.AdapterName, StringComparer.Ordinal)
                .ToList();

            Assert.AreEqual(11, completed.Count);
            Assert.AreEqual(7, deferred.Count);
            CollectionAssert.AreEqual(
                new[]
                {
                    "ClinicAppointmentRepositoryAdapter",
                    "CounselingCaseRepositoryAdapter",
                    "DisciplineCaseRepositoryAdapter",
                    "LibraryBookRepositoryAdapter",
                    "LibraryBorrowingRepositoryAdapter",
                    "MedicalClearanceRepositoryAdapter",
                    "MedicalRecordRepositoryAdapter"
                },
                deferred.Select(item => item.AdapterName).ToArray());
            Assert.IsTrue(deferred.All(item => !string.IsNullOrWhiteSpace(item.Reason)));
        }

        [TestMethod]
        public void CanonicalJsonNamesExposeSegregatedCollections()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var counseling = JsonSerializer.SerializeToElement(
                new PersistedCounselingCaseRecord(),
                options);
            var discipline = JsonSerializer.SerializeToElement(
                new PersistedDisciplineCaseRecord(),
                options);
            var medical = JsonSerializer.SerializeToElement(
                new PersistedMedicalRecord(),
                options);
            var clearance = JsonSerializer.SerializeToElement(
                new PersistedMedicalClearanceRecord(),
                options);

            Assert.IsTrue(counseling.TryGetProperty("confidentialSessions", out _));
            Assert.IsTrue(counseling.TryGetProperty("releasedSummaries", out _));
            Assert.IsTrue(discipline.TryGetProperty("restrictedEvidence", out _));
            Assert.IsTrue(discipline.TryGetProperty("restrictedFindings", out _));
            Assert.IsTrue(discipline.TryGetProperty("restrictedDecision", out _));
            Assert.IsTrue(discipline.TryGetProperty("releasedDecision", out _));
            Assert.IsTrue(medical.TryGetProperty("confidentialConsultations", out _));
            Assert.IsTrue(medical.TryGetProperty("releasedSummaries", out _));
            Assert.IsTrue(clearance.TryGetProperty("restrictedHistory", out _));
        }

        private static void AssertHasProperty<T>(string propertyName)
        {
            Assert.IsNotNull(
                typeof(T).GetProperty(propertyName),
                typeof(T).Name + " must contain " + propertyName + ".");
        }

        private static void AssertDoesNotHaveProperty<T>(string propertyName)
        {
            Assert.IsNull(
                typeof(T).GetProperty(propertyName),
                typeof(T).Name + " must not contain " + propertyName + ".");
        }
    }
}