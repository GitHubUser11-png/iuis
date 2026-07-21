using System;
using System.Collections.Generic;

namespace IUIS.Application.StudentSelfService.Scholarship
{
    public sealed class ScholarshipProgramView
    {
        public ScholarshipProgramView()
        {
            ProgramId = string.Empty;
            ScholarshipName = string.Empty;
            Provider = string.Empty;
            
            Description = string.Empty;
            CoverageType = string.Empty;
            
            CoverageAmount = 0m;
            CoveragePercentage = 0;
            
            Requirements = new List<string>();
            
            ApplicationDeadline = DateTime.MinValue;
            
            IsActive = false;
        }

        public string ProgramId { get; set; }
        
        public string ScholarshipName { get; set; }
        
        public string Provider { get; set; }
        
        public string Description { get; set; }
        
        public string CoverageType { get; set; }
        
        public decimal CoverageAmount { get; set; }
        
        public int CoveragePercentage { get; set; }
        
        public IReadOnlyList<string> Requirements { get; set; }
        
        public DateTime ApplicationDeadline { get; set; }
        
        public bool IsActive { get; set; }
    }

    public sealed class StudentScholarshipApplicationView
    {
        public StudentScholarshipApplicationView()
        {
            ApplicationId = string.Empty;
            ScholarshipName = string.Empty;
            Provider = string.Empty;
            
            ApplicationDate = DateTime.MinValue;
            
            Status = string.Empty;
            
            CoverageAmount = 0m;
            CoveragePercentage = 0;
            
            AcademicYear = string.Empty;
            Semester = string.Empty;
            
            Remarks = string.Empty;
        }

        public string ApplicationId { get; set; }
        
        public string ScholarshipName { get; set; }
        
        public string Provider { get; set; }
        
        public DateTime ApplicationDate { get; set; }
        
        public string Status { get; set; }
        
        public decimal CoverageAmount { get; set; }
        
        public int CoveragePercentage { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public string Remarks { get; set; }
    }

    public sealed class StudentScholarshipApplicationRequest
    {
        public string ScholarshipProgramId { get; set; }
        
        public string AcademicYear { get; set; }
        
        public string Semester { get; set; }
        
        public string StatementOfPurpose { get; set; }
        
        public Dictionary<string, string> AdditionalInformation { get; set; }
    }
}
