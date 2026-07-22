# DataGridView Implementation Examples

## Quick Start Guide

This document provides practical examples of how to use the `AppDataGridViewFactory` to create consistent, hardcoded DataGridView controls throughout the IUIS application.

---

## Example 1: Creating a Basic DataGridView

### Minimal Implementation

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using IUIS.SharedUI.DataGridViews;

// In your Form or UserControl class:

private DataGridView _booksGrid;

private void SetupBooksGrid()
{
    // Create the styled grid
    _booksGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    
    // Set position and size
    _booksGrid.Location = new Point(0, 90);
    _booksGrid.Size = new Size(960, 500);
    
    // Add columns
    AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "BookId", "ID", 100);
    AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "Title", "Title", 300);
    AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "Author", "Author", 200);
    AppDataGridViewFactory.AddTextBoxColumn(_booksGrid, "ISBN", "ISBN", 150);
    
    // Add to form
    this.Controls.Add(_booksGrid);
}

// Load data
private void LoadBooks()
{
    _booksGrid.DataSource = GetBooksFromService();
}
```

**Result:** A styled DataGridView with 4 text columns, consistent appearance, and standard sizing.

---

## Example 2: Grid with Multiple Column Types

### Complex Grid with Mixed Columns

```csharp
private DataGridView _enrollmentsGrid;

private void SetupEnrollmentsGrid()
{
    _enrollmentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    _enrollmentsGrid.Location = new Point(0, 150);
    _enrollmentsGrid.Size = new Size(1200, 400);
    
    // Text columns
    AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "EnrollmentId", "ID", 100);
    AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "StudentId", "Student ID", 100);
    AppDataGridViewFactory.AddTextBoxColumn(_enrollmentsGrid, "ProgramName", "Program", 200);
    
    // Numeric column
    AppDataGridViewFactory.AddNumericColumn(_enrollmentsGrid, "TotalUnits", "Units", 80, "N0");
    
    // Currency column
    AppDataGridViewFactory.AddCurrencyColumn(_enrollmentsGrid, "TuitionFee", "Tuition", 120);
    
    // Date column
    AppDataGridViewFactory.AddDateColumn(_enrollmentsGrid, "EnrollmentDate", "Date Enrolled", "MM/dd/yyyy", 120);
    
    // Status column (special styling)
    AppDataGridViewFactory.AddStatusColumn(_enrollmentsGrid, "Status", "Status", 100);
    
    // Button columns for actions
    AppDataGridViewFactory.AddButtonColumn(_enrollmentsGrid, "View", "View", "View", 70);
    AppDataGridViewFactory.AddButtonColumn(_enrollmentsGrid, "Edit", "Edit", "Edit", 70);
    
    // Set up events
    _enrollmentsGrid.CellClick += OnEnrollmentsCellClick;
    
    this.Controls.Add(_enrollmentsGrid);
}
```

**Result:** A professional DataGridView with text, numeric, currency, date, status, and button columns all properly formatted.

---

## Example 3: Tab Control with Multiple Grids

### Multi-Tab Interface Pattern

```csharp
private TabControl _tabControl;
private DataGridView[] _grids;

private void SetupTabInterface()
{
    var mainPanel = new Panel
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(20),
        BackColor = Color.FromArgb(249, 250, 251)
    };
    
    var headerLabel = new Label
    {
        Text = "User Management",
        Font = new Font("Segoe UI", 18F, FontStyle.Bold),
        ForeColor = Color.FromArgb(17, 24, 39),
        Location = new Point(0, 0),
        AutoSize = true
    };
    
    _tabControl = new TabControl
    {
        Location = new Point(0, 50),
        Size = new Size(1360, 800),
        Font = new Font("Segoe UI", 9F)
    };
    
    var tabs = new[] { "All Users", "Students", "Employees", "Administrators" };
    _grids = new DataGridView[tabs.Length];
    
    for (int i = 0; i < tabs.Length; i++)
    {
        var tabPage = new TabPage(tabs[i]);
        var grid = CreateUsersGrid();  // See example 4
        grid.Dock = DockStyle.Fill;
        tabPage.Controls.Add(grid);
        _tabControl.TabPages.Add(tabPage);
        _grids[i] = grid;
    }
    
    _tabControl.SelectedIndexChanged += (s, e) => LoadDataForCurrentTab();
    
    mainPanel.Controls.Add(headerLabel);
    mainPanel.Controls.Add(_tabControl);
    this.Controls.Add(mainPanel);
}

