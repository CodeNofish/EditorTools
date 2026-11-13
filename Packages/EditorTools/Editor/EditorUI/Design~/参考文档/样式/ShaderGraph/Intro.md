**内容摘要：** 本文介绍 UI Shader Graph 的功能与特性，包括其 UI 专用渲染机制、用于处理多种渲染类型的 Render Type Branch 节点、各渲染类型的默认节点，以及 Element Texture UV、Sample Element Texture 等 UI 专用输入节点。



# UI Shader Graph 简介

UI Shader Graph 的功能与特性概述。

你可以使用 Shader Graph 创建着色器，以实现以下效果：

- **自定义按钮效果**：为按钮添加发光、模糊或交互式颜色变化。
- **文本渲染**：创建带有轮廓的清晰、可缩放文本。
- **动画 UI 元素**：使用着色器对渐变或纹理进行动画处理。
- **颜色处理**：对 UI 中的图像应用灰度、棕褐色或颜色校正等效果。

**重要提示** UI Shader Graph 旨在创建直接渲染 UI 网格元素的着色器。你不能使用它来创建自定义滤镜，自定义滤镜属于后处理着色器，用于对包含视觉元素子树的渲染目标施加效果。这是两种根本不同的渲染方式，不能互换使用。



## UI 专用渲染

当你为 UI 元素创建着色器并在 Shader Graph 编辑器中打开它时，通常会包含一个 **Fragment（片元）** 节点和一个 **Vertex（顶点）** 节点。

- **Fragment 节点**具有 **Base Color** 和 **Alpha** 输入，用于定义着色器的颜色和透明度。
- **主预览窗口（Main Preview window）** 显示着色器的结果。它提供即时视觉反馈，展示自定义着色器如何影响 UI 元素，并允许你对比应用前后的状态。
- **No Shader（无着色器）** 区域显示未应用任何着色器的示例图。这作为对比着色器效果的基准。它包括：
  - 一个示例纹理图
  - 一个示例渐变图
  - 一个带有纯色背景和边框、内部包含 SDF 文本的图。Signed Distance Fields（SDF，有符号距离场）字体常用于生成清晰、可缩放的文本，在各种尺寸下都有良好表现，且无需高分辨率纹理图集即可渲染。
  - 一个带有纯色背景和边框、内部包含位图文本的图。位图字体是基于像素的字体，使用纹理图集进行渲染。
- **With Shader（有着色器）** 区域显示与 No Shader 区域相同的示例图，但应用了着色器。这让你可以对比原始外观与应用着色器修改后的结果。当你调整着色器时，预览会实时更新，因此你可以立即查看更改对每个 UI 元素的影响。



## 一个着色器支持多种渲染类型

UI Toolkit 网格可以包含对应于各种渲染类型（文本、纯色、纹理、渐变）的三角形。与为每种渲染类型使用单独着色器的传统系统不同，UI Toolkit 在单个着色器内管理所有渲染类型，以实现最佳的 CPU 性能。

为了适应多种渲染类型，UI Shader Graph 提供了专用节点，让你能够根据当前渲染类型分支着色器逻辑。



### Render Type Branch 节点

关键工具是 **Render Type Branch（渲染类型分支）** 节点，它允许你在一个着色器中为纯色、纹理、SDF 和位图文本以及渐变定义不同的行为。

当你添加 Render Type Branch 节点时，它会为每种渲染类型包含所有带有默认值的输入端口。Render Type Branch 节点的每个输入端口对应一种特定的渲染类型。你可以将不同节点连接到每个端口，以定义每种渲染类型的处理方式，然后将 Render Type Branch 节点的输出连接到 Fragment 节点的输入，以定义着色器的最终外观。

以下图展示了一个使用 Render Type Branch 节点为每种渲染类型分配不同颜色的简单示例：

!Render Type Branch 示例

在此示例中，每种渲染类型都获得一种纯色：纯色渲染类型为红色，因此按钮的背景和边框显示为红色。同样，SDF 文本显示为蓝色，位图文本显示为紫色；纹理显示为绿色，而矢量图像显示为湖蓝色。

下表总结了每种渲染类型、其适用对象以及在主预览窗口中的外观。

