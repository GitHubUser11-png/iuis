# IUIS Implementation State

Last updated: 2026-07-19

## Status vocabulary

The project uses the following evidence levels. These terms are not interchangeable.

| State | Meaning |
|---|---|
| Specified | A requirement or contract has been documented. |
| Created | Source or configuration exists in the authoritative Git repository. |
| Committed | The created material belongs to a named Git commit. |
| Verified in GitHub | The connector refetched the committed material from the expected repository and branch. |
| Compiled | A compatible compiler completed successfully and build evidence exists. |
| Tested | A test runner completed and machine-readable results exist. |
| Merged | A reviewed implementation pass was merged into its target branch. |
| Release-certified | Every mandatory build, test, security, backup, restore, and recovery gate passed. |

## Authoritative baseline

| Item | State | Evidence |
|---|---|---|
| Private GitHub repository | Created | `GitHubUser11-png/iuis` |
| Default branch | Finalized through Pass 7 | `main` |
| Integration branch | Recreated from finalized Pass 7 mainline | `develop` |
| Passes 1 and 2 | Completed, compiled, tested, and merged | PRs #1–#4 |
| Pass 3 Domain foundations | Completed, compiled, tested, and merged | PRs #5 and #6 |
| Pass 4 core identity/person aggregates | Completed, compiled, tested, and merged | PR #8 and synchronized follow-up history |
| Pass 4 preflight repair | Completed, compiled, tested, and merged | PR #12; run `29684773571`; 33 tests passed |
| Pass 5 academic aggregates | Completed and merged | PR #11; implementation merge `7dffa7498bf1efece00cffe417cc76b86c285547` |
| Pass 5 validation correction | Completed and merged | PR #14; final integration commit `6b5db90fce2691f7b76fbd5eed2731aa01179b82` |
| Pass 5 closure | Completed and merged | PR #16; closure commit `3fe05ae42f076a4c9ca7ff9e3f197ca8c8d4a9dd` |
| Pass 6 Finance foundations | Completed, compiled, tested, and merged | PR #17; integration commit `d5b24245009bfc8b6639a5bbdc7fa1e6d7af59eb` |
| Pass 6 closure | Completed, validated, and merged | PR #18; closure commit `9dcff9616dc8afb19af6d5bcf0497db77b31caa6` |
| Pass 7 Student Service Operations | Completed, compiled, tested, and merged | PR #23; integration commit `b4cb980d3989160969a02b4b5a51a162a088d695` |
| Pass 7 closure | Completed, validated, and mainline-synchronized | PRs #24–#26; final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 production repository/security bootstrap | Created on implementation branch; validation pending | `build/pass-08-production-repository-security-bootstrap` |
| Visual Studio solution | Created and merged | `IUIS.sln` |
| C# projects | 7 created and compiled through Pass 7; Pass 8 validation pending | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and operational | `.github/workflows/windows-build.yml` |
| Production Domain foundations | Created, compiled, tested, and merged | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Core identity/person aggregates | Created, compiled, tested, and merged | InstitutionIdentifier, UserAccount, UserSession, StudentRecord, EmployeeRecord |
| Academic foundation aggregates | Created, compiled, tested, and merged | Course, Curriculum, Subject, prerequisite graph, AcademicPeriod, Enrollment, snapshots |
| Finance foundation aggregates | Created, compiled, tested, and merged | charge rules, Tuition Assessment, Scholarship Award effects, Financial Adjustment, Payment, derived Student Ledger |
| Student Service Domain aggregates | Created, compiled, tested, merged, and closure-validated | Library, Counseling, Discipline, Clinic, Medical Record, Medical Clearance |
| Production repository catalog | Created; validation pending | exactly 49 descriptors: 14 principal and 35 supporting |
| Production JSON templates | 49 created; validation pending | `templates/production-data/*.json` |
| Persistence coordination foundation | Created; validation pending | repository envelopes, revision checks, cross-process locks, atomic writes, journaled transactions, recovery |
| Security bootstrap foundation | Created; validation pending | login attempts, lockout, PBKDF2, restricted sessions, forced password change, one-time bootstrap |
| Pass 8 expected test count | 110 | 96 existing plus 14 Infrastructure tests |
| Executable certification | Not achieved | final release gate only |

## Locked implementation target

- C# 7.3
- Windows Forms
- .NET Framework 4.8
- `System.Text.Json`
- seven-project solution
- separate User and Administrator executables
- layered architecture
- shared synchronized JSON persistence
- exactly 49 authoritative production JSON files
- centralized identifiers and journaled multi-file mutations
- no Forms that read or write JSON directly

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0 | Repository access, initial baseline, governance, and integration branch | Completed and merged |
| 1 | Seven-project Visual Studio solution foundation | Completed, compiled, tested, and merged |
| 2 | Windows build, NuGet, MSBuild, MSTest, and artifact foundation | Completed, compiled, tested, and merged |
| 3 | Production Domain foundations | Completed, compiled, tested, and merged |
| 4 | Core identity and person aggregates | Completed, compiled, tested, and merged |
| 5 | Academic foundation aggregates | Completed, compiled, tested, and merged through PR #11, correction PR #14, and closure PR #16 |
| 6 | Finance Domain foundations | Completed, compiled, tested, merged, and closure-validated through PRs #17 and #18 |
| 7 | Student Service Operations Domain foundations | Completed, compiled, tested, merged, closure-validated, and mainline-synchronized through PRs #23–#26 |
| 8 | Production repository and security bootstrap foundation | Created; Windows validation and PR integration pending |
| 9+ | Application orchestration, complete typed repositories, UI, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 7 are integrated into the authoritative repository history. Pass 8 source now defines the 49-repository catalog and initial templates, central ID allocation, cross-process locks, hardened atomic writes, journaled multi-file transactions, login-attempt tracking and temporary lockout, forced first-login password change, and one-time production bootstrap without a fixed credential. Fourteen Infrastructure tests have been added, producing an expected total of 110 tests. No Pass 8 compilation or automated-test success claim is made until the Windows workflow validates the final branch head. Application authorization orchestration, complete typed repositories for every module, business-module Forms, backup/restore execution, deployment, and release certification remain incomplete.

## Exact next gate

Open the Pass 8 implementation pull request, run source-tree and 49-template validation, NuGet restoration, Release MSBuild, all 110 MSTest cases, TRX verification, and artifact publication. Correct every compiler, test, or architecture failure before integration into `develop`, then independently closure-validate the merged Infrastructure baseline.
