using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Application.Authorization;
using IUIS.Application.Dtos;
using IUIS.Application.Repositories;
using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Library;
using IUIS.Domain.Time;

namespace IUIS.Application.Orchestration
{
    public sealed class LibraryIssueRequest
    {
        public long ExpectedBookRepositoryRevision { get; set; }
        public long ExpectedBorrowingRepositoryRevision { get; set; }
        public long ExpectedBookEntityVersion { get; set; }
        public string StudentId { get; set; }
        public string BookId { get; set; }
        public string CopyId { get; set; }
        public InstitutionLocalDate DueDate { get; set; }
    }

    public sealed class LibraryReturnRequest
    {
        public long ExpectedBookRepositoryRevision { get; set; }
        public long ExpectedBorrowingRepositoryRevision { get; set; }
        public long ExpectedBookEntityVersion { get; set; }
        public long ExpectedBorrowingEntityVersion { get; set; }
        public string BorrowingId { get; set; }
        public LibraryCopyCondition ReturnCondition { get; set; }
    }

    public sealed class LibraryLostRequest
    {
        public long ExpectedBookRepositoryRevision { get; set; }
        public long ExpectedBorrowingRepositoryRevision { get; set; }
        public long ExpectedBookEntityVersion { get; set; }
        public long ExpectedBorrowingEntityVersion { get; set; }
        public string BorrowingId { get; set; }
    }

    internal static class LibraryCirculationCommandGuard
    {
        public static SessionApplicationKind ApplicationKind(
            AuthorizationPrincipal principal)
        {
            return principal.PrimaryRole == PrimaryRole.Administrator
                ? SessionApplicationKind.AdministratorApplication
                : SessionApplicationKind.UserApplication;
        }

        public static void RequireRevision(
            long expected,
            long actual,
            string repositoryName)
        {
            if (expected < 0L)
                throw new ArgumentOutOfRangeException(nameof(expected));
            if (expected != actual)
            {
                throw new InvalidOperationException(
                    "The request is based on a stale "
                    + repositoryName + " repository revision.");
            }
        }

        public static void RequireVersion(
            long expected,
            long actual,
            string entityName)
        {
            if (expected < 1L)
                throw new ArgumentOutOfRangeException(nameof(expected));
            if (expected != actual)
            {
                throw new InvalidOperationException(
                    "The request is based on a stale "
                    + entityName + " entity version.");
            }
        }

