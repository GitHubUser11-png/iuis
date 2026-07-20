param()

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$validationRoot = Join-Path $repositoryRoot 'artifacts\validation'
New-Item -ItemType Directory -Force -Path $validationRoot | Out-Null

$errors = @()
$projectFiles = @(
    'src\IUIS.Domain\IUIS.Domain.csproj',
    'src\IUIS.Application\IUIS.Application.csproj',
    'src\IUIS.Infrastructure\IUIS.Infrastructure.csproj',
    'src\IUIS.SharedUI\IUIS.SharedUI.csproj',
    'src\IUIS.UserApp\IUIS.UserApp.csproj',
    'src\IUIS.AdminApp\IUIS.AdminApp.csproj',
    'tests\IUIS.Tests\IUIS.Tests.csproj'
)

$requiredFiles = @('IUIS.sln', 'Directory.Build.props', 'Directory.Build.targets') + $projectFiles
foreach ($relativePath in $requiredFiles) {
    $fullPath = Join-Path $repositoryRoot $relativePath
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        $errors += "Required file is missing: $relativePath"
    }
}

$propsPath = Join-Path $repositoryRoot 'Directory.Build.props'
if (Test-Path -LiteralPath $propsPath -PathType Leaf) {
    $propsContent = Get-Content -LiteralPath $propsPath -Raw
    if (-not $propsContent.Contains('<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>')) {
        $errors += 'Directory.Build.props does not lock TargetFrameworkVersion to v4.8.'
    }
    if (-not $propsContent.Contains('<LangVersion>7.3</LangVersion>')) {
        $errors += 'Directory.Build.props does not lock LangVersion to 7.3.'
    }
}

$expectedReferenceNames = @{
    'src\IUIS.Domain\IUIS.Domain.csproj' = @()
    'src\IUIS.Application\IUIS.Application.csproj' = @('IUIS.Domain')
    'src\IUIS.Infrastructure\IUIS.Infrastructure.csproj' = @('IUIS.Domain', 'IUIS.Application')
    'src\IUIS.SharedUI\IUIS.SharedUI.csproj' = @('IUIS.Domain', 'IUIS.Application')
    'src\IUIS.UserApp\IUIS.UserApp.csproj' = @('IUIS.Domain', 'IUIS.Application', 'IUIS.Infrastructure', 'IUIS.SharedUI')
    'src\IUIS.AdminApp\IUIS.AdminApp.csproj' = @('IUIS.Domain', 'IUIS.Application', 'IUIS.Infrastructure', 'IUIS.SharedUI')
    'tests\IUIS.Tests\IUIS.Tests.csproj' = @('IUIS.Domain', 'IUIS.Application', 'IUIS.Infrastructure')
}

$projectResults = @()
foreach ($relativePath in $projectFiles) {
    $fullPath = Join-Path $repositoryRoot $relativePath
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) { continue }

    $projectContent = Get-Content -LiteralPath $fullPath -Raw
    $expectedOutputType = if ($relativePath -like '*UserApp*' -or $relativePath -like '*AdminApp*') { 'WinExe' } else { 'Library' }
    if (-not $projectContent.Contains("<OutputType>$expectedOutputType</OutputType>")) {
        $errors += "$relativePath does not declare OutputType $expectedOutputType."
    }

    $actualReferences = @([regex]::Matches($projectContent, '<Name>(IUIS\.[^<]+)</Name>') |
        ForEach-Object { $_.Groups[1].Value } | Sort-Object)
    $expectedReferences = @($expectedReferenceNames[$relativePath] | Sort-Object)
    if (($actualReferences -join '|') -ne ($expectedReferences -join '|')) {
        $errors += "$relativePath references '$($actualReferences -join ', ')', expected '$($expectedReferences -join ', ')'."
    }

    $projectResults += [ordered]@{
        path = $relativePath
        outputType = $expectedOutputType
        references = $actualReferences
    }
}

$infrastructureProjectPath = Join-Path $repositoryRoot 'src\IUIS.Infrastructure\IUIS.Infrastructure.csproj'
if (Test-Path -LiteralPath $infrastructureProjectPath -PathType Leaf) {
    $infrastructureProject = Get-Content -LiteralPath $infrastructureProjectPath -Raw
    if (-not $infrastructureProject.Contains('<PackageReference Include="System.Text.Json" Version="8.0.5" />')) {
        $errors += 'IUIS.Infrastructure does not lock System.Text.Json to version 8.0.5.'
    }
}

