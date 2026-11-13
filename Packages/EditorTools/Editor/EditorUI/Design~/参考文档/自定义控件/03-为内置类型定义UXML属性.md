# **为内置类型定义 UXML 属性**

为自定义控件定义 UXML 属性，以便在 UXML 和 UI Builder 中公开属性。 

要为自定义控件定义 UXML 属性，请为你希望在 UXML 和 UI Builder 中公开的属性添加 `UxmlAttribute` 特性。

`UxmlAttribute` 会将字段或属性链接到 UXML 属性。Unity 会自动处理值与 UXML 属性字符串之间的转换。Unity 为常见类型提供了一组内置转换器，例如 `int`、`float`、`bool`、`string` 和 `Vector2`。

默认情况下，UI Toolkit 会将 C# 的驼峰命名（Camel case）或帕斯卡命名（Pascal case）的属性名解析为用小写字母表示、并用连字符连接的单词。例如，C# 中名为 `MyFloat` 或 `myFloat` 的属性在 UXML 和 UI Builder 中会变成 `my-float`。你也可以自定义 UXML 属性的名称。

以下示例创建了一个包含多个自定义属性的自定义控件：

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ExampleVisualElement : VisualElement
{
    [UxmlAttribute]
    public string myStringValue { get; set; }

    [UxmlAttribute]
    public int myIntValue { get; set; }

    [UxmlAttribute]
    public float myFloatValue { get; set; }

    [UxmlAttribute]
    public List<int> myListOfInts { get; set; }

    [UxmlAttribute, UxmlTypeReference(typeof(VisualElement))]
    public Type myType { get; set; }

    [UxmlAttribute]
    public Texture2D myTexture { get; set; }

    [UxmlAttribute]
    public Color myColor { get; set; }
}
```

以下示例 UXML 文档使用了该自定义控件：

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ExampleElement my-string-value="Hello World" my-int-value="123" />
    <ExampleElement my-float-value="3.14" my-list-of-ints="1,2,3" />
    <ExampleElement my-string-value="Hello World" my-int-value="123" />
    <ExampleElement my-texture="project://database/Assets/MyTexture.png" />
    <ExampleElement my-color="#FF0000FF" />
    <ExampleElement my-type="UnityEngine.UIElements.Button, UnityEngine.UIElementsModule" />
</ui:UXML>
```



### 其他资源

- **Define UXML attributes for complex data types（为复杂数据类型定义 UXML 属性）**

------

当前文档结尾提到了复杂数据类型的UXML属性，需要我接着为你翻译那部分文档吗？