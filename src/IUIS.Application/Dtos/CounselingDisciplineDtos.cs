using System.Collections.Generic;

namespace IUIS.Application.Dtos
{
    public sealed class StudentCounselingDisciplineOverviewDto
    {
        public string StudentId { get; set; }
        public long CounselingRepositoryRevision { get; set; }
        public long DisciplineRepositoryRevision { get; set; }
        public IReadOnlyList<CounselingReleasedCaseDto> CounselingCases { get; set; }
        public IReadOnlyList<DisciplineReleasedCaseDto> DisciplineCases { get; set; }
    }

    public sealed class RestrictedCounselingCaseViewDto
    {
        public long RepositoryRevision { get; set; }
        public long EntityVersion { get; set; }
        public CounselingInternalCaseDto Case { get; set; }
    }

    public sealed class RestrictedDisciplineCaseViewDto
    {
        public long RepositoryRevision { get; set; }
        public long EntityVersion { get; set; }
        public DisciplineInternalCaseDto Case { get; set; }
    }

    public sealed class StudentServiceCommandResult
    {
        public string TransactionId { get; set; }
        public string RecordId { get; set; }
        public string SecondaryRecordId { get; set; }
        public long RepositoryRevision { get; set; }
        public long SecondaryRepositoryRevision { get; set; }
        public long EntityVersion { get; set; }
        public long SecondaryEntityVersion { get; set; }
    }
}
