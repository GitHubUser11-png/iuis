param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$SolutionPath = 'IUIS.sln'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$solutionFullPath = Join-Path $repositoryRoot $SolutionPath
$artifactsRoot = Join-Path $repositoryRoot 'artifacts'
$logsRoot = Join-Path $artifactsRoot 'logs'
$testResultsRoot = Join-Path $artifactsRoot 'TestResults'

New-Item -ItemType Directory -Force -Path $logsRoot | Out-Null
New-Item -ItemType Directory -Force -Path $testResultsRoot | Out-Null

if (-not (Test-Path -LiteralPath $solutionFullPath -PathType Leaf)) {
    throw "Solution file was not found: $solutionFullPath"
}

$msbuildCommand = Get-Command msbuild.exe -ErrorAction SilentlyContinue
if ($null -eq $msbuildCommand) {
    $msbuildCommand = Get-Command msbuild -ErrorAction SilentlyContinue
}
if ($null -eq $msbuildCommand) {
    throw 'MSBuild was not found on PATH.'
}

$buildLog = Join-Path $logsRoot "IUIS-$Configuration-build.log"
$binaryLog = Join-Path $logsRoot "IUIS-$Configuration-build.binlog"

$msbuildArguments = @(
    $solutionFullPath,
    '/m',
    '/t:Rebuild',
    "/p:Configuration=$Configuration",
    '/p:Platform=Any CPU',
    '/verbosity:minimal',
    "/bl:$binaryLog",
    '/fl',
    "/flp:logfile=$buildLog;verbosity=diagnostic"
)

Write-Host "Building $solutionFullPath in $Configuration configuration."
& $msbuildCommand.Source @msbuildArguments
if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE."
}

$testAssembly = Join-Path $artifactsRoot "bin\$Configuration\IUIS.Tests\IUIS.Tests.dll"
if (-not (Test-Path -LiteralPath $testAssembly -PathType Leaf)) {
    throw "Test assembly was not produced: $testAssembly"
}

$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswherePath -PathType Leaf)) {
    throw "vswhere was not found: $vswherePath"
}

$vstestPath = & $vswherePath -latest -products * -requires Microsoft.VisualStudio.Component.VSTest -find 'Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($vstestPath) -or -not (Test-Path -LiteralPath $vstestPath -PathType Leaf)) {
    throw 'VSTest.Console.exe was not found in the selected Visual Studio installation.'
}

$adapterPath = Join-Path $repositoryRoot 'packages\MSTest.TestAdapter.3.6.4\build\_common'
if (-not (Test-Path -LiteralPath $adapterPath -PathType Container)) {
    throw "MSTest adapter directory was not restored: $adapterPath"
}

$trxFileName = 'IUIS.Tests.trx'
Write-Host "Executing tests from $testAssembly."
& $vstestPath $testAssembly "/TestAdapterPath:$adapterPath" "/Logger:trx;LogFileName=$trxFileName" "/ResultsDirectory:$testResultsRoot"
if ($LASTEXITCODE -ne 0) {
    throw "VSTest failed with exit code $LASTEXITCODE."
}

$summary = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    configuration = $Configuration
    solution = $SolutionPath
    buildSucceeded = $true
    testsSucceeded = $true
    testAssembly = $testAssembly.Substring($repositoryRoot.Length).TrimStart('\')
    trxFile = (Join-Path 'artifacts\TestResults' $trxFileName)
}

$summaryPath = Join-Path $logsRoot 'build-summary.json'
$summary | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
Write-Host "Build and test evidence written under $artifactsRoot."
