param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$validationRoot = Join-Path $repositoryRoot 'artifacts\validation'
New-Item -ItemType Directory -Force -Path $validationRoot | Out-Null

$expectedProjects = [ordered]@{
    'IUIS.Domain' = [ordered]@{ Path = 'src\IUIS.Domain\IUIS.Domain.csproj'; References = @(); OutputType = 'Library' }
    'IUIS.Application' = [ordered]@{ Path = 'src\IUIS.Application\IUIS.Application.csproj'; References = @('IUIS.Domain'); OutputType = 'Library' }
    'IUIS.Infrastructure' = [ordered]@{ Path = 'src\IUIS.Infrastructure\IUIS.Infrastructure.csproj'; References = @('IUIS.Domain', 'IUIS.Application'); OutputType = 'Library' }
    'IUIS.SharedUI' = [ordered]@{ Path = 'src\IUIS.SharedUI\IUIS.SharedUI.csproj'; References = @('IUIS.Domain', 'IUIS.Application'); OutputType = 'Library' }
    'IUIS.UserApp' = [ordered]@{ Path = 'src\IUIS.UserApp\IUIS.UserApp.csproj'; References = @('IUIS.Domain', 'IUIS.Application', 'IUIS.Infrastructure', 'IUIS.SharedUI'); OutputType = 'WinExe' }
    'IUIS.AdminApp' = [ordered]@{ Path = 'src\IUIS.AdminApp\IUIS.AdminApp.csproj'; References = @('IUIS.Domain', 'IUIS.Application', 'IUIS.Infrastructure', 'IUIS.SharedUI'); OutputType = 'WinExe' }
    'IUIS.Tests' = [ordered]@{ Path = 'tests\IUIS.Tests\IUIS.Tests.csproj'; References = @('IUIS.Domain', 'IUIS.Application', 'IUIS.Infrastructure'); OutputType = 'Library' }
}

$errors = New-Object System.Collections.Generic.List[string]
$results = New-Object System.Collections.Generic.List[object]

$solutionPath = Join-Path $repositoryRoot 'IUIS.sln'
if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    $errors.Add('IUIS.sln is missing.')
}

$propsPath = Join-Path $repositoryRoot 'Directory.Build.props'
if (-not (Test-Path -LiteralPath $propsPath -PathType Leaf)) {
    $errors.Add('Directory.Build.props is missing.')
} else {
    [xml]$propsXml = Get-Content -LiteralPath $propsPath -Raw
    $targetFramework = [string]($propsXml.Project.PropertyGroup.TargetFrameworkVersion | Select-Object -First 1)
    $languageVersion = [string]($propsXml.Project.PropertyGroup.LangVersion | Select-Object -First 1)
    if ($targetFramework -ne 'v4.8') {
        $errors.Add("Directory.Build.props TargetFrameworkVersion is '$targetFramework', expected 'v4.8'.")
    }
    if ($languageVersion -ne '7.3') {
        $errors.Add("Directory.Build.props LangVersion is '$languageVersion', expected '7.3'.")
    }
}

$projectNameByFullPath = @{}
foreach ($projectName in $expectedProjects.Keys) {
    $projectFullPath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $expectedProjects[$projectName].Path))
    $projectNameByFullPath[$projectFullPath] = $projectName
}

foreach ($projectName in $expectedProjects.Keys) {
    $definition = $expectedProjects[$projectName]
    $projectPath = Join-Path $repositoryRoot $definition.Path
    if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
        $errors.Add("Required project is missing: $($definition.Path)")
        continue
    }

    [xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
    $namespaceManager = New-Object System.Xml.XmlNamespaceManager($projectXml.NameTable)
    $namespaceManager.AddNamespace('msb', 'http://schemas.microsoft.com/developer/msbuild/2003')

    $outputTypeNode = $projectXml.SelectSingleNode('//msb:OutputType', $namespaceManager)
    $actualOutputType = if ($null -eq $outputTypeNode) { '' } else { $outputTypeNode.InnerText }
    if ($actualOutputType -ne $definition.OutputType) {
        $errors.Add("$projectName OutputType is '$actualOutputType', expected '$($definition.OutputType)'.")
    }

    $actualReferences = New-Object System.Collections.Generic.List[string]
    foreach ($referenceNode in $projectXml.SelectNodes('//msb:ProjectReference', $namespaceManager)) {
        $includePath = [string]$referenceNode.Include
        $referenceFullPath = [System.IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $projectPath) $includePath))
        if (-not $projectNameByFullPath.ContainsKey($referenceFullPath)) {
            $errors.Add("$projectName has an unknown project reference: $includePath")
            continue
        }
        $actualReferences.Add($projectNameByFullPath[$referenceFullPath])
    }

    $expectedReferenceSet = @($definition.References | Sort-Object)
    $actualReferenceSet = @($actualReferences | Sort-Object)
    if (($expectedReferenceSet -join '|') -ne ($actualReferenceSet -join '|')) {
        $errors.Add("$projectName references '$($actualReferenceSet -join ', ')', expected '$($expectedReferenceSet -join ', ')'.")
    }

    $results.Add([ordered]@{
        project = $projectName
        path = $definition.Path
        outputType = $actualOutputType
        references = @($actualReferenceSet)
    })
}

$formFiles = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot 'src') -Recurse -Filter '*Form.cs' -File
foreach ($formFile in $formFiles) {
    $formContent = Get-Content -LiteralPath $formFile.FullName -Raw
    if ($formContent -match 'System\.IO' -or $formContent -match 'System\.Text\.Json') {
        $errors.Add("Form contains prohibited file or JSON dependency: $($formFile.FullName)")
    }
}

$report = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    repositoryRoot = $repositoryRoot
    expectedProjectCount = 7
    validatedProjects = @($results)
    errors = @($errors)
    succeeded = ($errors.Count -eq 0)
}

$reportPath = Join-Path $validationRoot 'source-tree-validation.json'
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8

if ($errors.Count -gt 0) {
    foreach ($validationError in $errors) {
        Write-Error $validationError
    }
    throw "Source-tree validation failed with $($errors.Count) error(s)."
}

Write-Host 'Source-tree and project-reference validation succeeded.'
