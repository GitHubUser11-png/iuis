# Pass 5 Preflight — Identity Merge-Corruption Repair

## Trigger

The first Pass 5 pull-request workflow compiled the GitHub-generated merge ref and failed before reaching the new academic sources. The compiler reported syntax errors in `InstitutionIdentifier.cs`, `UserAccount.cs`, and `UserSession.cs`.

Inspection showed that two independently merged Pass 4 histories had been synchronized through `main`. Git auto-merged overlapping implementations into single malformed files, concatenating two class bodies. The same synchronization also left an unused second Student/Employee model in the repository outside the authoritative project file.

## Repair scope

This repair branch restores the previously validated canonical Pass 4 implementation and removes duplicate uncompiled alternatives.

Restored:

- `src/IUIS.Domain/Identity/InstitutionIdentifier.cs`
- `src/IUIS.Domain/Identity/UserAccount.cs`
- `src/IUIS.Domain/Identity/UserSession.cs`

Removed:

- `src/IUIS.Domain/People/Employee.cs`
- `src/IUIS.Domain/People/PersonEnums.cs`
- `src/IUIS.Domain/People/PersonRecordBase.cs`
- `src/IUIS.Domain/People/Student.cs`

The authoritative person aggregates remain:

- `StudentRecord`
- `EmployeeRecord`
- `MasterRecordEnums`

## Invariant preservation

- one canonical `InstitutionIdentifier` implementation;
- one canonical `StudentStatus` and `EmploymentStatus` definition;
- one canonical Student master-record aggregate;
- one canonical Employee master-record aggregate;
- separate User account and person records;
- raw session token remains absent from persisted Domain state;
- C# 7.3 and .NET Framework 4.8 compatibility;
- no JSON, file-system, WinForms, Application, or Infrastructure dependency in Domain.

## Evidence boundary

This repair changes no Pass 5 academic behavior. It is a mandatory integration-baseline correction discovered by compiler evidence. The repair must pass the full Windows build and test workflow before it is merged into `develop`. After merge, PR #11 must be revalidated against the repaired base.
