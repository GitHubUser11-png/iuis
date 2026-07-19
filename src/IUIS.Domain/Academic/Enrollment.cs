using System;
using System.Collections.Generic;

using IUIS.Domain.Common;
using IUIS.Domain.Identity;

namespace IUIS.Domain.Academic
{
    public sealed class EnrollmentSubjectLine
    {
        private EnrollmentSubjectLine()
        {
            SubjectId = string.Empty;
            SubjectCodeSnapshot = string.Empty;
            SubjectTitleSnapshot = string.Empty;
        }

        public EnrollmentSubjectLine(
            string subjectId,
            string subjectCodeSnapshot,
            string subjectTitleSnapshot,
            decimal unitsSnapshot,
            int yearLevelSnapshot,
            int termNumberSnapshot,
            bool isRequiredSnapshot,
            string sectionCode)
        {
            SubjectId = RequireIdentifier(subjectId, "SUB", "Subject");
            SubjectCodeSnapshot = NormalizeCode(
                subjectCodeSnapshot,
                nameof(subjectCodeSnapshot),
                30);
            SubjectTitleSnapshot = TextNormalizer.Required(
                subjectTitleSnapshot,
                nameof(subjectTitleSnapshot),
                200);
            UnitsSnapshot = AcademicUnitRules.RequireValid(
                unitsSnapshot,
                nameof(unitsSnapshot));
            YearLevelSnapshot = RequireRange(
                yearLevelSnapshot,
                1,
                8,
                nameof(yearLevelSnapshot));
            TermNumberSnapshot = RequireRange(
                termNumberSnapshot,
                1,
                4,
                nameof(termNumberSnapshot));
            IsRequiredSnapshot = isRequiredSnapshot;
            SectionCode = NormalizeOptionalCode(sectionCode, nameof(sectionCode), 40);
        }

        public string SubjectId { get; private set; }

        public string SubjectCodeSnapshot { get; private set; }

        public string SubjectTitleSnapshot { get; private set; }

        public decimal UnitsSnapshot { get; private set; }

        public int YearLevelSnapshot { get; private set; }

        public int TermNumberSnapshot { get; private set; }

        public bool IsRequiredSnapshot { get; private set; }

        public string SectionCode { get; private set; }

        private static int RequireRange(int value, int minimum, int maximum, string parameterName)
        {
            if (value < minimum || value > maximum)
            {
                throw new DomainValidationException(
                    parameterName + " must be between " + minimum + " and " + maximum + ".");
            }

            return value;
        }

        private static string NormalizeOptionalCode(
            string value,
            string parameterName,
            int maximumLength)
        {
            var normalized = TextNormalizer.Optional(value, parameterName, maximumLength);
            return normalized == null
                ? null
                : NormalizeCode(normalized, parameterName, maximumLength);
        }

        private static string NormalizeCode(
            string value,
            string parameterName,
            int maximumLength)
        {
            var normalized = TextNormalizer.Required(
                value,
                parameterName,
                maximumLength).ToUpperInvariant();

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
                    parameterName + " may contain only letters, digits, hyphens, and periods.");
            }

            return normalized;
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

    public sealed class Enrollment : EntityBase
    {
        private readonly List<EnrollmentSubjectLine> _subjectLines;

        private Enrollment()
        {
            StudentId = string.Empty;
            AcademicPeriodId = string.Empty;
            CourseIdSnapshot = string.Empty;
            CourseCodeSnapshot = string.Empty;
            CourseNameSnapshot = string.Empty;
            CurriculumIdSnapshot = string.Empty;
            CurriculumVersionSnapshot = string.Empty;
            _subjectLines = new List<EnrollmentSubjectLine>();
        }

