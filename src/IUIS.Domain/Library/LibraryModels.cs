using System;
using System.Collections.Generic;
using System.Linq;

using IUIS.Domain.Common;
using IUIS.Domain.Services;
using IUIS.Domain.Time;

namespace IUIS.Domain.Library
{
    public enum LibraryBookStatus
    {
        Active = 0,
        Inactive = 1,
        Retired = 2
    }

    public enum LibraryCopyStatus
    {
        Available = 0,
        OnLoan = 1,
        Maintenance = 2,
        Lost = 3
    }

    public enum LibraryCopyCondition
    {
        New = 0,
        Good = 1,
        Fair = 2,
        Damaged = 3,
        Lost = 4
    }

    public enum LibraryBorrowingStatus
    {
        Prepared = 0,
        Issued = 1,
        Overdue = 2,
        Returned = 3,
        Lost = 4,
        Cancelled = 5
    }

    public sealed class LibraryBookCopy
    {
        internal LibraryBookCopy(
            string copyId,
            string barcode,
            LibraryCopyCondition condition)
        {
            CopyId = ServiceDomainGuard.RequireIdentifier(copyId, "LCP", nameof(copyId));
            Barcode = ServiceDomainGuard.RequiredCode(barcode, nameof(barcode), 64);
            Condition = ServiceDomainGuard.RequireDefined(condition, nameof(condition));
            if (condition == LibraryCopyCondition.Lost)
            {
                throw new DomainValidationException(
                    "A new Library Book Copy cannot begin in Lost condition.");
            }

            Status = condition == LibraryCopyCondition.Damaged
                ? LibraryCopyStatus.Maintenance
                : LibraryCopyStatus.Available;
        }

        public string CopyId { get; }

        public string Barcode { get; }

        public LibraryCopyCondition Condition { get; private set; }

        public LibraryCopyStatus Status { get; private set; }

        public static LibraryBookCopy Rehydrate(
            string copyId,
            string barcode,
            LibraryCopyCondition condition,
            LibraryCopyStatus status)
        {
            condition = ServiceDomainGuard.RequireDefined(condition, nameof(condition));
            status = ServiceDomainGuard.RequireDefined(status, nameof(status));
            ValidatePersistedState(condition, status);

            var seedCondition = condition == LibraryCopyCondition.Lost
                ? LibraryCopyCondition.Good
                : condition;
            var copy = new LibraryBookCopy(copyId, barcode, seedCondition)
            {
                Condition = condition,
                Status = status
            };
            return copy;
        }

        internal void MarkOnLoan()
        {
            if (Status != LibraryCopyStatus.Available)
            {
                throw new DomainValidationException(
                    "Only an available Library Book Copy can be placed on loan.");
            }

            Status = LibraryCopyStatus.OnLoan;
        }

        internal void ReturnFromLoan(LibraryCopyCondition condition)
        {
            if (Status != LibraryCopyStatus.OnLoan)
            {
                throw new DomainValidationException(
                    "Only an on-loan Library Book Copy can be returned.");
            }

            condition = ServiceDomainGuard.RequireDefined(condition, nameof(condition));
            if (condition == LibraryCopyCondition.Lost)
            {
                throw new DomainValidationException(
                    "A lost copy must use the explicit lost workflow.");
            }

            Condition = condition;
            Status = condition == LibraryCopyCondition.Damaged
                ? LibraryCopyStatus.Maintenance
                : LibraryCopyStatus.Available;
        }

        internal void SendToMaintenance()
        {
            if (Status != LibraryCopyStatus.Available)
            {
                throw new DomainValidationException(
                    "Only an available Library Book Copy can enter maintenance.");
            }

            Condition = LibraryCopyCondition.Damaged;
            Status = LibraryCopyStatus.Maintenance;
        }

        internal void ReturnFromMaintenance(LibraryCopyCondition condition)
        {
            if (Status != LibraryCopyStatus.Maintenance)
            {
                throw new DomainValidationException(
                    "Only a maintenance Library Book Copy can return to inventory.");
            }

            condition = ServiceDomainGuard.RequireDefined(condition, nameof(condition));
            if (condition == LibraryCopyCondition.Damaged
                || condition == LibraryCopyCondition.Lost)
            {
                throw new DomainValidationException(
                    "A copy returning from maintenance must be usable.");
            }

            Condition = condition;
            Status = LibraryCopyStatus.Available;
        }

