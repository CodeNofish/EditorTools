以下是该 Unity 编辑器文档的中文翻译：

# 调色板
**内置 USS 变量 ->**
调色板是 UI 设计师在编辑器中创建界面时使用的一组颜色组合。调色板是编辑器用户体验的核心，有助于保持视觉一致性。

## 概述
请参阅编辑器内置颜色变量，以清晰了解编辑器中的颜色变量和主题机制。
颜色用于支撑软件应用的信息架构。恰当的颜色使用能向用户传达 UI 元素的重要性和优先级。
颜色有助于引导用户操作，并使各个控件之间具有明显的区分度。
在编辑器中应限制颜色的使用范围，采用细微差别的调色板，避免过多过于鲜艳的颜色导致用户界面杂乱无章。

### 颜色变量
### 主题
Unity 编辑器提供两种主题：浅色模式和深色模式。主题选择属于个人偏好。用户通常会在编辑器中工作数小时，在特定环境下使用合适的主题选项可以极大减轻眼部疲劳。

> *[图示说明]* 左侧为浅色主题下的检视面板（Inspector），右侧为深色主题下的检视面板。
> Unity 编辑器提供深色和浅色主题以满足用户偏好。

### 变量
变量是代码中用于赋值的标识符。颜色通过颜色变量来实现，以便于轻松修改。例如，每个颜色变量都有对应的浅色值和深色值，因此当切换编辑器主题时，UI 会自动更新为对应主题的正确颜色。

避免在编辑器 UI 中使用硬编码颜色，因为硬编码的值不会随颜色更新而改变。请始终使用颜色变量而非直接使用十六进制或 RGBA 颜色值来设置编辑器 UI 样式。系统已提供大量预置变量，如有需要也可创建新变量。

### Unity 变量命名规范
Unity 对内置变量采用特定的命名约定以表明其用途。变量名由一个或多个部分组成，各部分之间用连字符分隔。每个部分包含一个或多个单词，单词之间用下划线分隔。

变量命名示例：
`--unity-{group}-{role_and_control}-{sub_element}-{pseudo_state_sequence}`

例如，变量名 `--unity-colors-button-text-hover` 提供了以下关于 Unity 如何使用该变量的信息：

| 区段名称 | 示例值 | 描述 |
| :--- | :--- | :--- |
| 组 (Group) | colors | 该变量表示颜色数据 |
| 角色/控件 (Role/Control) | button | 该变量作用于按钮 |
| 子元素 (Sub-Element) | text | 该变量作用于文本。因其所属组为 colors，可知它影响的是文本颜色 |
| 伪状态 (Pseudo-States) | hover | Unity 在鼠标指针悬停于元素上时，将该变量应用于对应样式 |

有关 Unity 内置变量的更多信息，请参阅 Unity 内置样式表。

### 伪状态
伪状态序列是 Unity 使用该变量时所对应的 UI 状态列表。

例如，当你看到变量名中包含 `hover` 伪状态时，意味着 Unity 会在用户将鼠标指针悬停在元素上时，将该变量应用于影响该元素的样式。

示例：`--unity-colors-toolbar_button-text-hover`

一个变量名可以包含多个伪状态。多个伪状态按字母顺序排列，并用下划线分隔。

示例：`--unity-colors-toolbar_button-text-focus_selected`

Unity 变量名可以包含以下伪状态的任意组合：

| 伪状态 | 描述 |
| :--- | :--- |
| (无) | 正常状态 |
| checked | 控件被选中（如复选框） |
| disabled | 控件被禁用 |
| focus | 控件获得焦点 |
| hover | 用户悬停在控件上 |
| inactive | 控件未获得焦点 |
| pressed | 控件被按下 |
| selected | 控件被选定 |

### 扁平化设计
> *[图标说明]* 红色三角形图标，表示警告信息。

Unity 编辑器遵循扁平化设计原则。
避免过度使用阴影来增加 UI 的立体感。应改用强调色边框和基础图层来为设计增添层次感。
虽然 Unity 编辑器整体遵循扁平化 UI 设计原则，但它通过不同的方式引入层次感：