        public Enrollment(
            string id,
            string studentId,
            string academicPeriodId,
            string courseIdSnapshot,
            string courseCodeSnapshot,
            string courseNameSnapshot,
            string curriculumIdSnapshot,
            string curriculumVersionSnapshot,
            DateTime createdAtUtc,
            string createdByUserId)
            : base(RequireIdentifier(id, "ENR", "Enrollment"), createdAtUtc, createdByUserId)
        {
            StudentId = RequireIdentifier(studentId, "STU", "Student");
            AcademicPeriodId = RequireIdentifier(
                academicPeriodId,
                "APD",
                "Academic Period");
            CourseIdSnapshot = RequireIdentifier(
                courseIdSnapshot,
                "CRS",
                "Course");
            CourseCodeSnapshot = NormalizeCode(
                courseCodeSnapshot,
                nameof(courseCodeSnapshot),
                30);
            CourseNameSnapshot = TextNormalizer.Required(
                courseNameSnapshot,
                nameof(courseNameSnapshot),
                200);
            CurriculumIdSnapshot = RequireIdentifier(
                curriculumIdSnapshot,
                "CUR",
                "Curriculum");
            CurriculumVersionSnapshot = NormalizeCode(
                curriculumVersionSnapshot,
                nameof(curriculumVersionSnapshot),
                30);
            Status = EnrollmentStatus.Draft;
            _subjectLines = new List<EnrollmentSubjectLine>();
        }

        public string StudentId { get; private set; }

        public string AcademicPeriodId { get; private set; }

        public string CourseIdSnapshot { get; private set; }

        public string CourseCodeSnapshot { get; private set; }

        public string CourseNameSnapshot { get; private set; }

        public string CurriculumIdSnapshot { get; private set; }

        public string CurriculumVersionSnapshot { get; private set; }

        public EnrollmentStatus Status { get; private set; }

        public DateTime? SubmittedAtUtc { get; private set; }

        public string SubmittedByUserId { get; private set; }

        public DateTime? ReviewStartedAtUtc { get; private set; }

        public string ReviewStartedByUserId { get; private set; }

        public DateTime? DecisionAtUtc { get; private set; }

        public string DecisionByUserId { get; private set; }

        public string DecisionReason { get; private set; }

        public IReadOnlyList<EnrollmentSubjectLine> SubjectLines
        {
            get { return _subjectLines.AsReadOnly(); }
        }

        public decimal TotalUnits
        {
            get
            {
                var total = 0m;
                foreach (var line in _subjectLines)
                {
                    total = AcademicUnitRules.Sum(total, line.UnitsSnapshot);
                }

                return total;
            }
        }

