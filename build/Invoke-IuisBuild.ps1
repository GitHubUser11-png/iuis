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
$summaryPath = Join-Path $logsRoot 'build-summary.json'
$testLogPath = Join-Path $logsRoot 'IUIS-tests.log'

New-Item -ItemType Directory -Force -Path $logsRoot | Out-Null
New-Item -ItemType Directory -Force -Path $testResultsRoot | Out-Null

$summary = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    configuration = $Configuration
    solution = $SolutionPath
    buildSucceeded = $false
    testsSucceeded = $false
    currentStage = 'initialization'
    failureMessage = $null
    testAssembly = $null
    testAdapterPath = $null
    vstestPath = $null
    trxFile = $null
}

function Write-Summary
{
    $summary.generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    $summary | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $summaryPath -Encoding UTF8
}

try {
    if (-not (Test-Path -LiteralPath $solutionFullPath -PathType Leaf)) {
        throw "Solution file was not found: $solutionFullPath"
    }

    $summary.currentStage = 'msbuild-discovery'
    Write-Summary

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

    $summary.currentStage = 'release-build'
    Write-Summary
    Write-Host "Building $solutionFullPath in $Configuration configuration."
    & $msbuildCommand.Source @msbuildArguments
    if ($LASTEXITCODE -ne 0) {
        throw "MSBuild failed with exit code $LASTEXITCODE."
    }

    $summary.buildSucceeded = $true
    $summary.currentStage = 'test-assembly-validation'
    Write-Summary

    $testAssembly = Join-Path $artifactsRoot "bin\$Configuration\IUIS.Tests\IUIS.Tests.dll"
    if (-not (Test-Path -LiteralPath $testAssembly -PathType Leaf)) {
        throw "Test assembly was not produced: $testAssembly"
    }
    $summary.testAssembly = $testAssembly.Substring($repositoryRoot.Length).TrimStart('\')

    $summary.currentStage = 'vstest-discovery'
    Write-Summary

    $vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswherePath -PathType Leaf)) {
        throw "vswhere was not found: $vswherePath"
    }

    $vstestPath = & $vswherePath -latest -products * -find 'Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($vstestPath) -or -not (Test-Path -LiteralPath $vstestPath -PathType Leaf)) {
        throw 'VSTest.Console.exe was not found in the selected Visual Studio installation.'
    }
    $summary.vstestPath = $vstestPath

    $summary.currentStage = 'test-adapter-discovery'
    Write-Summary

    $adapterPackageRoot = Join-Path $repositoryRoot 'packages\MSTest.TestAdapter.4.3.2'
    if (-not (Test-Path -LiteralPath $adapterPackageRoot -PathType Container)) {
        throw "MSTest adapter package was not restored: $adapterPackageRoot"
    }

    $adapterAssembly = Get-ChildItem -LiteralPath $adapterPackageRoot -Recurse -File |
        Where-Object { $_.Name -eq 'Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll' } |
        Select-Object -First 1

    if ($null -eq $adapterAssembly) {
        throw "Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll was not found under $adapterPackageRoot"
    }

    $adapterPath = $adapterAssembly.Directory.FullName
    $summary.testAdapterPath = $adapterPath
    $summary.currentStage = 'test-execution'
    Write-Summary

    $trxFileName = 'IUIS.Tests.trx'
    $trxPath = Join-Path $testResultsRoot $trxFileName
    Write-Host "Executing tests from $testAssembly."

    & $vstestPath $testAssembly "/TestAdapterPath:$adapterPath" "/Logger:trx;LogFileName=$trxFileName" "/ResultsDirectory:$testResultsRoot" 2>&1 |
        Tee-Object -FilePath $testLogPath

    if ($LASTEXITCODE -ne 0) {
        throw "VSTest failed with exit code $LASTEXITCODE."
    }

    if (-not (Test-Path -LiteralPath $trxPath -PathType Leaf)) {
        throw "VSTest completed without producing the expected TRX file: $trxPath"
    }

    $summary.testsSucceeded = $true
    $summary.currentStage = 'completed'
    $summary.trxFile = $trxPath.Substring($repositoryRoot.Length).TrimStart('\')
    Write-Summary
    Write-Host "Build and test evidence written under $artifactsRoot."
}
catch {
    $summary.failureMessage = $_.Exception.Message
    $summary.currentStage = 'failed'
    Write-Summary
    throw
}
