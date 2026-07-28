using System;
using System.Windows.Forms;
using IUIS.UserApp.Composition;
// ... other usings ...

internal sealed class UserPageFactory
{
    private readonly IFormServiceResolver _resolver;
    private readonly string _sessionToken;

    public UserPageFactory(object compositionRoot, string sessionToken, IFormServiceResolver resolver)
    {
        _resolver = resolver;
        _sessionToken = sessionToken;
    }

    public Form CreatePage(string pageKey, string displayText)
    {
        switch (pageKey)
        {
            case "STU-DASH-01":
                var dashService = _resolver.Resolve<IStudentDashboardService>();
                return new StudentDashboardPage(dashService); // Inject service

            case "STU-PROF-01":
                var profService = _resolver.Resolve<IStudentProfileService>();
                return new StudentProfilePage(profService);

            case "LIB-INV-01":
                var libService = _resolver.Resolve<ILibraryCirculationService>();
                return new BookInventoryPage(libService);
            
            // ... add cases for all pages ...
            
            default:
                return new Panel { Text = "Page not found" };
        }
    }
}