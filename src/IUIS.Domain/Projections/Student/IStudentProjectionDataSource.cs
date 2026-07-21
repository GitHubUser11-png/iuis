using System;
using System.Collections.Generic;
using IUIS.Domain.Academic;
using IUIS.Domain.Finance;
using IUIS.Domain.Library;
using IUIS.Domain.People;

namespace IUIS.Domain.Projections.Student
{
    public interface IStudentProjectionDataSource
    {
        StudentProjectionSnapshot ReadStudentSources(
            string studentId);
    }

    public sealed class StudentProjectionSnapshot
    {
        public StudentProjectionSnapshot()
        {
            Students = new List<StudentRecord>();
            Courses = new List<Course>();
            Subjects = new List<Subject>();
            Enrollments = new List<Enrollment>();
            TuitionAssessments = new List<TuitionAssessment>();
            Payments = new List<Payment>();
            Scholarships = new List<ScholarshipAward>();
            Books = new List<LibraryBook>();
            Borrowings = new List<LibraryBorrowing>();
            
            SourceRevisions = new Dictionary<string, long>();
        }

        public DateTime CapturedAtUtc { get; set; }

        public IReadOnlyList<StudentRecord> Students { get; set; }

        public IReadOnlyList<Course> Courses { get; set; }

        public IReadOnlyList<Subject> Subjects { get; set; }

        public IReadOnlyList<Enrollment> Enrollments { get; set; }

        public IReadOnlyList<TuitionAssessment> TuitionAssessments { get; set; }

        public IReadOnlyList<Payment> Payments { get; set; }

        public IReadOnlyList<ScholarshipAward> Scholarships { get; set; }

        public IReadOnlyList<LibraryBook> Books { get; set; }

        public IReadOnlyList<LibraryBorrowing> Borrowings { get; set; }

        public IReadOnlyDictionary<string, long> SourceRevisions { get; set; }
    }
}
