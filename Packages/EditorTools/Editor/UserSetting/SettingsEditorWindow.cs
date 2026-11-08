using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorTools {

  /// <summary>
  /// 通用 EditorWindow：把任意 UserSettingsSingleton&lt;T&gt; 的单例实例
  /// 渲染成可编辑的 Inspector，并在修改后自动保存回 UserSettings/*.asset。
  /// </summary>
  public class SettingsEditorWindow : EditorWindow {
    private Type? _settingsType;

    // 复用 Inspector 绘制（含所有 PropertyDrawer）
    private Editor? _cachedEditor;

    private string _title = "Settings";

    /// <summary>
    /// 若类型打开方式
    /// </summary>
    public static void Open(Type settingsType) {
      if (settingsType == null)
        throw new ArgumentNullException(nameof(settingsType));

      if (!IsSettingsType(settingsType))
        throw new ArgumentException(settingsType.Name + " 不是 UserSettingsSingleton<> 的非抽象子类。", nameof(settingsType));

      var window = GetWindow<SettingsEditorWindow>();
      window._settingsType = settingsType;
      window.Init();
    }

    /// <summary>
    /// 强类型打开方式
    /// </summary>
    public static void Open<T>() where T : UserSettingsSingleton<T> {
      Open(typeof(T));
    }

    private void Init() {
      if (_settingsType == null) return;

      _title = ObjectNames.NicifyVariableName(_settingsType.Name);
      titleContent = new GUIContent(_title, "Edit " + _settingsType.FullName + " (auto-saved)");
      minSize = new Vector2(320, 200);
      Repaint();
    }

    private void OnEnable() => Init();
    private void OnDisable() => DisposeEditor();

    private void OnGUI() {
      if (_settingsType == null) {
        EditorGUILayout.HelpBox("窗口未绑定 Settings 类型，请重新打开。", MessageType.Info);
        return;
      }

      var instance = GetInstance(_settingsType);
      if (instance == null) {
        EditorGUILayout.HelpBox("无法获取单例实例。", MessageType.Error);
        return;
      }

      // 复用同一个 Editor；类型/实例变化时重建。
      if (_cachedEditor == null || _cachedEditor.target != instance) {
        DisposeEditor();
        _cachedEditor = Editor.CreateEditor(instance);
      }

      var serialized = _cachedEditor.serializedObject;

      using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
        EditorGUI.BeginChangeCheck();
        _cachedEditor.DrawHeader(); // 顶部标题（如重写了 OnHeaderGUI）
        bool hadErrors = _cachedEditor.DrawDefaultInspector(); // 兼容任意字段+自定义Drawer
        bool changed = EditorGUI.EndChangeCheck() || hadErrors;

        if (changed) {
          serialized.ApplyModifiedProperties();
          Save(instance); // 改即存
          Repaint();
        }
      }

      // 底部按钮
      EditorGUILayout.Space();
      using (new EditorGUILayout.HorizontalScope()) {
        if (GUILayout.Button(new GUIContent("Reset", "还原为字段默认值"), GUILayout.Width(80))) {
          Undo.RecordObject(instance, "Reset Settings");
          ResetInstance(instance);
          Save(instance);
        }
        if (GUILayout.Button(new GUIContent("Save", "强制保存"), GUILayout.Width(80))) {
          serialized.ApplyModifiedProperties();
          Save(instance);
        }
      }
    }

    // ---------------------------------------------------------------
    // 反射工具：调用基类的静态 Instance / Save
    // ---------------------------------------------------------------
    private static UnityEngine.Object? GetInstance(Type type) {
      var prop = type.GetProperty("Instance",
        BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      return prop == null ? null : prop.GetValue(null) as UnityEngine.Object;
    }

    private static void Save(UnityEngine.Object instance) {
      var method = instance.GetType().GetMethod("Save", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      if (method != null) method.Invoke(null, null);
    }

    private static void ResetInstance(UnityEngine.Object instance) {
      // 用 ScriptableObject 的默认构造值覆盖当前字段。
      var fresh = ScriptableObject.CreateInstance(instance.GetType());
      var cur = new SerializedObject(instance);

      var p = new SerializedObject(fresh).GetIterator();
      bool enter = true;
      while (p.NextVisible(enter)) {
        enter = false;
        var c = cur.FindProperty(p.name);
        if (c != null && c.propertyType != SerializedPropertyType.Generic)
          c.serializedObject.CopyFromSerializedProperty(p);
      }
      cur.ApplyModifiedProperties();
      UnityEngine.Object.DestroyImmediate(fresh);
    }

    private void DisposeEditor() {
      if (_cachedEditor != null) {
        UnityEngine.Object.DestroyImmediate(_cachedEditor);
        _cachedEditor = null;
      }
    }

    /// <summary>
    /// 判断某类型是否 UserSettingsSingleton 的非抽象子类
    /// </summary>
    public static bool IsSettingsType(Type type) {
      if (type.IsAbstract) return false;
      for (var t = type.BaseType; t != null; t = t.BaseType) {
        if (t.IsGenericType &&
            t.GetGenericTypeDefinition() == typeof(UserSettingsSingleton<>))
          return true;
      }
      return false;
    }
  }

}
