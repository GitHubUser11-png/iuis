using System;
using System.Collections.Generic;

namespace IUIS.Application.StudentSelfService.Enrollment
{
    public sealed class StudentEnrollmentSummaryView
    {
        public StudentEnrollmentSummaryView()
        {
            CurrentEnrollmentId = string.Empty;
            ProgramName = string.Empty;
            YearLevel = 0;
            Section = string.Empty;
            
            AcademicYear = string.Empty;
            Semester = string.Empty;
            
            EnrollmentStatus = string.Empty;
            EnrollmentDate = DateTime.MinValue;
            
            TotalUnits = 0;
            SubjectCount = 0;
            
            OutstandingBalance = 0m;
        }

        public string CurrentEnrollmentId { get; set; }
        
        public string ProgramName { get; set; }
        
        public int YearLevel { get; set; }
        
        public string Section { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public string EnrollmentStatus { get; set; }
        
        public DateTime EnrollmentDate { get; set; }
        
        public int TotalUnits { get; set; }
        
        public int SubjectCount { get; set; }
        
        public decimal OutstandingBalance { get; set; }
    }

    public sealed class StudentEnrollmentDetailsView
    {
        public StudentEnrollmentDetailsView()
        {
            EnrollmentId = string.Empty;
            ProgramName = string.Empty;
            YearLevel = 0;
            Section = string.Empty;
            
            AcademicYear = string.Empty;
            Semester = string.Empty;
            
            EnrollmentStatus = string.Empty;
            EnrollmentDate = DateTime.MinValue;
            
            Subjects = new List<EnrolledSubjectView>();
        }

        public string EnrollmentId { get; set; }
        
        public string ProgramName { get; set; }
        
        public int YearLevel { get; set; }
        
        public string Section { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public string EnrollmentStatus { get; set; }
        
        public DateTime EnrollmentDate { get; set; }
        
        public IReadOnlyList<EnrolledSubjectView> Subjects { get; set; }
    }

    public sealed class EnrolledSubjectView
    {
        public string SubjectId { get; set; }
        
        public string SubjectCode { get; set; }
        
        public string SubjectTitle { get; set; }
        
        public int Units { get; set; }
        
        public string Schedule { get; set; }
        
        public string Room { get; set; }
        
        public string InstructorName { get; set; }
    }

    public sealed class StudentEnrollmentListItem
    {
        public string EnrollmentId { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public string ProgramName { get; set; }
        
        public int YearLevel { get; set; }
        
        public string Section { get; set; }
        
        public string Status { get; set; }
        
        public DateTime EnrollmentDate { get; set; }
    }
}
