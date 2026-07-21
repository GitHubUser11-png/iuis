using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Application.Authorization;
using IUIS.Application.Orchestration;
using IUIS.Domain.Identity;
using IUIS.Domain.Library;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Bootstrap;
using IUIS.Infrastructure.Composition;
using IUIS.Infrastructure.Identity;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass12LibraryIntegrationTests
    {
        private static readonly DateTime Now =
            new DateTime(2026, 7, 21, 5, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void JournaledIssueSurvivesCompositionRootRestart()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var first = new IuisCompositionRoot(root);
                var book = CreateBook(
                    "LBK-2026-010001",
                    "LCP-2026-010001",
                    bootstrap.AdministratorUserId);
                first.LibraryBooks.Write(
                    new[] { book },
                    0,
                    bootstrap.AdministratorUserId);

                var service = NewCommandService(
                    root,
                    EmployeePrincipal("library.circulation.issue"));
                var result = service.Issue(
                    "SES-2026-010001",
                    "token",
                    new LibraryIssueRequest
                    {
                        ExpectedBookRepositoryRevision = 1,
                        ExpectedBorrowingRepositoryRevision = 0,
                        ExpectedBookEntityVersion = book.Version,
                        StudentId = "STU-2026-010001",
                        BookId = book.Id,
                        CopyId = "LCP-2026-010001",
                        DueDate = new InstitutionLocalDate(2026, 8, 15)
                    },
                    Now.AddMinutes(5));

                var restarted = new IuisCompositionRoot(root);
                var restoredBook = restarted.LibraryBooks.FindById(book.Id);
                var restoredBorrowing = restarted.LibraryBorrowings.FindById(
                    result.BorrowingId);

                Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId));
                Assert.AreEqual(2L, restarted.LibraryBooks.Read().Revision);
                Assert.AreEqual(1L, restarted.LibraryBorrowings.Read().Revision);
                Assert.AreEqual(
                    LibraryCopyStatus.OnLoan,
                    restoredBook.Copies.Single().Status);
                Assert.AreEqual(
                    LibraryBorrowingStatus.Issued,
                    restoredBorrowing.Status);
                Assert.AreEqual("STU-2026-010001", restoredBorrowing.StudentId);
                Assert.AreEqual(restoredBook.Id, restoredBorrowing.BookId);
                Assert.AreEqual(
                    restoredBook.Copies.Single().CopyId,
                    restoredBorrowing.CopyId);
                Assert.IsNotNull(restarted.StudentLibraryCirculation);
                Assert.IsNotNull(restarted.LibraryCirculation);
            });
        }

        [TestMethod]
        public void DeterministicTransactionFailureRollsBackBothRepositoriesByteForByte()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var catalog = new ProductionRepositoryCatalog();
                var options = new JsonInfrastructureOptions(root);
                var store = new JsonRepositoryStore(catalog, options);
                var books = new LibraryBookRepositoryAdapter(store);
                var borrowings = new LibraryBorrowingRepositoryAdapter(store);
                var book = CreateBook(
                    "LBK-2026-010101",
                    "LCP-2026-010101",
                    bootstrap.AdministratorUserId);
                books.Write(
                    new[] { book },
                    0,
                    bootstrap.AdministratorUserId);

                var booksPath = Path.Combine(root, "books.json");
                var borrowingsPath = Path.Combine(root, "borrowings.json");
                var beforeBooks = File.ReadAllBytes(booksPath);
                var beforeBorrowings = File.ReadAllBytes(borrowingsPath);
                var transactions = new JournaledApplicationTransactionCoordinator(
                    new JournaledTransactionCoordinator(
                        catalog,
                        options,
                        new FailAfterFirstMutation()));
                var service = new LibraryCirculationCommandService(
                    Executor(EmployeePrincipal("library.circulation.issue")),
                    books,
                    borrowings,
                    transactions,
                    new ApplicationIdentifierAllocator(catalog, options));

                Assert.ThrowsException<InvalidOperationException>(() =>
                    service.Issue(
                        "SES-2026-010101",
                        "token",
                        new LibraryIssueRequest
                        {
                            ExpectedBookRepositoryRevision = 1,
                            ExpectedBorrowingRepositoryRevision = 0,
                            ExpectedBookEntityVersion = book.Version,
                            StudentId = "STU-2026-010101",
                            BookId = book.Id,
                            CopyId = "LCP-2026-010101",
                            DueDate = new InstitutionLocalDate(2026, 8, 15)
                        },
                        Now.AddMinutes(5)));

                CollectionAssert.AreEqual(
                    beforeBooks,
                    File.ReadAllBytes(booksPath));
                CollectionAssert.AreEqual(
                    beforeBorrowings,
                    File.ReadAllBytes(borrowingsPath));
                Assert.AreEqual(1L, books.Read().Revision);
                Assert.AreEqual(0L, borrowings.Read().Revision);
                Assert.AreEqual(0, borrowings.Read().Records.Count);
                Assert.AreEqual(
                    LibraryCopyStatus.Available,
                    books.FindById(book.Id).Copies.Single().Status);
            });
        }

        [TestMethod]
        public void RealRepositoriesRejectStaleRevisionAndEntityVersionBeforeMutation()
        {
            WithBootstrap((root, bootstrap) =>
            {
                var composition = new IuisCompositionRoot(root);
                var book = CreateBook(
                    "LBK-2026-010201",
                    "LCP-2026-010201",
                    bootstrap.AdministratorUserId);
                composition.LibraryBooks.Write(
                    new[] { book },
                    0,
                    bootstrap.AdministratorUserId);
                var service = NewCommandService(
                    root,
                    EmployeePrincipal("library.circulation.issue"));

                Assert.ThrowsException<InvalidOperationException>(() =>
                    service.Issue(
                        "SES-2026-010201",
                        "token",
                        new LibraryIssueRequest
                        {
                            ExpectedBookRepositoryRevision = 0,
                            ExpectedBorrowingRepositoryRevision = 0,
                            ExpectedBookEntityVersion = book.Version,
                            StudentId = "STU-2026-010201",
                            BookId = book.Id,
                            CopyId = "LCP-2026-010201",
                            DueDate = new InstitutionLocalDate(2026, 8, 15)
                        },
                        Now.AddMinutes(5)));
                Assert.ThrowsException<InvalidOperationException>(() =>
                    service.Issue(
                        "SES-2026-010201",
                        "token",
                        new LibraryIssueRequest
                        {
                            ExpectedBookRepositoryRevision = 1,
                            ExpectedBorrowingRepositoryRevision = 0,
                            ExpectedBookEntityVersion = checked(book.Version + 1L),
                            StudentId = "STU-2026-010201",
                            BookId = book.Id,
                            CopyId = "LCP-2026-010201",
                            DueDate = new InstitutionLocalDate(2026, 8, 15)
                        },
                        Now.AddMinutes(5)));

                var restarted = new IuisCompositionRoot(root);
                Assert.AreEqual(1L, restarted.LibraryBooks.Read().Revision);
                Assert.AreEqual(0L, restarted.LibraryBorrowings.Read().Revision);
                Assert.AreEqual(0, restarted.LibraryBorrowings.Read().Records.Count);
                Assert.AreEqual(
                    LibraryCopyStatus.Available,
                    restarted.LibraryBooks.FindById(book.Id).Copies.Single().Status);
            });
        }

        private static LibraryBook CreateBook(
            string bookId,
            string copyId,
            string actorUserId)
        {
            var book = new LibraryBook(
                bookId,
                "978-0-00-010001-1",
                "Restart-Safe Library Persistence",
                "Library Integration Author",
                "University Press",
                "Technology",
                Now,
                actorUserId);
            book.AddCopy(
                copyId,
                "BARCODE-" + copyId.Substring(copyId.Length - 6),
                LibraryCopyCondition.Good,
                Now.AddMinutes(1),
                actorUserId);
            return book;
        }

        private static LibraryCirculationCommandService NewCommandService(
            string root,
            AuthorizationPrincipal principal)
        {
            var catalog = new ProductionRepositoryCatalog();
            var options = new JsonInfrastructureOptions(root);
            var store = new JsonRepositoryStore(catalog, options);
            return new LibraryCirculationCommandService(
                Executor(principal),
                new LibraryBookRepositoryAdapter(store),
                new LibraryBorrowingRepositoryAdapter(store),
                new JournaledApplicationTransactionCoordinator(
                    new JournaledTransactionCoordinator(catalog, options)),
                new ApplicationIdentifierAllocator(catalog, options));
        }

        private static SessionAwareRequestExecutor Executor(
            AuthorizationPrincipal principal)
        {
            return new SessionAwareRequestExecutor(
                new FixedPrincipalProvider(principal),
                new PermissionResolver());
        }

        private static AuthorizationPrincipal EmployeePrincipal(
            params string[] permissions)
        {
            return new AuthorizationPrincipal(
                "USR-2026-000001",
                "EMP-2026-000001",
                PrimaryRole.EmployeeFaculty,
                SessionApplicationKind.UserApplication,
                SessionPurpose.FullAccess,
                "SST-2026-000001",
                new[]
                {
                    new PermissionProfileAssignment(
                        "PPR-2026-000001",
                        true,
                        permissions)
                },
                null,
                null);
        }

        private static void WithBootstrap(
            Action<string, ProductionBootstrapResult> action)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "IUIS-Pass12-Library-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var result = new ProductionBootstrapper(
                    new ProductionRepositoryCatalog(),
                    new JsonInfrastructureOptions(root))
                    .Initialize(new ProductionBootstrapRequest
                    {
                        AdministratorLoginId = "root.admin",
                        AdministratorInitialPassword =
                            "Temporary-Admin-Password-1",
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
                    });
                action(root, result);
            }
            finally
            {
                try { Directory.Delete(root, true); }
                catch { }
            }
        }

        private sealed class FixedPrincipalProvider :
            IAuthorizationPrincipalProvider
        {
            private readonly AuthorizationPrincipal _principal;

            public FixedPrincipalProvider(AuthorizationPrincipal principal)
            {
                _principal = principal;
            }

            public AuthorizationPrincipal Load(
                string sessionId,
                string sessionToken,
                DateTime utcNow)
            {
                return _principal;
            }
        }

        private sealed class FailAfterFirstMutation :
            ITransactionFailureInjector
        {
            public void OnStage(TransactionExecutionContext context)
            {
                if (context.Stage == TransactionExecutionStage.MutationApplied
                    && context.AppliedMutationCount == 1)
                {
                    throw new InvalidOperationException(
                        "Deterministic Pass 12 Library transaction failure.");
                }
            }
        }
    }
}