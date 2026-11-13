## 内容摘要

本文介绍如何使用 `VisualElementReference` 在 Inspector（检视面板）中引用运行时（runtime）UI 的可视元素，以替代基于查询（UQuery）的访问方式，从而让脚本与具体 UI 层级解耦，并提供「类型化引用」和「非类型化引用」两种实现方式及示例代码。



------

## 在 Inspector 中引用运行时可视元素

学习如何使用 `VisualElementReference` 在 Inspector 中引用运行时可视元素。

你可以使用 UQuery 在 UI 层级结构中查找可视元素（visual element）。但如果选择器名称（selector name）发生变化，或者层级结构被修改，这种做法就可能引发问题。为了解决这一问题，你可以使用可视元素引用（visual element reference）来访问运行时 UI 的可视元素——它让你能够通过 Inspector（而不是代码查询）把脚本与元素连接起来。这样一来，脚本无需改动代码即可适配不同的 UI 布局，也更容易复用；同时由于脚本不再与特定的 UI 结构紧密耦合，它还能促成一种更模块化、更易维护的代码结构。

> **注意** 可视元素引用不适用于编辑器 UI（Editor UI）。



### 何时使用

**适合使用可视元素引用的场景：**

- UI 设计师需要自行配置元素之间的连接关系。
- 元素名称或结构可能频繁变动。
- 你希望把脚本与特定的 UI 层级解耦。
- 由多名团队成员分别负责 UI 布局与脚本编写。

**适合使用查询式访问（query-based access）的场景：**

- 你的可视元素名称是稳定的。
- 脚本的作者掌控着 UI 结构。
- 你需要动态地发现元素。
- 你处理的是动态生成的可视元素。

> **注意** 可视元素引用仅对 Panel Renderer（面板渲染器）组件有效。如果你使用的是旧版的 UI Document 组件来搭建运行时 UI，则必须通过查询式访问来查找可视元素。若要把 UI Document 组件迁移到 Panel Renderer 组件，请参考「迁移到 Panel Renderer（Migration to Panel Renderer）」。



### 实现细节

要为你的运行时 UI 实现可视元素引用，请在 MonoBehaviour 脚本中完成以下步骤：

1. 为你要访问的可视元素定义**类型化**或**非类型化**的引用。
2. 在 `Start()` 方法中，为每个元素引用注册引用回调（reference callback）。
3. 实现相应的初始化方法（setup method），这些方法会在引用解析成功、元素变为可用时执行。

#### 类型化引用（Typed references）

对于特定类型的元素，使用 `VisualElementReference<T>`。类型化引用在 Inspector 中只允许你选择该种类型的元素，并把该类型传递给回调。

下面的示例基于「开始使用运行时 UI（Get started with runtime UI）」一文，用可视元素引用替代了查询式访问。它分别使用 `VisualElementReference<Button>`、`VisualElementReference<Toggle>` 和 `VisualElementReference<TextField>` 来引用按钮、开关（toggle）和输入框。

你可以把这个脚本挂载到示例的 GameObject 上，以替换原来的 `SimpleRuntimeUI.cs` 脚本，并在 Inspector 中把 Button、Toggle、TextField 分别赋给对应的引用字段。它与原脚本保持相同的功能。

```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class RuntimeUIWithReferences : MonoBehaviour
{
    // 为你要访问的可视元素定义 VisualElementReference。
    [Header("UI Element References")]
    public VisualElementReference<Button> buttonReference = new VisualElementReference<Button>();
    public VisualElementReference<Toggle> toggleReference = new VisualElementReference<Toggle>();
    public VisualElementReference<TextField> inputFieldReference = new VisualElementReference<TextField>();

    private Button button;
    private Toggle toggle;
    private TextField inputField;

    private int clickCount;

    private void Start()
    {
        // 为每个可视元素引用解析成功时注册回调。
        buttonReference.RegisterReferenceResolvedCallback(SetupButton);
        toggleReference.RegisterReferenceResolvedCallback(SetupToggle);
        inputFieldReference.RegisterReferenceResolvedCallback(SetupInputField);
    }

    private void OnDisable()
    {
        // 在组件被禁用时清理事件监听器。
        if (button != null)
        {
            button.UnregisterCallback<ClickEvent>(PrintClickMessage);
        }
        if (inputField != null)
        {
            inputField.UnregisterCallback<ChangeEvent<string>>(InputMessage);
        }
    }

    // 为每个可视元素引用实现初始化方法，
    // 这些方法会在引用解析成功时被调用。
    private void SetupButton(Button resolvedButton)
    {
        button = resolvedButton;
        button.RegisterCallback<ClickEvent>(PrintClickMessage);
    }

    private void SetupToggle(Toggle resolvedToggle)
    {
        toggle = resolvedToggle;
    }

    private void SetupInputField(TextField resolvedInputField)
    {
        inputField = resolvedInputField;
        inputField.RegisterCallback<ChangeEvent<string>>(InputMessage);
    }

    private void PrintClickMessage(ClickEvent evt)
    {
        ++clickCount;

        Debug.Log($"{"button"} was clicked!" +
                (toggle != null && toggle.value ? " Count: " + clickCount : ""));
    }

    public static void InputMessage(ChangeEvent<string> evt)
    {
        Debug.Log($"{evt.newValue} -> {evt.target}");
    }
}
```

#### 非类型化引用（Untyped references）

对于任意类型的元素，使用 `VisualElementReference`。它允许你在 Inspector 中选择任意类型的元素，并把该类型传递给回调。

下面的示例把某个元素的背景色设置为红色。你可以把这个脚本挂载到上一个示例的 GameObject 上，并在 Inspector 中把任意元素赋给引用字段，从而让该元素的背景变为红色。

```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class VisualElementReference_Example : MonoBehaviour
{
    // 在检视面板中设置
    public VisualElementReference elementReference = new VisualElementReference();
    public Color customColor = Color.red;

    void Start()
    {
        elementReference.RegisterReferenceResolvedCallback(SetupElementBackground);
    }

    void SetupElementBackground(VisualElement ve)
    {
        ve.style.backgroundColor = customColor;
    }
}
```

------

### 关键术语注释

1. **visual element** → 可视元素：UI Toolkit 中构成 UI 的基本构建块（如 Button、Label 等），不是 GameObject。
2. **visual element reference / `VisualElementReference`** → 可视元素引用：Unity 提供的一种机制，可在 Inspector 中把脚本字段与运行时 UI 元素关联，避免使用代码查询。
3. **UQuery** → 保留英文：Unity UI Toolkit 提供的一套基于选择器（如 `Q<T>("name")`）查询元素的 API，此处按行业惯例保留原名。
4. **Panel Renderer** → 面板渲染器：较新的运行时 UI 渲染组件，可视元素引用仅在其下可用。
5. **UI Document** → UI 文档组件：旧版用于在场景中承载运行时 UI 的组件，仅支持查询式访问。
6. **Inspector** → 检视面板：Unity 编辑器中查看与配置组件属性的窗口，首次出现加注原文，后文保留「Inspector」。

------

> 原文中「A typed reference only allows to select that element type in the Inspector」一句排版时把「Inspector」一词拆到了下一行（疑似源文档断行错误），译文已按完整句意合并理解。