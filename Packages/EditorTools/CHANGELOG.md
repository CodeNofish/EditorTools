# CHANGELOG

## Unreleased

- 新增根节点切换主题 Class 方案 `EditorUITheme`，支持 Light/Dark 切换、局部换肤、跟随编辑器皮肤与主题变更事件
- `EditorUI.tss` 扩充全局语义色 Token 与间距、圆角、字号、透明度、动效度量 Token，`EditorUI.Light.tss` 与 `EditorUI.Dark.tss` 提供两套完整默认参数
- Accordion 的 USS 视觉属性全部抽取为 `--editorui-accordion-*` 变量，独立到 `EditorUI.Accordion.Variables.uss`，组件 USS 只消费变量
- Accordion 新增组件级 Light/Dark 主题表，补充颜色、背景色、边框、内外边距、焦点、按压、展开、禁用等默认样式
- `Accordion` 新增 `VariablesStyleSheetOverride` 变量表替换入口，指示器箭头线宽改由 USS 变量控制并在主题切换后重绘
