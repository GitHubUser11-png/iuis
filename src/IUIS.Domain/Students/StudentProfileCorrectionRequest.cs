using System;
using IUIS.Domain.Common;

namespace IUIS.Domain.Students
{
    public sealed class StudentProfileCorrectionRequest : EntityBase
    {
        public StudentProfileCorrectionRequest()
        {
            StudentId = string.Empty;
            RequestedField = string.Empty;
            CurrentValue = string.Empty;
            RequestedValue = string.Empty;
            Reason = string.Empty;
            ReviewedByEmployeeId = string.Empty;
            InternalRemarks = string.Empty;
            
            Status = CorrectionStatus.Pending;
        }

        public string StudentId { get; set; }

        public string RequestedField { get; set; }

        public string CurrentValue { get; set; }

        public string RequestedValue { get; set; }

        public string Reason { get; set; }

        public CorrectionStatus Status { get; set; }

        public DateTime RequestDateUtc { get; set; }

        public DateTime? ReviewedDateUtc { get; set; }

        public string ReviewedByEmployeeId { get; set; }

        public string InternalRemarks { get; set; }

        public string RejectionReason { get; set; }
    }

    public enum CorrectionStatus
    {
        Pending,
        UnderReview,
        Approved,
        Rejected,
        Completed,
        Cancelled
    }
}
