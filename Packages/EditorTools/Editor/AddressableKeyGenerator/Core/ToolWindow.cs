using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace EditorTools.AddressableKeyGenerator {

  internal class ToolWindow : EditorWindow {
    [MenuItem("Tools/EditorTools/Generate AddressableKey")]
    internal static void GenerateAddressableKey() {
      var window = GetWindow<ToolWindow>("Generate AddressableKey");
      window.minSize = new UnityEngine.Vector2(400, 200);
    }

    private SerializedObject? _serializedSettings;

    private void OnDisable() {
      _serializedSettings?.Dispose();
      _serializedSettings = null;
    }

    public void CreateGUI() {
      var root = rootVisualElement;
      root.Clear();

      root.style.paddingTop = 10;
      root.style.paddingBottom = 10;
      root.style.paddingLeft = 10;
      root.style.paddingRight = 10;

      // 安全检查并初始化 SerializedObject
      if (GenerateSettings.instance == null) {
        root.Add(new Label("⚠️ GenerateSettings instance not found. Please create it first."));
        return;
      }

      // Content Root
      var contentRootfield = new TextField("Addressable Content Root") {
        bindingPath = nameof(GenerateSettings.contentRoot)
      };
      root.Add(contentRootfield);

      // Output Path
      var outputPathField = new TextField("AddressableKey Output Path") {
        bindingPath = nameof(GenerateSettings.outputPath)
      };
      root.Add(outputPathField);

      // TODO 支持绑定 string[] 
      // var list = new List<string>(items ?? System.Array.Empty<string>());
      //
      // var listView = new ListView {
      //   itemsSource = list,
      //   fixedItemHeight = 24,
      //   makeItem = () => new TextField(),
      //   bindItem = (element, index) => {
      //     var field = (TextField)element;
      //     field.SetValueWithoutNotify(list[index]);
      //
      //     field.RegisterValueChangedCallback(evt => {
      //       list[index] = evt.newValue;
      //       ApplyToStringArray();
      //     });
      //   }
      // };
      // root.Add(outputPathField);

      // Class Name
      var outputClassNameField = new TextField("OutputClass Name") {
        bindingPath = nameof(GenerateSettings.classname)
      };
      root.Add(outputClassNameField);

      // Namespace
      var outputClassNameSpaceField = new TextField("OutputClass Namespace") {
        bindingPath = nameof(GenerateSettings.nameSpace)
      };
      root.Add(outputClassNameSpaceField);

      // 每次 CreateGUI 都重新创建 SerializedObject，确保有效性
      _serializedSettings?.Dispose();
      _serializedSettings = new SerializedObject(GenerateSettings.instance);

      // 绑定到根节点
      root.Bind(_serializedSettings);

      // Generate Button 
      var generateButton = new Button(() => {
        _serializedSettings?.ApplyModifiedProperties();
        Program.GenerateAddressableKey();
      }) {
        text = "Generate",
      };
      root.Add(generateButton);
    }
  }

}
