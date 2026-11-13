# UI Shader Graph

使用 Shader Graph 为你的 UI 元素创建独特的视觉风格和效果，例如渐变、动画或颜色混合。

UI Shader Graph 允许你为 UI 元素创建独特的视觉风格和效果，例如渐变、动画或颜色混合。

**注意** UI Shader Graph 仅适用于 URP（通用渲染管线）。

| 主题                     | 描述                                                         |
| ------------------------ | ------------------------------------------------------------ |
| **UI Shader Graph 入门** | 一个简单的示例，演示如何创建一个自定义着色器，并将渐变效果应用于按钮。 |
| **UI Shader Graph 简介** | 概述 UI Shader Graph 的功能与特性。                          |





# UI Shader Graph 入门

一个简单的示例，用于创建一个将渐变效果应用于按钮的自定义着色器。

你可以使用 Shader Graph 为 UI 元素创建自定义着色器。为了了解 UI Shader Graph 的工作原理，请尝试以下简单示例作为入门。本示例将创建一个自定义着色器，并将渐变效果应用于按钮。有关 UI Shader Graph 的更详细概述，请参阅 UI Shader Graph 简介。

**注意** UI Shader Graph 仅适用于 URP（通用渲染管线）。

## 前提条件

本指南面向熟悉 Unity Editor、UI Toolkit 和 C# 脚本编写的开发者。在开始之前，请熟悉以下内容：

- Shader Graph
- 材质（Materials）
- UI Builder
- 按钮（Button）

## 创建具有渐变效果的着色器

为 UI 元素创建着色器时，通常使用 **Render Type Branch（渲染类型分支）** 节点来处理不同的渲染类型。以下步骤将创建一个简单的着色器，对按钮的纯色（Solid Color）渲染类型应用 UV 扭曲效果。

1. 使用任意 URP 模板创建一个项目。
2. 在 **Project（项目）** 窗口中，右键单击 **Assets** 文件夹，选择 **Create > Shader Graph > URP > UI Shader Graph**。这将创建一个配置用于 UI 渲染的 UI Shader Graph 资源。
3. 将新资源命名为 `MyCustomShader`。
4. 双击 `MyCustomShader` 在 Shader Graph 编辑器中打开它。
5. 选择 **Create Node > UI > Render Type Branch**。
6. 选择 **Create Node > UV > UV Distortion**。
7. 将 **UV Distortion** 节点的 **UV** 输出连接到 **Render Type Branch** 节点的 **Solid** 输入。
8. 将 **Render Type Branch** 节点的 **Color** 输出连接到 **Fragment** 节点的 **Base Color** 输入。
9. 将 **Render Type Branch** 节点的 **Alpha** 输出连接到 **Fragment** 节点的 **Alpha** 输入。主预览窗口（Main Preview window）中的 **With Shader** 将显示着色器应用于 SDF 和位图（Bitmap）文本背景的效果。



## 将着色器应用于 UI 元素

要将自定义着色器应用于 UI 元素，你需要创建一个使用该着色器的新材质，然后将该材质分配给 UI 元素。以下步骤演示如何将上一节中创建的自定义着色器分配给 UI Builder 中的按钮。你也可以通过 USS 或 C# 脚本分配自定义着色器。

1. 在 **Assets** 文件夹中右键单击，选择 **Create > Material**。
2. 将新材质命名为 `MyCustomMaterial`。
3. 在新材质的 **Inspector（检查器）** 窗口中，从 **Shader** 下拉菜单中选择 `MyCustomShader`，将其分配给该材质。
4. 从菜单栏选择 **Window > UI Toolkit > UI Builder** 以打开 UI Builder 窗口。
5. 从 **Library（库）** 面板中，将一个 **Button（按钮）** 元素拖入 **Hierarchy（层级）** 面板，以将按钮添加到你的 UI 文档中。
6. 在按钮的 **Inspector** 面板中，从 **Material** 下拉列表中选择 `MyCustomMaterial`，将材质分配给按钮。UI Builder 窗口中的 **Viewport（视口）** 将显示应用了自定义着色器的按钮。

**注意** 当你将自定义着色器分配给某个 UI 元素时，该着色器会影响所选元素及其所有子元素。着色器中定义的任何视觉效果或修改都将应用于整个层级结构，从而在嵌套的 UI 元素之间实现一致的样式和行为。







