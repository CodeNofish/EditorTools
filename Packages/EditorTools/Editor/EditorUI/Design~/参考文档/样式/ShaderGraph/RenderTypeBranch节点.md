**Render Type Branch 是 UI Shader Graph 专用节点，用于按当前 UI 渲染类型分发处理逻辑，最终输出 Fragment 节点所需的 Color 和 Alpha。**

### 核心定位

该节点仅适用于 **Material 设置为 UI（用于 UI Toolkit）的 Shader**，在普通 URP/HDRP/3D 渲染中使用会报错或产生异常行为。

### 端口说明

| 端口        | 方向 | 类型    | 说明                       |
| ----------- | ---- | ------- | -------------------------- |
| Solid       | 输入 | Color   | 背景、边框等纯色内容的颜色 |
| Texture     | 输入 | Texture | 纹理图形使用的纹理         |
| SDF Text    | 输入 | Texture | SDF 文本使用的纹理         |
| Bitmap Text | 输入 | Texture | 位图文本使用的纹理         |
| Gradient    | 输入 | Texture | 矢量图形渐变使用的纹理     |
| Color       | 输出 | Color   | 输出的颜色                 |
| Alpha       | 输出 | Float   | 输出的透明度               |

### 使用要点

- 节点内部会根据当前片段属于哪种 UI 渲染类型，自动从对应输入端口取值输出。
- 如果某个渲染类型想保持默认行为，**不要连接该端口**，节点会自动使用默认值，这样性能更好。
- 只有需要自定义某种渲染类型的处理时才连接对应端口，可配合 Default Solid、Default Texture、Default SDF Text 等默认节点作为起点。

### 与普通 Branch 节点的区别

- **普通 Branch 节点**：根据布尔条件在 True/False 之间二选一，属于通用逻辑分支。
- **Render Type Branch 节点**：根据 UI 渲染类型自动路由，专用于 UI Toolkit 的 UI Shader Graph，不是通用条件分支。

简单说：做 UI Toolkit 自定义 UI Shader 时用它按渲染类型分流；做普通 3D/URP 材质时不需要这个节点。

------

需要我为你介绍如何从零搭建一个基础的UI Shader Graph吗？