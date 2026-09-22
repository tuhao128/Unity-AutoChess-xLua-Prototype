# Unity AutoChess xLua Prototype

> 一个 **Unity + xLua 的自走棋原型**，用来验证两件事：
>
> 1. 用 **Lua 做「数据 + 行为」的脚本层**——单位在 Lua 里定义、由 C# 侧实例化，实现热更 / 配置驱动；
> 2. 一套**树状 + 栈式调度的回合阶段机**（像炉石 / 万智那种 Enter → Draw → Play → Battle 的回合结构）。
>
> 它是一个**早期的架构实验**：核心机制跑通，但内容基本是骨架。

---

## 项目状态

> ⚠️ **这是所有项目里最早、最不完整的一个，请作为「补充技能点」看待，不要当作主项目。**

- **明显是早期骨架**：
  - `card_GenerateSolider.lua`、`LuaRegisters.cs`、`CsharpCallLuaAPIs.cs` 是**空文件 / 空类**；
  - `TestEnter.cs` 全是 `NotImplementedException` 占位；
  - `AssetBundleLoader` 类名是 AB Loader，**但没真正加载 AssetBundle**——只是序列化字段 + `switch`。`Res/` 下只有一个 `Card_Heart2.prefab`，也没有任何加载器去读它。
- **大量半成品与残留**：`GameManager` 里大段注释掉的旧阶段树、`Log()` 调试打印、`Chess` / `Card` 里注释掉的 `FixedUpdate` / 刚体逻辑。
- **编码问题**：部分注释是 GBK 编码，UTF-8 下显示乱码。
- **缺工程化**：无单元测试、无构建配置、无性能剖析。

**它证明的是**：碰过 xLua 热更、做过数据驱动、搭过回合阶段机——这三样是「会做游戏框架」的补充证据。尤其是 **Lua 热更**这条，是这个项目独有的技能点。

---

## 技术栈

| 项 | 内容 |
|---|---|
| 引擎 | Unity **2022.3.57f1c1**（Unity 2022.3 LTS，中国版） |
| 渲染 | URP 14.0.11 |
| 语言 | C#（约 20 个自写脚本）+ Lua（3 个自写文件，`Assets/LuaFiles/`） |
| 脚本层 | Tencent **xLua**（`LuaEnv`、`[LuaCallCSharp]` / `[CSharpCallLua]` 绑定） |
| 玩法 | 自走棋 / 卡牌 + 棋盘：选卡 → 下棋 → 按速度结算战斗，回合分阶段推进 |

**一句话说明玩法**：回合制自走棋——每回合按「准备 → 抽卡 → 出牌 → 战斗」四个阶段推进；卡牌从牌库抽到手牌，打出到棋盘生成棋子（Chess），战斗阶段所有棋子按「速度」从低到高依次结算行动。

---

## 架构总览

```
GameManager.Start()
  ├─ LuaDataFactory.GenerateData()  ── 跑 Lua（DataGenerator.lua）
  │      └─ require 各棋子/卡牌 .lua → LuaAPIs.RegisterChessBy(table)
  │             └─ 生成 ChessConfig/CardConfig → 工厂实例化 GameObject
  └─ PhaseManager.InitEnterPhase( 嵌套 Phase 树 )
         └─ FixedUpdate 里栈式调度：Enter→Prepare→Draw→Play→Battle
```

| 系统 | 职责 |
|---|---|
| **`LuaSystem/`** | xLua 环境（`LuaEnvManager`）+ C# 暴露给 Lua 的 API（`LuaAPIs`）+ 数据工厂（`LuaDataFactory`） |
| **`CardSystem/`** | 卡牌区抽象（`GameZone` 模板方法 + `GameZoneType`：手牌 / 牌库 / 弃牌 / 空中） |
| **`ChessSystem/`** | 棋盘（`GameSceneController`）+ 棋子（`Chess`）+ 行动结算（`SceneAsk` / `ActionMessage`） |
| **`PhaseSystem/`** | 回合阶段（`Phase` + `PhaseManager` 栈调度） |
| **`PlayerSystem/`** | 玩家状态机（`PlayerState`）+ 输入层（`PlayerController`） |
| **`AssetsSystem/`** | 资源加载（`AssetBundleLoader`） |

