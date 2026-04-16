# sts2-mods-by-kairong

A personal Slay the Spire 2 mod collection by kairong.

This repository is intended to host multiple mods over time.  
`SearingSwoop` is the first practice mod in the collection.

## Current Mods

### 1) SearingSwoop

Theme: a Searing Blow-style joke progression built on the Byrdonis Egg route.

Main gameplay loop:

- Adds two custom cards: `Searing Egg` and `Searing Swoop`.
- If unlocked, they are visible in the in-game compendium.
- When enabled, the vanilla `Byrdonis Egg` event path is replaced by this mod flow.
- You start runs with the egg card.
- Repeated campfire hatch actions repeatedly upgrade `Searing Swoop`.

Note:

- Starting with the egg and replacing the vanilla event will be exposed as config options in future updates.

## Installation (Players)

1. Download the latest release zip for your platform (`mac` or `win`).
2. Extract the `SearingSwoop` folder.
3. Copy it into the game's `mods` directory.
4. Launch the game and enable the mod.
5. Restart the game after changing mod config toggles.

## Development

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

- `dist/<version>-<timestamp>/SearingSwoop-<version>-mac.zip`
- `dist/<version>-<timestamp>/SearingSwoop-<version>-win.zip`

## Roadmap

- Add more STS2 experiment mods to this mono-repo.
- Add cleaner cross-mod shared build/release tooling.
- Add configurable toggles for starting card and event replacement behavior.
