using System.Collections.Generic;

namespace IUIS.Application.StudentSelfService.Profile
{
    public static class StudentProfileCorrectionCatalog
    {
        private static readonly Dictionary<string, CorrectionFieldDefinition> 
            _fields = new Dictionary<string, CorrectionFieldDefinition>
        {
            {
                "ContactNumber",
                new CorrectionFieldDefinition
                {
                    FieldName = "ContactNumber",
                    DisplayName = "Contact Number",
                    RequiresApproval = true
                }
            },
            {
                "EmailAddress",
                new CorrectionFieldDefinition
                {
                    FieldName = "EmailAddress",
                    DisplayName = "Email Address",
                    RequiresApproval = true
                }
            },
            {
                "Address",
                new CorrectionFieldDefinition
                {
                    FieldName = "Address",
                    DisplayName = "Address",
                    RequiresApproval = true
                }
            }
        };

        public static IReadOnlyDictionary<string, CorrectionFieldDefinition> 
            GetCorrectableFields()
        {
            return _fields;
        }

        public static bool IsFieldCorrectable(string fieldName)
        {
            return _fields.ContainsKey(fieldName);
        }
    }

    public sealed class CorrectionFieldDefinition
    {
        public string FieldName { get; set; }
        
        public string DisplayName { get; set; }
        
        public bool RequiresApproval { get; set; }
    }
}
