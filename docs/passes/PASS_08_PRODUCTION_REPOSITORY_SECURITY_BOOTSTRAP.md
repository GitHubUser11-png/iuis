# Pass 8 — Production Repository and Security Bootstrap Foundation

## Objective

Create the first production Infrastructure baseline for synchronized JSON persistence and security bootstrap without allowing Domain entities or Windows Forms to access JSON or the file system directly.

## Starting point

- repository: `GitHubUser11-png/iuis`
- baseline branch: `develop`
- starting commit: `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd`
- implementation branch: `build/pass-08-production-repository-security-bootstrap`
- Passes 1 through 7 were integrated and closure-validated before this branch was created

## Authoritative production repository catalog

Pass 8 defines exactly 49 authoritative production JSON repositories: 14 principal repositories and 35 supporting repositories.

### Principal repositories

1. `students.json`
2. `courses.json`
3. `subjects.json`
4. `enrollments.json`
5. `payments.json`
6. `books.json`
7. `borrowings.json`
8. `counseling.json`
9. `violations.json`
10. `medical_records.json`
11. `employees.json`
12. `attendance.json`
13. `clearances.json`
14. `users.json`

### Supporting repositories

1. `academic_periods.json`
2. `assessments.json`
3. `assessment_charge_rules.json`
4. `scholarship_programs.json`
5. `scholarship_applications.json`
6. `scholarship_awards.json`
7. `appointments.json`
8. `consultations.json`
9. `subject_assignments.json`
10. `notifications.json`
11. `account_applications.json`
12. `permission_profiles.json`
13. `login_attempts.json`
14. `sessions.json`
15. `security_policy.json`
16. `password_assistance_requests.json`
17. `admin_access_rules.json`
18. `administrative_approvals.json`
19. `discipline_incidents.json`
20. `violation_responses.json`
21. `work_schedules.json`
22. `attendance_corrections.json`
23. `employee_profile_corrections.json`
24. `student_profile_corrections.json`
25. `payment_void_requests.json`
26. `financial_adjustments.json`
27. `audit_logs.json`
28. `id_sequences.json`
29. `transaction_journal.json`
30. `repository_manifest.json`
31. `system_settings.json`
32. `backup_catalog.json`
33. `repository_health_history.json`
34. `operational_report_runs.json`
35. `restore_history.json`

Every initial template uses schema version 1, revision 0, UTC metadata, a system actor marker, and an empty records array. The source-tree validator rejects missing, extra, malformed, or incorrectly named templates.

## Persistence contracts

### Repository envelopes

Every repository is represented by a versioned envelope containing:

- repository name;
- schema version;
- revision;
- creation and update UTC timestamps;
- updating actor ID;
- records collection.

`JsonRepositoryStore` validates repository identity, schema version, revision, and records before returning or publishing an envelope. Writes use expected-revision checks to reject stale mutations.

### Cross-process file locking

`CrossProcessFileLock` combines:

- a deterministic named Windows mutex derived from the canonical file path;
- a same-target `.lock` file opened with `FileShare.None`;
- bounded lock acquisition time;
- abandoned-mutex recovery.

Multi-file operations acquire canonical file paths in ordinal-insensitive sorted order to prevent inconsistent lock ordering.

### Hardened atomic writes

`AtomicFileWriter` performs:

1. same-directory unique temporary-file creation;
2. UTF-8 writing without a byte-order mark;
3. write-through and durable flush;
4. SHA-256 verification of staged bytes;
5. `File.Replace` for existing targets or atomic same-volume move for new targets;
6. temporary and backup cleanup.

A reader therefore sees either the previous complete repository file or the newly published complete repository file.

### Journaled multi-file transactions

`JournaledTransactionCoordinator`:

- rejects duplicate repository mutations;
- acquires all target and journal locks in canonical order;
- creates per-target rollback backups;
- writes Prepared and Applying journal states before publication;
- publishes each complete replacement through the atomic writer;
- records Committed after all target writes succeed;
- restores original files and records RolledBack on failure;
- supports recovery of an incomplete prepared or applying transaction.

This is a local shared-file transaction coordinator. It is not a distributed database transaction manager.

