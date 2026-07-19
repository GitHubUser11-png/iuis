using System;
using System.Collections.Generic;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Academic
{
    public sealed class CurriculumSubject
    {
        private CurriculumSubject()
        {
            SubjectId = string.Empty;
        }

        public CurriculumSubject(
            string subjectId,
            int yearLevel,
            int termNumber,
            decimal units,
            bool isRequired)
        {
            SubjectId = RequireIdentifier(subjectId, "SUB", "Subject");
            YearLevel = RequireRange(yearLevel, 1, 8, nameof(yearLevel));
            TermNumber = RequireRange(termNumber, 1, 4, nameof(termNumber));
            Units = AcademicUnitRules.RequireValid(units, nameof(units));
            IsRequired = isRequired;
        }

        public string SubjectId { get; private set; }

        public int YearLevel { get; private set; }

        public int TermNumber { get; private set; }

        public decimal Units { get; private set; }

        public bool IsRequired { get; private set; }

        private static int RequireRange(int value, int minimum, int maximum, string parameterName)
        {
            if (value < minimum || value > maximum)
            {
                throw new DomainValidationException(
                    parameterName + " must be between " + minimum + " and " + maximum + ".");
            }

            return value;
        }

        private static string RequireIdentifier(string value, string prefix, string entityName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
            {
                throw new DomainValidationException(
                    entityName + " IDs must use the " + prefix + " prefix.");
            }

            return identifier.Value;
        }
    }

    public sealed class Curriculum : EntityBase
    {
        private readonly List<CurriculumSubject> _subjects;

        private Curriculum()
        {
            CourseId = string.Empty;
            VersionCode = string.Empty;
            _subjects = new List<CurriculumSubject>();
        }

        public Curriculum(
            string id,
            string courseId,
            string versionCode,
            int effectiveYear,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "CUR", "Curriculum"), createdAtUtc, createdByUserId)
        {
            CourseId = RequireIdentifier(courseId, "CRS", "Course");
            VersionCode = NormalizeVersionCode(versionCode);
            EffectiveYear = RequireEffectiveYear(effectiveYear);
            Status = CurriculumStatus.Draft;
            _subjects = new List<CurriculumSubject>();
        }

        public string CourseId { get; private set; }

        public string VersionCode { get; private set; }

        public int EffectiveYear { get; private set; }

        public CurriculumStatus Status { get; private set; }

        public IReadOnlyList<CurriculumSubject> Subjects
        {
            get { return _subjects.AsReadOnly(); }
        }

        public decimal TotalUnits
        {
            get
            {
                var total = 0m;
                foreach (var subject in _subjects)
                {
                    total = AcademicUnitRules.Sum(total, subject.Units);
                }

                return total;
            }
        }

        public void AddSubject(
            CurriculumSubject subject,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureDraft();
            if (subject == null)
            {
                throw new DomainValidationException("Curriculum subject is required.");
            }

            if (FindSubjectIndex(subject.SubjectId) >= 0)
            {
                throw new DomainValidationException(
                    "A Subject can appear only once in a Curriculum version.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            _subjects.Add(subject);
        }

        public void RemoveSubject(
            string subjectId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureDraft();
            var normalizedSubjectId = RequireIdentifier(subjectId, "SUB", "Subject");
            var index = FindSubjectIndex(normalizedSubjectId);
            if (index < 0)
            {
                throw new DomainValidationException("The Subject is not part of this Curriculum.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            _subjects.RemoveAt(index);
        }

        public void Approve(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != CurriculumStatus.Draft)
            {
                throw new DomainValidationException("Only a Draft Curriculum can be approved.");
            }

            if (_subjects.Count == 0)
            {
                throw new DomainValidationException(
                    "A Curriculum must contain at least one Subject before approval.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            Status = CurriculumStatus.Approved;
        }

        public void Activate(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != CurriculumStatus.Approved)
            {
                throw new DomainValidationException("Only an Approved Curriculum can be activated.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            Status = CurriculumStatus.Active;
        }

        public void Supersede(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != CurriculumStatus.Active)
            {
                throw new DomainValidationException("Only an Active Curriculum can be superseded.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            Status = CurriculumStatus.Superseded;
        }

        public void Retire(DateTime changedAtUtc, string changedByUserId)
        {
            if (Status != CurriculumStatus.Approved
                && Status != CurriculumStatus.Active
                && Status != CurriculumStatus.Superseded)
            {
                throw new DomainValidationException(
                    "Only an Approved, Active, or Superseded Curriculum can be retired.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            Status = CurriculumStatus.Retired;
        }

        private void EnsureDraft()
        {
            if (Status != CurriculumStatus.Draft)
            {
                throw new DomainValidationException(
                    "Curriculum Subjects can be changed only while the Curriculum is Draft.");
            }
        }

        private int FindSubjectIndex(string subjectId)
        {
            for (var index = 0; index < _subjects.Count; index++)
            {
                if (StringComparer.Ordinal.Equals(_subjects[index].SubjectId, subjectId))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string NormalizeVersionCode(string value)
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
                    "Curriculum version code may contain only letters, digits, hyphens, and periods.");
            }

            return normalized;
        }

        private static int RequireEffectiveYear(int value)
        {
            if (value < 2000 || value > 9999)
            {
                throw new DomainValidationException(
                    "Curriculum effective year must be between 2000 and 9999.");
            }

            return value;
        }

        private static string RequireIdentifier(string value, string prefix, string entityName)
        {
            var identifier = InstitutionIdentifier.Parse(value);
            if (!StringComparer.Ordinal.Equals(identifier.Prefix, prefix))
            {
                throw new DomainValidationException(
                    entityName + " IDs must use the " + prefix + " prefix.");
            }

            return identifier.Value;
        }
    }
}
