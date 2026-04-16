# PROGRESS.md

Last updated: 2026-04-16

## Repo Status

This repo is positioned as a multi-mod `Slay the Spire 2` collection, but only `SearingSwoop` is currently active. The surrounding `_baselib/` and `_template/` directories are support material, not parallel feature work.

## Active Mod Snapshot

`SearingSwoop`

- Mod version: `v0.1.0`
- Target game version: `Slay the Spire 2 v0.99.1`
- Supported BaseLib versions: `3.0.3` and `3.0.5`
- Current status: playable prototype with release packaging

## Implemented

- Added custom cards `Searing Egg` and `Searing Swoop`.
- Injected `Searing Egg` into the starting deck when mod content is enabled.
- Blocked the vanilla `Byrdonis Nest` route when the run already starts with the egg.
- Added repeated campfire hatch flow through `HatchRestSiteOption`.
- Tracked hatch count on `RelicByrdpip` using saved fields.
- Granted Byrdpip companions based on hatch progression.
- Added repeated upgrade handling for `Searing Swoop`.
- Patched dynamic card titles and descriptions so they reflect current hatch state.
- Patched relic counter, title, description, and bird skin setup.
- Added runtime portrait loading so custom portraits can be loaded from disk reliably.
- Added release packaging script that emits zip bundles for both supported BaseLib versions.

## Partially Implemented Or Mismatched

- README roadmap says the mod should expose separate toggles for:
  - starting with the egg
  - replacing the vanilla egg event
- Actual code only exposes one config switch:
  - `EnableModContent`

- `Searing Swoop` is framed as repeated forging / scaling progression.
- Current method `GetSwoopDamageForUpgradeLevel()` returns a constant `14`.
  This means upgrade progression is currently reflected more in naming and flow than in raw damage scaling.

## Technical Debt

- `SearingSwoopPatches.cs` is carrying too many responsibilities:
  - state
  - gameplay patches
  - localization patching
  - upgrade orchestration
  - skin handling
  - save/load scope handling

- Build and publish depend on local machine setup:
  - STS2 install path discovery
  - Godot path availability for publish

- There is no automated regression coverage.
  Manual in-game verification is still the main safety net.

- Release packaging is matrix-labeled for `mac` and `win`, but the current script mostly packages the local build output into different bundle names.
  Treat this as packaging convenience, not as fully validated cross-platform export.

## Immediate Priorities

1. Split the single config toggle into two explicit settings:
   - start with egg
   - replace vanilla event
2. Decide the intended upgrade scaling for `Searing Swoop` and implement it in `GetSwoopDamageForUpgradeLevel()`.
3. Break `SearingSwoopPatches.cs` into smaller files by concern.
4. Document a reliable local dev setup for STS2 path overrides and Godot publish prerequisites.
5. Add a lightweight smoke-check routine for future changes:
   - build
   - start run
   - verify egg injection
   - verify hatch increments
   - verify swoop upgrade text and hit count

## Nice-To-Have Next

- Add clearer shared tooling for future sibling mods in this mono-repo.
- Add a second mod once the repo-level workflow has stabilized.
- Replace heavy runtime text patching with cleaner static localization where possible.
- Clarify art source and asset workflow around `extracted-card-art/`.

## Verification Baseline For Future Feature Work

When changing `SearingSwoop`, re-check at least:

- mod loads without dependency path errors
- starting deck behavior matches config
- vanilla Byrdonis event is allowed or blocked as intended
- hatch count persists and updates relic counter
- swoop upgrade level survives save/load
- card portrait fallback still works
- release bundles still generate under `SearingSwoop/dist/`
