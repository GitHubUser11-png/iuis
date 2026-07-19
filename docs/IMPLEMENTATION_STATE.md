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
| Default branch | Finalized through Pass 7 | `main` at `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Integration branch | Pass 8 implementation integrated | `develop` at integration commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce` before closure |
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
| Pass 8 predecessor PR | Closed without merge | PR #27 |
| Pass 8 implementation | Recovered, validated, and merged into `develop` | PR #28; integration commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce` |
| Pass 8 original validation | Successful | run `29691593386`; 0 warnings, 0 errors; 110 tests passed |
| Pass 8 original artifact | Verified | `iuis-windows-build-evidence-88`, artifact `8443750845`, SHA-256 `d79a4789460820da807e3147ede3edef6a754a490af14f158fa4c9b45f84d0ca` |
| Pass 8 replacement-PR validation | Successful | run `29692698528`; 0 warnings, 0 errors; 110 tests passed |
| Pass 8 replacement artifact | Verified | `iuis-windows-build-evidence-90`, artifact `8444064263`, SHA-256 `8955298d4693623d585e62bdae83750106d74a269d5aa0c488b8d6436bc931d3` |
| Pass 8 closure | Branch created; integrated-tree validation pending | `build/pass-08-closure` from `4920f3dec56e584bb3d4aa620b194b5dd31c36ce` |
| Visual Studio solution | Created and compiled through integrated Pass 8 | `IUIS.sln` |
| C# projects | 7 created and compiled through integrated Pass 8 | Domain, Application, Infrastructure, SharedUI, UserApp, AdminApp, Tests |
| Central build properties | Created and enforced | `Directory.Build.props`, `Directory.Build.targets` |
| Windows CI workflow | Created and operational | `.github/workflows/windows-build.yml` |
| Production Domain foundations | Created, compiled, tested, and merged | entity contracts, value objects, monetary rules, identity enums, and compatibility policy |
| Core identity/person aggregates | Created, compiled, tested, and merged | InstitutionIdentifier, UserAccount, UserSession, StudentRecord, EmployeeRecord |
| Academic foundation aggregates | Created, compiled, tested, and merged | Course, Curriculum, Subject, prerequisite graph, AcademicPeriod, Enrollment, snapshots |
| Finance foundation aggregates | Created, compiled, tested, and merged | charge rules, Tuition Assessment, Scholarship Award effects, Financial Adjustment, Payment, derived Student Ledger |
| Student Service Domain aggregates | Created, compiled, tested, merged, and closure-validated | Library, Counseling, Discipline, Clinic, Medical Record, Medical Clearance |
| Production repository catalog | Created and validated | exactly 49 descriptors: 14 principal and 35 supporting |
| Production JSON templates | 49 created and validated | `templates/production-data/*.json` |
| Persistence coordination foundation | Created, validated, and integrated | envelopes, revision checks, cross-process locks, atomic writes, journaled transactions, recovery |
| Security bootstrap foundation | Created, validated, and integrated | login attempts, lockout, PBKDF2, restricted sessions, forced password change, one-time bootstrap |
| Pass 8 test count | 110 passed | 96 existing plus 14 Infrastructure tests |
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
| 8 | Production repository and security bootstrap foundation | Implementation validated and integrated through PR #28; closure and mainline promotion pending |
| 9+ | Application orchestration, complete typed repositories, UI, operations, and certification | Not started |

## Current truthful completion statement

Passes 1 through 7 are integrated and mainline-synchronized. Pass 8 has been recovered from the exact validated commit, independently revalidated on replacement PR #28, and merged into `develop`. The integrated baseline contains the exact 49-repository catalog and templates, central ID allocation, cross-process locks, hardened atomic writes, journaled transactions, login-attempt lockout, forced password change, and production bootstrap. Both successful Pass 8 implementation runs completed with zero warnings, zero errors, and all 110 tests passing. Pass 8 is not closure-complete or promoted to `main` until the actual merged tree receives independent closure validation and exact mainline validation.

Application authorization orchestration, complete typed repositories for every module, restricted DTO projection, business-module Forms, backup/restore execution, deployment, and release certification remain incomplete.

## Exact next gate

Validate the `build/pass-08-closure` head created from integration commit `4920f3dec56e584bb3d4aa620b194b5dd31c36ce`; merge the validated closure into `develop`; promote the complete Pass 8 baseline to `main`; validate the exact mainline integration commit; synchronize `develop` to final `main`; verify zero divergence; and record the final Pass 8 evidence and Pass 9 starting point.