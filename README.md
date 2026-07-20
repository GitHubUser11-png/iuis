# Integrated University Information System (IUIS)

This repository contains the authoritative C# implementation of the Integrated University Information System specified for the IT332 final project.

## Current implementation state

Passes 1 through 8 establish the seven-project .NET Framework 4.8 solution, Domain foundations, the exact 49-file production JSON catalog, central ID allocation, cross-process locks, hardened atomic writes, journaled transactions, login lockout, forced password change, and one-time production bootstrap.

Pass 9 adds:

- Application authorization from roles, active profiles, direct grants, direct restrictions, application kind, session purpose, ownership, and confidentiality;
- session-aware command and query execution;
- Student own-record enforcement from the authenticated session;
- Employee and Administrator permission boundaries;
- separate released and internal Counseling, Discipline, and Medical DTOs;
- revision-aware typed repository contracts and Infrastructure adapters;
- journal-coordinated Application mutations;
- in-lock expected revision revalidation for staged Application transactions; and
- an explicit mapper-readiness catalog for all 18 production aggregate adapters.

Pass 9 is integrated into `develop`. Its closure branch is undergoing independent Windows validation before mainline promotion.

Progress and evidence are recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md). Pass-specific evidence is recorded in [`docs/passes/PASS_09_APPLICATION_AUTHORIZATION_TYPED_REPOSITORIES.md`](docs/passes/PASS_09_APPLICATION_AUTHORIZATION_TYPED_REPOSITORIES.md) and [`docs/passes/PASS_09_CLOSURE.md`](docs/passes/PASS_09_CLOSURE.md).

## Locked technical target

- C# 7.3
- Windows Forms
- .NET Framework 4.8
- `System.Text.Json`
- separate `IUIS.UserApp.exe` and `IUIS.AdminApp.exe`
- layered seven-project Visual Studio solution
- shared synchronized JSON persistence
- exactly 49 authoritative production JSON files
- centralized identifiers and journaled related mutations
- no direct JSON or file-system access from Forms

## Branch model

- `main`: reviewed release-ready baselines
- `develop`: integration branch
- `build/pass-*`: controlled implementation and closure passes created from exact integration commits

No component is described as compiled or tested unless machine-generated build or test evidence exists.
