# Searing Swoop（灼热鸟蛋）

一个基于《杀戮尖塔 2》多尼斯异鸟蛋路线的趣味模组，致敬 STS1 的 `灼热打击`。

## 版本与兼容

- 当前模组版本：`v0.1.1`
- 适配游戏版本：`Slay the Spire 2 v0.103.2`
- 可选 BaseLib 版本：`3.0.3` / `3.0.5`

## 依赖

- 本模组运行时依赖 `BaseLib`
- 安装本模组前，请先安装兼容版本的 `BaseLib`
- 游戏的 `mods` 目录里需要同时存在：
  - `BaseLib`
  - `SearingSwoop`

## 模组效果

- 加载模组后，游戏中新增两张卡：
- `灼热鸟蛋`
- `灼热扑击`
- 这两张卡可以在百科中查看（前提是已解锁）。

- 启用 mod 后，原版 `多尼斯异鸟蛋` 事件不会再出现。
- 开局会直接获得 `灼热鸟蛋`。

- `灼热鸟蛋` 可以在篝火反复孵化。
- 每次孵化都会继续强化 `灼热扑击`。
- 即：多次孵化，多次锻造 `灼热扑击`。

## 卡图说明

- 当前两张 mod 卡图（`灼热鸟蛋`、`灼热扑击`）由 GPT 生成。

## TODO

- 将“开局获得鸟蛋”与“是否屏蔽事件获得鸟蛋”做成可选配置项。

## 安装

1. 先安装 `BaseLib`，版本使用 `3.0.3` 或 `3.0.5`
2. 再把 `SearingSwoop` 解压后的文件夹放进游戏 `mods` 目录
3. 启动游戏后同时启用 `BaseLib` 和 `SearingSwoop`

## 发布（mac + win）

发布前统一执行：

```bash
cd /Users/kairong/project/sts2-mods-by-kairong/SearingSwoop
./scripts/release_dual_platform.sh
```

脚本会生成：

- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-win.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-win.zip`
