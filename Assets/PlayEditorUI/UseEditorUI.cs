using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UseEditorUI : EditorWindow {
  [SerializeField]
  private VisualTreeAsset m_VisualTreeAsset = null;

  [MenuItem("Tools/UseEditorUI")]
  public static void ShowExample() {
    var wnd = GetWindow<UseEditorUI>();
    wnd.titleContent = new GUIContent("UseEditorUI");
  }

  public void CreateGUI() {
    // Each editor window contains a root VisualElement object
    var root = rootVisualElement;

    // VisualElements objects can contain other VisualElement following a tree hierarchy.
    VisualElement label = new Label("Hello World! From C#");
    root.Add(label);

    // Instantiate UXML
    VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
    root.Add(labelFromUXML);
  }
}