        public void AddSubjectLine(
            EnrollmentSubjectLine subjectLine,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureSubjectLinesEditable();
            if (subjectLine == null)
            {
                throw new DomainValidationException("Enrollment Subject line is required.");
            }

            if (FindSubjectLineIndex(subjectLine.SubjectId) >= 0)
            {
                throw new DomainValidationException(
                    "A Subject can appear only once in an Enrollment.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            _subjectLines.Add(subjectLine);
        }

        public void RemoveSubjectLine(
            string subjectId,
            DateTime changedAtUtc,
            string changedByUserId)
        {
            EnsureSubjectLinesEditable();
            var normalizedSubjectId = RequireIdentifier(subjectId, "SUB", "Subject");
            var index = FindSubjectLineIndex(normalizedSubjectId);
            if (index < 0)
            {
                throw new DomainValidationException(
                    "The Subject is not part of this Enrollment.");
            }

            RecordChange(changedAtUtc, changedByUserId);
            _subjectLines.RemoveAt(index);
        }

        public void Submit(DateTime submittedAtUtc, string submittedByUserId)
        {
            if (Status != EnrollmentStatus.Draft
                && Status != EnrollmentStatus.ReturnedForCorrection)
            {
                throw new DomainValidationException(
                    "Only a Draft or Returned Enrollment can be submitted.");
            }

            EnsureHasSubjectLines();
            var actorId = DomainGuard.RequiredActorIdentifier(
                submittedByUserId,
                nameof(submittedByUserId));

            RecordChange(submittedAtUtc, actorId);
            Status = EnrollmentStatus.Submitted;
            SubmittedAtUtc = submittedAtUtc;
            SubmittedByUserId = actorId;
            ReviewStartedAtUtc = null;
            ReviewStartedByUserId = null;
            ClearDecision();
        }

        public void BeginReview(DateTime reviewedAtUtc, string reviewedByUserId)
        {
            if (Status != EnrollmentStatus.Submitted)
            {
                throw new DomainValidationException(
                    "Only a Submitted Enrollment can enter review.");
            }

            var actorId = DomainGuard.RequiredActorIdentifier(
                reviewedByUserId,
                nameof(reviewedByUserId));
            RecordChange(reviewedAtUtc, actorId);
            Status = EnrollmentStatus.UnderReview;
            ReviewStartedAtUtc = reviewedAtUtc;
            ReviewStartedByUserId = actorId;
        }

        public void ReturnForCorrection(
            string reason,
            DateTime decidedAtUtc,
            string decidedByUserId)
        {
            EnsureUnderReview();
            SetDecision(
                EnrollmentStatus.ReturnedForCorrection,
                TextNormalizer.Required(reason, nameof(reason), 500),
                decidedAtUtc,
                decidedByUserId);
        }

        public void Approve(
            string reason,
            DateTime decidedAtUtc,
            string decidedByUserId)
        {
            EnsureUnderReview();
            EnsureHasSubjectLines();
            SetDecision(
                EnrollmentStatus.Approved,
                TextNormalizer.Optional(reason, nameof(reason), 500),
                decidedAtUtc,
                decidedByUserId);
        }

        public void Reject(
            string reason,
            DateTime decidedAtUtc,
            string decidedByUserId)
        {
            EnsureUnderReview();
            SetDecision(
                EnrollmentStatus.Rejected,
                TextNormalizer.Required(reason, nameof(reason), 500),
                decidedAtUtc,
                decidedByUserId);
        }

        public void Withdraw(
            string reason,
            DateTime decidedAtUtc,
            string decidedByUserId)
        {
            if (Status != EnrollmentStatus.Draft
                && Status != EnrollmentStatus.Submitted
                && Status != EnrollmentStatus.UnderReview
                && Status != EnrollmentStatus.ReturnedForCorrection)
            {
                throw new DomainValidationException(
                    "The Enrollment cannot be withdrawn from its current status.");
            }

            SetDecision(
                EnrollmentStatus.Withdrawn,
                TextNormalizer.Required(reason, nameof(reason), 500),
                decidedAtUtc,
                decidedByUserId);
        }

        public void Cancel(
            string reason,
            DateTime decidedAtUtc,
            string decidedByUserId)
        {
            if (Status != EnrollmentStatus.Approved)
            {
                throw new DomainValidationException(
                    "Only an Approved Enrollment can be cancelled.");
            }

            SetDecision(
                EnrollmentStatus.Cancelled,
                TextNormalizer.Required(reason, nameof(reason), 500),
                decidedAtUtc,
                decidedByUserId);
        }

        public void Complete(
            DateTime decidedAtUtc,
            string decidedByUserId)
        {
            if (Status != EnrollmentStatus.Approved)
            {
                throw new DomainValidationException(
                    "Only an Approved Enrollment can be completed.");
            }

            SetDecision(
                EnrollmentStatus.Completed,
                null,
                decidedAtUtc,
                decidedByUserId);
        }

        private void EnsureSubjectLinesEditable()
        {
            if (Status != EnrollmentStatus.Draft
                && Status != EnrollmentStatus.ReturnedForCorrection)
            {
                throw new DomainValidationException(
                    "Enrollment Subject lines can be changed only while Draft or Returned for Correction.");
            }
        }

        private void EnsureHasSubjectLines()
        {
            if (_subjectLines.Count == 0)
            {
                throw new DomainValidationException(
                    "Enrollment must contain at least one Subject line.");
            }
        }

        private void EnsureUnderReview()
        {
            if (Status != EnrollmentStatus.UnderReview)
            {
                throw new DomainValidationException(
                    "The Enrollment must be Under Review before a decision is recorded.");
            }
        }

        private void SetDecision(
            EnrollmentStatus targetStatus,
            string reason,
            DateTime decidedAtUtc,
            string decidedByUserId)
        {
            var actorId = DomainGuard.RequiredActorIdentifier(
                decidedByUserId,
                nameof(decidedByUserId));
            RecordChange(decidedAtUtc, actorId);
            Status = targetStatus;
            DecisionAtUtc = decidedAtUtc;
            DecisionByUserId = actorId;
            DecisionReason = reason;
        }

        private void ClearDecision()
        {
            DecisionAtUtc = null;
            DecisionByUserId = null;
            DecisionReason = null;
        }

        private int FindSubjectLineIndex(string subjectId)
        {
            for (var index = 0; index < _subjectLines.Count; index++)
            {
                if (StringComparer.Ordinal.Equals(_subjectLines[index].SubjectId, subjectId))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string NormalizeCode(
            string value,
            string parameterName,
            int maximumLength)
        {
            var normalized = TextNormalizer.Required(
                value,
                parameterName,
                maximumLength).ToUpperInvariant();

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
                    parameterName + " may contain only letters, digits, hyphens, and periods.");
            }

            return normalized;
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
