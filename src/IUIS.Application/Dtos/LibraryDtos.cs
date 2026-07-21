using System;
using System.Collections.Generic;

namespace IUIS.Application.Dtos
{
    public sealed class StudentLibraryBorrowingDto
    {
        public string BorrowingId { get; set; }
        public string BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string BookCategory { get; set; }
        public string CopyId { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? IssuedAtUtc { get; set; }
        public int RenewalCount { get; set; }
        public DateTime? ReturnedAtUtc { get; set; }
        public string ReturnCondition { get; set; }
        public DateTime? LostAtUtc { get; set; }
        public long EntityVersion { get; set; }
    }

    public sealed class StudentLibraryCirculationOverviewDto
    {
        public string StudentId { get; set; }
        public long BookRepositoryRevision { get; set; }
        public long BorrowingRepositoryRevision { get; set; }
        public IReadOnlyList<StudentLibraryBorrowingDto> Borrowings { get; set; }
    }

    public sealed class LibraryCirculationCommandResult
    {
        public string TransactionId { get; set; }
        public string BorrowingId { get; set; }
        public string BookId { get; set; }
        public string CopyId { get; set; }
        public long BookRepositoryRevision { get; set; }
        public long BorrowingRepositoryRevision { get; set; }
        public long BookEntityVersion { get; set; }
        public long BorrowingEntityVersion { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string UpdatedByUserId { get; set; }
    }
}