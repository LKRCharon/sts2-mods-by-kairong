# sts2-mods-by-kairong

A personal Slay the Spire 2 mod collection by kairong.

This repository is intended to host multiple mods over time.  
`SearingSwoop` is the first practice mod in the collection.

## Current Mods

### 1) SearingSwoop

Theme: a Searing Blow-style joke progression built on the Byrdonis Egg route.

Compatibility:

- Mod version: `v0.3`
- Game version: `Slay the Spire 2 v0.103.2`
- BaseLib: `3.0.3` / `3.0.5`

Dependency:

- `SearingSwoop` requires `BaseLib` at runtime.
- Install the matching `BaseLib` release in the game's `mods` folder before enabling this mod.

Main gameplay loop:

- Adds two custom cards: `Searing Egg` and `Searing Swoop`.
- If unlocked, they are visible in the in-game compendium.
- When enabled, the vanilla `Byrdonis Egg` event path is replaced by this mod flow.
- You start runs with the egg card.
- Repeated campfire hatch actions repeatedly upgrade `Searing Swoop`.

Note:

- Starting with the egg and replacing the vanilla event will be exposed as config options in future updates.

## Installation (Players)

This mod does not run standalone. `BaseLib` must already be installed in the game's `mods` directory.

1. Install `BaseLib` first, using a version compatible with this mod: `3.0.3` or `3.0.5`.
2. Download the latest `SearingSwoop` release zip for your platform (`mac` or `win`).
3. Extract the `SearingSwoop` folder.
4. Copy it into the game's `mods` directory alongside `BaseLib`.
5. Launch the game and enable the mod.
6. Restart the game after changing mod config toggles.

## Development

The active mod currently depends on the `Alchyr.Sts2.BaseLib` NuGet package and on `BaseLib` being present as a runtime mod dependency.

Project structure currently includes:

- `SearingSwoop/` (active mod)
- `_baselib/` (BaseLib source and docs mirror)
- `_template/` (mod template resources)

## Release Build (mac + win)

From the `SearingSwoop` directory:

```bash
./scripts/release_dual_platform.sh
```

It generates:

- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-win.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-win.zip`

## Roadmap

- Add more STS2 experiment mods to this mono-repo.
- Add cleaner cross-mod shared build/release tooling.
- Add configurable toggles for starting card and event replacement behavior.
