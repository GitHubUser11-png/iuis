# IUIS Decision Register

This register records implementation decisions that are already authoritative before source construction begins. Changes require an explicit replacement decision rather than silent drift.

## ADR-0001 — Specifications are not implementation

**Status:** Accepted

Parts 1–23 define requirements and implementation contracts. They do not prove that files, executables, tests, or deployments exist. Repository state and machine evidence determine implementation claims.

## ADR-0002 — Technical target

**Status:** Accepted

The implementation target is C# 7.3, Windows Forms, and .NET Framework 4.8. The project will not raise the language version or change the target framework merely to avoid compatibility corrections.

## ADR-0003 — Seven-project solution

**Status:** Accepted

The solution will contain:

1. `IUIS.Domain`
2. `IUIS.Application`
3. `IUIS.Infrastructure`
4. `IUIS.SharedUI`
5. `IUIS.UserApp`
6. `IUIS.AdminApp`
7. `IUIS.Tests`

Dependency direction is inward. Domain and Application remain independent of Infrastructure and Windows Forms.

## ADR-0004 — Separate executable boundaries

**Status:** Accepted

`IUIS.UserApp.exe` serves public, Student, and Employee/Faculty workflows. `IUIS.AdminApp.exe` serves restricted Administrator workflows. The general Login does not offer an Administrator option or role selector.

## ADR-0005 — JSON persistence contract

**Status:** Accepted

`System.Text.Json` is mandatory. Production persistence will use a shared synchronized repository containing exactly 49 authoritative JSON files after the repository-template pass. Missing production files are not silently recreated during ordinary startup.

## ADR-0006 — UI isolation

**Status:** Accepted

Windows Forms and UserControls receive Application services and safe DTOs. They do not open files, instantiate repositories, invoke `System.Text.Json`, or bind confidential Domain entities directly.

## ADR-0007 — Identity and transaction integrity

**Status:** Accepted

Entity IDs and public numbers are centrally generated, never reused, and allocated inside journaled transactions. Cross-process locks, deterministic lock order, durable staging, rollback evidence, and atomic replacement are required for multi-file mutations.

## ADR-0008 — Security boundary

**Status:** Accepted

Passwords are never stored or recoverable in plaintext. Sessions require both Session ID and a random bearer token. Administrator access additionally requires the restricted application, trusted-device validation, approved IPv4 network validation, and explicit permissions.

## ADR-0009 — Git flow

**Status:** Accepted

`main` contains reviewed release-ready baselines. `develop` is the integration branch. Each implementation pass uses `build/pass-*` from `develop`, is refetched and reviewed, and is merged only after its applicable validation gate.

## ADR-0010 — Evidence-based status claims

**Status:** Accepted

A file is created only after a successful GitHub write. A build is compiled only after compatible MSBuild evidence exists. Tests pass only after machine-readable runner output exists. Release certification requires every mandatory gate.

## ADR-0011 — No development seed in production

**Status:** Accepted

No fixed Administrator, Student, Employee, password, session token, or demo credential will be committed as production behavior. First production identity creation uses the controlled bootstrap workflow.

## ADR-0012 — Repository templates follow contracts

**Status:** Accepted

The 49 JSON templates will not be created before their catalog, envelope, schema, and structural-document contracts are implemented and tested. This prevents templates from becoming an accidental competing schema authority.
