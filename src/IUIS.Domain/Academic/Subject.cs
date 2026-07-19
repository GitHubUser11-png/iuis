using System;
using System.Collections.Generic;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Academic
{
    public sealed class SubjectPrerequisite
    {
        private SubjectPrerequisite()
        {
            PrerequisiteSubjectId = string.Empty;
        }

        public SubjectPrerequisite(string prerequisiteSubjectId)
        {
            PrerequisiteSubjectId = RequireSubjectIdentifier(prerequisiteSubjectId);
        }

        public string PrerequisiteSubjectId { get; private set; }

        private static string RequireSubjectIdentifier(string value)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, "SUB"))
            {
                throw new DomainValidationException(
                    "Subject prerequisite IDs must use the SUB prefix.");
            }

            return identifier.Value;
        }
    }

    public sealed class Subject : EntityBase
    {
        private readonly List<SubjectPrerequisite> _prerequisites;

        private Subject()
        {
            Code = string.Empty;
            Title = string.Empty;
            _prerequisites = new List<SubjectPrerequisite>();
        }

        public Subject(
            string id,
            string code,
            string title,
            decimal units,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireSubjectIdentifier(id), createdAtUtc, createdByUserId)
        {
            Code = NormalizeCode(code);
            Title = TextNormalizer.Required(title, nameof(title), 200);
            Units = AcademicUnitRules.RequireValid(units, nameof(units));
            Status = SubjectStatus.Draft;
            _prerequisites = new List<SubjectPrerequisite>();
        }

        public string Code { get; private set; }

        public string Title { get; private set; }

        public decimal Units { get; private set; }

        public SubjectStatus Status { get; private set; }

        public IReadOnlyList<SubjectPrerequisite> Prerequisites
        {
            get { return _prerequisites.AsReadOnly(); }
        }

        public void UpdateDetails(
            string title,
            decimal units,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureDefinitionEditable();
            var normalizedTitle = TextNormalizer.Required(title, nameof(title), 200);
            var normalizedUnits = AcademicUnitRules.RequireValid(units, nameof(units));

            RecordChange(changedAtUtc, changedByUserId);
            Title = normalizedTitle;
            Units = normalizedUnits;
        }

        public void AddPrerequisite(
            SubjectPrerequisite prerequisite,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureDefinitionEditable();
            if (prerequisite == null)
            {
                throw new DomainValidationException("Subject prerequisite is required.");
            }

            if (StringComparer.Ordinal.Equals(Id, prerequisite.PrerequisiteSubjectId))
            {
                throw new DomainValidationException("A Subject cannot require itself.");
            }

            if (FindPrerequisiteIndex(prerequisite.PrerequisiteSubjectId) >= 0)
            {
                throw new DomainValidationException(
                    "The prerequisite Subject is already registered.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            _prerequisites.Add(prerequisite);
        }

        public void RemovePrerequisite(
            string prerequisiteSubjectId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureDefinitionEditable();
            var normalizedId = RequireSubjectIdentifier(prerequisiteSubjectId);
            var index = FindPrerequisiteIndex(normalizedId);
            if (index < 0)
            {
                throw new DomainValidationException(
                    "The prerequisite Subject is not registered.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            _prerequisites.RemoveAt(index);
        }

        public void ChangeStatus(
            SubjectStatus targetStatus,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            ValidateTransition(Status, targetStatus);
            RecordChange(changedAtUtc, changedByUserId);
            Status = targetStatus;
        }

        private void EnsureDefinitionEditable()
        {
            if (Status != SubjectStatus.Draft && Status != SubjectStatus.Inactive)
            {
                throw new DomainValidationException(
                    "Subject definitions can be changed only while Draft or Inactive.");
            }
        }

        private int FindPrerequisiteIndex(string prerequisiteSubjectId)
        {
            for (var index = 0; index < _prerequisites.Count; index++)
            {
                if (StringComparer.Ordinal.Equals(
                    _prerequisites[index].PrerequisiteSubjectId,
                    prerequisiteSubjectId))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string NormalizeCode(string value)
        {
            var normalized = TextNormalizer.Required(value, nameof(value), 30).ToUpperInvariant();
            for (var index = 0; index < normalized.Length; index++)
            {
                var character = normalized[index];
                if ((character >= 'A' && character <= 'Z')
                    || (character >= '0' && character <= '9')
                    || character == '-'
                    || character == '.')
                {
                    continue;
                }

                throw new DomainValidationException(
                    "Subject code may contain only letters, digits, hyphens, and periods.");
            }

            return normalized;
        }

        private static void ValidateTransition(SubjectStatus current, SubjectStatus target)
        {
            if (!Enum.IsDefined(typeof(SubjectStatus), target)
                || target == SubjectStatus.Unspecified
                || target == current)
            {
                throw new DomainValidationException("The requested Subject status transition is invalid.");
            }

            var valid = (current == SubjectStatus.Draft
                    && (target == SubjectStatus.Active || target == SubjectStatus.Retired))
                || (current == SubjectStatus.Active
                    && (target == SubjectStatus.Inactive || target == SubjectStatus.Retired))
                || (current == SubjectStatus.Inactive
                    && (target == SubjectStatus.Active || target == SubjectStatus.Retired));

            if (!valid)
            {
                throw new DomainValidationException(
                    "Subject status cannot transition from " + current + " to " + target + ".");
            }
        }

        private static string RequireSubjectIdentifier(string value)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, "SUB"))
            {
                throw new DomainValidationException("Subject IDs must use the SUB prefix.");
            }

            return identifier.Value;
        }
    }
}
