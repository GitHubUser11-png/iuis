using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class InfrastructureFoundationTests
    {
        private static readonly DateTime Now = new DateTime(2026, 7, 19, 14, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void CatalogContainsExactlyFortyNineRepositories()
        {
            var catalog = new ProductionRepositoryCatalog();
            Assert.AreEqual(49, catalog.All.Count);
            Assert.AreEqual(14, catalog.PrincipalRepositories.Count);
            Assert.AreEqual(35, catalog.SupportingRepositories.Count);
            Assert.AreEqual(49, catalog.All.Select(x => x.FileName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [TestMethod]
        public void CatalogContainsSecurityAndCoordinationRepositories()
        {
            var catalog = new ProductionRepositoryCatalog();
            Assert.AreEqual("id_sequences.json", catalog.Get("id_sequences").FileName);
            Assert.AreEqual("transaction_journal.json", catalog.Get("transaction_journal").FileName);
            Assert.AreEqual("login_attempts.json", catalog.Get("login_attempts").FileName);
            Assert.AreEqual("sessions.json", catalog.Get("sessions").FileName);
        }

        [TestMethod]
        public void BootstrapCreatesExactlyFortyNineJsonFiles()
        {
            WithBootstrap((root, result) =>
            {
                Assert.AreEqual(49, result.RepositoryFileCount);
                Assert.AreEqual(49, Directory.GetFiles(root, "*.json").Length);
                Assert.IsTrue(result.MustChangePassword);
            });
        }

        [TestMethod]
        public void BootstrapUsesCallerSuppliedCredentialAndForcesChange()
        {
            WithBootstrap((root, result) =>
            {
                var service = CreateAuthentication(root);
                var auth = service.Authenticate("root.admin", "Temporary-Admin-Password-1", "AdministratorApplication", Now.AddMinutes(1));
                Assert.IsTrue(auth.Succeeded);
                Assert.IsTrue(auth.MustChangePassword);
                Assert.IsFalse(string.IsNullOrWhiteSpace(auth.SessionId));
            });
        }

        [TestMethod]
        public void BootstrapRejectsNonEmptyDataDirectory()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                var root = NewRoot();
                try
                {
                    File.WriteAllText(Path.Combine(root, "existing.json"), "{}");
                    CreateBootstrapper(root).Initialize(CreateRequest());
                }
                finally { DeleteRoot(root); }
            });
        }

        [TestMethod]
        public void CentralIdSequenceNeverReusesAllocatedNumber()
        {
            WithBootstrap((root, result) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var ids = new CentralIdSequenceService(catalog, new JsonInfrastructureOptions(root));
                Assert.AreEqual("STU-2026-000001", ids.Allocate("STU", 2026, result.AdministratorUserId));
                Assert.AreEqual("STU-2026-000002", ids.Allocate("STU", 2026, result.AdministratorUserId));
            });
        }

        [TestMethod]
        public void AtomicWriterPublishesCompleteReplacementAndRemovesTemporaryFiles()
        {
            var root = NewRoot();
            try
            {
                var path = Path.Combine(root, "sample.json");
                var writer = new AtomicFileWriter();
                writer.WriteUtf8(path, "{\"value\":1}");
                writer.WriteUtf8(path, "{\"value\":2}");
                Assert.AreEqual("{\"value\":2}", File.ReadAllText(path));
                Assert.AreEqual(0, Directory.GetFiles(root, "*.tmp").Length);
                Assert.AreEqual(0, Directory.GetFiles(root, "*.bak").Length);
            }
            finally { DeleteRoot(root); }
        }

        [TestMethod]
        public void RepositoryStoreRejectsStaleRevision()
        {
            WithBootstrap((root, result) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var store = new JsonRepositoryStore(catalog, new JsonInfrastructureOptions(root));
                var envelope = store.Read<object>("courses");
                store.Write("courses", envelope, 0);
                Assert.ThrowsException<InvalidOperationException>(() => store.Write("courses", envelope, 0));
            });
        }

        [TestMethod]
        public void JournaledTransactionCommitsMultipleRepositoryMutations()
        {
            WithBootstrap((root, result) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var options = new JsonInfrastructureOptions(root);
                var store = new JsonRepositoryStore(catalog, options);
                var courses = store.Read<object>("courses");
                var subjects = store.Read<object>("subjects");
                courses.Revision++;
                subjects.Revision++;
                var json = new System.Text.Json.JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
                var tx = new JournaledTransactionCoordinator(catalog, options);
                var id = tx.Execute(new[]
                {
                    new TransactionMutation("courses", System.Text.Json.JsonSerializer.Serialize(courses, json)),
                    new TransactionMutation("subjects", System.Text.Json.JsonSerializer.Serialize(subjects, json))
                });
                Assert.IsFalse(string.IsNullOrWhiteSpace(id));
                Assert.AreEqual(1L, store.Read<object>("courses").Revision);
                Assert.AreEqual(1L, store.Read<object>("subjects").Revision);
            });
        }

        [TestMethod]
        public void FiveFailuresCauseFifteenMinuteLockout()
        {
            WithBootstrap((root, result) =>
            {
                var service = CreateAuthentication(root);
                AuthenticationResult last = null;
                for (var i = 0; i < 5; i++)
                    last = service.Authenticate("root.admin", "wrong-password", "AdministratorApplication", Now.AddMinutes(i));
                Assert.IsTrue(last.IsLockedOut);
                Assert.AreEqual(Now.AddMinutes(19), last.LockedUntilUtc.Value);
            });
        }

        [TestMethod]
        public void LockoutExpiresAfterConfiguredDuration()
        {
            WithBootstrap((root, result) =>
            {
                var service = CreateAuthentication(root);
                for (var i = 0; i < 5; i++)
                    service.Authenticate("root.admin", "wrong-password", "AdministratorApplication", Now.AddMinutes(i));
                var after = service.Authenticate("root.admin", "Temporary-Admin-Password-1", "AdministratorApplication", Now.AddMinutes(20));
                Assert.IsTrue(after.Succeeded);
            });
        }

        [TestMethod]
        public void ForcedPasswordChangeRevokesRestrictedSessionAndIssuesFullSession()
        {
            WithBootstrap((root, result) =>
            {
                var service = CreateAuthentication(root);
                var first = service.Authenticate("root.admin", "Temporary-Admin-Password-1", "AdministratorApplication", Now.AddMinutes(1));
                var changed = service.CompleteForcedPasswordChange(result.AdministratorUserId, first.SessionId,
                    "Permanent-Admin-Password-2", Now.AddMinutes(2));
                Assert.IsTrue(changed.Succeeded);
                Assert.IsFalse(changed.MustChangePassword);
                Assert.AreNotEqual(first.SessionId, changed.SessionId);
                var second = service.Authenticate("root.admin", "Permanent-Admin-Password-2", "AdministratorApplication", Now.AddMinutes(3));
                Assert.IsTrue(second.Succeeded);
                Assert.IsFalse(second.MustChangePassword);
            });
        }

        [TestMethod]
        public void OldPasswordFailsAfterForcedPasswordChange()
        {
            WithBootstrap((root, result) =>
            {
                var service = CreateAuthentication(root);
                var first = service.Authenticate("root.admin", "Temporary-Admin-Password-1", "AdministratorApplication", Now.AddMinutes(1));
                service.CompleteForcedPasswordChange(result.AdministratorUserId, first.SessionId,
                    "Permanent-Admin-Password-2", Now.AddMinutes(2));
                var old = service.Authenticate("root.admin", "Temporary-Admin-Password-1", "AdministratorApplication", Now.AddMinutes(3));
                Assert.IsFalse(old.Succeeded);
            });
        }

        [TestMethod]
        public void PasswordHasherUsesSaltedPbkdf2Sha256()
        {
            var hasher = new PasswordHasher();
            var first = hasher.Hash("A-Strong-Test-Password", 100000);
            var second = hasher.Hash("A-Strong-Test-Password", 100000);
            Assert.AreNotEqual(first, second);
            Assert.IsTrue(hasher.Verify("A-Strong-Test-Password", first));
            Assert.IsFalse(hasher.Verify("wrong", first));
        }

        private static AuthenticationService CreateAuthentication(string root)
        {
            var catalog = new ProductionRepositoryCatalog();
            return new AuthenticationService(catalog, new JsonInfrastructureOptions(root));
        }

        private static ProductionBootstrapper CreateBootstrapper(string root)
        {
            var catalog = new ProductionRepositoryCatalog();
            return new ProductionBootstrapper(catalog, new JsonInfrastructureOptions(root));
        }

        private static ProductionBootstrapRequest CreateRequest()
        {
            return new ProductionBootstrapRequest
            {
                AdministratorLoginId = "root.admin",
                AdministratorInitialPassword = "Temporary-Admin-Password-1",
                AdministratorGivenName = "Initial",
                AdministratorFamilyName = "Administrator",
                AdministratorEmailAddress = "admin@example.edu",
                AdministratorMobileNumber = "+639171234567",
                AdministratorAddressLine1 = "1 University Road",
                AdministratorBarangay = "Poblacion",
                AdministratorCityMunicipality = "Malvar",
                AdministratorProvince = "Batangas",
                AdministratorPostalCode = "4233",
                AdministratorCountryCode = "PH",
                AdministratorBirthDate = "1990-01-01",
                DepartmentId = "DEPT-ADMIN",
                PositionTitle = "System Administrator",
                BootstrapAtUtc = Now
            };
        }

        private static void WithBootstrap(Action<string, ProductionBootstrapResult> action)
        {
            var root = NewRoot();
            try
            {
                var result = CreateBootstrapper(root).Initialize(CreateRequest());
                action(root, result);
            }
            finally { DeleteRoot(root); }
        }

        private static string NewRoot()
        {
            var path = Path.Combine(Path.GetTempPath(), "IUIS-Pass8-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void DeleteRoot(string root)
        {
            if (!Directory.Exists(root)) return;
            try { Directory.Delete(root, true); } catch { }
        }
    }
}