private void LoadDataForCurrentTab()
{
    int selectedIndex = _tabControl.SelectedIndex;
    var data = GetDataByTab(selectedIndex);
    _grids[selectedIndex].DataSource = data;
}
```

**Result:** A tabbed interface with multiple synchronized DataGridViews.

---

## Example 4: Grid with Filter Controls

### Grid with Search and Filter

```csharp
private DataGridView _paymentsGrid;
private FilterBarControl _filterBar;

private void SetupFilteredGrid()
{
    var mainPanel = new Panel
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(20),
        BackColor = Color.FromArgb(249, 250, 251)
    };
    
    var headerLabel = new Label
    {
        Text = "Payment History",
        Font = new Font("Segoe UI", 18F, FontStyle.Bold),
        ForeColor = Color.FromArgb(17, 24, 39),
        Location = new Point(0, 0),
        AutoSize = true
    };
    
    // Filter control
    _filterBar = new FilterBarControl
    {
        Location = new Point(0, 40),
        Size = new Size(1160, 40)
    };
    _filterBar.SetFilterOptions(new[] { "All", "This Month", "This Year", "Overdue" });
    
    // Grid
    _paymentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    _paymentsGrid.Location = new Point(0, 90);
    _paymentsGrid.Size = new Size(1160, 650);
    
    AppDataGridViewFactory.AddDateColumn(_paymentsGrid, "PaymentDate", "Date", "MM/dd/yyyy", 120);
    AppDataGridViewFactory.AddCurrencyColumn(_paymentsGrid, "Amount", "Amount", 120);
    AppDataGridViewFactory.AddTextBoxColumn(_paymentsGrid, "Reference", "Reference #", 150);
    AppDataGridViewFactory.AddStatusColumn(_paymentsGrid, "Status", "Status", 100);
    AppDataGridViewFactory.AddButtonColumn(_paymentsGrid, "Details", "Details", "View", 80);
    
    // Connect events
    _filterBar.SearchRequested += (s, e) => LoadPayments();
    _filterBar.ClearRequested += (s, e) => ClearFilters();
    _paymentsGrid.CellClick += OnPaymentCellClick;
    
    mainPanel.Controls.Add(headerLabel);
    mainPanel.Controls.Add(_filterBar);
    mainPanel.Controls.Add(_paymentsGrid);
    
    this.Controls.Add(mainPanel);
}

private void LoadPayments()
{
    string filterCriteria = _filterBar.GetSelectedFilter();
    var payments = GetPaymentsByFilter(filterCriteria);
    _paymentsGrid.DataSource = payments;
}

private void OnPaymentCellClick(object sender, DataGridViewCellEventArgs e)
{
    if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
    
    if (_paymentsGrid.Columns[e.ColumnIndex].Name == "Details")
    {
        var paymentId = _paymentsGrid.Rows[e.RowIndex].Cells["PaymentId"].Value?.ToString();
        if (!string.IsNullOrEmpty(paymentId))
        {
            // Open payment details dialog
            ShowPaymentDetails(paymentId);
        }
    }
}
```

**Result:** A grid with integrated search/filter controls and cell click event handling.

---

## Example 5: Grid with Status-Based Row Coloring

### Dynamic Row Styling Based on Status

```csharp
private DataGridView _casesGrid;

