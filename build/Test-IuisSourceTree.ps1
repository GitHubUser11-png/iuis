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
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        continue
    }

    $projectContent = Get-Content -LiteralPath $fullPath -Raw
    $expectedOutputType = if ($relativePath -like '*UserApp*' -or $relativePath -like '*AdminApp*') { 'WinExe' } else { 'Library' }
    if (-not $projectContent.Contains("<OutputType>$expectedOutputType</OutputType>")) {
        $errors += "$relativePath does not declare OutputType $expectedOutputType."
    }

    $actualReferences = @([regex]::Matches($projectContent, '<Name>(IUIS\.[^<]+)</Name>') | ForEach-Object { $_.Groups[1].Value } | Sort-Object)
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

$formFiles = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'src') -Recurse -Filter '*Form.cs' -File
foreach ($formFile in $formFiles) {
    $formContent = Get-Content -LiteralPath $formFile.FullName -Raw
    if ($formContent.Contains('System.IO') -or $formContent.Contains('System.Text.Json')) {
        $errors += "Form contains prohibited file or JSON dependency: $($formFile.FullName)"
    }
}

$report = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    expectedProjectCount = 7
    validatedProjectCount = $projectResults.Count
    projects = $projectResults
    errors = $errors
    succeeded = ($errors.Count -eq 0)
}

$reportPath = Join-Path $validationRoot 'source-tree-validation.json'
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8

if ($errors.Count -gt 0) {
    foreach ($validationError in $errors) {
        Write-Host "VALIDATION ERROR: $validationError"
    }
    throw "Source-tree validation failed with $($errors.Count) error(s)."
}

Write-Host 'Source-tree and project-reference validation succeeded.'