---

## 架构设计

### 1. xLua 脚本层 + 双向绑定（本项目独有的技能点）

把「单位 / 卡牌是什么、有什么行为」交给 Lua 描述，C# 只负责解析和实例化——实现**热更 + 配置驱动 + 逻辑脚本化**。

- **自定义 Loader**：`LuaEnvManager` 单例持有 `LuaEnv`，`AddLoader(CustomLoader)`，`CustomLoader` 用 `File.ReadAllBytes(Application.dataPath + "/LuaFiles/" + filename + ".lua")` 从磁盘读脚本——**不打包进 Resources，改 .lua 就能热更**。`RunFile(name)` → `DoString($"require '{name}'")`。
- **双向绑定**：
  - **C# → Lua**：`LuaAPIs` 打 `[LuaCallCSharp]`，静态方法 `RegisterChessBy(LuaTable)` / `RegisterCardBy(LuaTable)` / `MoveTo` / `PosTowards`。Lua 里 `LuaAPIs = CS.LuaAPIs` 直接调。
  - **Lua → C#**：委托 `AskedAction`、`CardEventHandler` 打 `[CSharpCallLua]`，Lua 函数被包成 C# 委托，存进 `ChessConfig` / `CardConfig`，后续由 C# 侧调用。
- **数据流**：`GameManager.Start` → `LuaDataFactory.GenerateData()` → `require "DataGenerator"` → Lua 里遍历 `chessRegisters` / `cardRegisters` 逐个 `require` 并 `LuaAPIs.RegisterChessBy(datas)`。

> 完整跑通了一次「C# ↔ Lua」双向调用——自定义 Loader 做热更、`[LuaCallCSharp]` / `[CSharpCallLua]` 属性桥接、Lua 函数当委托存进 C# 配置对象。

**代码**：`Assets/Scripts/Runtime/GameSystem/LuaSystem/LuaEnvManager.cs`、`LuaAPIs.cs`、`LuaDataFactory.cs`、`Assets/LuaFiles/DataGenerator.lua`、`chess_SoliderCharactor.lua`

### 2. 数据驱动的单位定义 — Lua table → C# config → 工厂

一个棋子 / 卡牌 = 一张 Lua 表（数据 + 行为函数），由 `LuaDataFactory` 转成 C# 配置对象，再由工厂拼出 GameObject。

- `chess_SoliderCharactor.lua` 是一张 Lua 表：`name` / `movingSpeed` / `material` / `model` / `x`/`y`/`z` + `skill` / `attack` / `collect` 三个函数。其中 `collect` 里回调 C#：`LuaAPIs.MoveTo(msg, LuaAPIs.PosTowards(msg, 0, 1, 1))`——**Lua 直接操控 C# 棋子移动**。
- `LuaAPIs.RegisterChessBy(table)` 从表里 `table.Get<...>()` 取字段，组一个 `ChessConfig`（`ModelPack` + `AskedAction`×3 + `movingSpeed` + `firstPos`），`table.Dispose()` 后交给 `LuaDataFactory`。
- `LuaDataFactory.CreateChessAt()`：`new GameObject` + `AddComponent<MeshFilter / MeshRenderer / Chess>`，把 config 填进 `Chess`（`InitActions` 绑三个行为 + 移动），`PutChessOn` 落格。
- `DataGenerator.lua` 里还有个 `metaGenerateTable`（`__index` 提供 name / HP / range / material / model 等默认值），体现「缺省字段」的思路。

> 单位是「数据 + 函数」的脚本表，C# 侧是纯数据 config + 工厂。加一个新棋子 = 加一个 `.lua` 表 + 在 `chessRegisters` 里登记名字，主流程零改动——和其他项目「数据驱动」一脉相承，只是数据载体换成了 Lua。

**代码**：`Assets/LuaFiles/chess_SoliderCharactor.lua`、`Assets/Scripts/Runtime/GameSystem/LuaSystem/LuaDataFactory.cs`