private void SetupCasesGrid()
{
    _casesGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    _casesGrid.Location = new Point(0, 50);
    _casesGrid.Size = new Size(1000, 600);
    
    AppDataGridViewFactory.AddTextBoxColumn(_casesGrid, "CaseId", "Case ID", 100);
    AppDataGridViewFactory.AddTextBoxColumn(_casesGrid, "StudentName", "Student", 250);
    AppDataGridViewFactory.AddTextBoxColumn(_casesGrid, "ViolationType", "Violation", 200);
    AppDataGridViewFactory.AddDateColumn(_casesGrid, "FiledDate", "Date Filed", "MM/dd/yyyy", 120);
    AppDataGridViewFactory.AddStatusColumn(_casesGrid, "Status", "Status", 100);
    
    this.Controls.Add(_casesGrid);
}

private void LoadCases()
{
    var cases = GetDisciplineCases();
    _casesGrid.DataSource = cases;
    
    // Apply row coloring based on status
    AppDataGridViewFactory.ApplyStatusRowColoring(_casesGrid, "Status");
}

// Status color reference:
// - "Active" => Green
// - "Inactive" => Gray
// - "Pending" => Orange
// - "Suspended" => Red
```

**Result:** A grid where rows are automatically colored based on their status values.

---

## Example 6: Checkbox Column for Multi-Select

### Grid with Boolean Selection

```csharp
private DataGridView _studentsGrid;

private void SetupStudentSelectionGrid()
{
    _studentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    _studentsGrid.Location = new Point(0, 50);
    _studentsGrid.Size = new Size(900, 500);
    
    // Checkbox column for selection
    AppDataGridViewFactory.AddCheckBoxColumn(_studentsGrid, "IsSelected", "Select", 60);
    
    AppDataGridViewFactory.AddTextBoxColumn(_studentsGrid, "StudentId", "ID", 100);
    AppDataGridViewFactory.AddTextBoxColumn(_studentsGrid, "Name", "Name", 250);
    AppDataGridViewFactory.AddTextBoxColumn(_studentsGrid, "Email", "Email", 250);
    
    this.Controls.Add(_studentsGrid);
}

private void GetSelectedStudents()
{
    var selectedStudents = new List<string>();
    
    foreach (DataGridViewRow row in _studentsGrid.Rows)
    {
        bool isSelected = (bool)(row.Cells["IsSelected"].Value ?? false);
        if (isSelected)
        {
            string studentId = row.Cells["StudentId"].Value?.ToString();
            selectedStudents.Add(studentId);
        }
    }
    
    return selectedStudents;
}
```

**Result:** A grid with checkbox column for selecting multiple rows.

---

## Example 7: ComboBox Column for Editing

### Grid with Dropdown Selections

```csharp
private DataGridView _assignmentsGrid;

private void SetupAssignmentsGrid()
{
    _assignmentsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    _assignmentsGrid.Location = new Point(0, 50);
    _assignmentsGrid.Size = new Size(1000, 500);
    
    AppDataGridViewFactory.AddTextBoxColumn(_assignmentsGrid, "AssignmentId", "ID", 80);
    AppDataGridViewFactory.AddTextBoxColumn(_assignmentsGrid, "CourseCode", "Course", 100);
    AppDataGridViewFactory.AddTextBoxColumn(_assignmentsGrid, "Title", "Title", 300);
    
    // ComboBox column for instructors
    string[] instructors = { "Dr. Smith", "Prof. Johnson", "Ms. Williams", "Dr. Brown" };
    AppDataGridViewFactory.AddComboBoxColumn(_assignmentsGrid, "AssignedTo", "Assigned To", 200, instructors);
    
    this.Controls.Add(_assignmentsGrid);
}
```

**Result:** A grid with editable dropdown columns for selecting from predefined options.

---

## Example 8: Summary Panel Above Grid

### Metric Panel + Data Grid Pattern

```csharp
private Panel _summaryPanel;
private DataGridView _detailsGrid;

