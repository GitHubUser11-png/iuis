# Pass 1 — Seven-Project Visual Studio Solution Foundation

## Objective

Create the first authoritative C# source foundation for the Integrated University Information System without overstating compilation or test status.

## Starting point

- Repository: `GitHubUser11-png/iuis`
- Base branch: `develop`
- Base commit: `286184c97dacf655df380dbe9809f0affcc1a979`
- Pass branch: `build/pass-01-solution-foundation`

## Created

- `IUIS.sln`
- `Directory.Build.props`
- `Directory.Build.targets`
- seven legacy-style .NET Framework project files
- seven AssemblyInfo files
- minimal Domain, Application, and Infrastructure project markers
- SharedUI application identity constants
- minimal UserApp startup entry point and Form
- minimal AdminApp startup entry point and Form
- two .NET Framework 4.8 App.config files
- test assembly foundation

## Dependency graph

```text
IUIS.Domain
IUIS.Application -> IUIS.Domain
IUIS.Infrastructure -> IUIS.Domain + IUIS.Application
IUIS.SharedUI -> IUIS.Domain + IUIS.Application
IUIS.UserApp -> Domain + Application + Infrastructure + SharedUI
IUIS.AdminApp -> Domain + Application + Infrastructure + SharedUI
IUIS.Tests -> Domain + Application + Infrastructure
```

## Compatibility contract

- C# language version: 7.3
- target framework: .NET Framework 4.8
- platform: Any CPU
- UserApp and AdminApp output type: WinExe
- remaining projects: Library
- central build validation rejects a different framework or language version

## Deliberately deferred

- `System.Text.Json` package installation
- GitHub Actions and Windows MSBuild evidence
- test-framework and test-adapter packages
- Designer-generated Forms and RESX files
- Domain entities and Application services
- persistence, security, modules, and 49 JSON templates

## Verification state

- GitHub files: to be refetched after commit
- structural branch comparison: to be executed after commit
- Release build: not executed in Pass 1
- automated test runner: not executed in Pass 1
- pull request: to be opened into `develop`

Pass 2 will introduce Windows CI and produce the first evidence-backed compilation result.
