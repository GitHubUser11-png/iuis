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
    }
}