private void SetupPanelAndGrid()
{
    var mainPanel = new Panel
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(20),
        BackColor = Color.FromArgb(249, 250, 251)
    };
    
    var headerLabel = new Label
    {
        Text = "Financial Summary",
        Font = new Font("Segoe UI", 18F, FontStyle.Bold),
        ForeColor = Color.FromArgb(17, 24, 39),
        Location = new Point(0, 0),
        AutoSize = true
    };
    
    // Summary panel (white background with metrics)
    _summaryPanel = new Panel
    {
        Location = new Point(0, 40),
        Size = new Size(1160, 100),
        BackColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle
    };
    BuildSummaryPanel();
    
    // Details grid below
    _detailsGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    _detailsGrid.Location = new Point(0, 160);
    _detailsGrid.Size = new Size(1160, 600);
    
    AppDataGridViewFactory.AddTextBoxColumn(_detailsGrid, "TransactionId", "ID", 100);
    AppDataGridViewFactory.AddDateColumn(_detailsGrid, "Date", "Date", "MM/dd/yyyy", 120);
    AppDataGridViewFactory.AddCurrencyColumn(_detailsGrid, "Amount", "Amount", 120);
    AppDataGridViewFactory.AddTextBoxColumn(_detailsGrid, "Description", "Description", 400);
    
    mainPanel.Controls.Add(headerLabel);
    mainPanel.Controls.Add(_summaryPanel);
    mainPanel.Controls.Add(_detailsGrid);
    
    this.Controls.Add(mainPanel);
}

private void BuildSummaryPanel()
{
    _summaryPanel.Controls.Clear();
    
    // Create labels for each metric
    var metrics = new[]
    {
        ("Total Income", "₱ 150,000.00"),
        ("Total Expenses", "₱ 45,000.00"),
        ("Net Balance", "₱ 105,000.00"),
        ("Year-to-Date", "₱ 1,250,000.00")
    };
    
    int x = 20;
    int y = 15;
    
    foreach (var (label, value) in metrics)
    {
        var labelControl = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.FromArgb(107, 114, 128),
            Location = new Point(x, y),
            AutoSize = true
        };
        
        var valueControl = new Label
        {
            Text = value,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Location = new Point(x, y + 18),
            AutoSize = true
        };
        
        _summaryPanel.Controls.Add(labelControl);
        _summaryPanel.Controls.Add(valueControl);
        
        x += 270;
    }
}
```

**Result:** A layout with summary metrics above a detail grid.

---

## Example 9: Empty State Handling

### Showing Empty State Message

```csharp
private DataGridView _resultsGrid;
private Panel _emptyStatePanel;

private void LoadSearchResults(string query)
{
    var results = SearchDatabase(query);
    
    if (results == null || results.Count == 0)
    {
        // Show empty state
        _resultsGrid.Visible = false;
        _emptyStatePanel = AppDataGridViewFactory.CreateEmptyStatePanel(
            "No results found. Try a different search term.");
        _emptyStatePanel.Visible = true;
        this.Controls.Add(_emptyStatePanel);
    }
    else
    {
        // Show grid with data
        if (_emptyStatePanel != null)
        {
            this.Controls.Remove(_emptyStatePanel);
        }
        _resultsGrid.DataSource = results;
        _resultsGrid.Visible = true;
    }
}
```

**Result:** Graceful handling of empty datasets with user-friendly messages.

---

## Example 10: Grid with Sorting

### Implementing Column Header Sorting

```csharp
private DataGridView _employeesGrid;
private SortOrder _currentSortOrder = SortOrder.Ascending;
private string _lastSortedColumn = string.Empty;

