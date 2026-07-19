using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Domain.Clinic;
using IUIS.Domain.Common;
using IUIS.Domain.Counseling;
using IUIS.Domain.Discipline;
using IUIS.Domain.Library;
using IUIS.Domain.Time;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class StudentServiceDomainTests
    {
        private static readonly DateTime CreatedAtUtc =
            new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

        private const string ActorId = "USR-2026-999999";
        private const string StudentId = "STU-2026-000001";
        private const string LibrarianId = "USR-2026-000010";
        private const string CounselorEmployeeId = "EMP-2026-000020";
        private const string ClinicianEmployeeId = "EMP-2026-000030";

        [TestMethod]
        public void LibraryInventoryMaintainsLockedCopyEquation()
        {
            var book = CreateBook();
            book.AddCopy(
                "LCP-2026-000002",
                "BC-0002",
                LibraryCopyCondition.Damaged,
                CreatedAtUtc.AddMinutes(2),
                LibrarianId);
            book.AddCopy(
                "LCP-2026-000003",
                "BC-0003",
                LibraryCopyCondition.Good,
                CreatedAtUtc.AddMinutes(3),
                LibrarianId);
            book.MarkCopyOnLoan(
                "LCP-2026-000001",
                CreatedAtUtc.AddMinutes(4),
                LibrarianId);
            book.MarkCopyLost(
                "LCP-2026-000003",
                CreatedAtUtc.AddMinutes(5),
                LibrarianId);

            Assert.AreEqual(3, book.TotalCopies);
            Assert.AreEqual(0, book.AvailableCopies);
            Assert.AreEqual(1, book.ActiveBorrowedCopies);
            Assert.AreEqual(1, book.MaintenanceCopies);
            Assert.AreEqual(1, book.LostCopies);
            Assert.AreEqual(
                book.TotalCopies,
                book.AvailableCopies
                    + book.ActiveBorrowedCopies
                    + book.MaintenanceCopies
                    + book.LostCopies);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void LibraryBookRejectsLoanForMaintenanceCopy()
        {
            var book = CreateBook();
            book.SendCopyToMaintenance(
                "LCP-2026-000001",
                CreatedAtUtc.AddMinutes(2),
                LibrarianId);
            book.MarkCopyOnLoan(
                "LCP-2026-000001",
                CreatedAtUtc.AddMinutes(3),
                LibrarianId);
        }

        [TestMethod]
        public void BorrowingSupportsIssueOverdueRenewAndReturn()
        {
            var borrowing = CreateBorrowing(new InstitutionLocalDate(2026, 1, 10));
            borrowing.Issue(CreatedAtUtc.AddMinutes(1), LibrarianId);
            borrowing.MarkOverdue(
                new InstitutionLocalDate(2026, 1, 11),
                CreatedAtUtc.AddMinutes(2),
                LibrarianId);
            borrowing.Renew(
                new InstitutionLocalDate(2026, 1, 20),
                CreatedAtUtc.AddMinutes(3),
                LibrarianId);
            borrowing.Return(
                CreatedAtUtc.AddMinutes(4),
                LibraryCopyCondition.Good,
                LibrarianId);

            Assert.AreEqual(LibraryBorrowingStatus.Returned, borrowing.Status);
            Assert.AreEqual(1, borrowing.RenewalCount);
            Assert.AreEqual(new InstitutionLocalDate(2026, 1, 20), borrowing.DueDate);
            Assert.AreEqual(LibraryCopyCondition.Good, borrowing.ReturnCondition.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void BorrowingCannotBeMarkedOverdueOnDueDate()
        {
            var borrowing = CreateBorrowing(new InstitutionLocalDate(2026, 1, 10));
            borrowing.Issue(CreatedAtUtc.AddMinutes(1), LibrarianId);
            borrowing.MarkOverdue(
                new InstitutionLocalDate(2026, 1, 10),
                CreatedAtUtc.AddMinutes(2),
                LibrarianId);
        }

        [TestMethod]
        public void LostBorrowingRetainsStableBookAndCopyReferences()
        {
            var borrowing = CreateBorrowing(new InstitutionLocalDate(2026, 1, 10));
            borrowing.Issue(CreatedAtUtc.AddMinutes(1), LibrarianId);
            borrowing.MarkLost(CreatedAtUtc.AddMinutes(2), LibrarianId);

            Assert.AreEqual(LibraryBorrowingStatus.Lost, borrowing.Status);
            Assert.AreEqual("LBK-2026-000001", borrowing.BookId);
            Assert.AreEqual("LCP-2026-000001", borrowing.CopyId);
            Assert.AreEqual(LibrarianId, borrowing.LostRecordedByUserId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void CounselingSessionRequiresAssignedCounselor()
        {
            var counselingCase = CreateCounselingCase();
            counselingCase.RecordSession(
                "CSN-2026-000001",
                CreatedAtUtc.AddHours(2),
                CounselingRiskLevel.Routine,
                "Internal session notes.",
                CreatedAtUtc.AddHours(2),
                ActorId);
        }

        [TestMethod]
        public void CounselingCaseSeparatesConfidentialSessionAndReleasedSummary()
        {
            var counselingCase = CreateAssignedCounselingCase();
            counselingCase.RecordSession(
                "CSN-2026-000001",
                CreatedAtUtc.AddHours(2),
                CounselingRiskLevel.Elevated,
                "Confidential internal counseling narrative.",
                CreatedAtUtc.AddHours(2),
                ActorId);
            counselingCase.ReleaseSessionSummary(
                "CSR-2026-000001",
                "CSN-2026-000001",
                "CRL-2026-000001",
                "Released summary suitable for the authorized recipient.",
                CreatedAtUtc.AddHours(3),
                ActorId);

            Assert.AreEqual(
                "Confidential internal counseling narrative.",
                counselingCase.ConfidentialSessions[0].InternalNotes);
            Assert.AreEqual(
                "Released summary suitable for the authorized recipient.",
                counselingCase.ReleasedSummaries[0].Summary);
            Assert.AreNotEqual(
                counselingCase.ConfidentialSessions[0].InternalNotes,
                counselingCase.ReleasedSummaries[0].Summary);
        }

        [TestMethod]
        public void CounselingReleaseAuthorizationAllowsOnlyExactReleasedScope()
        {
            var authorization = CreateCounselingReleaseAuthorization();

            Assert.IsTrue(
                authorization.Allows(
                    CounselingReleaseScope.ReleasedSessionSummary,
                    CreatedAtUtc.AddHours(2)));
            Assert.IsFalse(
                authorization.Allows(
                    CounselingReleaseScope.ReferralSummary,
                    CreatedAtUtc.AddHours(2)));
        }

        [TestMethod]
        public void RevokedCounselingReleaseAuthorizationIsNotUsable()
        {
            var authorization = CreateCounselingReleaseAuthorization();
            authorization.Revoke(
                "Student withdrew authorization.",
                CreatedAtUtc.AddHours(2),
                ActorId);

            Assert.IsFalse(
                authorization.Allows(
                    CounselingReleaseScope.ReleasedSessionSummary,
                    CreatedAtUtc.AddHours(3)));
            Assert.AreEqual(CounselingReleaseStatus.Revoked, authorization.Status);
        }

        [TestMethod]
        public void CounselingCaseCompletesRequestedToClosedLifecycle()
        {
            var counselingCase = CreateAssignedCounselingCase();
            counselingCase.RecordSession(
                "CSN-2026-000001",
                CreatedAtUtc.AddHours(2),
                CounselingRiskLevel.Routine,
                "Internal notes.",
                CreatedAtUtc.AddHours(2),
                ActorId);
            counselingCase.Close(
                "Counseling objectives completed.",
                CreatedAtUtc.AddHours(3),
                ActorId);

            Assert.AreEqual(CounselingCaseStatus.Closed, counselingCase.Status);
            Assert.AreEqual(5L, counselingCase.Version);
        }

        [TestMethod]
        public void DisciplineCaseCompletesNoticeResponseDecisionLifecycle()
        {
            var disciplineCase = CreateNoticeReleasedDisciplineCase();
            disciplineCase.RecordStudentResponse(
                "DSR-2026-000001",
                "Student response.",
                "response-evidence-001",
                CreatedAtUtc.AddHours(4),
                ActorId);
            disciplineCase.RecordFinding(
                "DFN-2026-000001",
                true,
                "Restricted investigation finding.",
                CreatedAtUtc.AddHours(5),
                ActorId);
            disciplineCase.PrepareDecision(
                "DDC-2026-000001",
                DisciplineDecisionOutcome.CorrectiveAction,
                "Restricted decision rationale.",
                "Complete assigned corrective action.",
                CreatedAtUtc.AddHours(6),
                ActorId);
            disciplineCase.ReleaseDecision(
                "The case was resolved with corrective action.",
                CreatedAtUtc.AddHours(7),
                ActorId);
            disciplineCase.Close(CreatedAtUtc.AddHours(8), ActorId);

            Assert.AreEqual(DisciplineCaseStatus.Closed, disciplineCase.Status);
            Assert.AreEqual(1, disciplineCase.StudentResponses.Count);
            Assert.AreEqual(1, disciplineCase.RestrictedFindings.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void DisciplineDecisionRequiresRecordedFinding()
        {
            var disciplineCase = CreateNoticeReleasedDisciplineCase();
            disciplineCase.PrepareDecision(
                "DDC-2026-000001",
                DisciplineDecisionOutcome.Warning,
                "Internal rationale.",
                "Written warning.",
                CreatedAtUtc.AddHours(4),
                ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void DismissedDisciplineCaseCannotOpenViolation()
        {
            var disciplineCase = CreateDisciplineCase();
            disciplineCase.BeginReview(CreatedAtUtc.AddHours(1), ActorId);
            disciplineCase.Dismiss(
                "Report was not substantiated.",
                CreatedAtUtc.AddHours(2),
                ActorId);
            disciplineCase.ConvertToViolation(
                "VIO-2026-000001",
                "CODE-1",
                "Violation description.",
                DisciplineSeverity.Minor,
                CreatedAtUtc.AddHours(3),
                ActorId);
        }

        [TestMethod]
        public void ReleasedDisciplineDecisionDoesNotReplaceInternalRationale()
        {
            var disciplineCase = CreateNoticeReleasedDisciplineCase();
            disciplineCase.RecordFinding(
                "DFN-2026-000001",
                true,
                "Restricted finding.",
                CreatedAtUtc.AddHours(4),
                ActorId);
            disciplineCase.PrepareDecision(
                "DDC-2026-000001",
                DisciplineDecisionOutcome.Warning,
                "Restricted decision rationale.",
                "Written warning.",
                CreatedAtUtc.AddHours(5),
                ActorId);
            disciplineCase.ReleaseDecision(
                "Released outcome summary.",
                CreatedAtUtc.AddHours(6),
                ActorId);

            Assert.AreEqual(
                "Restricted decision rationale.",
                disciplineCase.Decision.InternalRationale);
            Assert.AreEqual(
                "Released outcome summary.",
                disciplineCase.Decision.ReleasedDecisionSummary);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void DisciplineCaseRejectsDuplicateEvidenceIdentifier()
        {
            var disciplineCase = CreateDisciplineCase();
            disciplineCase.BeginReview(CreatedAtUtc.AddHours(1), ActorId);
            disciplineCase.AddEvidenceReference(
                "DEV-2026-000001",
                "evidence-a",
                "First evidence.",
                CreatedAtUtc.AddHours(2),
                ActorId);
            disciplineCase.AddEvidenceReference(
                "DEV-2026-000001",
                "evidence-b",
                "Duplicate ID.",
                CreatedAtUtc.AddHours(3),
                ActorId);
        }

        [TestMethod]
        public void ClinicAppointmentCompletesWithConsultationReference()
        {
            var appointment = CreateClinicAppointment();
            appointment.Schedule(
                CreatedAtUtc.AddDays(1),
                ClinicianEmployeeId,
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            appointment.Confirm(CreatedAtUtc.AddMinutes(2), ActorId);
            appointment.CheckIn(CreatedAtUtc.AddDays(1), ActorId);
            appointment.Complete(
                "CON-2026-000001",
                CreatedAtUtc.AddDays(1).AddMinutes(30),
                ActorId);

            Assert.AreEqual(ClinicAppointmentStatus.Completed, appointment.Status);
            Assert.AreEqual("CON-2026-000001", appointment.ConsultationId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void ClinicAppointmentCannotCheckInBeforeConfirmation()
        {
            CreateClinicAppointment().CheckIn(CreatedAtUtc.AddHours(1), ActorId);
        }

        [TestMethod]
        public void MedicalRecordSeparatesConfidentialConsultationAndReleasedSummary()
        {
            var record = CreateMedicalRecord();
            AddConsultation(record);
            record.ReleaseConsultationSummary(
                "MRS-2026-000001",
                "CON-2026-000001",
                "Released consultation summary.",
                CreatedAtUtc.AddHours(3),
                ActorId);

            Assert.AreEqual(
                "Confidential clinical notes.",
                record.ConfidentialConsultations[0].InternalClinicalNotes);
            Assert.AreEqual(
                "Released consultation summary.",
                record.ReleasedSummaries[0].ReleasedSummary);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void ClosedMedicalRecordRejectsAdditionalConsultation()
        {
            var record = CreateMedicalRecord();
            AddConsultation(record);
            record.Close(CreatedAtUtc.AddHours(3), ActorId);
            record.AddConsultation(
                "CON-2026-000002",
                "CAP-2026-000002",
                ClinicianEmployeeId,
                CreatedAtUtc.AddHours(4),
                "More notes.",
                "More assessment.",
                null,
                CreatedAtUtc.AddHours(4),
                ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void MedicalRecordCannotBeArchived()
        {
            CreateMedicalRecord().Archive(CreatedAtUtc.AddHours(1), ActorId);
        }

        [TestMethod]
        public void MedicalClearanceFollowsRequestReviewIssueLifecycle()
        {
            var clearance = CreateMedicalClearance();
            clearance.BeginReview(
                "MCH-2026-000002",
                ClinicianEmployeeId,
                CreatedAtUtc.AddHours(1),
                ActorId);
            clearance.Issue(
                "MCH-2026-000003",
                "MCN-2026-000001",
                new InstitutionLocalDate(2026, 1, 2),
                new InstitutionLocalDate(2026, 1, 31),
                "Student is medically cleared for the stated purpose.",
                CreatedAtUtc.AddHours(2),
                ActorId);

            Assert.AreEqual(MedicalClearanceStatus.Issued, clearance.Status);
            Assert.AreEqual("MCN-2026-000001", clearance.ClearanceNumber);
            Assert.AreEqual(3, clearance.History.Count);
            Assert.IsTrue(clearance.IsValidOn(new InstitutionLocalDate(2026, 1, 15)));
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void MedicalClearanceCannotIssueBeforeReview()
        {
            CreateMedicalClearance().Issue(
                "MCH-2026-000002",
                "MCN-2026-000001",
                new InstitutionLocalDate(2026, 1, 2),
                null,
                "Released clearance summary.",
                CreatedAtUtc.AddHours(1),
                ActorId);
        }

        [TestMethod]
        public void RevokedMedicalClearanceRetainsNumberAndHistory()
        {
            var clearance = CreateMedicalClearance();
            clearance.BeginReview(
                "MCH-2026-000002",
                ClinicianEmployeeId,
                CreatedAtUtc.AddHours(1),
                ActorId);
            clearance.Issue(
                "MCH-2026-000003",
                "MCN-2026-000001",
                new InstitutionLocalDate(2026, 1, 2),
                null,
                "Released clearance summary.",
                CreatedAtUtc.AddHours(2),
                ActorId);
            clearance.Revoke(
                "Material medical information changed.",
                CreatedAtUtc.AddHours(3),
                ActorId,
                "MCH-2026-000004");

            Assert.AreEqual(MedicalClearanceStatus.Revoked, clearance.Status);
            Assert.AreEqual("MCN-2026-000001", clearance.ClearanceNumber);
            Assert.AreEqual(4, clearance.History.Count);
            Assert.IsFalse(clearance.IsValidOn(new InstitutionLocalDate(2026, 1, 15)));
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void MedicalClearanceRejectsNonClearanceNumberPrefix()
        {
            var clearance = CreateMedicalClearance();
            clearance.BeginReview(
                "MCH-2026-000002",
                ClinicianEmployeeId,
                CreatedAtUtc.AddHours(1),
                ActorId);
            clearance.Issue(
                "MCH-2026-000003",
                "MCL-2026-000099",
                new InstitutionLocalDate(2026, 1, 2),
                null,
                "Released clearance summary.",
                CreatedAtUtc.AddHours(2),
                ActorId);
        }

        private static LibraryBook CreateBook()
        {
            var book = new LibraryBook(
                "LBK-2026-000001",
                "978-971-0000-01-1",
                "Integrated Systems",
                "A. Author",
                "University Press",
                "Information Technology",
                CreatedAtUtc,
                LibrarianId);
            book.AddCopy(
                "LCP-2026-000001",
                "BC-0001",
                LibraryCopyCondition.Good,
                CreatedAtUtc.AddMinutes(1),
                LibrarianId);
            return book;
        }

        private static LibraryBorrowing CreateBorrowing(InstitutionLocalDate dueDate)
        {
            return new LibraryBorrowing(
                "BRW-2026-000001",
                StudentId,
                "LBK-2026-000001",
                "LCP-2026-000001",
                dueDate,
                CreatedAtUtc,
                LibrarianId);
        }

        private static CounselingCase CreateCounselingCase()
        {
            return new CounselingCase(
                "CNS-2026-000001",
                StudentId,
                CreatedAtUtc.AddDays(1),
                "Student requested counseling support.",
                CreatedAtUtc,
                ActorId);
        }

        private static CounselingCase CreateAssignedCounselingCase()
        {
            var counselingCase = CreateCounselingCase();
            counselingCase.ConfirmAppointment(
                CreatedAtUtc.AddDays(1),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            counselingCase.AssignCounselor(
                CounselorEmployeeId,
                CreatedAtUtc.AddMinutes(2),
                ActorId);
            return counselingCase;
        }

        private static CounselingReleaseAuthorization CreateCounselingReleaseAuthorization()
        {
            return new CounselingReleaseAuthorization(
                "CRL-2026-000001",
                "CNS-2026-000001",
                StudentId,
                "Authorized university office",
                "Confirm released counseling outcome.",
                CounselingReleaseScope.ReleasedSessionSummary,
                CreatedAtUtc.AddHours(1),
                CreatedAtUtc.AddDays(1),
                CreatedAtUtc,
                ActorId);
        }

        private static DisciplineCase CreateDisciplineCase()
        {
            return new DisciplineCase(
                "DIN-2026-000001",
                StudentId,
                CreatedAtUtc,
                "Main Campus",
                "Restricted incident narrative.",
                ActorId,
                CreatedAtUtc,
                ActorId);
        }

        private static DisciplineCase CreateNoticeReleasedDisciplineCase()
        {
            var disciplineCase = CreateDisciplineCase();
            disciplineCase.BeginReview(CreatedAtUtc.AddHours(1), ActorId);
            disciplineCase.ConvertToViolation(
                "VIO-2026-000001",
                "CODE-1",
                "Violation description.",
                DisciplineSeverity.Moderate,
                CreatedAtUtc.AddHours(2),
                ActorId);
            disciplineCase.ReleaseNotice(
                "DNT-2026-000001",
                "Released notice summary.",
                new InstitutionLocalDate(2026, 1, 15),
                CreatedAtUtc.AddHours(3),
                ActorId);
            return disciplineCase;
        }

        private static ClinicAppointment CreateClinicAppointment()
        {
            return new ClinicAppointment(
                "CAP-2026-000001",
                StudentId,
                CreatedAtUtc.AddDays(1),
                "General consultation request.",
                CreatedAtUtc,
                ActorId);
        }

        private static MedicalRecord CreateMedicalRecord()
        {
            return new MedicalRecord(
                "MDR-2026-000001",
                StudentId,
                CreatedAtUtc,
                ActorId);
        }

        private static void AddConsultation(MedicalRecord record)
        {
            record.AddConsultation(
                "CON-2026-000001",
                "CAP-2026-000001",
                ClinicianEmployeeId,
                CreatedAtUtc.AddHours(2),
                "Confidential clinical notes.",
                "Confidential clinical assessment.",
                "Confidential treatment plan.",
                CreatedAtUtc.AddHours(2),
                ActorId);
        }

        private static MedicalClearance CreateMedicalClearance()
        {
            return new MedicalClearance(
                "MCL-2026-000001",
                "MCH-2026-000001",
                StudentId,
                "MDR-2026-000001",
                "Medical clearance requested.",
                CreatedAtUtc,
                ActorId);
        }
    }
}