*   强调色边框
*   内凹和外凸控件
*   背景色基础图层

#### 强调色边框
强调色边框旨在通过内凹和外凸效果来支撑用户界面的分层结构。这种对控件区分度的微妙提升对可用性有显著影响。例如，文本字段控件采用内凹设计，并具有不同的顶部边框颜色。

> *[图示说明]* 使用较深边框阴影展示的内凹边框和外凸边框。
> 输入字段使用内凹边框，点击类控件使用外凸边框。

#### 基础图层
编辑器中的颜色设计借鉴了现实世界中触觉对象和物理分层的概念。图层就像桌上的纸张一样，一层叠一层地排列。这种 UI 和颜色的分层设计旨在支撑信息架构和内容的视觉组织，以实现最佳可用性。颜色按照三个基础图层从深到浅进行分层：

> *[图示说明]* 三个基础图层叠加展示的插图，最深层在底部，最浅层在顶部。
> 颜色在三个基础图层中按从深到浅的顺序排列。

*   **Base 1** - 应用程序栏和控件
*   **Base 2** - 组件和窗口
*   **Base 3** - 工具栏和工具栏控件

> *[信息提示]* 蓝色圆圈内的字母 "I" 图标，表示此为信息性内容。
> **框架中的颜色变量**
> 本文档记录的颜色变量仅适用于 IMGUI 和 UI Toolkit 框架，不建议在运行时（Runtime）中使用。

---

## Base 1
### 应用程序栏
> *[图示说明]* 带有常用颜色名称标签的应用程序工具栏插图，对应下方图表中的颜色。
> 位于编辑器顶部的应用程序工具栏包含许多按钮，包括播放器控制按钮。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | App Toolbar Background | `--unity-colors-app_toolbar-background` | 主工具栏的背景色 | #8A8A8A | #191919 |
| 2 | App Toolbar Button Background | `--unity-colors-app_toolbar_button-background` | 主工具栏中按钮的背景色 | #C8C8C8 | #383838 |
| 3 | App Toolbar Button Background Checked | `--unity-colors-app_toolbar_button-background-checked` | 主工具栏中按钮被选中时的背景色 | #656565 | #6A6A6A |
| | App Toolbar Button Background Hover | `--unity-colors-app_toolbar_button-background-hover` | 鼠标指针悬停时主工具栏按钮的背景色 | #BBBBBB | #424242 |
| | App Toolbar Button Background Pressed | `--unity-colors-app_toolbar_button-background-pressed` | 主工具栏中按钮被按下时的背景色 | #656565 | #6A6A6A |
| 4 | App Toolbar Button Border | `--unity-colors-app_toolbar_button-border` | 主工具栏中按钮的边框颜色 | #6B6B6B | #191919 |
| | App Toolbar Button Border Accent | `--unity-colors-app_toolbar_button-border_accent` | 主工具栏中按钮的强调边框颜色 | #6B6B6B | #222222 |

---

## Base 2
### 按钮颜色
> *[图示说明]* 深色主题下的切换按钮和基本按钮。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Button Background | `--unity-colors-button-background` | 按钮控件的背景色 | #E4E4E4 | #585858 |
| | Button Background Focus | `--unity-colors-button-background-focus` | 按钮控件获得焦点时的背景色 | #BEBEBE | #6E6E6E |
| | Button Background Hover | `--unity-colors-button-background-hover` | 鼠标指针悬停时按钮控件的背景色 | #ECECEC | #676767 |
| | Button Background Hover Pressed | `--unity-colors-button-background-hover_pressed` | 按钮控件悬停并被按下时的背景色 | #B0D2FC | #4F657F |
| | Button Background Pressed | `--unity-colors-button-background-pressed` | 按钮控件被按下时的背景色 | #96C3FB | #46607C |
| | Button Border | `--unity-colors-button-border` | 按钮控件的边框颜色 | #B2B2B2 | #303030 |
| | Button Border Accent | `--unity-colors-button-border_accent` | 按钮控件的强调边框颜色 | #939393 | #242424 |
| | Button Border Accent Focus | `--unity-colors-button-border_accent-focus` | 按钮控件获得焦点时的强调边框颜色 | #018CFF | #7BAEFA |
| | Button Border Pressed | `--unity-colors-button-border-pressed` | 按钮控件被按下时的边框颜色 | #707070 | #0D0D0D |
| | Button Text | `--unity-colors-button-text` | 按钮控件的文本颜色 | #090909 | #EEEEEE |

