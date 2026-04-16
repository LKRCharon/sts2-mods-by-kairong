# sts2-mods-by-kairong

这是 `kairong` 的一个《杀戮尖塔 2》个人模组合集仓库。

这个仓库的定位不是只放一个 mod，而是准备长期收纳多个 STS2 练手模组、实验模组和一些偏趣味性的想法。

目前已经落地并可玩的第一个模组是：`SearingSwoop`。

## 当前收录模组

### 1) SearingSwoop（灼热鸟蛋 / 灼热扑击）

这是一个基于原版 `Byrdonis Egg` 路线改出来的趣味模组，整体灵感来自 STS1 里的 `灼热打击`。

当前兼容信息：

- 模组版本：`v0.1.0`
- 适配游戏版本：`Slay the Spire 2 v0.99.1`
- 支持 BaseLib：`3.0.3` / `3.0.5`

依赖说明：

- `SearingSwoop` 运行时依赖 `BaseLib`
- 启用本模组前，需要先把对应版本的 `BaseLib` 放进游戏的 `mods` 目录

核心玩法：

- 新增两张自定义卡牌：`Searing Egg` / `Searing Swoop`
- 开启模组后，这两张卡会进入对应的模组玩法链路
- 开局会直接获得 `Searing Egg`
- 原版 `Byrdonis Egg` 事件路线会被这个模组流程接管，不再出现
- 可以在篝火处反复“孵化”鸟蛋
- 每次孵化都会继续强化 `Searing Swoop`

简单理解就是：

“从一次拿蛋，改成一整局围绕鸟蛋不断孵化、不断强化扑击的梗玩法。”

更详细的中文说明可以看：

- [SearingSwoop/README.md](/Users/kairong/project/sts2-mods-by-kairong/SearingSwoop/README.md)

## 给玩家的安装说明

这个模组不是独立运行的，必须先安装 `BaseLib`。

1. 先安装 `BaseLib`，版本建议与本模组兼容范围一致：`3.0.3` 或 `3.0.5`
2. 下载与你平台对应的 `SearingSwoop` 最新发布包，通常是 `mac` 或 `win` 版本
3. 解压后得到 `SearingSwoop` 文件夹
4. 把它复制到游戏的 `mods` 目录，并确保和 `BaseLib` 放在一起
5. 启动游戏并启用模组
6. 如果你改了模组配置，建议重启游戏后再测试

## 给开发者的仓库结构说明

当前活跃模组在开发层面同样依赖 `Alchyr.Sts2.BaseLib`，既有 NuGet 包依赖，也有运行时 mod 依赖。

目前仓库主要分成这几部分：

- `SearingSwoop/`
  当前正在开发的主模组
- `_baselib/`
  BaseLib 相关源码 / 参考内容镜像
- `_template/`
  后续新建模组时可以参考的模板资源
- `extracted-card-art/`
  一些卡图提取与编辑相关的工作区

如果你只是想看当前模组代码，主要关注：

- `SearingSwoop/SearingSwoopCode/`
- `SearingSwoop/SearingSwoop/`
- `SearingSwoop/scripts/release_dual_platform.sh`

## 发布构建

在 `SearingSwoop/` 目录下执行：

```bash
./scripts/release_dual_platform.sh
```

脚本会生成：

- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.3-win.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-mac.zip`
- `dist/<version>/SearingSwoop-<version>-baselib-3.0.5-win.zip`

## 这个仓库接下来会做什么

- 继续完善 `SearingSwoop`
- 把“开局给鸟蛋”和“是否替换原版事件”拆成更细的配置项
- 逐步整理出更干净的共享构建 / 发布流程
- 以后继续往这个 mono-repo 里加更多 STS2 小模组

## 说明

- 这个仓库目前更偏个人开发与实验性质
- 有些实现还在快速迭代中，文档和配置会继续补齐
- 如果你是来围观的朋友，优先看 `SearingSwoop` 就够了，它是当前唯一的活跃模组
