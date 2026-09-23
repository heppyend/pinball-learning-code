# Archive：已归档 / 已被取代的文档

> 建立日期：2026-09-22。**归档 ≠ 删除**：本目录是被取代文档的**副本 + 指向头**，根目录原件**未动**（原因见下）。

## 1. 为什么是"复制归档"而不是"移动归档"

| 原因 | 说明 |
| --- | --- |
| ① 没有 git 副本可救 | `pinball` 有 200+ 未跟踪文件，其中 `HANDOVER_PLAN.md`、`WEBGL_*`、`UI_*` 等**恰恰都未跟踪** ⇒ 物理删除**不可恢复** |
| ② 移动会产生新的删除改动 | 3 份归档对象是 **git 跟踪文件**（`HANDOFF_主开发1.md`、`VERSION_HISTORY.md`、`AI_MODEL_UPGRADE.md`），移走 = 在工作区（已有 40 项 `D`）再叠 3 项删除 |
| ③ 会打断治理文档的路由 | `AGENTS.md` §6 明确要求"发布、回滚或历史追溯按需读取 `VERSION_HISTORY.md`"；移走必须同步改 `AGENTS.md` |
| ④ 可逆成本为零 | 副本方式下，回滚只需删掉本目录 |

> 若负责人要求"真移动"（含同步修改 `AGENTS.md` §6 路由），请按点名清单逐项执行，并在操作前后各留一次 `git status --short` 快照。

## 2. 归档清单与替代关系

| 归档副本 | 原位置 | 状态 | 取代者 / 现状入口 |
| --- | --- | --- | --- |
| `TCP_WEBGL_HANDOFF.md` | 根目录（**工作区已删除**，`git status` = ` D`） | 内容有效但去向有变；本副本取自 `HEAD`（105 行） | `WEBGL_PORT_BASELINE.md` + `TCP_NETWORK_BREAK_ROOTCAUSE.md`；`HANDOVER_DEV.md` §2.2 / §5.1 / §6 |
| `HANDOFF_主开发1.md` | 根目录 | **已过期**（仍写"阶段 0 进行中"，提交号停留在早前） | 仍有效的部分已并入 `HANDOVER_DEV.md` §1.5 / §8.1 / §11.4 |
| `VERSION_HISTORY.md` | 根目录 | 条目停在 2026-08-28 | `CURRENT_STATE.md` 顶部快照 + `HANDOVER_DEV.md` §0；历史追溯仍可查本文件 |
| `AI_MODEL_UPGRADE.md` | 根目录 | 治理文档体系的**设计过程存档**（501 行），非交接材料 | `HANDOVER_DEV.md` §2.2 |

## 3. 尚未归档（待合并，暂不动手）

| 合并组 | 成员 | 状态 |
| --- | --- | --- |
| 真机文档 | `WEBGL_B2_DIAGNOSTICS.md` + `WEBGL_B2_REALDEVICE.md` | ✅ **已定案：不合并**（前者＝采集手段与判据清单，后者＝操作手册 + §八 实测记录），共同入口为 `WEBGL_MINIGAME_HANDBOOK.md` |
| UI 适配 | `UI_ADAPT_BACKLOG.md` + `UI_ANCHOR_SIGNOFF.md` + `UI_ACCEPTANCE_CHECKLIST.md` | ⏸ 待合并为一份；含**负责人手调值**，必须逐条搬运，不能摘要 |

## 4. 并发编辑提醒

本目录建立时（2026-09-22 20:18–20:26）实测**另一个会话正在改写移植侧文档**（新增 `WEBGL_MINIGAME_HANDBOOK.md`，并改动 `CURRENT_STATE.md` / `BUG_TRACKER.md` / `DEVELOPMENT_WORKFLOW.md` / `PROJECT_OVERVIEW.md` / `HANDOVER_PLAN.md` / `WEBGL_B2_REALDEVICE.md` / `Assets/Main/MODULE.md`）。
⇒ 归档动作只由一方执行；动任何文件前先看 mtime 与 `git status --short`。
