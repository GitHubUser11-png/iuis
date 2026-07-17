using System;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;
using IUIS.Domain.Time;

namespace IUIS.Domain.People
{
    public abstract class PersonRecordBase : EntityBase
    {
        protected PersonRecordBase()
        {
        }

        protected PersonRecordBase(
            string id,
            PersonRecordKind recordKind,
            PersonName name,
            ContactInformation contactInformation,
            PostalAddress postalAddress,
            InstitutionLocalDate? dateOfBirth,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(id, createdAtUtc, createdByUserId)
        {
            if (recordKind == PersonRecordKind.Unspecified)
            {
                throw new DomainValidationException("A person record kind is required.");
            }

            if (name == null)
            {
                throw new DomainValidationException(nameof(name) + " is required.");
            }

            if (contactInformation == null)
            {
                throw new DomainValidationException(nameof(contactInformation) + " is required.");
            }

            RecordKind = recordKind;
            Name = name;
            ContactInformation = contactInformation;
            PostalAddress = postalAddress;
            DateOfBirth = dateOfBirth;
        }

        public PersonRecordKind RecordKind { get; protected set; }

        public PersonName Name { get; protected set; }

        public ContactInformation ContactInformation { get; protected set; }

        public PostalAddress PostalAddress { get; protected set; }

        public InstitutionLocalDate? DateOfBirth { get; protected set; }

        public void ChangeName(
            PersonName name,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (name == null)
            {
                throw new DomainValidationException(nameof(name) + " is required.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            Name = name;
        }

        public void ChangeContactInformation(
            ContactInformation contactInformation,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            if (contactInformation == null)
            {
                throw new DomainValidationException(nameof(contactInformation) + " is required.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            ContactInformation = contactInformation;
        }

        public void ChangePostalAddress(
            PostalAddress postalAddress,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RecordChange(changedAtUtc, changedByUserId);
            PostalAddress = postalAddress;
        }

        public void ChangeDateOfBirth(
            InstitutionLocalDate? dateOfBirth,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            RecordChange(changedAtUtc, changedByUserId);
            DateOfBirth = dateOfBirth;
        }

        protected static string RequireDepartmentCode(string value, string parameterName)
        {
            var normalized = TextNormalizer.Required(value, parameterName, 30).ToUpperInvariant();
            for (var index = 0; index < normalized.Length; index++)
            {
                var character = normalized[index];
                var isUpperLetter = character >= 'A' && character <= 'Z';
                var isDigit = character >= '0' && character <= '9';
                var isAllowedSeparator = character == '-' || character == '_';
                if (!isUpperLetter && !isDigit && !isAllowedSeparator)
                {
                    throw new DomainValidationException(
                        parameterName + " may contain only uppercase letters, digits, hyphens, or underscores.");
                }
            }

            return normalized;
        }
    }
}
