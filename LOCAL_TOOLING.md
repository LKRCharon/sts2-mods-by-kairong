# Local Tooling (Not Committed)

This repo supports local reverse-engineering tooling without polluting git history.

## Policy

- Install local runtimes/tools under `.tools/`.
- Keep local study data under `local-mod-study/`.
- Both are git-ignored by default.

## ilspycmd + .NET 10 Runtime

`ilspycmd 10.x` needs a `.NET 10` runtime.

1. Install runtime to repo-local path:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --runtime dotnet --version 10.0.6 --install-dir ./.tools/dotnet10
```

2. Install `ilspycmd` to repo-local path:

```bash
./.dotnet/dotnet tool install --tool-path ./.tools/ilspycmd-local ilspycmd
```

3. Run `ilspycmd` with local runtime:

```bash
DOTNET_ROOT="$PWD/.tools/dotnet10" \
DOTNET_ROOT_ARM64="$PWD/.tools/dotnet10" \
"$PWD/.tools/ilspycmd-local/ilspycmd" --version
```

4. Example decompile command:

```bash
DOTNET_ROOT="$PWD/.tools/dotnet10" \
DOTNET_ROOT_ARM64="$PWD/.tools/dotnet10" \
"$PWD/.tools/ilspycmd-local/ilspycmd" -p -o ./local-mod-study/decompile/<mod-name> <mod.dll>
```

## Notes

- If the tool install fails once with a `.store/.stage` path error, remove `./.tools/ilspycmd-local` and retry.
- This repo already has a tracked shim at `./.tools/ilspycmd`; do not replace it with a directory.
- Keep release assets and downloaded third-party mods inside `local-mod-study/`.
