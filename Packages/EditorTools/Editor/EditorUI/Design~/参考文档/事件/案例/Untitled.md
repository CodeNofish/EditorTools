**内容摘要：** 本文介绍 Unity UI Toolkit 中的主题样式表（Theme Style Sheet，TSS）文件——它本质上是一种被作为独立资源类型管理的普通 USS 文件，并说明了如何创建、预览、应用主题，以及通过 Inspector 窗口或 `@import` 规则导入主题的方法。



# 主题样式表（Theme Style Sheet，TSS）

了解如何使用 TSS 文件进行管理。

主题样式表（TSS）文件就是普通的 USS 文件。UI Toolkit 将 TSS 视为一种独立的资源类型，并将其用于管理目的。

普通的 USS 语法和格式同样适用于 TSS 文件，但通常而言，主题文件会通过 `@import` 规则引用其他 USS 文件。

**注意** USS 文件同样支持 `@import` 规则。



## 创建 TSS

你可以为不同的主题创建不同的 TSS 文件，并在运行时（runtime）在它们之间切换。通常，你应当使用 USS 来编写样式。当你需要针对不同平台或设备定制外观，或者当你的游戏或应用支持多种语言、需要针对不同语言使用不同字体时，才考虑使用 TSS。

**注意** 当你向项目中的某个 GameObject（游戏对象）添加第一个 UIDocument 时，它会在 Assets/UI Toolkit/UnityThemes/UnityDefaultTheme.tss 路径下生成一个默认的运行时主题资源。尽管你可以从零开始创建 TSS，但为了让所有默认 UI 控件都能正常工作，你必须先导入默认运行时主题，然后再覆盖或添加新的样式，从而创建你自己的自定义主题。

要新建一个 TSS：

1. 创建一个或多个包含自定义样式的 USS 文件。
2. 执行以下操作之一：
   - 选择 **Assets > Create > UI Toolkit > TSS Theme File**，创建一个空的、并导入了默认主题的主题文件。
   - 选择 **Assets > Create > UI Toolkit > Default Runtime Theme File**，创建一份默认运行时主题文件的副本。如有需要，你可以对该文件重命名。
3. 将你的样式 USS 文件导入到你的主题中。



## 预览 TSS

你的自定义主题还会出现在 UI Builder 的 **Viewport（视口）** 的主题列表中。要预览你的主题，请在 Viewport 中选中它。

## 应用 TSS

你可以像引用 USS 一样，在 UXML 或 C# 中引用 TSS。

你也可以将某个 TSS 设置为 Panel Setting（面板设置）资源的默认主题。如果你有多个 Panel Setting，那么每一个都可以使用不同的 TSS 文件。

**注意** 如果你为某个 Panel Setting 资源设置了 TSS，这并不会使该 TSS 成为整个项目的全局样式。你仍然需要在 UXML 或 C# 中引用这个 TSS。

## 导入主题

你可以使用 TSS 资源的 Inspector（检查器）窗口，或使用 `@import` 规则来导入主题。

**注意** 如果你同时在当前的 TSS 和被导入的样式表中定义了 USS 变量或样式，那么当前的 TSS 会覆盖被导入的样式表。

### 使用 Inspector 窗口导入主题

1. 选中该 TSS 资源。
2. 进入 Inspector 窗口 > **Inherited Themes（继承的主题）**。
3. 选择 **+**。这会新增一个引用字段。
4. 选择该引用字段中的列表图标。此时会弹出一个下拉列表，显示你项目中的所有样式表。
5. 选择要使用的样式表。
6. 选择 **Apply（应用）**。



### 使用 @import 规则导入默认主题

要导入 Unity 的默认运行时主题，你必须使用以下语法：

```
@import url("unity-theme://default");
```

**注意** 此语法中的这个 URL 是一种特殊情况，它引用的是引擎内置的默认主题。你不能将这种语法用于任何其他主题。



### 使用 @import 规则导入其他主题

要导入除默认主题以外的主题，或导入一个 USS 文件，请使用以下语法：

```
@import url("<path-to-file>/<file-name-with-extension>");
```

你可以使用相对路径或绝对路径：

- 绝对路径从项目的 Assets 文件夹开始，并以 `/` 或 `project://database/` 开头。例如 `/Assets/myFolder/myFile.uss` 或 `project://database/Assets/myFolder/myFile.uss`。
- 相对路径从当前文件开始，且开头不含 `/`。例如 `../myFolder/myFile.uss`。

**注意** 要引用软件包（package）中的文件，请使用从 Packages 文件夹开始的、该软件包文件的绝对路径。例如 `/Packages/com.unity.package.name/file-name.uss` 或 `project://database/Packages/com.unity.package.name/file-name.uss`。

软件包名称必须使用 `com.unity.package.name` 这种格式，而不能使用普通的软件包名称（package name）。



## 其他资源

- USS 简介
- 使用 C# 应用样式
- 向 UXML 添加样式

------

> **术语说明**：TSS、USS、UI Toolkit、UIDocument、GameObject、UXML、C#、Panel Setting、UI Builder、Viewport、Inspector 等为 Unity 专有名词或产品界面术语，按行业惯例保留英文原文（首次出现时附中文注释）；`@import` 语法及路径示例作为代码原样保留，不做翻译。