### 3. 树状 + 栈式调度的回合阶段机

回合不是一段顺序代码，而是一棵「阶段树」（Enter → Prepare → Draw → Play → Battle，每个又含 Enter / In / Exit 子阶段），用**栈**驱动执行，用「回合计数」标记每个阶段在某一回合是否已完成。

- `Phase` 是节点：`name` + `needData` + `switchFunc`（→ `PlayerState`）+ `actFunc` + 子阶段 `Sons`。构造函数 `params Phase[]` 直接写出嵌套树。
- `PhaseManager` 持 `Stack<Phase> _phaseEvents` 和 `_time`（回合计数）。`PushPhases()` **递归压入最深未完成的子阶段**，保证从叶节点开始执行。
- `FixedUpdate` 驱动：当前阶段 `Act()` 返回 `false` 即「完成」→ 弹栈、把收集到的数据上抛父阶段（`CollectFromSon`）、找下一个未完成兄弟；整棵完成则 `_time++` 开启新一轮，重新压根节点。
- `Finished(num)` / `Finish(num)` 用 `int finished` 标记「第 num 回合已完成」。

> ① **结构是树、执行是栈**——嵌套声明 + 栈式遍历，天然支持「子阶段插队、数据自底向上汇总」；② **回合计数**——每个阶段记住自己在哪一回合完成过，避免同回合重复触发。这是自走棋 / 卡牌类「回合结构」的通用解法。

**代码**：`Assets/Scripts/Runtime/GameSystem/PhaseSystem/Phase.cs`、`PhaseManager.cs`、`Assets/Scripts/Runtime/GameSystem/GameManager.cs`

### 4. 按速度排序的行动结算

战斗阶段里所有棋子各自提交「行动」，按**速度从低到高**依次结算（自走棋的先后手 / 先手值）。

- `GameSceneController : IGameScene` 持 `GridBlock[,]` 棋盘 + `Dictionary<int, Stack<SceneAsk>> _speed_Actions`（**按速度分桶的栈**）。
- `CollectDataForPhase()`：遍历所有格子，让每个棋子 `CollectAction`，把行动 `AddAction` 压进对应速度的桶。
- `Act()` → `ActActions()` 枚举器：从速度 0 到 `maxSpeed` 逐个桶弹栈执行，每个行动 `while (@continue) { @continue = result.Act(); yield return @continue; }`——**行动可挂起 / 续跑**（协程式推进）。
- `Chess` 是棋子：`PutChessOn` 占格（DOTween 移动），`InitActions` 绑 `attack` / `skilled` / `collect` / `move` 四个 `AskedAction`，`CollectAction` 提交收集行动。

> ① 用「速度分桶 + 栈」做**先手排序**，执行顺序清晰；② 行动用委托 + 枚举器驱动，可 `yield` 挂起、可逐帧推进。

**代码**：`Assets/Scripts/Runtime/GameSystem/ChessSystem/GameSceneController.cs`、`Chess.cs`

### 5. 卡牌区系统 + 资源加载预留

卡牌在「牌库 / 手牌 / 弃牌 / 空中」等区域间流转，用统一的「区」抽象 + 模板方法管理；模型 / 材质用「名字 → 资源」的加载层统一取。

- **卡牌区**：`GameZone` 抽象基类用模板方法约定（`keepWorldPos` / `MaxCount` / `HasMaxCount` / `_type` / `CalculatePos` / `AfterAdd` / `AfterInMax` / `AfterRemove`），`Deck` / `HandZone` / `AirZone` 子类只填差异；`GridForCard` 管理器 `ReadZone(type)` / `AddCard` / `RemoveCardTo` / `DrawCard`。`Card` 实现拖拽（`IPointerEnter` / `Exit` / `Drag` / `EndDrag`）+ `MoveTo(zone)` + `Play()`。
- **资源加载预留**：`AssetBundleLoader.CollectPack(modelName, materialName)` 把字符串名映射成 `ModelPack`（Mesh + Material）——但目前是 `switch` default 返回序列化字段 `pawnMesh` / `pawnMaterial`，**只搭了「按名寻址」的抽象，没真正接 AssetBundle**。