## Central identifier sequence allocation

`CentralIdSequenceService` stores the last allocated sequence for each prefix and year in `id_sequences.json`.

Allocation is:

- protected by the cross-process repository lock;
- incremented with checked arithmetic;
- atomically persisted before the identifier is returned;
- formatted through the canonical Domain `InstitutionIdentifier` contract;
- independent of repository record counts;
- never reused after successful persistence.

## Login attempts and lockout

The production security policy seeds:

- maximum failed attempts: 5;
- observation window: 15 minutes;
- lockout duration: 15 minutes;
- minimum password length: 12 characters;
- PBKDF2-HMAC-SHA256 iterations: 210,000.

`LoginAttemptService` records successful and failed attempts in `login_attempts.json`. Five qualifying failures after the latest successful login cause a temporary lockout. Successful authentication does not bypass an active lockout.

## Password hashing and sessions

Passwords are stored as salted PBKDF2-HMAC-SHA256 hashes. Verification uses fixed-time byte comparison.

Authentication creates persisted session records with:

- stable Session ID;
- user linkage;
- token hash representation;
- security-stamp snapshot;
- application kind;
- purpose;
- inactivity expiration;
- absolute expiration;
- lifecycle status.

## Forced first-login password change

The production bootstrapper does not contain a fixed administrator password. It requires an explicit caller-supplied initial password and administrator employee information.

The initial Administrator account:

- is linked to an Employee record;
- is active;
- stores only a salted credential hash;
- is marked `MustChangePassword = true`;
- receives only a restricted `FirstLoginPasswordChange` session after authentication.

Completing the forced password change atomically:

- replaces the credential hash;
- rotates the security stamp;
- clears the forced-change flag;
- revokes the restricted session;
- creates a full-access session;
- writes `users.json` and `sessions.json` through the multi-file transaction coordinator.

## Production bootstrap

`ProductionBootstrapper` requires an empty data directory and then:

1. creates all 49 authoritative JSON files;
2. seeds the security policy;
3. creates the first Employee and Administrator user from caller-supplied data;
4. marks the account for forced password change;
5. seeds `EMP` and `USR` identifier sequences so bootstrap IDs cannot be reallocated;
6. creates the repository manifest with file hashes and revisions;
7. verifies that exactly 49 JSON repository files exist.

Bootstrap is intentionally one-time and refuses to overwrite an existing production data directory.

## Infrastructure tests

`InfrastructureFoundationTests.cs` adds 14 tests covering:

- the 49-entry catalog and 14/35 split;
- required coordination and security repositories;
- creation of exactly 49 JSON files;
- absence of a built-in production credential;
- forced first-login password change;
- rejection of non-empty bootstrap directories;
- non-reusing central ID sequences;
- atomic replacement and temporary-file cleanup;
- optimistic revision conflicts;
- journaled two-repository commits;
- five-failure lockout;
- lockout expiry;
- restricted-session replacement with full access;
- old-password invalidation;
- salted PBKDF2-HMAC-SHA256 verification.

The expected complete suite contains 110 tests after Pass 8. No successful compilation or test claim is made until the final branch head completes the Windows workflow and produces TRX and artifact evidence.

## Figma architecture model

Editable FigJam architecture model:

- `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH`

The diagram is explanatory. GitHub source and automated evidence remain authoritative.

## Architecture boundary

- JSON and file-system operations exist only in `IUIS.Infrastructure`.
- Domain remains independent of Infrastructure.
- Forms remain prohibited from direct `System.IO` and `System.Text.Json` access.
- Application orchestration may consume Infrastructure contracts in later passes but is not completed here.

## Explicitly deferred

- complete typed repositories for every business aggregate;
- permission-profile evaluation and authorization orchestration;
- production login and password-change Forms;
- account-application approval workflows;
- notification dispatch;
- backup creation, restore execution, disaster recovery, and retention scheduling;
- deployment packaging and release certification.

## Current validation state

The Pass 8 production Infrastructure source, 49 templates, tests, validation rules, documentation, and Figma model are prepared on the implementation branch. Windows compilation and automated test evidence remain pending.
