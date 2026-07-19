using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IUIS.Tests
{
    [TestClass]
    public sealed class SolutionFoundationTests
    {
        [TestMethod]
        public void DomainProjectMarkerUsesCanonicalName()
        {
            Assert.AreEqual(
                "IUIS.Domain",
                Domain.SolutionFoundation.ProjectName);
        }

        [TestMethod]
        public void ApplicationProjectMarkerUsesCanonicalName()
        {
            Assert.AreEqual(
                "IUIS.Application",
                Application.SolutionFoundation.ProjectName);
        }

        [TestMethod]
        public void InfrastructureProjectMarkerUsesCanonicalName()
        {
            Assert.AreEqual(
                "IUIS.Infrastructure",
                Infrastructure.SolutionFoundation.ProjectName);
        }
    }
}