private void SetupSortableGrid()
{
    _employeesGrid = AppDataGridViewFactory.CreateStyledDataGridView();
    _employeesGrid.Location = new Point(0, 50);
    _employeesGrid.Size = new Size(1000, 600);
    
    AppDataGridViewFactory.AddTextBoxColumn(_employeesGrid, "EmployeeId", "ID", 100);
    AppDataGridViewFactory.AddTextBoxColumn(_employeesGrid, "Name", "Name", 250);
    AppDataGridViewFactory.AddTextBoxColumn(_employeesGrid, "Position", "Position", 200);
    AppDataGridViewFactory.AddDateColumn(_employeesGrid, "HireDate", "Hire Date", "MM/dd/yyyy", 120);
    
    // Enable sorting
    AppDataGridViewFactory.EnableSorting(_employeesGrid, OnColumnHeaderMouseClick);
    
    this.Controls.Add(_employeesGrid);
}

private void OnColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
{
    if (e.RowIndex != -1) return;
    
    string columnName = _employeesGrid.Columns[e.ColumnIndex].DataPropertyName;
    
    // Toggle sort order
    if (columnName == _lastSortedColumn)
    {
        _currentSortOrder = _currentSortOrder == SortOrder.Ascending 
            ? SortOrder.Descending 
            : SortOrder.Ascending;
    }
    else
    {
        _currentSortOrder = SortOrder.Ascending;
        _lastSortedColumn = columnName;
    }
    
    // Reload sorted data
    LoadEmployeesSorted(columnName, _currentSortOrder);
}

private void LoadEmployeesSorted(string column, SortOrder order)
{
    var employees = GetEmployeesSorted(column, order);
    _employeesGrid.DataSource = employees;
}
```

**Result:** A grid with clickable column headers for sorting.

---

## Common Patterns Summary

### Pattern 1: Basic Grid Setup
1. Create grid using `CreateStyledDataGridView()`
2. Set location and size
3. Add columns using appropriate factory methods
4. Connect event handlers
5. Add to parent control
6. Load data

### Pattern 2: Tabbed Interface
1. Create TabControl
2. Create multiple tabs
3. Add grid to each tab
4. Handle tab selection changes
5. Load data for active tab

### Pattern 3: Grid with Actions
1. Add button columns
2. Connect CellClick event handler
3. Check clicked column name
4. Open appropriate dialog or perform action
5. Refresh grid on completion

### Pattern 4: Filtered Data
1. Create FilterBarControl
2. Connect search/clear events
3. Load data based on filter criteria
4. Apply optional status coloring

---

## Styling Reference

### Color Constants Used Throughout

```csharp
// Backgrounds
Color.FromArgb(249, 250, 251)  // Light gray page background
Color.White                     // White card/grid background
Color.FromArgb(31, 41, 55)     // Dark gray headers

// Text
Color.FromArgb(17, 24, 39)     // Dark gray/black body text
Color.FromArgb(107, 114, 128)  // Medium gray secondary text

// Buttons
Color.FromArgb(79, 70, 229)    // Indigo primary button
Color.FromArgb(229, 231, 235)  // Light gray secondary button

// Status Badges
Color.FromArgb(220, 252, 231)  // Green for Active
Color.FromArgb(243, 244, 246)  // Gray for Inactive
Color.FromArgb(255, 243, 224)  // Orange for Pending
Color.FromArgb(254, 226, 226)  // Red for Suspended
```

---

## Best Practices

1. **Always use the factory** - Don't manually create DataGridView columns
2. **Consistent sizing** - Use factory for consistent appearance
3. **Column order matters** - Add columns in logical order
4. **Test empty datasets** - Provide empty state messages
5. **Handle cell clicks properly** - Always validate row/column indices
6. **Use appropriate column types** - Currency for money, Date for dates, etc.
7. **Keep grids responsive** - Add data loading in background if needed
8. **Color code status values** - Use factory status colors
9. **Provide user feedback** - Show messages for errors and actions
10. **Document custom grids** - Comment non-standard implementations