        public static T Find<T>(
            IEnumerable<T> records,
            string id,
            string entityName)
            where T : class, IEntity
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    entityName + " ID is required.",
                    nameof(id));
            }

            var record = records.SingleOrDefault(item =>
                StringComparer.Ordinal.Equals(item.Id, id.Trim()));
            if (record == null)
            {
                throw new InvalidOperationException(
                    entityName + " is unavailable.");
            }

            return record;
        }

        public static LibraryBookCopy FindCopy(
            LibraryBook book,
            string copyId)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (string.IsNullOrWhiteSpace(copyId))
                throw new ArgumentException("Library Book Copy ID is required.", nameof(copyId));
            var copy = book.Copies.SingleOrDefault(item =>
                StringComparer.Ordinal.Equals(item.CopyId, copyId.Trim()));
            if (copy == null)
                throw new InvalidOperationException("Library Book Copy is unavailable.");
            return copy;
        }

        public static bool IsOpen(LibraryBorrowing value)
        {
            return value.Status == LibraryBorrowingStatus.Prepared
                || value.Status == LibraryBorrowingStatus.Issued
                || value.Status == LibraryBorrowingStatus.Overdue;
        }

        public static LibraryCirculationCommandResult Result(
            string transactionId,
            LibraryBook book,
            LibraryBorrowing borrowing,
            long bookRepositoryRevision,
            long borrowingRepositoryRevision)
        {
            return new LibraryCirculationCommandResult
            {
                TransactionId = transactionId,
                BorrowingId = borrowing.Id,
                BookId = book.Id,
                CopyId = borrowing.CopyId,
                BookRepositoryRevision = bookRepositoryRevision,
                BorrowingRepositoryRevision = borrowingRepositoryRevision,
                BookEntityVersion = book.Version,
                BorrowingEntityVersion = borrowing.Version,
                UpdatedAtUtc = borrowing.UpdatedAtUtc,
                UpdatedByUserId = borrowing.UpdatedByUserId
            };
        }
    }

    public sealed class StudentLibraryCirculationQueryService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ILibraryBookRepository _books;
        private readonly ILibraryBorrowingRepository _borrowings;

        public StudentLibraryCirculationQueryService(
            SessionAwareRequestExecutor executor,
            ILibraryBookRepository books,
            ILibraryBorrowingRepository borrowings)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _books = books ?? throw new ArgumentNullException(nameof(books));
            _borrowings = borrowings ?? throw new ArgumentNullException(nameof(borrowings));
        }

        public StudentLibraryCirculationOverviewDto GetOwnOverview(
            string sessionId,
            string sessionToken,
            DateTime utcNow)
        {
            return _executor.Query(
                sessionId,
                sessionToken,
                utcNow,
                principal => new AuthorizationRequest(
                    "student.library.read",
                    SessionApplicationKind.UserApplication,
                    ConfidentialityClassification.OwnRecord,
                    principal.PersonRecordId,
                    new[] { PrimaryRole.Student }),
                principal => Build(principal.PersonRecordId));
        }

        private StudentLibraryCirculationOverviewDto Build(string studentId)
        {
            var bookSnapshot = _books.Read();
            var borrowingSnapshot = _borrowings.Read();
            var released = borrowingSnapshot.Records
                .Where(item => !item.IsArchived)
                .Where(item => StringComparer.Ordinal.Equals(
                    item.StudentId,
                    studentId))
                .Where(IsReleased)
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Select(item => ToReleased(item, bookSnapshot.Records))
                .ToList();

            return new StudentLibraryCirculationOverviewDto
            {
                StudentId = studentId,
                BookRepositoryRevision = bookSnapshot.Revision,
                BorrowingRepositoryRevision = borrowingSnapshot.Revision,
                Borrowings = released.AsReadOnly()
            };
        }

        private static bool IsReleased(LibraryBorrowing value)
        {
            return value.Status == LibraryBorrowingStatus.Issued
                || value.Status == LibraryBorrowingStatus.Overdue
                || value.Status == LibraryBorrowingStatus.Returned
                || value.Status == LibraryBorrowingStatus.Lost;
        }

        private static StudentLibraryBorrowingDto ToReleased(
            LibraryBorrowing borrowing,
            IEnumerable<LibraryBook> books)
        {
            var book = books.SingleOrDefault(item =>
                StringComparer.Ordinal.Equals(item.Id, borrowing.BookId));
            if (book == null)
            {
                throw new InvalidOperationException(
                    "A released Library Borrowing references an unavailable Library Book.");
            }

            return new StudentLibraryBorrowingDto
            {
                BorrowingId = borrowing.Id,
                BookId = borrowing.BookId,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BookCategory = book.Category,
                CopyId = borrowing.CopyId,
                DueDate = borrowing.DueDate.ToString(),
                Status = borrowing.Status.ToString(),
                IssuedAtUtc = borrowing.IssuedAtUtc,
                RenewalCount = borrowing.RenewalCount,
                ReturnedAtUtc = borrowing.ReturnedAtUtc,
                ReturnCondition = borrowing.ReturnCondition.HasValue
                    ? borrowing.ReturnCondition.Value.ToString()
                    : null,
                LostAtUtc = borrowing.LostAtUtc,
                EntityVersion = borrowing.Version
            };
        }
    }

    public sealed class LibraryCirculationCommandService
    {
        private readonly SessionAwareRequestExecutor _executor;
        private readonly ILibraryBookRepository _books;
        private readonly ILibraryBorrowingRepository _borrowings;
        private readonly IApplicationTransactionCoordinator _transactions;
        private readonly IApplicationIdentifierAllocator _ids;

        public LibraryCirculationCommandService(
            SessionAwareRequestExecutor executor,
            ILibraryBookRepository books,
            ILibraryBorrowingRepository borrowings,
            IApplicationTransactionCoordinator transactions,
            IApplicationIdentifierAllocator ids)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _books = books ?? throw new ArgumentNullException(nameof(books));
            _borrowings = borrowings ?? throw new ArgumentNullException(nameof(borrowings));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
        }

        public LibraryCirculationCommandResult Issue(
            string sessionId,
            string sessionToken,
            LibraryIssueRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => InternalRequest(
                    principal,
                    "library.circulation.issue"),
                principal =>
                {
                    var bookSnapshot = _books.Read();
                    var borrowingSnapshot = _borrowings.Read();
                    LibraryCirculationCommandGuard.RequireRevision(
                        request.ExpectedBookRepositoryRevision,
                        bookSnapshot.Revision,
                        _books.RepositoryName);
                    LibraryCirculationCommandGuard.RequireRevision(
                        request.ExpectedBorrowingRepositoryRevision,
                        borrowingSnapshot.Revision,
                        _borrowings.RepositoryName);
                    var book = LibraryCirculationCommandGuard.Find(
                        bookSnapshot.Records,
                        request.BookId,
                        "Library Book");
                    LibraryCirculationCommandGuard.RequireVersion(
                        request.ExpectedBookEntityVersion,
                        book.Version,
                        "Library Book");
                    var copy = LibraryCirculationCommandGuard.FindCopy(
                        book,
                        request.CopyId);
                    if (book.Status != LibraryBookStatus.Active
                        || copy.Status != LibraryCopyStatus.Available)
                    {
                        throw new InvalidOperationException(
                            "Only an available copy of an active Library Book can be issued.");
                    }

                    if (request.DueDate < InstitutionLocalDate.FromDateTime(utcNow))
                    {
                        throw new ArgumentException(
                            "Library Borrowing due date cannot precede the issue date.",
                            nameof(request));
                    }

                    if (borrowingSnapshot.Records.Any(item =>
                        StringComparer.Ordinal.Equals(item.CopyId, copy.CopyId)
                        && LibraryCirculationCommandGuard.IsOpen(item)))
                    {
                        throw new InvalidOperationException(
                            "The Library Book Copy already has an open Borrowing.");
                    }

                    var borrowing = new LibraryBorrowing(
                        _ids.Allocate("BRW", utcNow.Year, principal.UserId),
                        request.StudentId,
                        book.Id,
                        copy.CopyId,
                        request.DueDate,
                        utcNow,
                        principal.UserId);
                    borrowing.Issue(utcNow, principal.UserId);
                    book.MarkCopyOnLoan(copy.CopyId, utcNow, principal.UserId);

                    var books = bookSnapshot.Records.ToList();
                    var borrowings = borrowingSnapshot.Records.ToList();
                    borrowings.Add(borrowing);
                    var transactionId = _transactions.Execute(scope =>
                    {
                        scope.Stage(
                            _books,
                            books,
                            bookSnapshot.Revision,
                            principal.UserId);
                        scope.Stage(
                            _borrowings,
                            borrowings,
                            borrowingSnapshot.Revision,
                            principal.UserId);
                    });
                    return LibraryCirculationCommandGuard.Result(
                        transactionId,
                        book,
                        borrowing,
                        checked(bookSnapshot.Revision + 1L),
                        checked(borrowingSnapshot.Revision + 1L));
                });
        }

        public LibraryCirculationCommandResult Return(
            string sessionId,
            string sessionToken,
            LibraryReturnRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!Enum.IsDefined(typeof(LibraryCopyCondition), request.ReturnCondition)
                || request.ReturnCondition == LibraryCopyCondition.Lost)
            {
                throw new ArgumentException(
                    "A valid non-lost return condition is required.",
                    nameof(request));
            }

            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => InternalRequest(
                    principal,
                    "library.circulation.return"),
                principal =>
                {
                    var bookSnapshot = _books.Read();
                    var borrowingSnapshot = _borrowings.Read();
                    LibraryCirculationCommandGuard.RequireRevision(
                        request.ExpectedBookRepositoryRevision,
                        bookSnapshot.Revision,
                        _books.RepositoryName);
                    LibraryCirculationCommandGuard.RequireRevision(
                        request.ExpectedBorrowingRepositoryRevision,
                        borrowingSnapshot.Revision,
                        _borrowings.RepositoryName);
                    var borrowing = LibraryCirculationCommandGuard.Find(
                        borrowingSnapshot.Records,
                        request.BorrowingId,
                        "Library Borrowing");
                    LibraryCirculationCommandGuard.RequireVersion(
                        request.ExpectedBorrowingEntityVersion,
                        borrowing.Version,
                        "Library Borrowing");
                    var book = LibraryCirculationCommandGuard.Find(
                        bookSnapshot.Records,
                        borrowing.BookId,
                        "Library Book");
                    LibraryCirculationCommandGuard.RequireVersion(
                        request.ExpectedBookEntityVersion,
                        book.Version,
                        "Library Book");
                    var copy = ValidateOpenPair(book, borrowing);

                    borrowing.Return(
                        utcNow,
                        request.ReturnCondition,
                        principal.UserId);
                    book.MarkCopyReturned(
                        copy.CopyId,
                        request.ReturnCondition,
                        utcNow,
                        principal.UserId);
                    var transactionId = StageBoth(
                        principal.UserId,
                        bookSnapshot,
                        borrowingSnapshot);
                    return LibraryCirculationCommandGuard.Result(
                        transactionId,
                        book,
                        borrowing,
                        checked(bookSnapshot.Revision + 1L),
                        checked(borrowingSnapshot.Revision + 1L));
                });
        }

        public LibraryCirculationCommandResult MarkLost(
            string sessionId,
            string sessionToken,
            LibraryLostRequest request,
            DateTime utcNow)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return _executor.Command(
                sessionId,
                sessionToken,
                utcNow,
                principal => InternalRequest(
                    principal,
                    "library.circulation.lost.record"),
                principal =>
                {
                    var bookSnapshot = _books.Read();
                    var borrowingSnapshot = _borrowings.Read();
                    LibraryCirculationCommandGuard.RequireRevision(
                        request.ExpectedBookRepositoryRevision,
                        bookSnapshot.Revision,
                        _books.RepositoryName);
                    LibraryCirculationCommandGuard.RequireRevision(
                        request.ExpectedBorrowingRepositoryRevision,
                        borrowingSnapshot.Revision,
                        _borrowings.RepositoryName);
                    var borrowing = LibraryCirculationCommandGuard.Find(
                        borrowingSnapshot.Records,
                        request.BorrowingId,
                        "Library Borrowing");
                    LibraryCirculationCommandGuard.RequireVersion(
                        request.ExpectedBorrowingEntityVersion,
                        borrowing.Version,
                        "Library Borrowing");
                    var book = LibraryCirculationCommandGuard.Find(
                        bookSnapshot.Records,
                        borrowing.BookId,
                        "Library Book");
                    LibraryCirculationCommandGuard.RequireVersion(
                        request.ExpectedBookEntityVersion,
                        book.Version,
                        "Library Book");
                    var copy = ValidateOpenPair(book, borrowing);

                    borrowing.MarkLost(utcNow, principal.UserId);
                    book.MarkCopyLost(copy.CopyId, utcNow, principal.UserId);
                    var transactionId = StageBoth(
                        principal.UserId,
                        bookSnapshot,
                        borrowingSnapshot);
                    return LibraryCirculationCommandGuard.Result(
                        transactionId,
                        book,
                        borrowing,
                        checked(bookSnapshot.Revision + 1L),
                        checked(borrowingSnapshot.Revision + 1L));
                });
        }

        private AuthorizationRequest InternalRequest(
            AuthorizationPrincipal principal,
            string permission)
        {
            return new AuthorizationRequest(
                permission,
                LibraryCirculationCommandGuard.ApplicationKind(principal),
                ConfidentialityClassification.Internal,
                null,
                new[]
                {
                    PrimaryRole.EmployeeFaculty,
                    PrimaryRole.Administrator
                });
        }

        private static LibraryBookCopy ValidateOpenPair(
            LibraryBook book,
            LibraryBorrowing borrowing)
        {
            if (!StringComparer.Ordinal.Equals(book.Id, borrowing.BookId))
            {
                throw new InvalidOperationException(
                    "Library Book and Borrowing references are inconsistent.");
            }

            if (borrowing.Status != LibraryBorrowingStatus.Issued
                && borrowing.Status != LibraryBorrowingStatus.Overdue)
            {
                throw new InvalidOperationException(
                    "Only an issued or overdue Library Borrowing can be completed.");
            }

            var copy = LibraryCirculationCommandGuard.FindCopy(
                book,
                borrowing.CopyId);
            if (copy.Status != LibraryCopyStatus.OnLoan)
            {
                throw new InvalidOperationException(
                    "The Library Book Copy is not currently on loan.");
            }

            return copy;
        }

        private string StageBoth(
            string actorUserId,
            RepositorySnapshot<LibraryBook> bookSnapshot,
            RepositorySnapshot<LibraryBorrowing> borrowingSnapshot)
        {
            return _transactions.Execute(scope =>
            {
                scope.Stage(
                    _books,
                    bookSnapshot.Records.ToList(),
                    bookSnapshot.Revision,
                    actorUserId);
                scope.Stage(
                    _borrowings,
                    borrowingSnapshot.Records.ToList(),
                    borrowingSnapshot.Revision,
                    actorUserId);
            });
        }
    }
}