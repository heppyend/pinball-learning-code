# Assets 模块说明

## 职责

Unity 项目内容根目录，包含项目自有启动、业务、场景及资源，也包含第三方插件与框架源码。

## 进入本目录前

先阅读根目录 `PROJECT_OVERVIEW.md`、`VERSION_HISTORY.md`、`BUG_TRACKER.md`，再根据目标读取对应子目录的 `MODULE.md`。

## 约束

- 移动或删除任何资源时，必须连同 `.meta` 文件处理并验证 GUID 引用、场景/预制体引用和 YooAsset 地址。
- `Main`、`Scripts`、`Scenes`、资源目录由本项目维护；插件、框架与样例目录未确认前视作第三方或遗留内容，不作批量整理。
- Unity 导入此 Markdown 后会生成对应 `.meta`；该行为正常，不得手工复制或替换 GUID。