        internal void MarkLost()
        {
            if (Status == LibraryCopyStatus.Lost)
            {
                throw new DomainValidationException("The Library Book Copy is already lost.");
            }

            Condition = LibraryCopyCondition.Lost;
            Status = LibraryCopyStatus.Lost;
        }

        private static void ValidatePersistedState(
            LibraryCopyCondition condition,
            LibraryCopyStatus status)
        {
            if (status == LibraryCopyStatus.Lost)
            {
                if (condition != LibraryCopyCondition.Lost)
                {
                    throw new DomainValidationException(
                        "A persisted lost Library Book Copy must have Lost condition.");
                }

                return;
            }

            if (condition == LibraryCopyCondition.Lost)
            {
                throw new DomainValidationException(
                    "Only a persisted lost Library Book Copy may have Lost condition.");
            }

            if (status == LibraryCopyStatus.Maintenance)
            {
                if (condition != LibraryCopyCondition.Damaged)
                {
                    throw new DomainValidationException(
                        "A persisted maintenance Library Book Copy must have Damaged condition.");
                }

                return;
            }

            if (condition == LibraryCopyCondition.Damaged)
            {
                throw new DomainValidationException(
                    "A persisted damaged Library Book Copy must be in maintenance.");
            }
        }
    }

    public sealed class LibraryBook : EntityBase
    {
        private readonly List<LibraryBookCopy> _copies = new List<LibraryBookCopy>();

