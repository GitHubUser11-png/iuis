# Integrated University Information System (IUIS)

This repository contains the authoritative C# implementation of the Integrated University Information System specified for the IT332 final project.

## Current implementation state

Passes 1 through 7 established and validated:

- the seven-project Visual Studio solution;
- Windows Release build and MSTest evidence;
- Domain foundations and identity/person aggregates;
- Academic aggregates;
- Finance aggregates;
- Student Service Operations aggregates.

Pass 8 introduces the first production Infrastructure baseline:

- an authoritative 49-repository JSON catalog;
- 49 initial production JSON templates;
- `System.Text.Json` repository envelopes and revision checks;
- central ID sequence allocation;
- cross-process repository locking;
- hardened atomic writes;
- journaled multi-file transactions and recovery;
- login-attempt tracking and temporary lockout;
- PBKDF2-HMAC-SHA256 password hashing;
- forced first-login password change;
- one-time production bootstrap without a fixed default credential.

Progress and evidence are recorded in [`docs/IMPLEMENTATION_STATE.md`](docs/IMPLEMENTATION_STATE.md).

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
- `build/pass-*`: controlled implementation passes created from `develop`

No component is described as compiled or tested unless machine-generated build or test evidence exists.
