private readonly IStudentDashboardService _service;

public StudentDashboardPage(IStudentDashboardService service)
{
    _service = service ?? throw new ArgumentNullException(nameof(service));
    InitializeComponent();
    // Now you can call _service.GetDashboardViewAsync() in Load event
}