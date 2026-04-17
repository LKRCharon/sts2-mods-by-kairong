# Searing Swoop

A Slay the Spire 2 gameplay mod inspired by STS1 `Searing Blow`.

## Version & Compatibility

- Current mod version: `v0.1.1`
- Target game version: `Slay the Spire 2 v0.103.2`
- Supported BaseLib versions: `3.0.3` / `3.0.5`

## Dependency

- This mod requires `BaseLib` at runtime.
- Install a compatible `BaseLib` version before enabling `SearingSwoop`.
- The game's `mods` directory should contain both:
  - `BaseLib`
  - `SearingSwoop`

## What This Mod Adds

- Two custom cards:
- `Searing Egg`
- `Searing Swoop`
- If unlocked, both cards can be viewed in the in-game compendium.

- When the mod is enabled, the vanilla `Byrdonis Egg` event route is replaced by this mod flow.
- You start each run with `Searing Egg`.

- `Searing Egg` can be hatched repeatedly at campfires.
- Each hatch further upgrades `Searing Swoop`.
- In short: repeated hatching, repeated forging of `Searing Swoop`.

## Card Art

- The two active mod card portraits (`Searing Egg` and `Searing Swoop`) are GPT-generated.

## TODO

- Expose "start with egg" and "replace vanilla egg event" as separate config options.

## Installation

1. Install `BaseLib` first, using version `3.0.3` or `3.0.5`.
2. Put the extracted `SearingSwoop` folder into the game's `mods` directory.
3. Launch the game and enable both `BaseLib` and `SearingSwoop`.

## Release (mac + win)

Before publishing:

```bash
cd /Users/kairong/project/sts2-mods-by-kairong/SearingSwoop
./scripts/release_dual_platform.sh
```

The script outputs:

- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-win.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-win.zip`