$formFiles = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'src') -Recurse -Filter '*Form.cs' -File
foreach ($formFile in $formFiles) {
    $formContent = Get-Content -LiteralPath $formFile.FullName -Raw
    if ($formContent.Contains('System.IO') -or $formContent.Contains('System.Text.Json')) {
        $errors += "Form contains prohibited file or JSON dependency: $($formFile.FullName)"
    }
}

$expectedRepositoryNames = @(
    'students','courses','subjects','enrollments','payments','books','borrowings','counseling',
    'violations','medical_records','employees','attendance','clearances','users',
    'academic_periods','assessments','assessment_charge_rules','scholarship_programs',
    'scholarship_applications','scholarship_awards','appointments','consultations','subject_assignments',
    'notifications','account_applications','permission_profiles','login_attempts','sessions','security_policy',
    'password_assistance_requests','admin_access_rules','administrative_approvals','discipline_incidents',
    'violation_responses','work_schedules','attendance_corrections','employee_profile_corrections',
    'student_profile_corrections','payment_void_requests','financial_adjustments','audit_logs','id_sequences',
    'transaction_journal','repository_manifest','system_settings','backup_catalog',
    'repository_health_history','operational_report_runs','restore_history'
)

$templateRoot = Join-Path $repositoryRoot 'templates\production-data'
$templateResults = @()
$canonicalEnvelopeFields = @('records','repositoryName','revision','schemaVersion','updatedAtUtc','updatedByUserId') | Sort-Object
if (-not (Test-Path -LiteralPath $templateRoot -PathType Container)) {
    $errors += 'Production template directory is missing: templates\production-data'
}
else {
    $templateFiles = @(Get-ChildItem -LiteralPath $templateRoot -Filter '*.json' -File | Sort-Object Name)
    if ($templateFiles.Count -ne 49) {
        $errors += "Production template directory contains $($templateFiles.Count) JSON files; exactly 49 are required."
    }

    $actualNames = @($templateFiles | ForEach-Object { $_.BaseName } | Sort-Object)
    $expectedNames = @($expectedRepositoryNames | Sort-Object)
    if (($actualNames -join '|') -ne ($expectedNames -join '|')) {
        $errors += 'Production template filenames do not match the locked 49-repository catalog.'
    }

    foreach ($templateFile in $templateFiles) {
        try {
            $document = Get-Content -LiteralPath $templateFile.FullName -Raw | ConvertFrom-Json
            $actualFields = @($document.PSObject.Properties.Name | Sort-Object)
            if (($actualFields -join '|') -ne ($canonicalEnvelopeFields -join '|')) {
                $errors += "$($templateFile.Name) does not contain exactly the six canonical envelope fields."
            }
            if ($document.repositoryName -ne $templateFile.BaseName) {
                $errors += "$($templateFile.Name) has repositoryName '$($document.repositoryName)' instead of '$($templateFile.BaseName)'."
            }
            if ($document.schemaVersion -ne 1) {
                $errors += "$($templateFile.Name) does not use schemaVersion 1."
            }
            if ($document.revision -ne 0) {
                $errors += "$($templateFile.Name) does not begin at revision 0."
            }
            if ($null -eq $document.updatedAtUtc) {
                $errors += "$($templateFile.Name) requires updatedAtUtc."
            }
            if ([string]::IsNullOrWhiteSpace([string]$document.updatedByUserId)) {
                $errors += "$($templateFile.Name) requires updatedByUserId."
            }
            if ($null -eq $document.records -or $document.records.GetType().Name -ne 'Object[]') {
                $errors += "$($templateFile.Name) must contain a records JSON array."
            }
            $templateResults += [ordered]@{
                file = $templateFile.Name
                repositoryName = $document.repositoryName
                schemaVersion = $document.schemaVersion
                revision = $document.revision
            }
        }
        catch {
            $errors += "$($templateFile.Name) is not valid production-template JSON: $($_.Exception.Message)"
        }
    }
}

$report = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    expectedProjectCount = 7
    validatedProjectCount = $projectResults.Count
    expectedProductionRepositoryCount = 49
    validatedProductionTemplateCount = $templateResults.Count
    canonicalEnvelopeFields = $canonicalEnvelopeFields
    projects = $projectResults
    productionTemplates = $templateResults
    errors = $errors
    succeeded = ($errors.Count -eq 0)
}

$reportPath = Join-Path $validationRoot 'source-tree-validation.json'
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8

if ($errors.Count -gt 0) {
    foreach ($validationError in $errors) { Write-Host "VALIDATION ERROR: $validationError" }
    throw "Source-tree validation failed with $($errors.Count) error(s)."
}

Write-Host 'Source-tree, project-reference, canonical-envelope, and 49-template validation succeeded.'
