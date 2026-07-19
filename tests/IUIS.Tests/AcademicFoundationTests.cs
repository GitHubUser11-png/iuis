using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Domain.Academic;
using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.People;
using IUIS.Domain.Time;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class AcademicFoundationTests
    {
        private static readonly DateTime CreatedAtUtc =
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private const string ActorId = "USR-2026-999999";

        [TestMethod]
        public void CourseNormalizesCodeAndUsesCanonicalIdentifier()
        {
            var course = CreateCourse();

            Assert.AreEqual("CRS-2026-000001", course.Id);
            Assert.AreEqual("BSIT", course.Code);
            Assert.AreEqual(CourseStatus.Draft, course.Status);
            Assert.AreEqual(4, course.DurationYears);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void CourseCannotReactivateAfterRetirement()
        {
            var course = CreateCourse();
            course.ChangeStatus(CourseStatus.Retired, CreatedAtUtc.AddMinutes(1), ActorId);
            course.ChangeStatus(CourseStatus.Active, CreatedAtUtc.AddMinutes(2), ActorId);
        }

        [TestMethod]
        public void CurriculumCalculatesUnitTotalAndApproves()
        {
            var curriculum = CreateCurriculum();
            curriculum.AddSubject(
                new CurriculumSubject("SUB-2026-000001", 1, 1, 3.00m, true),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            curriculum.AddSubject(
                new CurriculumSubject("SUB-2026-000002", 1, 1, 1.50m, true),
                CreatedAtUtc.AddMinutes(2),
                ActorId);

            curriculum.Approve(CreatedAtUtc.AddMinutes(3), ActorId);

            Assert.AreEqual(4.50m, curriculum.TotalUnits);
            Assert.AreEqual(CurriculumStatus.Approved, curriculum.Status);
            Assert.AreEqual(4L, curriculum.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void CurriculumRejectsDuplicateSubject()
        {
            var curriculum = CreateCurriculum();
            curriculum.AddSubject(
                new CurriculumSubject("SUB-2026-000001", 1, 1, 3.00m, true),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            curriculum.AddSubject(
                new CurriculumSubject("SUB-2026-000001", 2, 1, 3.00m, true),
                CreatedAtUtc.AddMinutes(2),
                ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void CurriculumApprovalRequiresSubject()
        {
            CreateCurriculum().Approve(CreatedAtUtc.AddMinutes(1), ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void ApprovedCurriculumCannotChangeSubjects()
        {
            var curriculum = CreateCurriculum();
            curriculum.AddSubject(
                new CurriculumSubject("SUB-2026-000001", 1, 1, 3.00m, true),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            curriculum.Approve(CreatedAtUtc.AddMinutes(2), ActorId);
            curriculum.AddSubject(
                new CurriculumSubject("SUB-2026-000002", 1, 1, 3.00m, true),
                CreatedAtUtc.AddMinutes(3),
                ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void SubjectRejectsSelfPrerequisite()
        {
            var subject = CreateSubject(1, "IT101", "Introduction to Computing", 3.00m);
            subject.AddPrerequisite(
                new SubjectPrerequisite(subject.Id),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
        }

        [TestMethod]
        public void PrerequisiteGraphAcceptsAcyclicChain()
        {
            var first = CreateSubject(1, "IT101", "Introduction to Computing", 3.00m);
            var second = CreateSubject(2, "IT102", "Programming Fundamentals", 3.00m);
            var third = CreateSubject(3, "IT201", "Data Structures", 3.00m);

            second.AddPrerequisite(
                new SubjectPrerequisite(first.Id),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            third.AddPrerequisite(
                new SubjectPrerequisite(second.Id),
                CreatedAtUtc.AddMinutes(1),
                ActorId);

            SubjectPrerequisiteGraph.ValidateAcyclic(
                new[] { third, first, second });
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PrerequisiteGraphRejectsCycle()
        {
            var first = CreateSubject(1, "IT101", "Introduction to Computing", 3.00m);
            var second = CreateSubject(2, "IT102", "Programming Fundamentals", 3.00m);
            var third = CreateSubject(3, "IT201", "Data Structures", 3.00m);

            first.AddPrerequisite(
                new SubjectPrerequisite(third.Id),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            second.AddPrerequisite(
                new SubjectPrerequisite(first.Id),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            third.AddPrerequisite(
                new SubjectPrerequisite(second.Id),
                CreatedAtUtc.AddMinutes(1),
                ActorId);

            SubjectPrerequisiteGraph.ValidateAcyclic(
                new[] { first, second, third });
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void PrerequisiteGraphRejectsMissingSubjectReference()
        {
            var subject = CreateSubject(1, "IT201", "Data Structures", 3.00m);
            subject.AddPrerequisite(
                new SubjectPrerequisite("SUB-2026-000999"),
                CreatedAtUtc.AddMinutes(1),
                ActorId);

            SubjectPrerequisiteGraph.ValidateAcyclic(new[] { subject });
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void AcademicPeriodRejectsInvalidDateOrder()
        {
            new AcademicPeriod(
                "APD-2026-000001",
                "AY2026-T1",
                "Academic Year 2026 First Term",
                new InstitutionLocalDate(2026, 6, 1),
                new InstitutionLocalDate(2026, 5, 31),
                new InstitutionLocalDate(2026, 8, 1),
                new InstitutionLocalDate(2026, 12, 15),
                CreatedAtUtc,
                ActorId);
        }

        [TestMethod]
        public void AcademicPeriodFollowsCanonicalLifecycle()
        {
            var period = CreateAcademicPeriod();

            period.TransitionTo(
                AcademicPeriodStatus.Scheduled,
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            period.TransitionTo(
                AcademicPeriodStatus.EnrollmentOpen,
                CreatedAtUtc.AddMinutes(2),
                ActorId);
            period.TransitionTo(
                AcademicPeriodStatus.EnrollmentClosed,
                CreatedAtUtc.AddMinutes(3),
                ActorId);
            period.TransitionTo(
                AcademicPeriodStatus.InProgress,
                CreatedAtUtc.AddMinutes(4),
                ActorId);
            period.TransitionTo(
                AcademicPeriodStatus.Completed,
                CreatedAtUtc.AddMinutes(5),
                ActorId);

            Assert.AreEqual(AcademicPeriodStatus.Completed, period.Status);
            Assert.AreEqual(6L, period.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void AcademicPeriodCannotSkipLifecycleStages()
        {
            CreateAcademicPeriod().TransitionTo(
                AcademicPeriodStatus.EnrollmentOpen,
                CreatedAtUtc.AddMinutes(1),
                ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void EnrollmentRequiresSubjectBeforeSubmission()
        {
            CreateEnrollment().Submit(CreatedAtUtc.AddMinutes(1), ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void EnrollmentRejectsDuplicateSubjectLine()
        {
            var enrollment = CreateEnrollment();
            enrollment.AddSubjectLine(
                CreateEnrollmentLine(1, "IT101", "Introduction to Computing", 3.00m),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            enrollment.AddSubjectLine(
                CreateEnrollmentLine(1, "IT101", "Introduction to Computing", 3.00m),
                CreatedAtUtc.AddMinutes(2),
                ActorId);
        }

        [TestMethod]
        public void EnrollmentPreservesCourseCurriculumAndSubjectSnapshots()
        {
            var subject = CreateSubject(1, "IT101", "Introduction to Computing", 3.00m);
            var line = new EnrollmentSubjectLine(
                subject.Id,
                subject.Code,
                subject.Title,
                subject.Units,
                1,
                1,
                true,
                "IT1-A");
            var enrollment = CreateEnrollment();
            enrollment.AddSubjectLine(line, CreatedAtUtc.AddMinutes(1), ActorId);

            subject.UpdateDetails(
                "Foundations of Computing",
                4.00m,
                CreatedAtUtc.AddMinutes(1),
                ActorId);

            Assert.AreEqual("BSIT", enrollment.CourseCodeSnapshot);
            Assert.AreEqual("CUR-2026-000001", enrollment.CurriculumIdSnapshot);
            Assert.AreEqual("Introduction to Computing", line.SubjectTitleSnapshot);
            Assert.AreEqual(3.00m, line.UnitsSnapshot);
            Assert.AreEqual(3.00m, enrollment.TotalUnits);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void EnrollmentApprovalRequiresReview()
        {
            var enrollment = CreateEnrollmentWithOneLine();
            enrollment.Submit(CreatedAtUtc.AddMinutes(2), ActorId);
            enrollment.Approve(null, CreatedAtUtc.AddMinutes(3), ActorId);
        }

        [TestMethod]
        public void ReturnedEnrollmentCanBeEditedAndResubmitted()
        {
            var enrollment = CreateEnrollmentWithOneLine();
            enrollment.Submit(CreatedAtUtc.AddMinutes(2), ActorId);
            enrollment.BeginReview(CreatedAtUtc.AddMinutes(3), ActorId);
            enrollment.ReturnForCorrection(
                "Add the required laboratory Subject.",
                CreatedAtUtc.AddMinutes(4),
                ActorId);
            enrollment.AddSubjectLine(
                CreateEnrollmentLine(2, "IT101L", "Introduction to Computing Laboratory", 1.00m),
                CreatedAtUtc.AddMinutes(5),
                ActorId);
            enrollment.Submit(CreatedAtUtc.AddMinutes(6), ActorId);

            Assert.AreEqual(EnrollmentStatus.Submitted, enrollment.Status);
            Assert.AreEqual(2, enrollment.SubjectLines.Count);
            Assert.AreEqual(4.00m, enrollment.TotalUnits);
            Assert.IsNull(enrollment.DecisionReason);
        }

        [TestMethod]
        public void ReviewedEnrollmentCanBeApprovedWithoutCreatingFinanceState()
        {
            var enrollment = CreateEnrollmentWithOneLine();
            enrollment.Submit(CreatedAtUtc.AddMinutes(2), ActorId);
            enrollment.BeginReview(CreatedAtUtc.AddMinutes(3), ActorId);
            enrollment.Approve(
                "Academic requirements verified.",
                CreatedAtUtc.AddMinutes(4),
                ActorId);

            Assert.AreEqual(EnrollmentStatus.Approved, enrollment.Status);
            Assert.AreEqual("Academic requirements verified.", enrollment.DecisionReason);
            Assert.AreEqual(4L, enrollment.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void StudentRecordRejectsNonCanonicalCourseIdentifier()
        {
            new StudentRecord(
                "STU-2026-000001",
                "STU-2026-000001",
                new PersonName("Juan", null, "Dela Cruz", null),
                new ContactInformation("juan@example.edu", "+639171234567", null),
                new PostalAddress(
                    "1 Main Street",
                    null,
                    "Poblacion",
                    "Malvar",
                    "Batangas",
                    "4233",
                    "PH"),
                new InstitutionLocalDate(2005, 1, 1),
                "CRS-BSIT",
                StudentStatus.Active,
                CreatedAtUtc,
                ActorId);
        }

        [TestMethod]
        [ExpectedException(typeof(DomainValidationException))]
        public void AcademicUnitsRejectMoreThanTwoFractionalDigits()
        {
            AcademicUnitRules.RequireValid(3.125m, "units");
        }

        private static Course CreateCourse()
        {
            return new Course(
                "CRS-2026-000001",
                "bsit",
                "Bachelor of Science in Information Technology",
                "DEPT-CICS",
                4,
                CreatedAtUtc,
                ActorId);
        }

        private static Curriculum CreateCurriculum()
        {
            return new Curriculum(
                "CUR-2026-000001",
                "CRS-2026-000001",
                "2026.1",
                2026,
                CreatedAtUtc,
                ActorId);
        }

        private static Subject CreateSubject(
            int sequence,
            string code,
            string title,
            decimal units)
        {
            return new Subject(
                InstitutionIdentifier.Create("SUB", 2026, sequence).Value,
                code,
                title,
                units,
                CreatedAtUtc,
                ActorId);
        }

        private static AcademicPeriod CreateAcademicPeriod()
        {
            return new AcademicPeriod(
                "APD-2026-000001",
                "AY2026-T1",
                "Academic Year 2026 First Term",
                new InstitutionLocalDate(2026, 6, 1),
                new InstitutionLocalDate(2026, 7, 31),
                new InstitutionLocalDate(2026, 8, 1),
                new InstitutionLocalDate(2026, 12, 15),
                CreatedAtUtc,
                ActorId);
        }

        private static Enrollment CreateEnrollment()
        {
            return new Enrollment(
                "ENR-2026-000001",
                "STU-2026-000001",
                "APD-2026-000001",
                "CRS-2026-000001",
                "BSIT",
                "Bachelor of Science in Information Technology",
                "CUR-2026-000001",
                "2026.1",
                CreatedAtUtc,
                ActorId);
        }

        private static Enrollment CreateEnrollmentWithOneLine()
        {
            var enrollment = CreateEnrollment();
            enrollment.AddSubjectLine(
                CreateEnrollmentLine(1, "IT101", "Introduction to Computing", 3.00m),
                CreatedAtUtc.AddMinutes(1),
                ActorId);
            return enrollment;
        }

        private static EnrollmentSubjectLine CreateEnrollmentLine(
            int sequence,
            string code,
            string title,
            decimal units)
        {
            return new EnrollmentSubjectLine(
                InstitutionIdentifier.Create("SUB", 2026, sequence).Value,
                code,
                title,
                units,
                1,
                1,
                true,
                "IT1-A");
        }
    }
}