更多信息请参阅 按钮 ->

### 下拉菜单（弹出菜单）颜色
> *[图示说明]* 深色主题下的下拉菜单组件 / 深色主题下的下拉菜单/弹出菜单。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Dropdown Background | `--unity-colors-dropdown-background` | 下拉菜单控件的背景色 | #DFDFDF | #515151 |
| | Dropdown Background Hover | `--unity-colors-dropdown-background-hover` | 鼠标指针悬停时下拉菜单控件的背景色 | #E4E4E4 | #585858 |
| | Dropdown Border | `--unity-colors-dropdown-border` | 下拉菜单控件的边框颜色 | #B2B2B2 | #303030 |
| | Dropdown Border Accent | `--unity-colors-dropdown-border_accent` | 下拉菜单控件的强调边框颜色 | #939393 | #242424 |
| | Dropdown Text | `--unity-colors-dropdown-text` | 下拉菜单控件的文本颜色 | #090909 | #E4E4E4 |

更多信息请参阅 下拉菜单 ->

### 帮助框
> *[图示说明]* 着色器通道帮助框插图 / 着色器通道帮助框。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Helpbox Background | `--unity-colors-helpbox-background` | 帮助框控件的背景色 | rgba(235, 235, 235, 0.2039216) | rgba(96, 96, 96, 0.2039216) |
| | Helpbox Border | `--unity-colors-helpbox-border` | 帮助框控件的边框颜色 | #A9A9A9 | #232323 |
| | Helpbox Text | `--unity-colors-helpbox-text` | 帮助框控件的文本颜色 | #161616 | #BDBDBD |

更多信息请参阅 下拉菜单 -> *(注：原文此处链接指向下拉菜单，可能为原文档笔误)*

### 输入字段（数值、搜索、文本字段）颜色
> *[图示说明]* 深色主题下的文本字段组件 / 深色主题下的文本字段。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Input Field Background | `--unity-colors-input_field-background` | 输入字段控件的背景色 | #F0F0F0 | #2A2A2A |
| | Input Field Border | `--unity-colors-input_field-border` | 输入字段控件的边框颜色 | #B7B7B7 | #212121 |
| | Input Field Border Accent | `--unity-colors-input_field-border_accent` | 输入字段控件的强调边框颜色 | #A0A0A0 | #0D0D0D |
| | Input Field Border Focus | `--unity-colors-input_field-border-focus` | 输入字段控件获得焦点时的边框颜色 | #1D5087 | #3A79BB |
| | Input Field Border Hover | `--unity-colors-input_field-border-hover` | 鼠标指针悬停时输入字段控件的边框颜色 | #6C6C6C | #656565 |

更多信息请参阅 数值字段 ->
更多信息请参阅 搜索字段 ->
更多信息请参阅 文本字段 ->

### 多列标题头

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Header Bar Background | `--unity-colors-headerbar-background` | 标题栏控件的背景色 | #CBCBCB | #3C3C3C |
| | Header Bar Column Control Background | `--unity-colors-headerbar_column-background` | 标题栏中列控件的背景色 | #CBCBCB | #3C3C3C |
| | Header Bar Column Control Background Hover | `--unity-colors-headerbar_column-background-hover` | 鼠标悬停时列控件的背景色 | #C1C1C1 | #464646 |
| | Header Bar Column Control Background Pressed | `--unity-colors-headerbar_column-background-pressed` | 列控件被按下时的背景色 | #EFEFEF | #505050 |

