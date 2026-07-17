# IUIS Build Sequence

The source implementation is created incrementally. A later pass may depend on an earlier pass, but no pass is treated as complete merely because its specification exists.

## Pass 0 — Repository baseline

Create the initial `main` history, governance files, implementation-state registry, decision register, Git workflow, and `develop` branch.

**Gate:** Repository is visible, writable, refetched, and branch references are verified.

## Pass 1 — Visual Studio solution foundation

Create `IUIS.sln`, central build properties, seven project files, assembly metadata, minimal startup files, project references, and application configuration placeholders.

**Gate:** Solution and project graph are structurally valid.

## Pass 2 — Windows build and CI foundation

Create Windows GitHub Actions workflows and deterministic build scripts for NuGet restore, Release MSBuild, test discovery, TRX output, logs, and artifacts.

**Gate:** CI starts and reports an evidence-backed build result.

## Pass 3 — Domain foundations

Create `EntityBase`, value objects, canonical enums, institutional date/time primitives, money utilities, and identity-independent Domain foundations.

**Gate:** `IUIS.Domain` compiles without Infrastructure or WinForms references.

## Pass 4 — Core Domain aggregates

Implement Users, Students, Employees, academics, finance, Library, appointments, Counseling, Discipline, Clinic, Workforce, notifications, and operations aggregates in controlled subpasses.

**Gate:** Domain invariants and serialization-facing constructors are covered by tests.

## Pass 5 — Application foundations

Implement operation results, validation, paging, session credentials, access contexts, permission and page catalogs, DTO foundations, and service contracts.

**Gate:** `IUIS.Application` compiles with a Domain reference only.

## Pass 6 — Production repository catalog

Implement repository definitions, storage kinds, identifier policies, generic envelopes, structural-document contracts, and the exact 49-file catalog.

**Gate:** Catalog count, names, mappings, and schema metadata pass tests.

## Pass 7 — Repository templates

Create the 49 initial JSON files only after the contracts exist.

**Gate:** Every template parses through the production serializer and matches its catalog definition.

## Pass 8 — Persistence engine

Implement serializer options, path guards, generic reads, durable writes, hashes, initialization validation, manifests, and atomic replacement support.

**Gate:** Repository unit and fault tests pass.

## Pass 9 — Locking, transactions, and recovery detection

Implement cross-process locks, deterministic order, transaction participants, staging, rollback, journal states, coordinator logic, and startup recovery detection.

**Gate:** concurrency and fault-injection tests produce no false success.

## Pass 10 — Authentication and security

Implement password hashing, Login attempts, lockout, sessions, temporary credentials, forced password change, permissions, trusted devices, IPv4 rules, and production bootstrap.

**Gate:** security tests and restricted-session routing pass.

## Pass 11 — Shared Windows Forms framework

Implement base Forms and controls, navigation and page registries, DataGridView standards, validation presentation, busy states, paging, debounce, and session/runtime monitoring.

**Gate:** `IUIS.SharedUI` compiles without repository access.

## Pass 12 — Application startup

Implement User and Administrator application contexts, initialization, bootstrap, Login, forced password change, maintenance, and recovery routing.

**Gate:** both executables launch through their intended boundaries.

## Pass 13 — Administrator workspace

Implement account review, approval, identity issuance, accounts, permissions, password resets, sessions, Login Activity, trusted devices, network rules, and Security Policy.

## Pass 14 — Public and Student core workspace

Implement public applications and tracking, Student onboarding, Dashboard, Profile, Enrollment, Subjects, Assessments, Payments, Scholarship, and Notifications.

## Pass 15 — Student services

Implement Student Library views, appointments, released Counseling summaries, Medical Records, Consultations, Clearances, Violations, and responses.

## Pass 16 — Employee service operations

Implement Library inventory and Borrowings, Counseling operations, Discipline cases, Clinic operations, Clearance issuance, and confidential access controls.

## Pass 17 — Workforce

Implement HR records, employment lifecycle, Work Schedules, Attendance, corrections, Faculty assignments, conflict validation, and Employee self-service.

## Pass 18 — Registrar

Implement Student master records, Courses, curricula, Subjects, prerequisites, Academic Periods, Enrollments, Profile Corrections, and academic integrity validation.

## Pass 19 — Finance

Implement Charge Rules, Tuition Assessments, Adjustments, Payments, receipts, voids, reversals, Scholarship operations, derived ledgers, and financial integrity.

## Pass 20 — Operations

Implement repository health, Audit review, Backup, Restore, Recovery, Settings, reports, deployment validation, and certification evidence.

## Pass 21 — Automated test expansion

Complete architecture, Domain, serialization, persistence, security, module, concurrency, fault, Backup, Restore, and Recovery test suites.

## Pass 22 — Release hardening

Resolve Release warnings, binding redirects, resources, Designer consistency, dependency versions, Windows runtime smoke tests, artifact hashing, and package construction.

## Pass 23 — Final integration and certification evidence

Execute final builds, tests, Backup and Restore drills, Recovery drills, end-to-end workflows, evidence packaging, and truthful certification evaluation.

## Execution rule

Each pass records its starting branch and commit, files written, resulting commit, refetched verification, pull-request state, build state, test state, unresolved items, and exact continuation point.
