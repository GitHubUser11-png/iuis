using System;

namespace IUIS.Application.StudentSelfService.Profile
{
    public sealed class StudentProfileView
    {
        public StudentProfileView()
        {
            StudentId = string.Empty;
            FirstName = string.Empty;
            MiddleName = string.Empty;
            LastName = string.Empty;
            Suffix = string.Empty;
            
            BirthDate = DateTime.MinValue;
            Sex = string.Empty;
            
            Address = string.Empty;
            ContactNumber = string.Empty;
            EmailAddress = string.Empty;
            
            ProgramName = string.Empty;
            YearLevel = 0;
            Section = string.Empty;
            
            AdmissionYear = 0;
            StudentStatus = string.Empty;
            
            LastUpdatedUtc = DateTime.UtcNow;
        }

        public string StudentId { get; set; }
        
        public string FirstName { get; set; }
        
        public string MiddleName { get; set; }
        
        public string LastName { get; set; }
        
        public string Suffix { get; set; }
        
        public DateTime BirthDate { get; set; }
        
        public string Sex { get; set; }
        
        public string Address { get; set; }
        
        public string ContactNumber { get; set; }
        
        public string EmailAddress { get; set; }
        
        public string ProgramName { get; set; }
        
        public int YearLevel { get; set; }
        
        public string Section { get; set; }
        
        public int AdmissionYear { get; set; }
        
        public string StudentStatus { get; set; }
        
        public DateTime LastUpdatedUtc { get; set; }
    }
}