### 对象字段颜色

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Object Field Background | `--unity-colors-object_field-background` | 对象字段控件的背景色 | #EDEDED | #282828 |
| | Object Field Border | `--unity-colors-object_field-border` | 对象字段控件的边框颜色 | #B0B0B0 | #202020 |
| | Object Field Border Focus | `--unity-colors-object_field-border-focus` | 对象字段控件获得焦点时的边框颜色 | #1D5087 | #3A79BB |
| | Object Field Border Hover | `--unity-colors-object_field-border-hover` | 鼠标指针悬停时对象字段控件的边框颜色 | #6C6C6C | #656565 |
| | Object Field Button Background | `--unity-colors-object_field_button-background` | 对象字段控件中按钮的背景色 | #DEDEDE | #373737 |
| | Object Field Button Background Hover | `--unity-colors-object_field_button-background-hover` | 鼠标指针悬停时对象字段控件中按钮的背景色 | #CCCCCC | #4C4C4C |

更多信息请参阅 对象字段 ->

### 滚动条颜色

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Scrollbar Button Background | `--unity-colors-scrollbar_button-background` | 滚动条滚动按钮的背景色 | rgba(0, 0, 0, 0.05098039) | rgba(0, 0, 0, 0.05098039) |
| | Scrollbar Button Background Hover | `--unity-colors-scrollbar_button-background-hover` | 鼠标指针悬停时滚动条滚动按钮的背景色 | #A7A7A7 | #494949 |
| | Scrollbar Groove Background | `--unity-colors-scrollbar_groove-background` | 滚动条轨道（背景）的颜色 | rgba(0, 0, 0, 0.05098039) | rgba(0, 0, 0, 0.05098039) |
| | Scrollbar Groove Border | `--unity-colors-scrollbar_groove-border` | 滚动条轨道的边框颜色 | rgba(0, 0, 0, 0.1019608) | rgba(0, 0, 0, 0.1019608) |
| | Scrollbar Thumb Background | `--unity-colors-scrollbar_thumb-background` | 滚动条滑块/手柄的背景色 | #9A9A9A | #5F5F5F |
| | Scrollbar Thumb Background Hover | `--unity-colors-scrollbar_thumb-background-hover` | 鼠标指针悬停时滚动条滑块/手柄的背景色 | #8E8E8E | #686868 |
| | Scrollbar Thumb Border | `--unity-colors-scrollbar_thumb-border` | 滚动条滑块/手柄的边框颜色 | #B9B9B9 | #323232 |
| | Scrollbar Thumb Border Hover | `--unity-colors-scrollbar_thumb-border-hover` | 鼠标指针悬停时滚动条滑块/手柄的边框颜色 | #8E8E8E | #686868 |

### 滑动条颜色
> *[图示说明]* 深色主题下显示的基本滑动条和最小-最大滑动条。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| | Slider Groove Background | `--unity-colors-slider_groove-background` | 滑动条轨道（背景）的颜色 | #8F8F8F | #5E5E5E |
| | Slider Groove Background Disabled | `--unity-colors-slider_groove-background-disabled` | 滑动条轨道被禁用时的背景色 | #A4A4A4 | #575757 |
| | Slider Thumb Background | `--unity-colors-slider_thumb-background` | 滑动条滑块/手柄的背景色 | #616161 | #999999 |
| | Slider Thumb Background Disabled | `--unity-colors-slider_thumb-background-disabled` | 滑动条滑块/手柄被禁用时的背景色 | #9B9B9B | #666666 |
| | Slider Thumb Background Hover | `--unity-colors-slider_thumb-background-hover` | 鼠标指针悬停时滑动条滑块/手柄的背景色 | #4F4F4F | #EAEAEA |
| | Slider Thumb Border | `--unity-colors-slider_thumb-border` | 滑动条滑块的边框颜色 | #616161 | #999999 |
| | Slider Thumb Border Disabled | `--unity-colors-slider_thumb-border-disabled` | 滑动条滑块被禁用时的边框颜色 | #666666 | #666666 |
| | Slider Thumb Halo Background | `--unity-colors-slider_thumb_halo-background` | 滑动条滑块光晕的背景色 | rgba(12, 108, 203, 0.5019608) | rgba(16, 111, 205, 0.5019608) |

