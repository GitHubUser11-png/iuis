# Pass 2 — Windows Build and Test Foundation

## Objective

Create the first Windows-based evidence pipeline for the IUIS solution. This pass adds deterministic source-tree validation, NuGet restore, Release MSBuild, MSTest discovery and execution, TRX output, binary build logs, and artifact publication.

## Starting point

- Repository: `GitHubUser11-png/iuis`
- Base branch: `build/pass-01-solution-foundation`
- Base commit: `bc957bd3b165f2697ba37eb45758ce6317addebd`
- Pass branch: `build/pass-02-windows-ci`

Pass 2 is stacked on Pass 1 because the seven-project solution had not yet been merged into `develop` when this pass began.

## Created

- `.github/workflows/windows-build.yml`
- `build/Invoke-IuisBuild.ps1`
- `build/Test-IuisSourceTree.ps1`
- `tests/IUIS.Tests/packages.config`
- this Pass 2 implementation record

## Modified

- `tests/IUIS.Tests/IUIS.Tests.csproj`
- `tests/IUIS.Tests/SolutionFoundationTests.cs`
- `src/IUIS.UserApp/Program.cs`
- `src/IUIS.AdminApp/Program.cs`
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
8. locate the restored MSTest adapter assembly;
9. execute the MSTest assembly;
10. verify the expected TRX file;
11. publish build logs, binary logs, TRX results, validation reports, and Release binaries.

## Test integration

The test project uses:

- `MSTest.TestFramework` 3.6.4;
- `MSTest.TestAdapter` 3.6.4;
- .NET Framework 4.8;
- C# 7.3.

The initial tests verify the canonical Domain, Application, and Infrastructure foundation markers. The PowerShell structural validator separately verifies project count, output types, dependency direction, central framework/language settings, and the absence of direct `System.IO` or `System.Text.Json` references in Form source files.

## Compiler and pipeline corrections

The first real Windows executions exposed and corrected three implementation defects:

1. the original PowerShell XML-based project validator did not behave consistently on the hosted runner, so it was replaced by deterministic literal project-contract checks;
2. `Application.EnableVisualStyles()` and related calls conflicted with the referenced `IUIS.Application` namespace, so both executable entry points now use fully qualified `System.Windows.Forms.Application` calls;
3. rigid VSTest and adapter paths prevented test execution, so the build harness now discovers VSTest through `vswhere`, discovers the MSTest adapter assembly recursively, verifies TRX creation, and writes progressive failure-stage evidence.

## Validated implementation evidence

The Pass 2 implementation baseline was validated by GitHub Actions run `29550063410` on commit `7279793ab12adea13e899ca81f5980cd9b68d5b9`. Later commits in this branch only finalize evidence documentation and remain subject to the pull-request validation check before merge.

All workflow stages completed successfully:

- checkout: passed;
- MSBuild discovery: passed;
- NuGet discovery: passed;
- source-tree and architecture validation: passed;
- NuGet restore: passed;
- Release compilation: passed;
- MSTest execution: passed;
- evidence upload: passed.

The validated build result is:

- Release warnings: `0`;
- Release errors: `0`;
- tests executed: `3`;
- tests passed: `3`;
- tests failed: `0`;
- TRX: `artifacts/TestResults/IUIS.Tests.trx`.

The reference evidence artifact is:

- name: `iuis-windows-build-evidence-7`;
- artifact ID: `8395407334`;
- SHA-256: `35ab9b87b7158d90f4eb59499045ab40d48f53859d371fedf7a6ea05060bad46`;
- expiration: 2026-07-31.

## Compatibility notes

MSTest 3.6.4 supports .NET Framework 4.6.2 and later, which includes the IUIS .NET Framework 4.8 target. Package versions are pinned so that the compiler and test result are reproducible. All committed source remains compatible with C# 7.3.

## Deliberately deferred

- production `System.Text.Json` package integration;
- Domain entities and value objects;
- Application service contracts;
- persistence and repository templates;
- security and module implementation;
- broader unit, integration, concurrency, and fault-injection tests.

## Evidence boundary

Pass 2 establishes a compile-verified and test-verified structural foundation only. It does not certify production Domain behavior, persistence, authentication, business modules, operational recovery, deployment readiness, or final executables.

## Integration gate

PR #2 may be merged into `build/pass-01-solution-foundation` only after the latest pull-request validation succeeds and the pull-request description records the evidence. PR #1 may then be merged into `develop`, followed by a fresh Windows validation of the integrated `develop` baseline.