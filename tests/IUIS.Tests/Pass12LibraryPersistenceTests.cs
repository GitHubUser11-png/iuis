using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Orchestration;
using IUIS.Application.Repositories;
using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Library;
using IUIS.Domain.Time;
using IUIS.Infrastructure.Persistence;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class Pass12LibraryPersistenceTests
    {
        private static readonly DateTime StartUtc =
            new DateTime(2026, 7, 21, 1, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void LibraryBookMapperRoundTripPreservesInventoryState()
        {
            var book = CreateBookWithThreeCopies();
            book.MarkCopyOnLoan("LCP-2026-000001", StartUtc.AddMinutes(2), "USR-2026-000001");
            book.SendCopyToMaintenance("LCP-2026-000002", StartUtc.AddMinutes(3), "USR-2026-000001");
            book.MarkCopyLost("LCP-2026-000003", StartUtc.AddMinutes(4), "USR-2026-000001");

            var mapper = new LibraryBookJsonMapper();
            var options = JsonOptions();
            var restored = mapper.FromJson(mapper.ToJson(book, options), options);

            Assert.AreEqual(book.Version, restored.Version);
            Assert.AreEqual(3, restored.TotalCopies);
            Assert.AreEqual(0, restored.AvailableCopies);
            Assert.AreEqual(1, restored.ActiveBorrowedCopies);
            Assert.AreEqual(1, restored.MaintenanceCopies);
            Assert.AreEqual(1, restored.LostCopies);
            restored.AssertInventoryInvariant();
        }

        [TestMethod]
        public void LibraryBorrowingMapperRoundTripPreservesTerminalStates()
        {
            var mapper = new LibraryBorrowingJsonMapper();
            var options = JsonOptions();
            var returned = CreateIssuedBorrowing("BRW-2026-000001");
            returned.Return(StartUtc.AddMinutes(3), LibraryCopyCondition.Good, "USR-2026-000001");
            var lost = CreateIssuedBorrowing("BRW-2026-000002");
            lost.MarkLost(StartUtc.AddMinutes(4), "USR-2026-000001");

            var restoredReturned = mapper.FromJson(mapper.ToJson(returned, options), options);
            var restoredLost = mapper.FromJson(mapper.ToJson(lost, options), options);

            Assert.AreEqual(LibraryBorrowingStatus.Returned, restoredReturned.Status);
            Assert.AreEqual(LibraryCopyCondition.Good, restoredReturned.ReturnCondition);
            Assert.AreEqual(LibraryBorrowingStatus.Lost, restoredLost.Status);
            Assert.IsNotNull(restoredLost.LostAtUtc);
        }

        [TestMethod]
        public void RehydrationRejectsInconsistentPersistedState()
        {
            Assert.ThrowsException<DomainValidationException>(() =>
                LibraryBookCopy.Rehydrate(
                    "LCP-2026-000001",
                    "BC-001",
                    LibraryCopyCondition.Good,
                    LibraryCopyStatus.Lost));

            Assert.ThrowsException<DomainValidationException>(() =>
                LibraryBorrowing.Rehydrate(
                    "BRW-2026-000001",
                    "STU-2026-000001",
                    "LBK-2026-000001",
                    "LCP-2026-000001",
                    new InstitutionLocalDate(2026, 8, 1),
                    LibraryBorrowingStatus.Returned,
                    StartUtc.AddMinutes(1),
                    "USR-2026-000001",
                    0,
                    null,
                    null,
                    null,
                    null,
                    null,
                    2,
                    false,
                    StartUtc,
                    "USR-2026-000001",
                    StartUtc.AddMinutes(2),
                    "USR-2026-000001",
                    null,
                    null));
        }

        [TestMethod]
        public void MapperReadinessActivatesBothLibraryAdaptersOnly()
        {
            var completed = AggregateMapperReadinessCatalog.All
                .Where(item => item.Readiness == AggregateMapperReadiness.SpecializedMapperCompleted)
                .Select(item => item.AdapterName)
                .ToList();
            var deferred = AggregateMapperReadinessCatalog.All
                .Where(item => item.Readiness == AggregateMapperReadiness.DeferredWithExplicitReason)
                .Select(item => item.AdapterName)
                .ToList();

            Assert.AreEqual(13, completed.Count);
            CollectionAssert.Contains(completed, "LibraryBookRepositoryAdapter");
            CollectionAssert.Contains(completed, "LibraryBorrowingRepositoryAdapter");
            Assert.AreEqual(5, deferred.Count);
        }

        [TestMethod]
        public void IssueStagesBookAndBorrowingInOneTransaction()
        {
            var book = CreateBookWithOneCopy();
            var books = new FakeBookRepository(4, new[] { book });
            var borrowings = new FakeBorrowingRepository(7, new LibraryBorrowing[0]);
            var coordinator = new FakeCoordinator();
            var service = CreateCommandService(books, borrowings, coordinator);

            var result = service.Issue(
                "SES-2026-000001",
                "token",
                new LibraryIssueRequest
                {
                    ExpectedBookRepositoryRevision = 4,
                    ExpectedBorrowingRepositoryRevision = 7,
                    ExpectedBookEntityVersion = book.Version,
                    StudentId = "STU-2026-000001",
                    BookId = book.Id,
                    CopyId = "LCP-2026-000001",
                    DueDate = new InstitutionLocalDate(2026, 8, 1)
                },
                StartUtc.AddMinutes(2));

            Assert.AreEqual(1, coordinator.ExecutionCount);
            Assert.AreEqual(2, coordinator.LastStageCount);
            Assert.AreEqual(5L, books.Read().Revision);
            Assert.AreEqual(8L, borrowings.Read().Revision);
            Assert.AreEqual(LibraryCopyStatus.OnLoan, books.Read().Records[0].Copies[0].Status);
            Assert.AreEqual(LibraryBorrowingStatus.Issued, borrowings.Read().Records[0].Status);
            Assert.AreEqual(3L, result.BookEntityVersion);
            Assert.AreEqual(2L, result.BorrowingEntityVersion);
        }

        [TestMethod]
        public void ReturnAndLostMutateBothRepositories()
        {
            var returned = CreateIssuedPair("BRW-2026-000001");
            returned.Service.Return(
                "SES-2026-000001",
                "token",
                new LibraryReturnRequest
                {
                    ExpectedBookRepositoryRevision = 4,
                    ExpectedBorrowingRepositoryRevision = 7,
                    ExpectedBookEntityVersion = returned.Book.Version,
                    ExpectedBorrowingEntityVersion = returned.Borrowing.Version,
                    BorrowingId = returned.Borrowing.Id,
                    ReturnCondition = LibraryCopyCondition.Fair
                },
                StartUtc.AddMinutes(3));
            Assert.AreEqual(LibraryBorrowingStatus.Returned, returned.Borrowings.Read().Records[0].Status);
            Assert.AreEqual(LibraryCopyStatus.Available, returned.Books.Read().Records[0].Copies[0].Status);

            var lost = CreateIssuedPair("BRW-2026-000002");
            lost.Service.MarkLost(
                "SES-2026-000001",
                "token",
                new LibraryLostRequest
                {
                    ExpectedBookRepositoryRevision = 4,
                    ExpectedBorrowingRepositoryRevision = 7,
                    ExpectedBookEntityVersion = lost.Book.Version,
                    ExpectedBorrowingEntityVersion = lost.Borrowing.Version,
                    BorrowingId = lost.Borrowing.Id
                },
                StartUtc.AddMinutes(4));
            Assert.AreEqual(LibraryBorrowingStatus.Lost, lost.Borrowings.Read().Records[0].Status);
            Assert.AreEqual(LibraryCopyStatus.Lost, lost.Books.Read().Records[0].Copies[0].Status);
        }

        [TestMethod]
        public void StaleRevisionAndStaleEntityFailBeforeTransaction()
        {
            var pair = CreateIssuedPair("BRW-2026-000001");
            Assert.ThrowsException<InvalidOperationException>(() => pair.Service.Return(
                "SES-2026-000001",
                "token",
                new LibraryReturnRequest
                {
                    ExpectedBookRepositoryRevision = 3,
                    ExpectedBorrowingRepositoryRevision = 7,
                    ExpectedBookEntityVersion = pair.Book.Version,
                    ExpectedBorrowingEntityVersion = pair.Borrowing.Version,
                    BorrowingId = pair.Borrowing.Id,
                    ReturnCondition = LibraryCopyCondition.Good
                },
                StartUtc.AddMinutes(3)));
            Assert.AreEqual(0, pair.Coordinator.ExecutionCount);

            Assert.ThrowsException<InvalidOperationException>(() => pair.Service.Return(
                "SES-2026-000001",
                "token",
                new LibraryReturnRequest
                {
                    ExpectedBookRepositoryRevision = 4,
                    ExpectedBorrowingRepositoryRevision = 7,
                    ExpectedBookEntityVersion = pair.Book.Version,
                    ExpectedBorrowingEntityVersion = pair.Borrowing.Version - 1,
                    BorrowingId = pair.Borrowing.Id,
                    ReturnCondition = LibraryCopyCondition.Good
                },
                StartUtc.AddMinutes(3)));
            Assert.AreEqual(0, pair.Coordinator.ExecutionCount);
        }

        [TestMethod]
        public void StudentProjectionUsesSessionOwnershipAndExcludesInternalActors()
        {
            var book = CreateBookWithOneCopy();
            var own = CreateIssuedBorrowing("BRW-2026-000001");
            var other = LibraryBorrowing.Rehydrate(
                "BRW-2026-000002",
                "STU-2026-000002",
                book.Id,
                "LCP-2026-000002",
                new InstitutionLocalDate(2026, 8, 1),
                LibraryBorrowingStatus.Cancelled,
                null,
                null,
                0,
                null,
                null,
                null,
                null,
                null,
                1,
                false,
                StartUtc,
                "USR-2026-000001",
                StartUtc,
                "USR-2026-000001",
                null,
                null);
            var service = new StudentLibraryCirculationQueryService(
                Executor(StudentPrincipal()),
                new FakeBookRepository(2, new[] { book }),
                new FakeBorrowingRepository(3, new[] { own, other }));

            var result = service.GetOwnOverview(
                "SES-2026-000002",
                "token",
                StartUtc.AddMinutes(5));

            Assert.AreEqual("STU-2026-000001", result.StudentId);
            Assert.AreEqual(1, result.Borrowings.Count);
            Assert.AreEqual(own.Id, result.Borrowings[0].BorrowingId);
            Assert.IsNull(typeof(StudentLibraryBorrowingDto).GetProperty("IssuedByUserId"));
            Assert.IsNull(typeof(StudentLibraryBorrowingDto).GetProperty("LostRecordedByUserId"));
        }

        [TestMethod]
        public void StudentCannotInvokeLibrarianIssueCommand()
        {
            var book = CreateBookWithOneCopy();
            var service = new LibraryCirculationCommandService(
                Executor(StudentPrincipal()),
                new FakeBookRepository(1, new[] { book }),
                new FakeBorrowingRepository(1, new LibraryBorrowing[0]),
                new FakeCoordinator(),
                new FakeAllocator());

            Assert.ThrowsException<AuthorizationDeniedException>(() => service.Issue(
                "SES-2026-000001",
                "token",
                new LibraryIssueRequest
                {
                    ExpectedBookRepositoryRevision = 1,
                    ExpectedBorrowingRepositoryRevision = 1,
                    ExpectedBookEntityVersion = book.Version,
                    StudentId = "STU-2026-000001",
                    BookId = book.Id,
                    CopyId = "LCP-2026-000001",
                    DueDate = new InstitutionLocalDate(2026, 8, 1)
                },
                StartUtc.AddMinutes(2)));
        }

        private static LibraryBook CreateBookWithThreeCopies()
        {
            var book = NewBook();
            book.AddCopy("LCP-2026-000001", "BC-001", LibraryCopyCondition.New, StartUtc.AddSeconds(1), "USR-2026-000001");
            book.AddCopy("LCP-2026-000002", "BC-002", LibraryCopyCondition.Good, StartUtc.AddSeconds(2), "USR-2026-000001");
            book.AddCopy("LCP-2026-000003", "BC-003", LibraryCopyCondition.Fair, StartUtc.AddSeconds(3), "USR-2026-000001");
            return book;
        }

        private static LibraryBook CreateBookWithOneCopy()
        {
            var book = NewBook();
            book.AddCopy("LCP-2026-000001", "BC-001", LibraryCopyCondition.Good, StartUtc.AddMinutes(1), "USR-2026-000001");
            return book;
        }

        private static LibraryBook NewBook()
        {
            return new LibraryBook(
                "LBK-2026-000001",
                "978-0-00-000001-1",
                "Systems",
                "Author",
                "Publisher",
                "Technology",
                StartUtc,
                "USR-2026-000001");
        }

        private static LibraryBorrowing CreateIssuedBorrowing(string id)
        {
            var borrowing = new LibraryBorrowing(
                id,
                "STU-2026-000001",
                "LBK-2026-000001",
                "LCP-2026-000001",
                new InstitutionLocalDate(2026, 8, 1),
                StartUtc,
                "USR-2026-000001");
            borrowing.Issue(StartUtc.AddMinutes(1), "USR-2026-000001");
            return borrowing;
        }

        private static IssuedPair CreateIssuedPair(string borrowingId)
        {
            var book = CreateBookWithOneCopy();
            book.MarkCopyOnLoan("LCP-2026-000001", StartUtc.AddMinutes(2), "USR-2026-000001");
            var borrowing = CreateIssuedBorrowing(borrowingId);
            var books = new FakeBookRepository(4, new[] { book });
            var borrowings = new FakeBorrowingRepository(7, new[] { borrowing });
            var coordinator = new FakeCoordinator();
            return new IssuedPair
            {
                Book = book,
                Borrowing = borrowing,
                Books = books,
                Borrowings = borrowings,
                Coordinator = coordinator,
                Service = CreateCommandService(books, borrowings, coordinator)
            };
        }

        private static LibraryCirculationCommandService CreateCommandService(
            ILibraryBookRepository books,
            ILibraryBorrowingRepository borrowings,
            FakeCoordinator coordinator)
        {
            return new LibraryCirculationCommandService(
                Executor(EmployeePrincipal()),
                books,
                borrowings,
                coordinator,
                new FakeAllocator());
        }

        private static SessionAwareRequestExecutor Executor(AuthorizationPrincipal principal)
        {
            return new SessionAwareRequestExecutor(
                new FixedPrincipalProvider(principal),
                new PermissionResolver());
        }

        private static AuthorizationPrincipal EmployeePrincipal()
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
                        new[]
                        {
                            "library.circulation.issue",
                            "library.circulation.return",
                            "library.circulation.lost.record"
                        })
                },
                null,
                null);
        }

        private static AuthorizationPrincipal StudentPrincipal()
        {
            return new AuthorizationPrincipal(
                "USR-2026-000002",
                "STU-2026-000001",
                PrimaryRole.Student,
                SessionApplicationKind.UserApplication,
                SessionPurpose.FullAccess,
                "SST-2026-000002",
                new[]
                {
                    new PermissionProfileAssignment(
                        "PPR-2026-000002",
                        true,
                        new[] { "student.library.read" })
                },
                null,
                null);
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private sealed class FixedPrincipalProvider : IAuthorizationPrincipalProvider
        {
            private readonly AuthorizationPrincipal _principal;
            public FixedPrincipalProvider(AuthorizationPrincipal principal) { _principal = principal; }
            public AuthorizationPrincipal Load(string sessionId, string sessionToken, DateTime utcNow) { return _principal; }
        }

        private sealed class FakeAllocator : IApplicationIdentifierAllocator
        {
            private int _next;
            public string Allocate(string prefix, int year, string allocatedByUserId)
            {
                _next++;
                return prefix + "-" + year + "-" + _next.ToString("000000");
            }
        }

        private abstract class FakeRepositoryBase<T> where T : class, IEntity
        {
            private List<T> _records;
            private long _revision;

            protected FakeRepositoryBase(string name, long revision, IEnumerable<T> records)
            {
                RepositoryName = name;
                _revision = revision;
                _records = records.ToList();
            }

            public string RepositoryName { get; private set; }
            public RepositorySnapshot<T> ReadCore()
            {
                return new RepositorySnapshot<T>(RepositoryName, _revision, _records.ToList());
            }
            public T FindCore(string id)
            {
                return _records.SingleOrDefault(item => StringComparer.Ordinal.Equals(item.Id, id));
            }
            public void WriteCore(IReadOnlyCollection<T> records, long expectedRevision)
            {
                if (_revision != expectedRevision)
                    throw new InvalidOperationException("Repository revision conflict for " + RepositoryName + ".");
                _records = records.ToList();
                _revision = checked(_revision + 1L);
            }
        }

        private sealed class FakeBookRepository : FakeRepositoryBase<LibraryBook>, ILibraryBookRepository
        {
            public FakeBookRepository(long revision, IEnumerable<LibraryBook> records)
                : base("books", revision, records) { }
            public RepositorySnapshot<LibraryBook> Read() { return ReadCore(); }
            public LibraryBook FindById(string id) { return FindCore(id); }
            public void Write(IReadOnlyCollection<LibraryBook> records, long expectedRevision, string updatedByUserId)
            {
                WriteCore(records, expectedRevision);
            }
        }

        private sealed class FakeBorrowingRepository : FakeRepositoryBase<LibraryBorrowing>, ILibraryBorrowingRepository
        {
            public FakeBorrowingRepository(long revision, IEnumerable<LibraryBorrowing> records)
                : base("borrowings", revision, records) { }
            public RepositorySnapshot<LibraryBorrowing> Read() { return ReadCore(); }
            public LibraryBorrowing FindById(string id) { return FindCore(id); }
            public void Write(IReadOnlyCollection<LibraryBorrowing> records, long expectedRevision, string updatedByUserId)
            {
                WriteCore(records, expectedRevision);
            }
        }

        private sealed class FakeCoordinator : IApplicationTransactionCoordinator
        {
            public int ExecutionCount { get; private set; }
            public int LastStageCount { get; private set; }
            public string Execute(Action<IRepositoryTransactionScope> stageMutations)
            {
                var scope = new FakeScope();
                stageMutations(scope);
                ExecutionCount++;
                LastStageCount = scope.Actions.Count;
                foreach (var action in scope.Actions) action();
                return "TXN-2026-000001";
            }

            private sealed class FakeScope : IRepositoryTransactionScope
            {
                public readonly List<Action> Actions = new List<Action>();
                public void Stage<T>(
                    IVersionedRepository<T> repository,
                    IReadOnlyCollection<T> records,
                    long expectedRevision,
                    string updatedByUserId)
                    where T : class, IEntity
                {
                    Actions.Add(() => repository.Write(records, expectedRevision, updatedByUserId));
                }
            }
        }

        private sealed class IssuedPair
        {
            public LibraryBook Book { get; set; }
            public LibraryBorrowing Borrowing { get; set; }
            public FakeBookRepository Books { get; set; }
            public FakeBorrowingRepository Borrowings { get; set; }
            public FakeCoordinator Coordinator { get; set; }
            public LibraryCirculationCommandService Service { get; set; }
        }
    }
}
