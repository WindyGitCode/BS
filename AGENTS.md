# BS 项目协作与版本控制约定

> 本文件是**强制约定**。任何在本仓库中修改文件的操作（包括 AI 助手）都必须遵守。

## 一、首要规则：改完必须立即提交

**每次对项目做出改动后，必须立即创建一次 git 提交，不得累积多个不相关的改动后一起提交。**

理由：本仓库的首要目的是**「改坏了能精确回滚」**。未提交的改动不在 git 保护范围内，
一旦文件被写坏且未提交，就无法还原。

### 执行要求

1. **改前先看状态**：`git status`，确认当前工作区是 clean 的。
   - 若已有未提交改动，先判断是否属于本次任务；不属于则先提交或询问用户。
2. **一个逻辑改动 = 一次提交**。不要把"修 A 的 bug"和"重构 B"混在一个提交里。
3. **改完立即提交**，不要等"全部做完再一起提交"。
4. **提交后必须验证**：`git status` 应为 `nothing to commit, working tree clean`。

### 提交信息格式

```
<类型>: <一句话说明>

<可选正文：为什么这么改、影响范围、注意事项>
```

类型使用：`feat`（新功能）、`fix`（修 bug）、`refactor`（重构）、`perf`（性能）、
`docs`（文档）、`chore`（杂项）、`style`（格式）。

示例：

```
fix: 修正 PlayerController 技能冷却被每帧递减两次的问题

PlayerController.Update 中 124-131 行与 UpdateSkillCooldown() 都递减了
skillCoolTimer，导致技能实际冷却只有配置值的一半。移除重复递减。
```

## 二、回滚速查

```bash
# 查看历史（一行一条）
git log --oneline

# 丢弃某个文件的未提交改动，还原到上次提交的状态
git checkout -- <文件路径>

# 丢弃所有未提交改动（危险：不可恢复）
git checkout -- .

# 撤销最近一次提交，但保留改动到工作区（用于重新提交）
git reset --soft HEAD~1

# 撤销最近一次提交并丢弃改动（危险）
git reset --hard HEAD~1

# 查看某次提交改了什么
git show <commit-id>

# 把某个文件还原到指定历史版本
git checkout <commit-id> -- <文件路径>
```

**回滚后务必用 `git status` 确认工作区状态符合预期。**

## 三、仓库配置说明

### 关键设计决策：字节精确（byte-exact）

`.gitattributes` 使用 `* -text`，**禁用一切行尾归一化**。

原因：本项目 `.cs` 文件混用 UTF-8 与 GBK(936) 两种编码，工作区行尾也不统一。
若启用 `text=auto`，git 会在仓库内存 LF、在 checkout 时按 `core.eol` 写回 CRLF，
导致**回滚后拿到的文件与原始状态字节不一致**（约 3000 个文件的行尾会被静默改写）。

经实测验证：`git ls-tree -l HEAD` 报告的 blob 字节数与磁盘文件字节数完全一致
（含 GBK 文件与二进制资源）。**请勿把 `* -text` 改回 `text=auto`。**

另注：项目 Unity 序列化模式为 ForceText，但 `Assets` 下 2115 个
`.unity/.prefab/.asset/.mat` 中仍有 14 个含 NUL 字节，是真正的二进制资源。
所以**不要**对这些扩展名写裸 `text`（强制文本），否则提交时会被改写损坏。

### 仓库级配置（存于 `.git/config`，不随仓库分发）

| 配置项 | 值 | 原因 |
|---|---|---|
| `core.autocrlf` | `false` | 禁止行尾转换，保证字节精确 |
| `core.longpaths` | `true` | Unity 资源路径很浅但层级深（`ArtRes/Tower Defense/...`），避免 Windows MAX_PATH 失败 |
| `core.quotepath` | `false` | 项目含中文路径，关闭转义后 `git status` 可读 |
| `gc.auto` | `0` | 见下方「已知环境限制」 |

### 提交身份

当前为占位身份：`ghost <ghost@localhost>`。
如需改为真实身份（例如后续要推送到 GitHub），执行：

```bash
git config user.name  "你的名字"
git config user.email "你的邮箱"
# 仅改写基线提交的作者信息
git commit --amend --reset-author --no-edit
```

### 跟踪范围

- **纳入跟踪**：`Assets/`（含 `.meta`）、`Packages/manifest.json`、
  `Packages/packages-lock.json`、`ProjectSettings/`、`.vsconfig`
- **排除**（见 `.gitignore`）：`Library/`（2.8 GB）、`Temp/`、`obj/`、`Logs/`、
  `.vs/`、`UserSettings/`，以及 Unity/IDE 自动生成的 `*.csproj`、`*.sln`

## 四、已知环境限制（沙箱）

在受管控的沙箱终端中执行 git 时遇到以下限制，**这不是仓库问题**：

1. **无法通过管道捕获子进程输出**。
   `git ls-files | Measure-Object`、`$x = git ...`、`Start-Process -RedirectStandardOutput`
   均会报 `Access is denied`。
   → 对策：改用能把结果**直接输出到控制台**的命令，例如 `git ls-tree -l`、`git show`、`git log`。

2. **`git gc` / 打包会失败**。
   自动 gc 报 `cannot create standard output pipe for pack-objects: Permission denied`，
   因此对象目前是**松散存储**（约 10000 个对象 / 1.56 GiB，`in-pack: 0`）。
   已设 `gc.auto 0` 抑制噪音。
   → 对策：**建议在沙箱外的普通终端手动执行一次 `git gc`** 完成打包，
   可显著减小体积并提升后续操作速度：

   ```bash
   cd D:\UnityProject\BS
   git gc
   git count-objects -vH   # 确认 in-pack 不为 0、size-pack 已下降
   ```

## 五、大体积资源说明

`Assets` 共 **3.1 GB**，其中 `Assets/ArtRes` 占 **3.06 GB（97%）**，主要是第三方美术资源，
含多个 48–60 MB 的 `.tif` 源贴图。当前策略是**全部纳入跟踪**，以换取完整的回滚能力。

如果后续磁盘占用成为问题，可选方案（需用户决策）：

- **Git LFS**：对大二进制走 LFS。注意本机 `git lfs` 可用性未经确认，且引入远端依赖。
- **排除 `ArtRes`**：体积从 3.1 GB 降到约 90 MB。代价是美术源文件丢失后无法从仓库恢复——
  仅在你另有 Asset Store 原始包备份时采用。
