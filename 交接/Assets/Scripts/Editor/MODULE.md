# Editor 模块说明

## 职责

仅供 Unity 编辑器使用的项目工具，例如图层调试。

## 约束与验证

- 不得引入运行时程序集依赖；新增工具保持在本目录或 Editor asmdef 下。
- 修改后在 Unity Editor 中验证菜单、Inspector 或目标工具的执行，不要求进入发布包。
