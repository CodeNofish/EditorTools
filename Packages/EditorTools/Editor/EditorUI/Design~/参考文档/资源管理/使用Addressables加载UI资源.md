# 使用 Addressables 加载 UI 资源

创建一个简单的 UXML 文件和 USS 文件，将它们设为可寻址（Addressable），并在 MonoBehaviour 脚本中于运行时加载它们。

使用 Unity 的 Addressable Asset System（可寻址资产系统）在运行时动态加载 UI 资源，以实现高效的内存管理和灵活的资产组织。

本示例展示了如何使用 Addressables 包加载 UI 资源。对于大型项目或需要更高效管理资产的情况，Addressables 是运行时引用 UI 资产的推荐方式。



#### 示例概览

在本示例中，你将创建一个带有按钮的简单 UXML 文件，以及一个用于设置按钮样式的简单 USS 文件。你将这两个文件设为可寻址，然后在运行时通过 MonoBehaviour 脚本加载它们。

你可以在这个 GitHub 仓库中找到本示例创建的完整文件。



#### 创建 UXML 和 USS 文件

创建一个包含按钮元素的 UXML 文件，以及一个将按钮背景设置为绿色的 USS 文件。

1. 使用任意模板创建一个 Unity 项目。
2. 如果尚未安装，请安装 Addressables 包。
3. 在 Project 窗口中，创建一个名为 `UXMLExample.uxml` 的 UXML 文件和一个名为 `USSExample.uss` 的 USS 文件。
4. 在 UI Builder 中打开 `UXMLExample.uxml` 文件，并向 Hierarchy 面板中添加一个 **Button**。
5. 保存更改。
6. 在文本编辑器中打开 `USSExample.uss` 文件，添加以下内容以设置按钮样式：

```css
Button {
    background-color: green;
}
```



#### 将 UXML 和 USS 文件设为可寻址

将 UXML 和 USS 文件设为可寻址，以便在运行时加载它们。当你将资产设为可寻址时，Unity 会根据资产名称生成一个默认的地址键（Address Key）。本示例为 UXML 文件设置了一个自定义地址键，以便在 C# 脚本中引用它。

1. 在 Project 窗口中，选择 `USSExample.uss` 文件。
2. 在 **Inspector** 窗口中，勾选 **Addressable**。
3. 选择 `UXMLExample.uxml` 文件。
4. 在 Inspector 窗口中，勾选 **Addressable** 并在地址键字段中输入 `uxmlexample`。

*(此时 Inspector 窗口应显示 Addressable 复选框已选中，且地址键字段已填写)*



#### 创建 C# 脚本

创建一个 C# 脚本，通过地址键加载 UXML，并通过可寻址资产引用加载 USS。

1. 在 Project 窗口中，创建一个名为 `AddressableExample.cs` 的 C# 脚本，内容如下：

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

