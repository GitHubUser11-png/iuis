using System.Collections.Generic;

namespace IUIS.Infrastructure.Projections.Student
{
    public sealed class StudentProjectionRevisionSet
    {
        public StudentProjectionRevisionSet()
        {
            Revisions = new Dictionary<string, long>();
        }

        public IReadOnlyDictionary<string, long> Revisions { get; set; }

        public void SetRevision(string repositoryName, long revision)
        {
            var mutable = new Dictionary<string, long>();
            foreach (var kvp in Revisions)
            {
                mutable[kvp.Key] = kvp.Value;
            }
            mutable[repositoryName] = revision;
            Revisions = mutable;
        }

        public long GetRevision(string repositoryName)
        {
            if (Revisions.TryGetValue(repositoryName, out long revision))
            {
                return revision;
            }
            return 0;
        }
    }
}
