# Pass 2 — Windows Build and Test Foundation

## Objective

Create the first Windows-based evidence pipeline for the IUIS solution. This pass adds deterministic source-tree validation, NuGet restore, Release MSBuild, MSTest discovery and execution, TRX output, binary build logs, and artifact publication.

## Starting point

- Repository: `GitHubUser11-png/iuis`
- Base branch: `build/pass-01-solution-foundation`
- Base commit: `bc957bd3b165f2697ba37eb45758ce6317addebd`
- Pass branch: `build/pass-02-windows-ci`

Pass 2 is stacked on Pass 1 because the seven-project solution has not yet been merged into `develop`.

## Created

- `.github/workflows/windows-build.yml`
- `build/Invoke-IuisBuild.ps1`
- `build/Test-IuisSourceTree.ps1`
- `tests/IUIS.Tests/packages.config`
- this Pass 2 implementation record

## Modified

- `tests/IUIS.Tests/IUIS.Tests.csproj`
- `tests/IUIS.Tests/SolutionFoundationTests.cs`
- `docs/IMPLEMENTATION_STATE.md`

## Build workflow

The Windows workflow performs the following ordered gates:

1. check out the exact commit;
2. add Visual Studio MSBuild to `PATH`;
3. add NuGet 6.x to `PATH`;
4. validate the seven-project source tree and project-reference graph;
5. restore NuGet packages;
6. execute a Release rebuild through MSBuild;
7. locate `VSTest.Console.exe` through `vswhere`;
8. execute the MSTest assembly;
9. publish build logs, binary logs, TRX results, validation reports, and Release binaries.

## Test integration

The test project uses:

- `MSTest.TestFramework` 3.6.4;
- `MSTest.TestAdapter` 3.6.4;
- .NET Framework 4.8;
- C# 7.3.

The initial tests verify the canonical Domain, Application, and Infrastructure foundation markers. The PowerShell structural validator separately verifies project count, output types, dependency direction, central framework/language settings, and the absence of direct `System.IO` or `System.Text.Json` references in Form source files.

## Compatibility notes

MSTest 3.6.4 supports .NET Framework 4.6.2 and later, which includes the IUIS .NET Framework 4.8 target. Package versions are pinned so that the first compiler result is reproducible.

## Deliberately deferred

- production `System.Text.Json` package integration;
- Domain entities and value objects;
- Application service contracts;
- persistence and repository templates;
- security and module implementation;
- broader unit, integration, concurrency, and fault-injection tests.

## Evidence boundary

The workflow and test integration are created and committed by this pass. Compilation and test success are not claimed until the corresponding GitHub Actions run reaches a completed successful conclusion and its artifacts are inspected.
