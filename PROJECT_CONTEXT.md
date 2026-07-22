# Integrated University Information System (IUIS) — Realigned Project Context

This document is the authoritative context for the IUIS academic project. It replaces the previous Flutter-oriented working agreement and aligns the repository with the Visual Studio Community 2022 / .NET Framework foundation.

## Technology stack

- **Language:** C# 7.3
- **Runtime:** .NET Framework 4.8
- **UI framework:** Windows Forms
- **IDE target:** Visual Studio Community 2022
- **JSON library:** Newtonsoft.Json 13.0.4
- **Storage:** shared JSON files (no relational database)
- **Version control:** Git with `main` / `develop` workflow

## Visual Studio solution layout

```text
IUIS.sln
├── Directory.Build.props
├── Directory.Build.targets
├── nuget.config
├── src/
│   ├── IUIS.Domain          (domain models, enums, aggregates, services)
│   ├── IUIS.Application     (orchestration, DTOs, repositories, security, navigation)
│   ├── IUIS.Infrastructure  (JSON persistence, identity, sessions, projections, composition)
│   ├── IUIS.SharedUI        (controls, theme, navigation, dialogs, DataGridView helpers)
│   ├── IUIS.UserApp.exe     (student / employee-faculty entry point)
│   └── IUIS.AdminApp.exe    (administrator entry point, separate access path)
├── tests/
│   └── IUIS.Tests           (MSTest unit and integration tests)
├── templates/
│   └── production-data/     (49 canonical JSON repository templates)
├── build/
│   ├── Test-IuisSourceTree.ps1
│   └── Invoke-IuisBuild.ps1
└── docs/                    (implementation state, pass records, decision register)
```

## Core modules

- **Student Records** — personal/academic records, profile corrections, status cards
- **Academic** — courses, subjects, curricula, academic periods, assessments, subject assignments
- **Enrollment** — student enrollment transactions and status
- **Finance** — tuition assessments, payments, scholarships, financial adjustments
- **Library** — book inventory, borrowing, returning, circulation history
- **Counseling & Discipline** — counseling cases, incidents, violations, responses
- **Medical & Health** — clinic appointments, medical records, consultations, clearances
- **Employee / Faculty** — employee records, attendance, work schedules, corrections
- **Identity & Security** — users, account applications, sessions, login attempts, admin access rules
- **Approval & Audit** — administrative approvals, audit logs, system settings

## Shared JSON repository

The system uses a central set of JSON files (currently 49 authoritative repositories). Each repository uses a canonical envelope:

```json
{
  "repositoryName": "students",
  "schemaVersion": 1,
  "revision": 0,
  "updatedAtUtc": "2026-01-01T00:00:00Z",
  "updatedByUserId": "system",
  "records": []
}
```

Cross-module relationships are maintained through unique identifiers (`StudentId`, `CourseId`, `SubjectId`, `EmployeeId`, `BookId`, etc.) rather than duplicated data.

## User roles

1. **Student** — views own records, enrollment, finance, library, medical clearance requests
2. **Employee / Faculty** — role-specific access based on office or assignment
3. **Administrator** — separate secured access path; reviews applications, issues IDs, configures system

## Architecture principles

- **Object-oriented:** encapsulation, inheritance, abstraction, polymorphism
- **Layered:** Domain → Application → Infrastructure → SharedUI / UserApp / AdminApp
- **Separation of concerns:** forms call services; services call repositories; repositories handle JSON I/O
- **Validation & exception handling** on every write path
- **Search, sorting, filtering, DataGridView presentation** on management screens
- **Atomic, journaled, cross-process-safe** JSON writes in the infrastructure layer

## Visual Studio Community 2022 setup

1. Clone the repository.
2. Open `IUIS.sln` in Visual Studio Community 2022.
3. Run `Restore NuGet packages` (or `nuget restore IUIS.sln` from the command line).
4. Build the solution (`Debug` or `Release`).
5. Set the start-up project to `IUIS.UserApp` or `IUIS.AdminApp` as needed.

## Notes

- The previous root `Project Context.md` (Flutter-oriented) has been retired.
- The active serialization implementation is migrating to **Newtonsoft.Json**. Remaining `System.Text.Json` references are being phased out.
