# Guide: Adding Controls Programmatically to Windows Forms

Since the Toolbox is not available, all controls must be added programmatically in the form's constructor or InitializeComponent method.

## Existing Control Patterns in IUIS

The project uses custom controls from `IUIS.SharedUI.Controls` for consistent styling:

- **LabeledFieldPanel** - Text input with label
- **PasswordFieldPanel** - Password input with label  
- **StatusBannerPanel** - Error/success message display
- **LoadingOverlayPanel** - Loading indicator overlay
- **FormCardPanel** - Card-style container
- **ApplicationIdentityPanel** - Branding panel
- **RoundedButton** - Styled button (via UiTheme.CreatePrimaryButton/CreateSecondaryButton)

## Common Control Addition Patterns

### 1. Adding a Text Input Field

```csharp
private readonly LabeledFieldPanel _myTextField;

public MyForm()
{
    InitializeComponent();
    
    _myTextField = new LabeledFieldPanel("Field Label", isRequired: true)
    {
        Dock = DockStyle.Fill,
        Margin = Padding.Empty
    };
    
    _myTextField.InputControl.AccessibleName = "Field Label";
    _myTextField.InputControl.TabIndex = 0;
    
    // Add to layout
    layout.Controls.Add(_myTextField, 0, 1);
}
```

### 2. Adding a Password Field

```csharp
private readonly PasswordFieldPanel _passwordField;

public MyForm()
{
    InitializeComponent();
    
    _passwordField = new PasswordFieldPanel("Password", isRequired: true)
    {
        Dock = DockStyle.Fill,
        Margin = Padding.Empty
    };
    
    _passwordField.InputControl.AccessibleName = "Password";
    _passwordField.InputControl.TabIndex = 1;
    
    layout.Controls.Add(_passwordField, 0, 2);
}
```

### 3. Adding a Button

```csharp
private readonly Button _myButton;

public MyForm()
{
    InitializeComponent();
    
    // Primary button
    _myButton = UiTheme.CreatePrimaryButton("Submit", 120, UiMetrics.StandardButtonHeight);
    _myButton.Dock = DockStyle.Left;
    _myButton.TabIndex = 3;
    _myButton.Click += MyButtonClick;
    
    // Secondary button
    var cancelButton = UiTheme.CreateSecondaryButton("Cancel", 120, UiMetrics.StandardButtonHeight);
    cancelButton.Dock = DockStyle.Left;
    cancelButton.TabIndex = 4;
    cancelButton.Click += CancelClick;
    
    layout.Controls.Add(_myButton, 0, 5);
}

private void MyButtonClick(object sender, EventArgs e)
{
    // Handle button click
}
```

### 4. Adding a Label

```csharp
public MyForm()
{
    InitializeComponent();
    
    var title = new Label
    {
        Text = "Form Title",
        Font = UiTheme.PageTitleFont,
        AutoSize = true,
        Location = new Point(32, 24)
    };
    
    var subtitle = new Label
    {
        Text = "Form description text",
        Font = UiTheme.BodyFont,
        ForeColor = UiTheme.TextSecondary,
        AutoSize = false,
        Location = new Point(32, 56),
        Size = new Size(456, 32)
    };
    
    this.Controls.Add(title);
    this.Controls.Add(subtitle);
}
```

### 5. Adding a ComboBox

```csharp
private readonly ComboBox _myComboBox;

public MyForm()
{
    InitializeComponent();
    
    _myComboBox = new ComboBox
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font = UiTheme.BodyFont,
        Width = 200,
        Location = new Point(100, 50)
    };
    
    _myComboBox.Items.AddRange(new object[] { "Option 1", "Option 2", "Option 3" });
    _myComboBox.SelectedIndex = 0;
    
    this.Controls.Add(_myComboBox);
}
```

### 6. Adding a CheckBox

```csharp
private readonly CheckBox _myCheckBox;

public MyForm()
{
    InitializeComponent();
    
    _myCheckBox = new CheckBox
    {
        Text = "Remember me",
        Font = UiTheme.BodyFont,
        ForeColor = UiTheme.TextPrimary,
        AutoSize = true,
        Location = new Point(100, 100)
    };
    
    this.Controls.Add(_myCheckBox);
}
```

### 7. Adding a Status Banner

```csharp
private readonly StatusBannerPanel _banner;

public MyForm()
{
    InitializeComponent();
    
    _banner = new StatusBannerPanel
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 4, 0, 8)
    };
    
    layout.Controls.Add(_banner, 0, 0);
}

// Show error
_banner.ShowError("Error message here");

// Show success
_banner.ShowSuccess("Success message here");

// Clear
_banner.Clear();
```

### 8. Using TableLayoutPanel for Form Layout

