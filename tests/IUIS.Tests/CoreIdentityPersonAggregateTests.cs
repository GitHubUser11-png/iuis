using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.People;
using IUIS.Domain.Time;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class CoreIdentityPersonAggregateTests
    {
        private static readonly DateTime CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void InstitutionIdentifierUsesCanonicalFormat()
        {
            var identifier = InstitutionIdentifier.Create("stu", 2026, 1);
            Assert.AreEqual("STU-2026-000001", identifier.Value);
            Assert.AreEqual(identifier, InstitutionIdentifier.Parse(identifier.Value));
        }

        [TestMethod]
        public void InstitutionIdentifierRejectsZeroSequence()
        {
            Assert.ThrowsExactly<DomainValidationException>(() =>
            {
                InstitutionIdentifier.Create("STU", 2026, 0);
            });
        }

        [TestMethod]
        public void StudentAccountNormalizesLoginAndLinksStudentRecord()
        {
            var account = new UserAccount(
                "USR-2026-000001",
                "  Student.One  ",
                PrimaryRole.Student,
                PersonRecordKind.Student,
                "STU-2026-000001",
                "credential-hash",
                "security-stamp",
                CreatedAtUtc,
                "USR-2026-999999");

            Assert.AreEqual("student.one", account.LoginId);
            Assert.AreEqual("STU-2026-000001", account.PersonRecordId);
            Assert.AreEqual(UserAccountStatus.Active, account.Status);
        }

        [TestMethod]
        public void StudentAccountRejectsEmployeeRecordLink()
        {
            Assert.ThrowsExactly<DomainValidationException>(() =>
            {
                new UserAccount(
                    "USR-2026-000001",
                    "student.one",
                    PrimaryRole.Student,
                    PersonRecordKind.EmployeeFaculty,
                    "EMP-2026-000001",
                    "credential-hash",
                    "security-stamp",
                    CreatedAtUtc,
                    "USR-2026-999999");
            });
        }

        [TestMethod]
        public void SessionRequiresMatchingStampAndValidWindow()
        {
            var session = new UserSession(
                "SES-2026-000001",
                "USR-2026-000001",
                "token-hash",
                "security-stamp",
                SessionApplicationKind.UserApplication,
                SessionPurpose.FullAccess,
                CreatedAtUtc,
                CreatedAtUtc.AddMinutes(15),
                CreatedAtUtc.AddHours(8),
                "USR-2026-000001");

            Assert.IsTrue(session.IsUsableAt(CreatedAtUtc.AddMinutes(1), "security-stamp"));
            Assert.IsFalse(session.IsUsableAt(CreatedAtUtc.AddMinutes(1), "other-stamp"));
        }

        [TestMethod]
        public void SessionRevocationInvalidatesSession()
        {
            var session = new UserSession(
                "SES-2026-000001",
                "USR-2026-000001",
                "token-hash",
                "security-stamp",
                SessionApplicationKind.UserApplication,
                SessionPurpose.FullAccess,
                CreatedAtUtc,
                CreatedAtUtc.AddMinutes(15),
                CreatedAtUtc.AddHours(8),
                "USR-2026-000001");

            session.Revoke(CreatedAtUtc.AddMinutes(2), "USR-2026-000001", "Password changed");
            Assert.AreEqual(UserSessionStatus.Revoked, session.Status);
            Assert.IsFalse(session.IsUsableAt(CreatedAtUtc.AddMinutes(3), "security-stamp"));
        }

        [TestMethod]
        public void StudentRecordPreservesImmutableStudentNumber()
        {
            var student = CreateStudent();
            Assert.AreEqual("STU-2026-000001", student.StudentNumber);
            Assert.AreEqual(StudentStatus.Active, student.Status);
        }

        [TestMethod]
        public void EmployeeAssignmentChangeAdvancesVersion()
        {
            var employee = CreateEmployee();
            employee.ChangeAssignment(
                "DEPT-IT",
                "Associate Professor",
                true,
                CreatedAtUtc.AddMinutes(1),
                "USR-2026-999999");

            Assert.AreEqual(2L, employee.Version);
            Assert.AreEqual("Associate Professor", employee.PositionTitle);
            Assert.IsTrue(employee.IsFaculty);
        }

        private static StudentRecord CreateStudent()
        {
            return new StudentRecord(
                "STU-2026-000001",
                "STU-2026-000001",
                new PersonName("Juan", null, "Dela Cruz", null),
                new ContactInformation("juan@example.edu", "+639171234567", null),
                new PostalAddress("1 Main Street", null, "Poblacion", "Malvar", "Batangas", "4233", "PH"),
                new InstitutionLocalDate(2005, 1, 1),
                "CRS-2026-000001",
                StudentStatus.Active,
                CreatedAtUtc,
                "USR-2026-999999");
        }

        private static EmployeeRecord CreateEmployee()
        {
            return new EmployeeRecord(
                "EMP-2026-000001",
                "EMP-2026-000001",
                new PersonName("Maria", null, "Santos", null),
                new ContactInformation("maria@example.edu", "+639181234567", null),
                new PostalAddress("2 Main Street", null, "Poblacion", "Malvar", "Batangas", "4233", "PH"),
                new InstitutionLocalDate(1990, 1, 1),
                "DEPT-IT",
                "Instructor",
                EmploymentStatus.Active,
                true,
                CreatedAtUtc,
                "USR-2026-999999");
        }
    }
}
