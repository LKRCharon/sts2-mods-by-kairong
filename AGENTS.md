# AGENTS.md

## Purpose

This repository is a long-lived mono-repo for `Slay the Spire 2` mods by `kairong`.

- The active mod today is `SearingSwoop/`.
- Future mods may be added at the repo root as sibling directories.
- `_baselib/` and `_template/` exist to support development, but they are not the main feature surface of this repo.

Default assumption for future work: if a task does not explicitly say otherwise, work in `SearingSwoop/`.

## Repo Map

- `README.md`: repo-level positioning, release notes, and high-level roadmap.
- `SearingSwoop/`: active gameplay mod.
- `SearingSwoop/SearingSwoopCode/`: C# gameplay logic and Harmony patches.
- `SearingSwoop/SearingSwoop/`: runtime assets, portraits, and localization JSON.
- `SearingSwoop/scripts/release_dual_platform.sh`: release packaging entrypoint.
- `_baselib/`: BaseLib source mirror / reference area. Treat as upstream reference unless a task explicitly targets it.
- `_template/`: template resources for future mods. Do not casually edit when working on `SearingSwoop`.
- `extracted-card-art/`: source art workspace, not core runtime logic.

## Current Mod Architecture

`SearingSwoop` is currently organized around a small set of files:

- `MainFile.cs`: mod initialization, config registration, Harmony bootstrap.
- `SearingSwoopPatches.cs`: most gameplay behavior.
  It currently contains state tracking, run-start injection, event suppression, hatch flow, Byrdpip scaling, dynamic card/relic text, and serialization-related upgrade handling.
- `Cards/`: thin card model classes.
- `Config/SearingSwoopConfig.cs`: mod config surface.
- `Utils/CardPortraitLoader.cs`: runtime portrait loading fallback.

Implication: before adding new behavior, check whether it truly belongs in the existing patch file or whether it should be split into a new focused file.

## Working Rules

- Keep gameplay-source edits under `SearingSwoop/SearingSwoopCode/`.
- Keep runtime assets under `SearingSwoop/SearingSwoop/`.
- Keep user-facing documentation aligned across:
  - `README.md`
  - `SearingSwoop/README.md`
  - `SearingSwoop/README_EN.md`
- When changing visible card or relic behavior, update both code and player-facing docs.
- When adding new player-facing strings, decide whether they are:
  - static text that belongs in localization JSON, or
  - dynamic text that must stay patched at runtime.
- Preserve support for BaseLib `3.0.3` and `3.0.5` unless the task explicitly changes the compatibility target.
- Avoid editing `_baselib/` or `_template/` as part of normal mod work.
- Do not commit release artifacts or editor/build outputs.

## Build And Release

Development and release work currently assume local STS2 and Godot paths are discoverable.

Important files:

- `SearingSwoop/SearingSwoop.csproj`
- `SearingSwoop/Sts2PathDiscovery.props`
- optional local overrides such as `local.props` or `Directory.Build.props`

Typical local build from `SearingSwoop/`:

```bash
../.dotnet/dotnet build SearingSwoop.csproj -c Debug
```

If repo-local `dotnet` is not present, the scripts fall back to system `dotnet`.

Release packaging from `SearingSwoop/`:

```bash
./scripts/release_dual_platform.sh
```

This produces zip bundles under `SearingSwoop/dist/<version>/`.

## Change Priorities

When making feature changes, prefer this order:

1. Keep gameplay behavior correct.
2. Keep save/load and upgrade state stable.
3. Keep localization and card text consistent with behavior.
4. Keep release packaging working.
5. Only then clean up structure or logs.

## Known Constraints

- There is no automated test suite in this repo today.
- Verification is primarily manual or build-based.
- `SearingSwoopPatches.cs` is the main concentration point for technical debt.
- Current config surface is smaller than the README roadmap suggests.

## Done Criteria For Future Tasks

A change is not complete until all applicable items below are true:

- The relevant mod builds successfully.
- Any new or changed user-facing behavior is reflected in README docs.
- Localization and card/relic descriptions still match actual gameplay.
- Release or runtime asset paths still resolve correctly.
- `PROGRESS.md` is updated if the task changes status, priorities, or known debt.