| 渲染类型                    | 适用于                                           | 在主预览窗口中的外观       |
| --------------------------- | ------------------------------------------------ | -------------------------- |
| **Solid（纯色）**           | UI 元素的纯色背景和边框                          | SDF 图和位图图的背景与边框 |
| **Texture（纹理）**         | 纹理图形                                         | 纹理图                     |
| **SDF Text（SDF 文本）**    | 使用 Signed Distance Fields（SDF）字体的文本渲染 | SDF 图中的 SDF 文本        |
| **Bitmap Text（位图文本）** | 使用位图字体的文本渲染                           | 位图图中的位图文本         |
| **Gradient（渐变）**        | 矢量图形                                         | 渐变图                     |

**提示**：如果你希望在创建新着色器时自动添加 Render Type Branch 节点，可以使用 **Create > Shader Graph > From Template > UI** 中的某个 UI Shader Graph 模板。

![image-20261113121021531](./assets/image-20261113121021531.png)



### Render Type 节点

**Render Type** 节点输出着色器当前正在处理的渲染类型。这允许你根据渲染类型实现条件逻辑。例如，你可以将 Render Type 节点的输出与比较或分支节点配合使用，仅将特定效果应用于某些渲染类型。



### 每种渲染类型的默认节点

尽管你可以在自定义着色器中使用任何节点，UI Shader Graph 仍为每种渲染类型提供了默认节点。每个默认节点代表其分支类型的默认实现。你可以将它们作为起点，并与其他节点结合以创建所需的视觉效果。

- Default Solid
- Default Texture
- Default Gradient
- Default Bitmap Text
- Default SDF Text

以下图将默认节点连接到 Render Type Branch 节点的每个端口：

![image-20261113121002280](./assets/image-20261113121002280.png)

**注意** 如果 Render Type Branch 节点的某个输入未连接，该节点会自动使用该输入的默认值。

为获得最佳性能，如果你希望某个输入使用其默认值，请保持该输入不连接，而不是将其连接到 Default Solid 节点或 Default Texture 节点等节点的输出。当输入断开连接时，节点会更高效地处理分支，从而优化所有渲染类型的性能。



### 自定义渲染类型

要自定义每种渲染类型的外观，请在将默认节点的输出连接到 Render Type Branch 节点之前对其进行修改。例如，你可以在 Default Solid 节点和 Render Type Branch 节点之间添加一个颜色乘法节点，为所有纯色元素着色。你还可以串联额外节点来混合颜色、应用效果或对属性进行动画处理。

以下图使用 **UV Distortion** 节点创建微妙的动画扭曲效果，为纹理应用紫色色调，同时让 SDF 文本、位图文本和渐变保持其默认外观：

![image-20261113121111807](./assets/image-20261113121111807.png)



## UI 专用输入节点

UI Shader Graph 包含多个专为 UI 元素设计的节点，提供有关当前正在渲染的 UI 元素的信息：

- **Element Texture UV**：用于采样指定纹理的纹理坐标。
- **Element Layout UV**：元素布局矩形内的 UV 坐标。
- **Element Texture Size**：指定纹理的尺寸。

以下图使用 Element Texture UV 节点，确保 UV 扭曲效果专门应用于 UI 元素的纹理。



### Sample Element Texture 节点

**Sample Element Texture** 节点允许你在特定 UV 坐标处采样纹理。你可以使用此节点对分配给元素的纹理进行多次采样，例如创建复杂的视觉效果或以各种方式操作纹理。

**注意** 分配给元素的纹理是该 UI 元素当前使用的纹理。它可能是字体纹理、元素背景图像、分配给 Image 元素的源图像，或通过自定义渲染代码中的 `MeshGenerationContext.DrawMesh` 提供的纹理。

以下图使用 Sample Element Texture 节点创建基于纹理的效果：

!Sample Element Texture 示例

此着色器图对元素纹理进行了两次采样：一次正常采样，另一次将其 UV 偏移 (0.1, 0.1)。然后将两个采样的颜色组合在一起，创建纹理的重影和双重影像效果：

!Sample Element Texture 效果



## 其他资源

- UI Shader Graph 入门
- 字体资源简介
- 使用矢量图形

------

> **术语说明**：UI Shader Graph、Shader Graph、URP、UI Toolkit、Fragment、Vertex、Render Type Branch、SDF、Bitmap、UV、MeshGenerationContext、DrawMesh 等为 Unity 专有名词、节点名称或 API 术语，按行业惯例保留英文原文（首次出现时附中文注释）。

