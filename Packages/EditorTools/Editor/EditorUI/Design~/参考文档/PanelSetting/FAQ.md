# 使用 UI Toolkit 的输入与事件系统常见问题（FAQ）

查找有关事件系统和输入系统的常见问题解答。

本页收录了在使用 UI Toolkit 时，关于事件系统与输入系统的常见问题：

- 如何判断鼠标是否悬停在运行时面板（runtime panel）的某个可视元素上？
- 如何重新映射（remap）基本 UI 操作？
- 使用方向导航时，如何更改下一个获得焦点的元素？
- 能否去掉方向导航的 WASD 输入，或提交（submit）操作的空格键？
- 如何在从未用鼠标点击任何位置的情况下就开始接收键盘输入？

------

## 如何判断鼠标是否悬停在运行时面板的某个可视元素上？

要判断鼠标是否位于运行时面板的某个可视元素之上，可使用以下两种方法之一：

- **方法一**借助事件系统（Event System）进行自动检测，适合 uGUI 与 UI Toolkit 混合使用的 UI。
- **方法二**提供手动控制权，在你需要自行管理多个面板时很有用。



### 方法一：使用事件系统

UI Toolkit 的元素（例如 Button 或 Label）并不是 GameObject，因此它们不会投射（cast）射线，也不能被赋值给 GameObject 类型的变量。你需要先用事件系统找到对应的面板（panel），再使用 UI Toolkit 的 API 找出指针下方的可视元素。

1. 在场景中添加一个 Event System 组件，方式与从 com.unity.ugui 包创建基于 GameObject 的 UI 内容时相同。
2. 使用 `EventSystem.current.IsPointerOverGameObject` 方法，当指针位于 uGUI 或 UI Toolkit 的 UI 内容之上时，该方法返回 true。
3. 若要精确列出指针下方的所有对象，可使用 `EventSystem.current.RaycastAll` 方法。
4. 找出指针下方的面板。在事件系统的环境中，每个被射线命中的 UI Toolkit 面板都由一个 GameObject 表示：
   - 该 GameObject 的名称与对应的 PanelSettings 一致。
   - 该 GameObject 的父对象是持有当前 Event System 组件的 GameObject。
   - 该 GameObject 上带有两个组件：PanelRaycaster 和 PanelEventHandler。这两个组件都有一个 `panel` 属性，返回其指向的 IPanel。
5. 调用 `panel.Pick` 方法，找出指针位置处的可视元素。
6. 使用 `RuntimePanelUtils.ScreenToPanel` 方法，将指针的屏幕坐标转换为面板坐标。

> **注意** uGUI 的屏幕坐标原点在左下角，而 UI Toolkit 的原点在左上角。要在两者之间转换，需要对 Y 坐标做镜像处理：`yTopLeft = Screen.height - yBottomLeft`。



### 方法二：手动搜索面板

1. 建立场景中所有活动面板的列表。做法是遍历场景中所有的 UIDocument 组件，并把 `uiDocument.rootVisualElement.panel` 加入列表。如果有多个文档共用同一个 PanelSettings，则每个不同的 PanelSettings 值只需保留一个文档。
2. 按深度顺序（从最上层到最下层）遍历面板列表。
3. 对每个面板调用 `panel.Pick`，直到某个调用返回了可视元素为止。

------



## 如何重新映射基本 UI 操作？

要重新映射基本 UI 操作：

1. 在场景中添加一个 Event System 组件，方式与从 com.unity.ugui 包创建基于 GameObject 的 UI 内容时相同。
2. 在 Inspector 窗口中，配置 Stand alone Input Module 或 Input System UI Input Module 的各项字段，以控制每个操作对应哪种输入。

> **注意** 映射到 Tab 和 Shift+Tab 输入的操作无法重新映射，因为这些输入并未通过事件系统的输入模块暴露出来。

------



## 使用方向导航时，如何更改下一个获得焦点的元素？

你可以配置方向导航，使其指向不同于默认目标的元素。

下面的代码示例让元素 A 在上、下、左、右导航时，分别跳转到元素 U、D、L、R：

```csharp
A.RegisterCallback<NavigationMoveEvent>(e =>
{
    switch (e.direction)
    {
        case NavigationMoveEvent.Direction.Up: U.Focus(); break;
        case NavigationMoveEvent.Direction.Down: D.Focus(); break;
        case NavigationMoveEvent.Direction.Left: L.Focus(); break;
        case NavigationMoveEvent.Direction.Right: R.Focus(); break;
    }
    e.StopPropagation();          // 阻止事件继续被处理
    A.focusController.IgnoreEvent(e); // 阻止默认导航行为
});
```

------



## 能否去掉方向导航的 WASD 输入，或提交操作的空格键？

可以。你可以在 Inspector 窗口中使用 EventSystem 的 StandaloneInputModule 或 InputSystemUIInputModule 字段来控制每个操作对应的输入。不过，由于这些操作是与 uGUI 输入共享的，这样做同时也会改变 uGUI 的控件行为。

要在不影响 uGUI 控件的前提下重新映射 UI Toolkit 的输入，可以禁用 UI Toolkit 的运行时事件处理，并手动把所有事件发送给各个面板。具体做法是在当前 EventSystem 的 start 方法被调用之前（例如场景中某个组件的 Awake 方法里）调用：

```csharp
EventSystem.SetUITookitEventSystemOverride(null, false, false);
```

有关这个 uGUI 方法的更多信息，请参考 `SetUITookitEventSystemOverride` 的文档。

------



## 如何在从未用鼠标点击任何位置的情况下就开始接收键盘输入？

在某些时刻，可能没有任何元素或面板处于焦点状态，例如首次加载游戏场景时。这种情况下，键盘导航无法从一个可预测的首元素开始。对于完全不用鼠标操作的游戏来说，这会成为一个问题。

要让导航从一开始就具有可预测的行为，可以添加一个 C# 脚本，并将其挂载到负责获取初始焦点的那个元素所对应的 UIDocument 所在的同一个 GameObject 上。

假设你的脚本名为 `FirstFocus`，初始聚焦元素名为 `first-focused`。在脚本的 `Start()` 方法中加一行代码来聚焦该元素，如下所示：

```csharp
public class FirstFocus : MonoBehaviour
{
    void Start()
    {
        FocusFirstElement();
    }

    public void FocusFirstElement()
    {
        GetComponent<UIDocument>().rootVisualElement.
            Q<VisualElement>("first-focused").Focus();
    }
}
```

> **注意** 如果你禁用了 UIDocument 所在的 GameObject，其整个底层层级结构都会被从头重建。因此，在重新启用该 GameObject 之后，必须再次调用你自定义的 `FocusFirstElement()` 方法。

------

需要我帮你整理一份事件系统与输入系统的关键 API 速查表吗？方便开发时快速查阅。