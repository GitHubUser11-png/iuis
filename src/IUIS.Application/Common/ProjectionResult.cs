using System.Collections.Generic;

namespace IUIS.Application.Common
{
    public sealed class ProjectionResult<T>
    {
        public ProjectionResult()
        {
            Errors = new List<string>();
        }

        public bool IsSuccess { get; set; }
        
        public T Data { get; set; }
        
        public IReadOnlyList<string> Errors { get; set; }
        
        public string SourceRevision { get; set; }
        
        public DateTime CapturedAtUtc { get; set; }

        public static ProjectionResult<T> Success(T data, string sourceRevision)
        {
            return new ProjectionResult<T>
            {
                IsSuccess = true,
                Data = data,
                SourceRevision = sourceRevision,
                CapturedAtUtc = System.DateTime.UtcNow
            };
        }

        public static ProjectionResult<T> Failure(params string[] errors)
        {
            return new ProjectionResult<T>
            {
                IsSuccess = false,
                Errors = errors
            };
        }
    }
}