> **资源这块要分两层说**：能证明的是 ① 写过真正的自定义加载入口（`LuaEnvManager.AddLoader` + `File.ReadAllBytes`，xLua 热更就靠它）；② 理解「按名寻址、资源来源对上层透明」的原理。**不能证明的是**：没写过 `AssetBundle.LoadFromFile` / `LoadAsset`，没做过 AB 打包、依赖、异步、卸载。

**代码**：`Assets/Scripts/Runtime/GameSystem/CardSystem/GridForCard.cs`、`Assets/Scripts/Runtime/GameSystem/AssetsSystem/AssetBundleLoader.cs`

---

## 设计理念

1. **逻辑脚本化** —— 单位 / 卡牌的行为放 Lua 表，C# 侧只做解析、实例化、调度；
2. **回合阶段树 + 栈调度** —— 回合结构用嵌套数据声明、用栈执行、用计数去重；
3. **行动按速度排序、协程式结算** —— 先手分桶 + 委托 + 枚举器推进；
4. **资源按名寻址** —— 业务只认名字，资源来源（序列化字段 / 将来 AssetBundle）对上层透明。

核心目标：验证「**Lua 热更 + 数据驱动 + 回合阶段机**」这条技术路线能不能跑通。

---

## 目录结构

```
Assets/
├── Scenes/
│   └── SampleScene.unity           # 唯一场景
├── Scripts/
│   ├── Runtime/
│   │   ├── GameManager.cs
│   │   └── GameSystem/
│   │       ├── LuaSystem/          # xLua 环境 + LuaAPIs + 数据工厂
│   │       ├── CardSystem/         # 卡牌区（GameZones）
│   │       ├── ChessSystem/        # 棋盘 + 棋子 + 行动结算
│   │       ├── PhaseSystem/        # 回合阶段树 + 栈调度
│   │       ├── PlayerSystem/       # 玩家状态机 + 输入
│   │       └── AssetsSystem/       # 资源加载（抽象层）
│   ├── Tools/
│   └── View/
├── LuaFiles/                       # 自写 Lua：DataGenerator / chess_ / card_
├── XLua/                           # xLua 框架（Tencent，MIT）
├── SubstanceAssets/                # 棋盘棋子美术（见下方说明）
├── Res/                            # Card_Heart2.prefab
└── Settings/                       # URP 渲染配置
Packages/manifest.json
ProjectSettings/
```

---

## 如何运行

1. 安装 **Unity 2022.3.57f1c1**（Unity 中国版；其他 2022.3 LTS 小版本通常也可，Unity 会提示升级）。
2. 用 Unity Hub 添加本仓库根目录并打开。首次打开会重新生成 `Library/`，需要几分钟。
3. 打开 `Assets/Scenes/SampleScene.unity`，按 Play 运行。

> **关于美术资源**：源工程的 `Assets/SubstanceAssets/LittleGamesPack/` 有 279 个文件、共 228 MB，但经引用图遍历（`SampleScene` → `fbx` / `mat` → 贴图），只有 **10 个文件**真正参与渲染。仓库只纳入这 10 个（39.9 MB），其余未提交。**不影响项目运行。**
>
> **关于音频**：源工程未包含音频资源。

---

## 第三方依赖

| 依赖 | 用途 | 来源 |
|---|---|---|
| [xLua](https://github.com/Tencent/xLua) | Lua 脚本层 / 热更 | 已内置于 `Assets/XLua/`（Tencent，MIT） |
| [DOTween](http://dotween.demigiant.com/) | 棋子落格 / 移动补间 | 已内置于 `Assets/Plugins/Demigiant/` |
| Universal RP 14.0.11 | 渲染管线 | Unity Package Manager |
| TextMesh Pro 3.0.7 | 文本渲染 | Unity Package Manager |
| Timeline 1.7.6 | 时间轴 | Unity Package Manager |

`Packages/manifest.json` 中列出了全部包依赖，Unity 打开工程时会自动还原。
