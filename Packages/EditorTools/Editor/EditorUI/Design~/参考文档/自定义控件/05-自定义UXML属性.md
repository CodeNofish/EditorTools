# **自定义 UXML 属性**

在 UI Builder 中自定义 UXML 属性的名称、行为、装饰器以及属性绘制器。

你可以通过 UI Builder 自定义 UXML 属性的名称、属性的行为，并添加装饰器和自定义属性绘制器。



### 重命名 UXML 属性

你可以通过 `UxmlAttribute` 特性的 `name` 参数来重命名自定义控件的属性。

以下是一个重命名了属性的自定义控件示例：

```csharp
using UnityEngine.UIElements;

[UxmlElement]
public partial class CustomAttributeNameExample : VisualElement
{
    [UxmlAttribute("character-name")]
    public string myStringValue { get; set; }

    [UxmlAttribute("credits")]
    public float myFloatValue { get; set; }

    [UxmlAttribute("level")]
    public int myIntValue { get; set; }

    [UxmlAttribute("usage")]
    public UsageHints myEnumValue { get; set; }
}
```

以下是一个使用了重命名后属性的 UXML 文档示例：

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <CustomAttributeNameExample character-name="Karl" credits="1.23" level="1" usage="DynamicColor" />
</ui:UXML>
```

如果你重命名了一个属性，但希望使用旧名称的 UXML 文件仍然保持兼容，可以向 `UxmlAttribute` 特性添加 `obsoleteNames` 参数。

以下示例展示了如何使用 `obsoleteNames` 参数：

```csharp
using UnityEngine.UIElements;

[UxmlElement]
public partial class CharacterDetails : VisualElement
{
    [UxmlAttribute("character-name", "npc-name")]
    public string npcName { get; set; }

    [UxmlAttribute("character-health", "health")]
    public float health { get; set; }
}
```

以下是在 UXML 中的使用示例：

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <CharacterDetails npc-name="Haydee" health="100" />
</ui:UXML>
```



### 在 UI Builder 中启用运行时数据绑定

为了简化控件属性的运行时数据绑定，你可以通过添加 `UxmlAttribute`（对于复杂数据类型则使用 `UxmlObjectReferenceAttribute`）和 `CreateProperty` 特性，在 UI Builder 中为自定义控件的属性启用 **Add Binding（添加绑定）** 菜单。例如：

```csharp
using UnityEngine.UIElements;
using Unity.Properties;

[UxmlElement]
public partial class ExampleCustomControl : VisualElement
{
    [UxmlAttribute, CreateProperty]
    public string description { get; set; }
}
```

> **注意**：请确保在脚本中包含 `using Unity.Properties;` 指令。更多信息请参考有关 **Unity Properties** 的文档。

在 UI Builder 中，当你在 **Hierarchy（层级）** 窗格中选择 `ExampleCustomControl`，然后在 **Inspector（检查器）** 窗格中右键点击 **Description** 时，**Add Binding** 菜单就会显示出来。随后，你就可以将控件的属性绑定到数据源。



### 覆盖 UXML 属性的行为

你可以覆盖自定义控件 UXML 属性的 `get` 和 `set` 行为。这将在 UI Builder 的属性视图中用你覆盖后的版本替换原始属性。要覆盖某个属性，请定义新属性并使用 `[UxmlAttribute("original-attribute-name"), new-attribute]` 引用原始属性。

你可以使用这种属性覆盖来自定义从子类继承的属性。例如，要在 `IntegerField` 中强制执行值限制，可以覆盖 `value` 属性并应用 `Range` 特性，如下所示：

```csharp
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class MyCustomIntField : IntegerField
{
    [UxmlAttribute("value"), Range(0, 100)]
    private int valueOverride
    {
        get => this.value;
        set => this.value = value;
    }
}
```

在此示例中，`MyCustomIntField` 类继承自 `IntegerField` 并覆盖了 `value` 属性。`Range` 特性将值限制在 0 到 100 之间。

当你在 UI Builder 中添加 `MyCustomIntField` 时，覆盖后的版本会替换原来的 `value` 属性。这允许你在指定范围内设置值。



### 在 UI Builder 中添加属性装饰器

你可以在 UXML 属性字段上添加以下装饰器特性。当你添加装饰器特性时，相应的 UI 控件（例如用于 `Range` 的滑块）会显示在 UI Builder 的检查器面板中：

- `TextArea`
- `Tooltip`
- `Range`
- `Header`
- `Min`
- `Multiline`
- `Space`
- `Delayed`

以下示例创建了一个在其属性字段上带有装饰器的自定义控件：

```csharp
using UnityEngine.UIElements;
using UnityEngine;

[UxmlElement]
public partial class ExampleText : VisualElement
{
    [TextArea, UxmlAttribute]
    public string myText;

    [Header("My Header")]
    [Range(0, 100)]
    [UxmlAttribute]
    public int rangedInt;

    [Tooltip("My custom tooltip")]
    [Min(10)]
    [UxmlAttribute]
    public int minValue = 100;
}
```

UI Builder 会显示带有装饰器的属性（如图所示）。



### 在 UI Builder 中添加自定义属性绘制器

你可以在 UI Builder 中的字段上添加自定义属性绘制器。

以下示例创建了一个带有自定义属性绘制器的自定义控件：

```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class MyDrawerAttribute : PropertyAttribute { }

[UxmlElement]
public partial class MyDrawerExample : Button
{
    [UxmlAttribute]
    public Color myColor;

    [MyDrawer, UxmlAttribute]
    public string myText;
}
```

要访问其他序列化属性，请在名称前加上 `serializedData` 前缀。以下代码示例使用 `serializedData.myColor` 来查找 `myColor` 属性：

```csharp
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(MyDrawerAttribute))]
public class MyDrawerAttributePropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
        var textField = new TextField("My Text") { style = { flexGrow = 1 } };
        var button = new Button { text = ":" };
        button.clicked += () => textField.value = "RESET";

        // 获取父属性
        var parentPropertyPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'));
        var parent = property.serializedObject.FindProperty(parentPropertyPath);

        var colorProp = parent.FindPropertyRelative("myColor");
        textField.TrackPropertyValue(colorProp, p =>
        {
            row.style.backgroundColor = p.colorValue;
        });

        row.style.backgroundColor = colorProp.colorValue;
        row.Add(textField);
        row.Add(button);
        textField.BindProperty(property);

        return row;
    }
}
```

自定义属性绘制器的外观如下图所示（带有颜色选择器的自定义属性绘制器）。

### 其他资源

- **create a custom inventory property drawer（创建自定义库存属性绘制器）**
- **PropertyDrawer**
- **DecoratorDrawer**
- **UxmlObjectReferenceAttribute**
- **Troubleshooting custom control library compilation（排查自定义控件库编译问题）**

------

需要我接着为你翻译“处理自定义控件事件”部分的文档吗？