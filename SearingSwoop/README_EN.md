# Searing Swoop

A Slay the Spire 2 gameplay mod inspired by STS1 `Searing Blow`.

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

## Release (mac + win)

Before publishing:

```bash
cd /Users/kairong/project/sts2-mods-by-kairong/SearingSwoop
./scripts/release_dual_platform.sh
```

The script outputs:

- `dist/<version>-<timestamp>/SearingSwoop-<version>-mac.zip`
- `dist/<version>-<timestamp>/SearingSwoop-<version>-win.zip`
