# IUIS Implementation State

Last updated: 2026-07-20

## Status vocabulary

The project distinguishes Specified, Created, Committed, Verified in GitHub, Compiled, Tested, Merged, and Release-certified states. These states are not interchangeable.

## Authoritative baseline

| Item | State | Evidence |
|---|---|---|
| Private GitHub repository | Created | `GitHubUser11-png/iuis` |
| Passes 0–7 | Completed, compiled, tested, merged, closure-validated, and synchronized | PRs #1–#26; Pass 7 final commit `8dae3e1f70ec41f8d51c3ac4cbc0af172dd3afcd` |
| Pass 8 final baseline | Promoted, exact-tree validated, and synchronized | `main` and `develop` at `55b69dcd0d2f82ec1cd6f6bba3db9b7f71ce320f` before Pass 9 |
| Pass 9 implementation | Validated and merged into `develop` | PR #35; integration commit `2b7b629889523a00d54d8e699f705e1ecc4f8358` |
| Pass 9 final implementation evidence | Successful | run `29715210764`; artifact `iuis-windows-build-evidence-107`; ID `8450333060`; SHA-256 `64525aa1f3c7b91362dbb91a1a4fb872248cbe95376663cc02ef0fa1f79f1a7c` |
| Pass 9 closure branch | Created from the integration commit | `build/pass-09-closure` |
| In-lock transaction revision gate | Created; closure validation pending | expected revisions are rechecked after canonical locks and before journal preparation |
| Aggregate mapper readiness catalog | Created; closure validation pending | 18 adapters classified; no false production-ready mapper claims |
| Pass 9 test count | Preserved at 127 | existing tests hardened for mapper classification and deterministic stale-stage concurrency |
| Production repository catalog and templates | Preserved | exactly 49 descriptors and 49 JSON files |
| Pass 9 Figma model | Created and being extended for closure | `https://www.figma.com/board/VGyuqaZDhIBfGqBfGjQJUH` |
| Executable certification | Not achieved | final release gate only |

## Locked target

C# 7.3; Windows Forms; .NET Framework 4.8; `System.Text.Json`; seven projects; separate User/Admin executables; shared synchronized JSON; exactly 49 authoritative JSON files; centralized IDs; journaled related mutations; no direct JSON/file access from Forms.

## Pass status

| Pass | Scope | Status |
|---:|---|---|
| 0–7 | Repository, solution, Domain, Academic, Finance, Student Service foundations | Completed, compiled, tested, merged, synchronized |
| 8 | Production repository and security bootstrap | Completed, validated, closure-validated, promoted, exact-tree validated, synchronized |
| 9 | Authorization, typed adapters, restricted projections, session-aware orchestration | Implemented and merged; closure hardening created; closure validation, promotion, exact-mainline validation, and synchronization pending |
| 10+ | Business UI, specialized persistence mappers, operations, deployment, certification | Not started |

## Current truthful completion statement

Pass 9 is integrated into `develop` at `2b7b629889523a00d54d8e699f705e1ecc4f8358`. The closure branch hardens Application transaction concurrency by rechecking staged expected revisions while canonical locks are held, and it classifies all 18 aggregate adapters by actual mapper readiness. These closure changes are created but are not yet compiled, tested, merged, or promoted.

Complete specialized JSON mappers for every evolved Domain aggregate shape, production Forms, trusted-device/network enforcement, backup/restore execution, deployment, and release certification remain incomplete.

## Exact next gate

Validate `build/pass-09-closure`, merge it into `develop`, promote to `main`, validate the exact mainline tree, synchronize `develop`, verify zero divergence, and then define the Pass 10 boundary.
