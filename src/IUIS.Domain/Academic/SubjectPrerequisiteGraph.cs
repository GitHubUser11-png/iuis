using System;
using System.Collections.Generic;

using IUIS.Domain.Common;

namespace IUIS.Domain.Academic
{
    public static class SubjectPrerequisiteGraph
    {
        public static void ValidateAcyclic(IEnumerable<Subject> subjects)
        {
            if (subjects == null)
            {
                throw new DomainValidationException("Subject collection is required.");
            }

            var subjectById = new Dictionary<string, Subject>(StringComparer.Ordinal);
            foreach (var subject in subjects)
            {
                if (subject == null)
                {
                    throw new DomainValidationException("Subject collection cannot contain null entries.");
                }

                if (subjectById.ContainsKey(subject.Id))
                {
                    throw new DomainValidationException(
                        "The Subject graph contains duplicate ID " + subject.Id + ".");
                }

                subjectById.Add(subject.Id, subject);
            }

            foreach (var subject in subjectById.Values)
            {
                foreach (var prerequisite in subject.Prerequisites)
                {
                    if (!subjectById.ContainsKey(prerequisite.PrerequisiteSubjectId))
                    {
                        throw new DomainValidationException(
                            "Subject " + subject.Id + " references missing prerequisite "
                            + prerequisite.PrerequisiteSubjectId + ".");
                    }
                }
            }

            var visitState = new Dictionary<string, int>(StringComparer.Ordinal);
            var path = new List<string>();
            var subjectIds = new List<string>(subjectById.Keys);
            subjectIds.Sort(StringComparer.Ordinal);

            foreach (var subjectId in subjectIds)
            {
                Visit(subjectId, subjectById, visitState, path);
            }
        }

        private static void Visit(
            string subjectId,
            IDictionary<string, Subject> subjectById,
            IDictionary<string, int> visitState,
            IList<string> path)
        {
            int state;
            if (visitState.TryGetValue(subjectId, out state))
            {
                if (state == 2)
                {
                    return;
                }

                if (state == 1)
                {
                    throw CreateCycleException(subjectId, path);
                }
            }

            visitState[subjectId] = 1;
            path.Add(subjectId);

            var prerequisiteIds = new List<string>();
            foreach (var prerequisite in subjectById[subjectId].Prerequisites)
            {
                prerequisiteIds.Add(prerequisite.PrerequisiteSubjectId);
            }

            prerequisiteIds.Sort(StringComparer.Ordinal);
            foreach (var prerequisiteId in prerequisiteIds)
            {
                int prerequisiteState;
                if (visitState.TryGetValue(prerequisiteId, out prerequisiteState)
                    && prerequisiteState == 1)
                {
                    throw CreateCycleException(prerequisiteId, path);
                }

                Visit(prerequisiteId, subjectById, visitState, path);
            }

            path.RemoveAt(path.Count - 1);
            visitState[subjectId] = 2;
        }

        private static DomainValidationException CreateCycleException(
            string repeatedSubjectId,
            IList<string> path)
        {
            var cycleStart = -1;
            for (var index = 0; index < path.Count; index++)
            {
                if (StringComparer.Ordinal.Equals(path[index], repeatedSubjectId))
                {
                    cycleStart = index;
                    break;
                }
            }

            var cycle = new List<string>();
            if (cycleStart >= 0)
            {
                for (var index = cycleStart; index < path.Count; index++)
                {
                    cycle.Add(path[index]);
                }
            }

            cycle.Add(repeatedSubjectId);
            return new DomainValidationException(
                "Subject prerequisite graph contains a cycle: "
                + string.Join(" -> ", cycle) + ".");
        }
    }
}
