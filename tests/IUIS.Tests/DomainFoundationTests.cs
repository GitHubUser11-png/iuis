using System;

using IUIS.Domain.Common;
using IUIS.Domain.Finance;
using IUIS.Domain.Identity;
using IUIS.Domain.People;
using IUIS.Domain.Time;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class DomainFoundationTests
    {
        [TestMethod]
        public void EntityBaseStartsAtVersionOneWithUtcAuditMetadata()
        {
            var createdAtUtc = Utc(2026, 7, 17, 1, 2, 3);
            var entity = new TestEntity("TST-2026-000001", createdAtUtc, "USR-2026-000001");

            Assert.AreEqual("TST-2026-000001", entity.Id);
            Assert.AreEqual(1L, entity.Version);
            Assert.AreEqual(createdAtUtc, entity.CreatedAtUtc);
            Assert.AreEqual(createdAtUtc, entity.UpdatedAtUtc);
            Assert.AreEqual("USR-2026-000001", entity.CreatedByUserId);
            Assert.IsFalse(entity.IsArchived);
        }

        [TestMethod]
        public void EntityBaseChangeAdvancesVersionAndAuditMetadata()
        {
            var entity = NewEntity();
            var changedAtUtc = Utc(2026, 7, 17, 2, 0, 0);

            entity.Change(changedAtUtc, "USR-2026-000002");

            Assert.AreEqual(2L, entity.Version);
            Assert.AreEqual(changedAtUtc, entity.UpdatedAtUtc);
            Assert.AreEqual("USR-2026-000002", entity.UpdatedByUserId);
        }

        [TestMethod]
        public void EntityBaseRejectsNonUtcCreationTimestamp()
        {
            Assert.ThrowsException<DomainValidationException>(() =>
                new TestEntity(
                    "TST-2026-000001",
                    new DateTime(2026, 7, 17, 1, 2, 3, DateTimeKind.Local),
                    "USR-2026-000001"));
        }

        [TestMethod]
        public void EntityBaseArchiveAndRestorePreserveHistoryAndAdvanceVersion()
        {
            var entity = NewEntity();
            var archivedAtUtc = Utc(2026, 7, 17, 2, 0, 0);
            var restoredAtUtc = Utc(2026, 7, 17, 3, 0, 0);

            entity.Archive(archivedAtUtc, "USR-2026-000002");

            Assert.IsTrue(entity.IsArchived);
            Assert.AreEqual(2L, entity.Version);
            Assert.AreEqual(archivedAtUtc, entity.ArchivedAtUtc);
            Assert.AreEqual("USR-2026-000002", entity.ArchivedByUserId);

            entity.Restore(restoredAtUtc, "USR-2026-000003");

            Assert.IsFalse(entity.IsArchived);
            Assert.AreEqual(3L, entity.Version);
            Assert.IsNull(entity.ArchivedAtUtc);
            Assert.IsNull(entity.ArchivedByUserId);
        }

        [TestMethod]
        public void EntityBaseRejectsChangesEarlierThanCurrentAuditTimestamp()
        {
            var entity = NewEntity();

            Assert.ThrowsException<DomainValidationException>(() =>
                entity.Change(Utc(2026, 7, 16, 23, 59, 59), "USR-2026-000002"));
        }

        [TestMethod]
        public void PersonNameNormalizesWhitespaceAndBuildsCanonicalDisplayNames()
        {
            var name = new PersonName("  Neil   Aldrin ", " D. ", " Tacla ", " Jr. ");

            Assert.AreEqual("Neil Aldrin", name.GivenName);
            Assert.AreEqual("D.", name.MiddleName);
            Assert.AreEqual("Tacla", name.FamilyName);
            Assert.AreEqual("Jr.", name.Suffix);
            Assert.AreEqual("Neil Aldrin D. Tacla, Jr.", name.DisplayName);
            Assert.AreEqual("Tacla, Neil Aldrin D. Jr.", name.SortName);
        }

        [TestMethod]
        public void PersonNameRejectsMissingRequiredFamilyName()
        {
            Assert.ThrowsException<DomainValidationException>(() =>
                new PersonName("Neil", null, "  ", null));
        }

        [TestMethod]
        public void PersonNameEqualityIsCaseInsensitiveAfterNormalization()
        {
            var first = new PersonName("Neil", null, "Tacla", null);
            var second = new PersonName("neil", null, "TACLA", null);

            Assert.AreEqual(first, second);
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }

        [TestMethod]
        public void ContactInformationNormalizesEmailAndPhoneNumbers()
        {
            var contact = new ContactInformation(
                " Student.Name@Example.COM ",
                "+63 (917) 123-4567",
                "043-123-4567");

            Assert.AreEqual("student.name@example.com", contact.EmailAddress);
            Assert.AreEqual("+639171234567", contact.MobileNumber);
            Assert.AreEqual("0431234567", contact.AlternatePhoneNumber);
            Assert.IsFalse(contact.IsEmpty);
        }

        [TestMethod]
        public void ContactInformationRejectsMalformedEmail()
        {
            Assert.ThrowsException<DomainValidationException>(() =>
                new ContactInformation("invalid@example", null, null));
        }

        [TestMethod]
        public void ContactInformationRejectsSeparatorOnlyPhoneValue()
        {
            Assert.ThrowsException<DomainValidationException>(() =>
                new ContactInformation(null, "---", null));
        }

        [TestMethod]
        public void PostalAddressNormalizesCountryAndProducesSingleLineRepresentation()
        {
            var address = new PostalAddress(
                " 123  Main Street ",
                null,
                " Barangay 1 ",
                " Malvar ",
                " Batangas ",
                "4233",
                "ph");

            Assert.AreEqual("PH", address.CountryCode);
            Assert.AreEqual(
                "123 Main Street, Barangay 1, Malvar, Batangas, 4233, PH",
                address.SingleLine);
        }

        [TestMethod]
        public void PostalAddressRejectsInvalidCountryCode()
        {
            Assert.ThrowsException<DomainValidationException>(() =>
                new PostalAddress("Line 1", null, null, "Malvar", "Batangas", null, "PHL"));
        }

        [TestMethod]
        public void InstitutionLocalDateParsesCanonicalLeapDay()
        {
            var date = InstitutionLocalDate.Parse("2024-02-29");

            Assert.AreEqual(2024, date.Year);
            Assert.AreEqual(2, date.Month);
            Assert.AreEqual(29, date.Day);
            Assert.AreEqual("2024-02-29", date.ToString());
            Assert.AreEqual(DateTimeKind.Unspecified, date.ToDateTimeUnspecified().Kind);
        }

        [TestMethod]
        public void InstitutionLocalDateRejectsInvalidOrNonCanonicalDate()
        {
            InstitutionLocalDate ignored;

            Assert.IsFalse(InstitutionLocalDate.TryParse("2023-02-29", out ignored));
            Assert.IsFalse(InstitutionLocalDate.TryParse("02/28/2026", out ignored));
        }

        [TestMethod]
        public void InstitutionLocalDateSupportsDeterministicOrdering()
        {
            var earlier = new InstitutionLocalDate(2026, 7, 16);
            var later = new InstitutionLocalDate(2026, 7, 17);

            Assert.IsTrue(earlier < later);
            Assert.IsTrue(later > earlier);
            Assert.IsTrue(earlier <= earlier);
        }

        [TestMethod]
        public void MoneyRoundsToTwoDecimalsAwayFromZero()
        {
            Assert.AreEqual(10.01m, Money.PhilippinePeso(10.005m).Amount);
            Assert.AreEqual(-10.01m, Money.PhilippinePeso(-10.005m).Amount);
        }

        [TestMethod]
        public void MoneyArithmeticRequiresMatchingCurrencies()
        {
            var pesos = Money.PhilippinePeso(100m);
            var dollars = new Money(10m, "USD");

            Assert.ThrowsException<DomainValidationException>(() => pesos.Add(dollars));
        }

        [TestMethod]
        public void MoneyArithmeticReturnsRoundedImmutableValue()
        {
            var first = Money.PhilippinePeso(100.10m);
            var second = Money.PhilippinePeso(25.25m);
            var result = first.Add(second);

            Assert.AreEqual(125.35m, result.Amount);
            Assert.AreEqual("PHP", result.CurrencyCode);
            Assert.AreEqual(100.10m, first.Amount);
        }

        [TestMethod]
        public void MoneyNonNegativeGuardRejectsNegativeAmount()
        {
            Assert.ThrowsException<DomainValidationException>(() =>
                Money.PhilippinePeso(-0.01m).RequireNonNegative("amount"));
        }

        [TestMethod]
        public void IdentityPolicySeparatesAdministratorAndGeneralApplications()
        {
            Assert.IsTrue(IdentityPolicy.IsCompatible(
                PrimaryRole.Student,
                SessionApplicationKind.UserApplication));
            Assert.IsTrue(IdentityPolicy.IsCompatible(
                PrimaryRole.EmployeeFaculty,
                SessionApplicationKind.UserApplication));
            Assert.IsTrue(IdentityPolicy.IsCompatible(
                PrimaryRole.Administrator,
                SessionApplicationKind.AdministratorApplication));
            Assert.IsFalse(IdentityPolicy.IsCompatible(
                PrimaryRole.Administrator,
                SessionApplicationKind.UserApplication));
            Assert.IsFalse(IdentityPolicy.IsCompatible(
                PrimaryRole.Student,
                SessionApplicationKind.AdministratorApplication));
        }

        [TestMethod]
        public void IdentityPolicyMapsOnlyPersonBackedRolesToPersonKinds()
        {
            Assert.AreEqual(
                PersonRecordKind.Student,
                IdentityPolicy.GetRequiredPersonRecordKind(PrimaryRole.Student));
            Assert.AreEqual(
                PersonRecordKind.EmployeeFaculty,
                IdentityPolicy.GetRequiredPersonRecordKind(PrimaryRole.EmployeeFaculty));
            Assert.AreEqual(
                PersonRecordKind.Unspecified,
                IdentityPolicy.GetRequiredPersonRecordKind(PrimaryRole.Administrator));
        }

        private static TestEntity NewEntity()
        {
            return new TestEntity(
                "TST-2026-000001",
                Utc(2026, 7, 17, 1, 0, 0),
                "USR-2026-000001");
        }

        private static DateTime Utc(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second)
        {
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }

        private sealed class TestEntity : EntityBase
        {
            public TestEntity(string id, DateTime createdAtUtc, string createdByUserId)
                : base(id, createdAtUtc, createdByUserId)
            {
            }

            public void Change(DateTime changedAtUtc, string changedByUserId)
            {
                RecordChange(changedAtUtc, changedByUserId);
            }
        }
    }
}
