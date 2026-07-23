using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using IUIS.Domain.Library;
using IUIS.Domain.Time;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class LibraryBookJsonMapper : IJsonRecordMapper<LibraryBook>
    {
        public LibraryBook FromJson(
            JsonElement element,
            JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedLibraryBookRecord>(
                element,
                options,
                "LibraryBook");
            var copies = (record.Copies
                ?? new List<PersistedLibraryBookCopyRecord>())
                .Select(item =>
                {
                    if (item == null)
                    {
                        throw new InvalidOperationException(
                            "LibraryBook persisted copy is invalid.");
                    }

                    return LibraryBookCopy.Rehydrate(
                        item.CopyId,
                        item.Barcode,
                        PersistedRecordMapperGuard.ParseEnum<LibraryCopyCondition>(
                            item.Condition,
                            nameof(item.Condition),
                            true),
                        PersistedRecordMapperGuard.ParseEnum<LibraryCopyStatus>(
                            item.Status,
                            nameof(item.Status),
                            true));
                })
                .ToList();

            return LibraryBook.Rehydrate(
                record.Id,
                record.Isbn,
                record.Title,
                record.Author,
                record.Publisher,
                record.Category,
                PersistedRecordMapperGuard.ParseEnum<LibraryBookStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                copies,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(
            LibraryBook value,
            JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            value.AssertInventoryInvariant();
            return PersistedRecordMapperGuard.Write(
                new PersistedLibraryBookRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    Isbn = value.Isbn,
                    Title = value.Title,
                    Author = value.Author,
                    Publisher = value.Publisher,
                    Category = value.Category,
                    Status = value.Status.ToString(),
                    Copies = value.Copies.Select(item =>
                        new PersistedLibraryBookCopyRecord
                        {
                            CopyId = item.CopyId,
                            Barcode = item.Barcode,
                            Condition = item.Condition.ToString(),
                            Status = item.Status.ToString()
                        }).ToList()
                },
                options);
        }
    }

    public sealed class LibraryBorrowingJsonMapper :
        IJsonRecordMapper<LibraryBorrowing>
    {
        public LibraryBorrowing FromJson(
            JsonElement element,
            JsonSerializerOptions options)
        {
            var record = PersistedRecordMapperGuard.Read<PersistedLibraryBorrowingRecord>(
                element,
                options,
                "LibraryBorrowing");
            return LibraryBorrowing.Rehydrate(
                record.Id,
                record.StudentId,
                record.BookId,
                record.CopyId,
                InstitutionLocalDate.Parse(record.DueDate),
                PersistedRecordMapperGuard.ParseEnum<LibraryBorrowingStatus>(
                    record.Status,
                    nameof(record.Status),
                    true),
                record.IssuedAtUtc,
                record.IssuedByUserId,
                record.RenewalCount,
                record.ReturnedAtUtc,
                record.ReturnedByUserId,
                ParseOptionalCondition(record.ReturnCondition),
                record.LostAtUtc,
                record.LostRecordedByUserId,
                record.Version,
                record.IsArchived,
                record.CreatedAtUtc,
                record.CreatedByUserId,
                record.UpdatedAtUtc,
                record.UpdatedByUserId,
                record.ArchivedAtUtc,
                record.ArchivedByUserId);
        }

        public JsonElement ToJson(
            LibraryBorrowing value,
            JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return PersistedRecordMapperGuard.Write(
                new PersistedLibraryBorrowingRecord
                {
                    Id = value.Id,
                    Version = value.Version,
                    IsArchived = value.IsArchived,
                    CreatedAtUtc = value.CreatedAtUtc,
                    CreatedByUserId = value.CreatedByUserId,
                    UpdatedAtUtc = value.UpdatedAtUtc,
                    UpdatedByUserId = value.UpdatedByUserId,
                    ArchivedAtUtc = value.ArchivedAtUtc,
                    ArchivedByUserId = value.ArchivedByUserId,
                    StudentId = value.StudentId,
                    BookId = value.BookId,
                    CopyId = value.CopyId,
                    DueDate = value.DueDate.ToString(),
                    Status = value.Status.ToString(),
                    IssuedAtUtc = value.IssuedAtUtc,
                    IssuedByUserId = value.IssuedByUserId,
                    RenewalCount = value.RenewalCount,
                    ReturnedAtUtc = value.ReturnedAtUtc,
                    ReturnedByUserId = value.ReturnedByUserId,
                    ReturnCondition = value.ReturnCondition.HasValue
                        ? value.ReturnCondition.Value.ToString()
                        : null,
                    LostAtUtc = value.LostAtUtc,
                    LostRecordedByUserId = value.LostRecordedByUserId
                },
                options);
        }

        private static LibraryCopyCondition? ParseOptionalCondition(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? (LibraryCopyCondition?)null
                : PersistedRecordMapperGuard.ParseEnum<LibraryCopyCondition>(
                    value,
                    nameof(PersistedLibraryBorrowingRecord.ReturnCondition),
                    true);
        }
    }
}