更多信息请参阅 滑动条 ->

### 标签页颜色
> *[图示说明]* 场景窗口，带有标记元素展示不同的窗口标签页颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Tab Background | `--unity-colors-tab-background` | 编辑器窗口标签页的背景色 | #B6B6B6 | #353535 |
| | Tab Background Checked | `--unity-colors-tab-background-checked` | 编辑器窗口标签页被选中时的背景色 | #CBCBCB | #3C3C3C |
| | Tab Background Hover | `--unity-colors-tab-background-hover` | 鼠标指针悬停时编辑器窗口标签页的背景色 | #B0B0B0 | #303030 |
| 2 | Highlight Background | `--unity-colors-highlight-background` | 选中项或选中文本的背景色 | #3A72B0 | #2C5D87 |

### 检视面板背景色
> *[图示说明]* 主摄像机检视面板窗口，带有标记元素展示不同的窗口背景颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 2 | Inspector Titlebar Background | `--unity-colors-inspector_titlebar-background` | 检视视图中标题栏的背景色 | #CBCBCB | #3E3E3E |
| | Inspector Titlebar Background Hover | `--unity-colors-inspector_titlebar-background-hover` | 鼠标指针悬停时检视视图标题栏的背景色 | #D6D6D6 | #474747 |
| 1 | Toolbar Background | `--unity-colors-toolbar-background` | 工具栏控件的背景色 | #CBCBCB | #3C3C3C |
| 3 | Window Background | `--unity-colors-window-background` | 窗口的背景色 | #C8C8C8 | #383838 |

### 检视面板边框
> *[图示说明]* 主摄像机检视面板窗口，带有标记元素展示不同的窗口边框颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Inspector Titlebar Border | `--unity-colors-inspector_titlebar-border` | 检视视图中标题栏的边框颜色 | #7F7F7F | #1A1A1A |
| 2 | Inspector Titlebar Border Accent | `--unity-colors-inspector_titlebar-border_accent` | 检视视图中标题栏的强调边框颜色 | #BABABA | #303030 |

### 窗口背景色
> *[图示说明]* 光照资源管理器窗口，带有标记元素展示不同的背景颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Default Background | `--unity-colors-default-background` | 控件的默认背景色 | #A5A5A5 | #282828 |
| | Highlight Background Inactive | `--unity-colors-highlight-background-inactive` | 控件未获得焦点时选中项或选中文本的背景色 | #AEAEAE | #4D4D4D |
| 2 | Highlight Background | `--unity-colors-highlight-background` | 选中项或选中文本的背景色 | #3A72B0 | #2C5D87 |
| | Highlight Background Hover | `--unity-colors-highlight-background-hover` | 鼠标指针悬停时列表项的背景色 | rgba(0, 0, 0, 0.0627451) | rgba(256, 256, 256, 0.0627451) |
| 3 | Window Background | `--unity-colors-window-background` | 窗口的背景色 | #C8C8C8 | #383838 |
| | Highlight Background Hover Lighter | `--unity-colors-highlight-background-hover-lighter` | 当鼠标悬停在具有特定行背景色的项目上时的背景色 | #9A9A9A | #5F5F5F |
| 4 | Alternated Rows Background | `--unity-colors-alternated_rows-background` | 交替行颜色视图中使用的交替背景色 | #CACACA | #3F3F3F |

### 窗口边框
> *[图示说明]* 项目窗口，带有标记元素展示不同的窗口边框颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 3 | Default Border | `--unity-colors-default-border` | 控件的默认边框颜色 | #999999 | #232323 |
| 1 | Toolbar Border | `--unity-colors-toolbar-border` | 工具栏控件的边框颜色 | #999999 | #232323 |
| 2 | Window Border | `--unity-colors-window-border` | 窗口的边框颜色 | #939393 | #242424 |

---