        public LibraryBook(
            string id,
            string isbn,
            string title,
            string author,
            string publisher,
            string category,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "LBK", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
        {
            Isbn = ServiceDomainGuard.RequiredCode(isbn, nameof(isbn), 32);
            Title = ServiceDomainGuard.RequiredText(title, nameof(title), 200);
            Author = ServiceDomainGuard.RequiredText(author, nameof(author), 160);
            Publisher = ServiceDomainGuard.OptionalText(publisher, nameof(publisher), 160);
            Category = ServiceDomainGuard.RequiredText(category, nameof(category), 100);
            Status = LibraryBookStatus.Active;
        }

        public string Isbn { get; }

        public string Title { get; private set; }

        public string Author { get; private set; }

        public string Publisher { get; private set; }

        public string Category { get; private set; }

        public LibraryBookStatus Status { get; private set; }

        public IReadOnlyList<LibraryBookCopy> Copies
        {
            get { return _copies.AsReadOnly(); }
        }

        public int TotalCopies
        {
            get { return _copies.Count; }
        }

        public int AvailableCopies
        {
            get { return CountCopies(LibraryCopyStatus.Available); }
        }

        public int ActiveBorrowedCopies
        {
            get { return CountCopies(LibraryCopyStatus.OnLoan); }
        }

        public int MaintenanceCopies
        {
            get { return CountCopies(LibraryCopyStatus.Maintenance); }
        }

        public int LostCopies
        {
            get { return CountCopies(LibraryCopyStatus.Lost); }
        }

        public static LibraryBook Rehydrate(
            string id,
            string isbn,
            string title,
            string author,
            string publisher,
            string category,
            LibraryBookStatus status,
            IEnumerable<LibraryBookCopy> copies,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            status = ServiceDomainGuard.RequireDefined(status, nameof(status));
            if (copies == null)
            {
                throw new DomainValidationException(
                    "Persisted Library Book copies are required.");
            }

            var book = new LibraryBook(
                id,
                isbn,
                title,
                author,
                publisher,
                category,
                createdAtUtc,
                createdByUserId);
            foreach (var copy in copies)
            {
                if (copy == null)
                {
                    throw new DomainValidationException(
                        "A persisted Library Book Copy is invalid.");
                }

                if (book._copies.Any(item =>
                    StringComparer.Ordinal.Equals(item.CopyId, copy.CopyId)))
                {
                    throw new DomainValidationException(
                        "Persisted Library Book Copy IDs must be unique.");
                }

                if (book._copies.Any(item =>
                    StringComparer.Ordinal.Equals(item.Barcode, copy.Barcode)))
                {
                    throw new DomainValidationException(
                        "Persisted Library Book Copy barcodes must be unique.");
                }

                book._copies.Add(copy);
            }

            book.Status = status;
            book.AssertInventoryInvariant();
            if (status == LibraryBookStatus.Retired
                && book.ActiveBorrowedCopies > 0)
            {
                throw new DomainValidationException(
                    "A persisted retired Library Book cannot contain on-loan copies.");
            }

            book.RestorePersistenceState(
                version,
                isArchived,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                archivedAtUtc,
                archivedByUserId);
            return book;
        }

        public void AddCopy(
            string copyId,
            string barcode,
            LibraryCopyCondition condition,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotRetired();
            var copy = new LibraryBookCopy(copyId, barcode, condition);
            if (_copies.Any(item => StringComparer.Ordinal.Equals(item.CopyId, copy.CopyId)))
            {
                throw new DomainValidationException(
                    "The Library Book already contains the Copy ID.");
            }

            if (_copies.Any(item => StringComparer.Ordinal.Equals(item.Barcode, copy.Barcode)))
            {
                throw new DomainValidationException(
                    "The Library Book already contains the barcode.");
            }

            _copies.Add(copy);
            RecordServiceChange(changedAtUtc, changedByUserId);
            AssertInventoryInvariant();
        }

        public void UpdateBibliographicDetails(
            string title,
            string author,
            string publisher,
            string category,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureNotRetired();
            Title = ServiceDomainGuard.RequiredText(title, nameof(title), 200);
            Author = ServiceDomainGuard.RequiredText(author, nameof(author), 160);
            Publisher = ServiceDomainGuard.OptionalText(publisher, nameof(publisher), 160);
            Category = ServiceDomainGuard.RequiredText(category, nameof(category), 100);
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void MarkCopyOnLoan(
            string copyId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != LibraryBookStatus.Active)
            {
                throw new DomainValidationException(
                    "Only an active Library Book can issue copies.");
            }

            FindCopy(copyId).MarkOnLoan();
            RecordServiceChange(changedAtUtc, changedByUserId);
            AssertInventoryInvariant();
        }

        public void MarkCopyReturned(
            string copyId,
            LibraryCopyCondition returnCondition,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            FindCopy(copyId).ReturnFromLoan(returnCondition);
            RecordServiceChange(changedAtUtc, changedByUserId);
            AssertInventoryInvariant();
        }

        public void SendCopyToMaintenance(
            string copyId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            FindCopy(copyId).SendToMaintenance();
            RecordServiceChange(changedAtUtc, changedByUserId);
            AssertInventoryInvariant();
        }

        public void ReturnCopyFromMaintenance(
            string copyId,
            LibraryCopyCondition condition,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            FindCopy(copyId).ReturnFromMaintenance(condition);
            RecordServiceChange(changedAtUtc, changedByUserId);
            AssertInventoryInvariant();
        }

        public void MarkCopyLost(
            string copyId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            FindCopy(copyId).MarkLost();
            RecordServiceChange(changedAtUtc, changedByUserId);
            AssertInventoryInvariant();
        }

        public void Deactivate(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != LibraryBookStatus.Active)
            {
                throw new DomainValidationException("Only an active Library Book can be deactivated.");
            }

            Status = LibraryBookStatus.Inactive;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void Activate(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != LibraryBookStatus.Inactive)
            {
                throw new DomainValidationException("Only an inactive Library Book can be activated.");
            }

            Status = LibraryBookStatus.Active;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void Retire(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status == LibraryBookStatus.Retired)
            {
                throw new DomainValidationException("The Library Book is already retired.");
            }

            if (ActiveBorrowedCopies > 0)
            {
                throw new DomainValidationException(
                    "A Library Book with active borrowed copies cannot be retired.");
            }

            Status = LibraryBookStatus.Retired;
            RecordServiceChange(changedAtUtc, changedByUserId);
        }

        public void AssertInventoryInvariant()
        {
            var accounted = AvailableCopies
                + ActiveBorrowedCopies
                + MaintenanceCopies
                + LostCopies;

            if (TotalCopies != accounted)
            {
                throw new DomainValidationException(
                    "Library inventory totals are internally inconsistent.");
            }
        }

        private int CountCopies(LibraryCopyStatus status)
        {
            return _copies.Count(copy => copy.Status == status);
        }

        private LibraryBookCopy FindCopy(string copyId)
        {
            var normalized = ServiceDomainGuard.RequireIdentifier(copyId, "LCP", nameof(copyId));
            var copy = _copies.FirstOrDefault(
                item => StringComparer.Ordinal.Equals(item.CopyId, normalized));
            if (copy == null)
            {
                throw new DomainValidationException("The Library Book Copy does not exist.");
            }

            return copy;
        }

        private void EnsureNotRetired()
        {
            if (Status == LibraryBookStatus.Retired)
            {
                throw new DomainValidationException("A retired Library Book cannot be changed.");
            }
        }