// 使用 Unity 的 Addressable Asset System 管理 UI 资产的动态加载和卸载。
// 此脚本在运行时加载 VisualTreeAsset 和 StyleSheet 资产，并将它们应用于 PanelRenderer 组件，
// 提供高效的内存管理和异步加载功能。
public class AddressableExample : MonoBehaviour
{
    [SerializeField] private string uxmlAssetKey; // 要加载的 UXML 资产的可寻址键。
    [SerializeField] private AssetReference ussAsset; // 要加载的 USS 资产的 AssetReference。
    private PanelRenderer panelRenderer;
    private AsyncOperationHandle<VisualTreeAsset> uxmlLoadHandle;
    private AsyncOperationHandle<StyleSheet> ussLoadHandle;
    private VisualTreeAsset loadedAsset;
    private StyleSheet loadedStyleSheet;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        if (panelRenderer != null)
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }
    }
    
    private void OnDisable()
    {
        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }
        UnloadUIDocument();
    }
    
    // 当 PanelRenderer 重新加载 UI 时调用的回调方法。
    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        // 当 UI 准备好接收资产时加载它们。
        if (!uxmlLoadHandle.IsValid())
        {
            LoadUIDocument();
        }
        
        if (!ussLoadHandle.IsValid())
        {
            LoadStyleSheet();
        }
        
        // 如果样式表已加载则应用；防止在每次重载时重复添加同一张样式表。
        if (loadedStyleSheet != null && !rootElement.styleSheets.Contains(loadedStyleSheet))
        {
            rootElement.styleSheets.Add(loadedStyleSheet);
        }
    }
    
    private void LoadUIDocument()
    {
        Debug.Log($"Loading {uxmlAssetKey}");
        uxmlLoadHandle = Addressables.LoadAssetAsync<VisualTreeAsset>(uxmlAssetKey);
        uxmlLoadHandle.Completed += UxmlHandle_Completed;
    }
    
    private void LoadStyleSheet()
    {
        if (ussAsset != null)
        {
            Debug.Log($"Loading {ussAsset.RuntimeKey}");
            ussLoadHandle = Addressables.LoadAssetAsync<StyleSheet>(ussAsset);
            ussLoadHandle.Completed += UssHandle_Completed;
        }
        else
        {
            Debug.LogWarning("USS AssetReference is null, skipping style sheet loading.");
        }
    }
    
    private void UxmlHandle_Completed(AsyncOperationHandle<VisualTreeAsset> obj)
    {
        if (!isActiveAndEnabled || panelRenderer == null)
            return;
            
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            loadedAsset = obj.Result;
            panelRenderer.visualTreeAsset = loadedAsset;
        }
        else
        {
            Debug.LogError($"AssetKey {uxmlAssetKey} failed to load.");
        }
    }
    
    private void UssHandle_Completed(AsyncOperationHandle<StyleSheet> obj)
    {
        if (!isActiveAndEnabled || panelRenderer == null)
            return;
            
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            loadedStyleSheet = obj.Result;
        }
        else
        {
            Debug.LogError($"AssetReference {(ussAsset != null ? ussAsset.RuntimeKey : "(null)")} failed to load.");
        }
    }
    
    private void UnloadUIDocument()
    {
        if (uxmlLoadHandle.IsValid())
        {
            Debug.Log($"Unloading {uxmlAssetKey}");
            uxmlLoadHandle.Completed -= UxmlHandle_Completed;
            loadedAsset = null;
            Addressables.Release(uxmlLoadHandle);
            uxmlLoadHandle = default;
        }
        
        if (ussLoadHandle.IsValid())
        {
            Debug.Log($"Unloading {(ussAsset != null ? ussAsset.RuntimeKey : "(null)")}");
            ussLoadHandle.Completed -= UssHandle_Completed;
            loadedStyleSheet = null;
            Addressables.Release(ussLoadHandle);
            ussLoadHandle = default;
        }
    }
    
    private void OnDestroy()
    {
        // 清理任何未在 UnloadUIDocument 中释放的句柄。
        if (uxmlLoadHandle.IsValid())
        {
            uxmlLoadHandle.Completed -= UxmlHandle_Completed;
            Addressables.Release(uxmlLoadHandle);
            uxmlLoadHandle = default;
        }
        
        if (ussLoadHandle.IsValid())
        {
            ussLoadHandle.Completed -= UssHandle_Completed;
            Addressables.Release(ussLoadHandle);
            ussLoadHandle = default;
        }
    }
}
```



#### 测试示例

要测试此示例，请创建一个新场景并将脚本附加到 Panel Renderer GameObject 上。

1. 在项目中创建一个新场景。
2. 在 Hierarchy 窗口中，选择 **+ > UI Toolkit > Panel Renderer** 以创建一个新的 Panel Renderer GameObject。
3. 选择 Panel Renderer GameObject，并在 Inspector 窗口中选择 **Add Component**。
4. 将 `AddressableExample` 脚本添加到该 GameObject。
5. 在 Addressable Example 组件中，在 **Uxml Asset Key** 字段输入 `uxmlexample`，并将 `USSExample` 分配给 **Uss Asset Reference** 字段。
6. 保存场景并进入 Play 模式查看结果。

进入 Play 模式后，Game 视图将显示一个带有绿色背景的按钮。这确认了脚本成功加载了 UXML 文件并应用了 USS 文件中的样式。控制台也会显示日志消息，确认两个资产均已成功加载。



#### 更多资源

- Manage asset references (管理资产引用)

