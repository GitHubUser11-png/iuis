using System;

namespace IUIS.Tests
{
    public static class SolutionFoundationTests
    {
        public static void AssertProjectNamesAreAvailable()
        {
            if (string.IsNullOrWhiteSpace(IUIS.Domain.SolutionFoundation.ProjectName))
            {
                throw new InvalidOperationException("The Domain project marker is unavailable.");
            }

            if (string.IsNullOrWhiteSpace(IUIS.Application.SolutionFoundation.ProjectName))
            {
                throw new InvalidOperationException("The Application project marker is unavailable.");
            }

            if (string.IsNullOrWhiteSpace(IUIS.Infrastructure.SolutionFoundation.ProjectName))
            {
                throw new InvalidOperationException("The Infrastructure project marker is unavailable.");
            }
        }
    }
}