## Base 3
### 工具栏
> *[图示说明]* 游戏窗口工具栏，带有标记元素展示不同颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Toolbar Background | `--unity-colors-toolbar-background` | 工具栏控件的背景色 | #CBCBCB | #3C3C3C |
| 2 | Toolbar Border | `--unity-colors-toolbar-border` | 工具栏控件的边框颜色 | #999999 | #232323 |
| 3 | Toolbar Button Background | `--unity-colors-toolbar_button-background` | 工具栏按钮控件的背景色 | #CBCBCB | #3C3C3C |
| | Toolbar Button Background Checked | `--unity-colors-toolbar_button-background-checked` | 工具栏按钮控件被选中时的背景色 | #EFEFEF | #505050 |
| | Toolbar Button Background Focus | `--unity-colors-toolbar_button-background-focus` | 工具栏按钮控件获得焦点时的背景色 | #C1C1C1 | #464646 |
| | Toolbar Button Background Hover | `--unity-colors-toolbar_button-background-hover` | 鼠标指针悬停时工具栏按钮控件的背景色 | #C1C1C1 | #464646 |
| 4 | Toolbar Button Border | `--unity-colors-toolbar_button-border` | 工具栏按钮控件的边框颜色 | #999999 | #232323 |

---

## 文本
### Base 1 - 默认文本颜色
> *[图示说明]* 控制台窗口，带有标记元素展示不同的文本颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Default Text | `--unity-colors-default-text` | 默认文本颜色 | #090909 | #D2D2D2 |
| | Default Text Hover | `--unity-colors-default-text-hover` | 鼠标指针悬停在文本上时的默认文本颜色 | #090909 | #BDBDBD |
| 2 | Error Text | `--unity-colors-error-text` | 错误消息的文本颜色 | #5A0000 | #D32222 |
| 3 | Link Text | `--unity-colors-link-text` | 未访问链接的文本颜色 | #4C7EFF | #4C7EFF |
| | Visited Link Text | `--unity-colors-visited_link-text` | 已访问链接的文本颜色 | #AA00AA | #FF00FF |
| | Warning Text | `--unity-colors-warning-text` | 警告消息的文本颜色 | #333308 | #F4BC02 |

### Base 2 - 窗口和组件文本颜色
> *[图示说明]* 项目设置窗口，带有标记元素展示不同的文本颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Helpbox Text | `--unity-colors-helpbox-text` | 帮助框控件的文本颜色 | #161616 | #BDBDBD |
| | Highlight Text | `--unity-colors-highlight-text` | 选中项或选中文本的文本颜色 | #0032E6 | #4C7EFF |
| | Highlight Text Inactive | `--unity-colors-highlight-text-inactive` | 控件未获得焦点时选中项或选中文本的文本颜色 | #FFFFFF | #FFFFFF |
| 2 | Label Text | `--unity-colors-label-text` | 标签控件的文本颜色 | #090909 | #C4C4C4 |
| 3 | Label Text Focus | `--unity-colors-label-text-focus` | 标签获得焦点时的文本颜色 | #003C88 | #81B4FF |
| | Preview Overlay Text | `--unity-colors-preview_overlay-text` | 预览控件覆盖层的文本颜色 | #FFFFFF | #DEDEDE |
| | Window Text | `--unity-colors-window-text` | 窗口的文本颜色 | #090909 | #BDBDBD |

### Base 3 - 工具栏文本颜色
> *[图示说明]* 场景窗口，带有标记元素展示工具栏中使用的不同文本颜色。

| # | 常用名称 | 变量名 | 变量描述 | 浅色主题 | 深色主题 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | Tab Text | `--unity-colors-tab-text` | 编辑器窗口标签页的文本颜色 | #090909 | #BDBDBD |
| 2 | Toolbar Button Text | `--unity-colors-toolbar_button-text` | 工具栏按钮控件的文本颜色 | #090909 | #C4C4C4 |
| | Toolbar Button Text Checked | `--unity-colors-toolbar_button-text-checked` | 工具栏按钮控件被选中时的文本颜色 | #090909 | #C4C4C4 |
| | Toolbar Button Text Hover | `--unity-colors-toolbar_button-text-hover` | 鼠标指针悬停时工具栏按钮控件的文本颜色 | #090909 | *(原文缺失深色主题值)* |