        private void RecordServiceChange(DateTime changedAtUtc, string changedByUserId)
        {
            RecordChange(
                changedAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    changedByUserId,
                    "USR",
                    nameof(changedByUserId)));
        }
    }

    public sealed class LibraryBorrowing : EntityBase
    {
        public LibraryBorrowing(
            string id,
            string studentId,
            string bookId,
            string copyId,
            InstitutionLocalDate dueDate,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(
                ServiceDomainGuard.RequireIdentifier(id, "BRW", nameof(id)),
                createdAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    createdByUserId,
                    "USR",
                    nameof(createdByUserId)))
        {
            StudentId = ServiceDomainGuard.RequireIdentifier(studentId, "STU", nameof(studentId));
            BookId = ServiceDomainGuard.RequireIdentifier(bookId, "LBK", nameof(bookId));
            CopyId = ServiceDomainGuard.RequireIdentifier(copyId, "LCP", nameof(copyId));
            DueDate = dueDate;
            Status = LibraryBorrowingStatus.Prepared;
        }

        public string StudentId { get; }

        public string BookId { get; }

        public string CopyId { get; }

        public InstitutionLocalDate DueDate { get; private set; }

        public LibraryBorrowingStatus Status { get; private set; }

        public DateTime? IssuedAtUtc { get; private set; }

        public string IssuedByUserId { get; private set; }

        public int RenewalCount { get; private set; }

        public DateTime? ReturnedAtUtc { get; private set; }

        public string ReturnedByUserId { get; private set; }

        public LibraryCopyCondition? ReturnCondition { get; private set; }

        public DateTime? LostAtUtc { get; private set; }

        public string LostRecordedByUserId { get; private set; }

        public static LibraryBorrowing Rehydrate(
            string id,
            string studentId,
            string bookId,
            string copyId,
            InstitutionLocalDate dueDate,
            LibraryBorrowingStatus status,
            DateTime? issuedAtUtc,
            string issuedByUserId,
            int renewalCount,
            DateTime? returnedAtUtc,
            string returnedByUserId,
            LibraryCopyCondition? returnCondition,
            DateTime? lostAtUtc,
            string lostRecordedByUserId,
            long version,
            bool isArchived,
            DateTime createdAtUtc,
            string createdByUserId,
            DateTime updatedAtUtc,
            string updatedByUserId,
            DateTime? archivedAtUtc,
            string archivedByUserId)
        {
            status = ServiceDomainGuard.RequireDefined(status, nameof(status));
            if (renewalCount < 0)
            {
                throw new DomainValidationException(
                    "Persisted Library Borrowing renewal count cannot be negative.");
            }

            ValidatePersistedWorkflow(
                status,
                createdAtUtc,
                updatedAtUtc,
                issuedAtUtc,
                issuedByUserId,
                renewalCount,
                returnedAtUtc,
                returnedByUserId,
                returnCondition,
                lostAtUtc,
                lostRecordedByUserId);

            var borrowing = new LibraryBorrowing(
                id,
                studentId,
                bookId,
                copyId,
                dueDate,
                createdAtUtc,
                createdByUserId)
            {
                Status = status,
                IssuedAtUtc = issuedAtUtc,
                IssuedByUserId = NormalizeOptionalActor(
                    issuedByUserId,
                    nameof(issuedByUserId)),
                RenewalCount = renewalCount,
                ReturnedAtUtc = returnedAtUtc,
                ReturnedByUserId = NormalizeOptionalActor(
                    returnedByUserId,
                    nameof(returnedByUserId)),
                ReturnCondition = returnCondition,
                LostAtUtc = lostAtUtc,
                LostRecordedByUserId = NormalizeOptionalActor(
                    lostRecordedByUserId,
                    nameof(lostRecordedByUserId))
            };
            borrowing.RestorePersistenceState(
                version,
                isArchived,
                createdAtUtc,
                createdByUserId,
                updatedAtUtc,
                updatedByUserId,
                archivedAtUtc,
                archivedByUserId);
            return borrowing;
        }

        public void Issue(DateTime issuedAtUtc, string librarianUserId)
        {
            if (Status != LibraryBorrowingStatus.Prepared)
            {
                throw new DomainValidationException(
                    "Only a prepared Library Borrowing can be issued.");
            }

            issuedAtUtc = ServiceDomainGuard.RequireUtc(issuedAtUtc, nameof(issuedAtUtc));
            var issuedDate = InstitutionLocalDate.FromDateTime(issuedAtUtc);
            if (DueDate < issuedDate)
            {
                throw new DomainValidationException(
                    "The Library Borrowing due date cannot precede its issue date.");
            }

            IssuedByUserId = ServiceDomainGuard.RequireIdentifier(
                librarianUserId,
                "USR",
                nameof(librarianUserId));
            IssuedAtUtc = issuedAtUtc;
            Status = LibraryBorrowingStatus.Issued;
            RecordChange(issuedAtUtc, IssuedByUserId);
        }

        public void Renew(
            InstitutionLocalDate newDueDate,
            DateTime renewedAtUtc,
            string librarianUserId)
        {
            if (Status != LibraryBorrowingStatus.Issued
                && Status != LibraryBorrowingStatus.Overdue)
            {
                throw new DomainValidationException(
                    "Only an issued or overdue Library Borrowing can be renewed.");
            }

            if (newDueDate <= DueDate)
            {
                throw new DomainValidationException(
                    "A renewed due date must be later than the current due date.");
            }

            DueDate = newDueDate;
            RenewalCount = checked(RenewalCount + 1);
            Status = LibraryBorrowingStatus.Issued;
            RecordChange(
                renewedAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    librarianUserId,
                    "USR",
                    nameof(librarianUserId)));
        }

        public void MarkOverdue(
            InstitutionLocalDate asOfDate,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (Status != LibraryBorrowingStatus.Issued)
            {
                throw new DomainValidationException(
                    "Only an issued Library Borrowing can become overdue.");
            }

            if (asOfDate <= DueDate)
            {
                throw new DomainValidationException(
                    "A Library Borrowing is not overdue on or before its due date.");
            }

            Status = LibraryBorrowingStatus.Overdue;
            RecordChange(
                changedAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    changedByUserId,
                    "USR",
                    nameof(changedByUserId)));
        }

        public void Return(
            DateTime returnedAtUtc,
            LibraryCopyCondition returnCondition,
            string librarianUserId)
        {
            if (Status != LibraryBorrowingStatus.Issued
                && Status != LibraryBorrowingStatus.Overdue)
            {
                throw new DomainValidationException(
                    "Only an issued or overdue Library Borrowing can be returned.");
            }

            returnCondition = ServiceDomainGuard.RequireDefined(
                returnCondition,
                nameof(returnCondition));
            if (returnCondition == LibraryCopyCondition.Lost)
            {
                throw new DomainValidationException(
                    "A lost borrowing must use the explicit lost workflow.");
            }

            ReturnedAtUtc = ServiceDomainGuard.RequireUtc(returnedAtUtc, nameof(returnedAtUtc));
            ReturnedByUserId = ServiceDomainGuard.RequireIdentifier(
                librarianUserId,
                "USR",
                nameof(librarianUserId));
            ReturnCondition = returnCondition;
            Status = LibraryBorrowingStatus.Returned;
            RecordChange(ReturnedAtUtc.Value, ReturnedByUserId);
        }

        public void MarkLost(DateTime lostAtUtc, string librarianUserId)
        {
            if (Status != LibraryBorrowingStatus.Issued
                && Status != LibraryBorrowingStatus.Overdue)
            {
                throw new DomainValidationException(
                    "Only an issued or overdue Library Borrowing can be marked lost.");
            }

            LostAtUtc = ServiceDomainGuard.RequireUtc(lostAtUtc, nameof(lostAtUtc));
            LostRecordedByUserId = ServiceDomainGuard.RequireIdentifier(
                librarianUserId,
                "USR",
                nameof(librarianUserId));
            Status = LibraryBorrowingStatus.Lost;
            RecordChange(LostAtUtc.Value, LostRecordedByUserId);
        }

        public void Cancel(DateTime cancelledAtUtc, string librarianUserId)
        {
            if (Status != LibraryBorrowingStatus.Prepared)
            {
                throw new DomainValidationException(
                    "Only a prepared Library Borrowing can be cancelled.");
            }

            Status = LibraryBorrowingStatus.Cancelled;
            RecordChange(
                cancelledAtUtc,
                ServiceDomainGuard.RequireIdentifier(
                    librarianUserId,
                    "USR",
                    nameof(librarianUserId)));
        }

        private static void ValidatePersistedWorkflow(
            LibraryBorrowingStatus status,
            DateTime createdAtUtc,
            DateTime updatedAtUtc,
            DateTime? issuedAtUtc,
            string issuedByUserId,
            int renewalCount,
            DateTime? returnedAtUtc,
            string returnedByUserId,
            LibraryCopyCondition? returnCondition,
            DateTime? lostAtUtc,
            string lostRecordedByUserId)
        {
            createdAtUtc = ServiceDomainGuard.RequireUtc(createdAtUtc, nameof(createdAtUtc));
            updatedAtUtc = ServiceDomainGuard.RequireUtc(updatedAtUtc, nameof(updatedAtUtc));
            if (updatedAtUtc < createdAtUtc)
            {
                throw new DomainValidationException(
                    "Persisted Library Borrowing update time cannot precede creation.");
            }

            var requiresIssue = status == LibraryBorrowingStatus.Issued
                || status == LibraryBorrowingStatus.Overdue
                || status == LibraryBorrowingStatus.Returned
                || status == LibraryBorrowingStatus.Lost;
            if (requiresIssue)
            {
                if (!issuedAtUtc.HasValue || string.IsNullOrWhiteSpace(issuedByUserId))
                {
                    throw new DomainValidationException(
                        "Persisted active or terminal Library Borrowing state requires issue metadata.");
                }

                var issueTime = ServiceDomainGuard.RequireUtc(
                    issuedAtUtc.Value,
                    nameof(issuedAtUtc));
                if (issueTime < createdAtUtc || issueTime > updatedAtUtc)
                {
                    throw new DomainValidationException(
                        "Persisted Library Borrowing issue time is outside the entity timeline.");
                }

                NormalizeOptionalActor(issuedByUserId, nameof(issuedByUserId));
            }
            else if (issuedAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(issuedByUserId)
                || renewalCount != 0)
            {
                throw new DomainValidationException(
                    "Prepared or cancelled Library Borrowing state cannot retain issue metadata.");
            }

            if (status == LibraryBorrowingStatus.Returned)
            {
                if (!returnedAtUtc.HasValue
                    || string.IsNullOrWhiteSpace(returnedByUserId)
                    || !returnCondition.HasValue)
                {
                    throw new DomainValidationException(
                        "Persisted returned Library Borrowing state requires return metadata.");
                }

                if (returnCondition.Value == LibraryCopyCondition.Lost
                    || !Enum.IsDefined(typeof(LibraryCopyCondition), returnCondition.Value))
                {
                    throw new DomainValidationException(
                        "Persisted returned Library Borrowing condition is invalid.");
                }

                var returnTime = ServiceDomainGuard.RequireUtc(
                    returnedAtUtc.Value,
                    nameof(returnedAtUtc));
                if (!issuedAtUtc.HasValue
                    || returnTime < issuedAtUtc.Value
                    || returnTime != updatedAtUtc)
                {
                    throw new DomainValidationException(
                        "Persisted Library Borrowing return time is inconsistent.");
                }

                NormalizeOptionalActor(returnedByUserId, nameof(returnedByUserId));
            }
            else if (returnedAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(returnedByUserId)
                || returnCondition.HasValue)
            {
                throw new DomainValidationException(
                    "Only returned Library Borrowing state may retain return metadata.");
            }

            if (status == LibraryBorrowingStatus.Lost)
            {
                if (!lostAtUtc.HasValue || string.IsNullOrWhiteSpace(lostRecordedByUserId))
                {
                    throw new DomainValidationException(
                        "Persisted lost Library Borrowing state requires lost metadata.");
                }

                var lostTime = ServiceDomainGuard.RequireUtc(
                    lostAtUtc.Value,
                    nameof(lostAtUtc));
                if (!issuedAtUtc.HasValue
                    || lostTime < issuedAtUtc.Value
                    || lostTime != updatedAtUtc)
                {
                    throw new DomainValidationException(
                        "Persisted Library Borrowing lost time is inconsistent.");
                }

                NormalizeOptionalActor(
                    lostRecordedByUserId,
                    nameof(lostRecordedByUserId));
            }
            else if (lostAtUtc.HasValue
                || !string.IsNullOrWhiteSpace(lostRecordedByUserId))
            {
                throw new DomainValidationException(
                    "Only lost Library Borrowing state may retain lost metadata.");
            }
        }

        private static string NormalizeOptionalActor(
            string value,
            string parameterName)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : ServiceDomainGuard.RequireIdentifier(value, "USR", parameterName);
        }
    }
}