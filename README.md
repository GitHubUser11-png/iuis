# Integrated University Information System (IUIS)

This repository contains the authoritative source implementation of the Integrated University Information System specified for the IT332 final project.

## Current implementation state

The repository is being constructed from an empty baseline. The implementation specifications completed before this repository was created are requirements and design contracts; they are not evidence that source code already existed.

Pass 1 establishes the seven-project Visual Studio solution foundation:

- `IUIS.Domain`
- `IUIS.Application`
- `IUIS.Infrastructure`
- `IUIS.SharedUI`
- `IUIS.UserApp`
- `IUIS.AdminApp`
- `IUIS.Tests`

Minimal startup Forms exist only to prove the intended executable boundaries structurally. Authentication, repositories, modules, production JSON templates, and real automated test-runner integration remain deferred.

Progress is recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md).

## Locked technical target

- C# 7.3
- Windows Forms
- .NET Framework 4.8
- `System.Text.Json`
- separate `IUIS.UserApp.exe` and `IUIS.AdminApp.exe`
- layered seven-project Visual Studio solution
- shared synchronized JSON persistence
- exactly 49 authoritative production JSON files after repository-template implementation
- no direct JSON or file-system access from Forms

## Branch model

- `main`: reviewed release-ready baselines
- `develop`: integration branch
- `build/pass-*`: controlled implementation passes created from `develop`

No component is described as compiled or tested unless machine-generated build or test evidence exists.