```csharp
public MyForm()
{
    InitializeComponent();
    
    var layout = new TableLayoutPanel
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 5,
        Padding = new Padding(40, 30, 40, 30)
    };
    
    // Set row heights
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));  // Title
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));  // Subtitle
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));  // Field 1
    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));  // Field 2
    layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // Buttons
    
    // Add controls to specific cells
    layout.Controls.Add(titleLabel, 0, 0);
    layout.Controls.Add(subtitleLabel, 0, 1);
    layout.Controls.Add(field1, 0, 2);
    layout.Controls.Add(field2, 0, 3);
    layout.Controls.Add(buttonPanel, 0, 4);
    
    this.Controls.Add(layout);
}
```

### 9. Adding a Standard TextBox (if not using LabeledFieldPanel)

```csharp
private readonly TextBox _myTextBox;

public MyForm()
{
    InitializeComponent();
    
    _myTextBox = new TextBox
    {
        Name = "myTextBox",
        Location = new Point(150, 50),
        Size = new Size(200, 25),
        Font = UiTheme.BodyFont
    };
    
    this.Controls.Add(_myTextBox);
}
```

### 10. Adding a Label + TextBox Pair (Manual)

```csharp
public MyForm()
{
    InitializeComponent();
    
    var label = new Label
    {
        Text = "Field Name:",
        Font = UiTheme.FieldLabelFont,
        Location = new Point(50, 50),
        AutoSize = true
    };
    
    var textBox = new TextBox
    {
        Name = "fieldNameTextBox",
        Location = new Point(150, 48),
        Size = new Size(200, 25),
        Font = UiTheme.BodyFont
    };
    
    this.Controls.Add(label);
    this.Controls.Add(textBox);
}
```

## Form Initialization Template

```csharp
public class MyForm : Form
{
    private readonly LabeledFieldPanel _nameField;
    private readonly PasswordFieldPanel _passwordField;
    private readonly Button _submitButton;
    private readonly StatusBannerPanel _banner;
    
    public MyForm()
    {
        // Form properties
        Text = "My Form Title";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(560, 400);
        UiTheme.ApplyBaseFormStyle(this);
        
        // Create layout
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(40, 30, 40, 30)
        };
        
        // Add controls
        _banner = new StatusBannerPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 8)
        };
        
        _nameField = new LabeledFieldPanel("Name", true)
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        
        _passwordField = new PasswordFieldPanel("Password", true)
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        
        _submitButton = UiTheme.CreatePrimaryButton("Submit", 120, UiMetrics.StandardButtonHeight);
        _submitButton.Dock = DockStyle.Left;
        _submitButton.Click += SubmitClick;
        
        // Add to layout
        layout.Controls.Add(_banner, 0, 0);
        layout.Controls.Add(_nameField, 0, 1);
        layout.Controls.Add(_passwordField, 0, 2);
        layout.Controls.Add(_submitButton, 0, 3);
        
        this.Controls.Add(layout);
    }
    
    private void SubmitClick(object sender, EventArgs e)
    {
        // Handle submission
    }
}
```

## Available UiTheme Colors

- `UiTheme.InstitutionalPrimary` - Primary blue
- `UiTheme.Surface` - Light gray background
- `UiTheme.ElevatedSurface` - White
- `UiTheme.TextPrimary` - Dark text
- `UiTheme.TextSecondary` - Medium gray text
- `UiTheme.Success` - Green
- `UiTheme.Error` - Red
- `UiTheme.Warning` - Orange
- `UiTheme.Information` - Blue
- `UiTheme.BorderNeutral` - Border color

## Available UiTheme Fonts

- `UiTheme.BodyFont` - Standard text (Segoe UI, 10f)
- `UiTheme.CaptionFont` - Small text (Segoe UI, 8.5f)
- `UiTheme.FieldLabelFont` - Bold labels (Segoe UI, 9f, Bold)
- `UiTheme.ButtonFont` - Button text (Segoe UI, 9.5f, Bold)
- `UiTheme.SectionHeadingFont` - Section headers (Segoe UI, 11f, Bold)
- `UiTheme.PageTitleFont` - Page titles (Segoe UI, 20f, Bold)

## UiMetrics Constants

- `UiMetrics.StandardButtonHeight` - Standard button height
- `UiMetrics.MinimumWindowWidth` - Minimum window width
- `UiMetrics.MinimumWindowHeight` - Minimum window height

## Tips

1. **Always call `UiTheme.ApplyBaseFormStyle(this)`** in constructor for consistent form styling
2. **Use custom controls from IUIS.SharedUI** for consistent UI
3. **Set TabIndex** for keyboard navigation order
4. **Set AccessibleName** for accessibility
5. **Use TableLayoutPanel** for responsive form layouts
6. **Set Dock properties** appropriately for layout containers
7. **Use StatusBannerPanel** for error/success messages instead of MessageBox
