using System;
using System.Collections.Generic;
using IUIS.Domain.Academic;
using IUIS.Domain.Finance;
using IUIS.Domain.Library;
using IUIS.Domain.Students;

namespace IUIS.Infrastructure.Projections.Student
{
    public sealed class StudentProjectionSnapshot
    {
        public StudentProjectionSnapshot()
        {
            Students = new List<Student>();
            Courses = new List<Course>();
            Subjects = new List<Subject>();
            Enrollments = new List<Enrollment>();
            TuitionAssessments = new List<TuitionAssessment>();
            Payments = new List<Payment>();
            Scholarships = new List<ScholarshipAward>();
            Books = new List<Book>();
            Borrowings = new List<Borrowing>();
            Notifications = new List<Notification>();
            
            SourceRevisions = new Dictionary<string, long>();
        }

        public DateTime CapturedAtUtc { get; set; }

        public IReadOnlyList<Student> Students { get; set; }

        public IReadOnlyList<Course> Courses { get; set; }

        public IReadOnlyList<Subject> Subjects { get; set; }

        public IReadOnlyList<Enrollment> Enrollments { get; set; }

        public IReadOnlyList<TuitionAssessment> TuitionAssessments { get; set; }

        public IReadOnlyList<Payment> Payments { get; set; }

        public IReadOnlyList<ScholarshipAward> Scholarships { get; set; }

        public IReadOnlyList<Book> Books { get; set; }

        public IReadOnlyList<Borrowing> Borrowings { get; set; }

        public IReadOnlyList<Notification> Notifications { get; set; }

        public IReadOnlyDictionary<string, long> SourceRevisions { get; set; }
    }
}
