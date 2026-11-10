// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using UnityEditor;
// using UnityEngine;
//
// namespace EditorTools {
//
//   /// <summary>
//   /// 用户设置中心（Split View 版）。
//   /// 左栏：所有 UserSettingsSingleton 子类列表（支持搜索 / 刷新）；
//   /// 右栏：选中项的实时 Inspector，修改即自动保存回 UserSettings/*.asset；
//   /// 两栏之间是可通过鼠标拖动调整宽度的分隔条。
//   /// </summary>
//   public class UserSettingsHubWindow : EditorWindow {
//     // ---- 数据源 ----
//     private List<Type> _types = new();
//
//     // ---- 左栏状态 ----
//     private string _search = "";
//     private Vector2 _leftScroll;
//     private Type? _selected;
//
//     // ---- 分栏状态 ----
//     private float _leftWidth = 260f; // 左栏宽度，可拖动
//     private bool _dragging;
//     const float MinLeftWidth = 150f;
//     const float MinRightWidth = 260f;
//     const float SplitterWidth = 4f;
//
//     // ---- 右栏状态 ----
//     private Vector2 _rightScroll;
//     private Editor? _cachedEditor;
//     private UnityEngine.Object? _cachedTarget;
//
//     [MenuItem("Tools/User Settings", false, 1)]
//     private static void OpenHub() {
//       var w = GetWindow<UserSettingsHubWindow>();
//       w.titleContent = new GUIContent("User Settings", "所有 UserSettingsSingleton 子类");
//       w.minSize = new Vector2(560, 260);
//       w.LoadTypes();
//       w.Show();
//     }
//
//     private void OnEnable() => LoadTypes();
//     private void OnDisable() => DisposeEditor();
//
//     private void LoadTypes() {
//       _types = DiscoverSettingsTypes().ToList();
//       if (_selected != null && !_types.Contains(_selected)) _selected = null;
//       Repaint();
//     }
//
//     private void OnGUI() {
//       DrawTopToolbar();
//
//       var total = position.width;
//       _leftWidth = Mathf.Clamp(_leftWidth, MinLeftWidth,
//         Mathf.Max(MinLeftWidth, total - MinRightWidth - SplitterWidth));
//
//       using (new EditorGUILayout.HorizontalScope()) {
//         DrawLeftPane();
//         DrawSplitter();
//         DrawRightPane();
//       }
//     }
//
//     private void DrawTopToolbar() {
//       using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
//         GUILayout.Label("User Settings", EditorStyles.toolbarButton, GUILayout.Width(110));
//         _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(80));
//         if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
//           LoadTypes();
//       }
//     }
//
//     // ---- 左栏：设置项列表 ----
//     private void DrawLeftPane() {
//       using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(_leftWidth))) {
//         var view = new EditorGUILayout.ScrollViewScope(_leftScroll, GUILayout.ExpandHeight(true));
//         _leftScroll = view.scrollPosition;
//
//         const StringComparison flag = StringComparison.Ordinal;
//         foreach (var type in _types) {
//           var nice = ObjectNames.NicifyVariableName(type.Name);
//           if (!string.IsNullOrEmpty(_search) &&
//               nice.IndexOf(_search, flag) < 0 &&
//               (type.FullName?.IndexOf(_search, flag) ?? -1) < 0)
//             continue;
//
//           var isSelected = type == _selected;
//           var content = new GUIContent(nice, type.FullName);
//
//           var prev = GUI.backgroundColor;
//           if (isSelected) GUI.backgroundColor = new Color(0.30f, 0.55f, 0.95f);
//           GUILayout.Toggle(isSelected, content, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
//           GUI.backgroundColor = prev;
//
//           var evt = Event.current;
//           var lastRect = GUILayoutUtility.GetLastRect();
//           if (evt.type == EventType.MouseUp && lastRect.Contains(evt.mousePosition)) {
//             if (_selected != type) {
//               _selected = type;
//               Repaint();
//             }
//             evt.Use();
//           }
//         }
//
//         if (_types.Count == 0) {
//           EditorGUILayout.HelpBox(
//             "当前工程中没有找到 UserSettingsSingleton<> 的非抽象子类。\n" +
//             "新建一个 : UserSettingsSingleton<MySettings> 的类后，点上方 Refresh 即可。",
//             MessageType.Info);
//         }
//       }
//     }
//
//     // ---- 中间：可拖动分隔条 ----
//     private void DrawSplitter() {
//       var rect = GUILayoutUtility.GetRect(SplitterWidth, 0,
//         GUILayout.Width(SplitterWidth), GUILayout.ExpandHeight(true));
//
//       EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.35f));
//
//       var evt = Event.current;
//       var grab = rect;
//       grab.xMin -= 3;
//       grab.xMax += 3;
//
//       if (grab.Contains(evt.mousePosition))
//         EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
//
//       switch (evt.type) {
//       case EventType.MouseDown when grab.Contains(evt.mousePosition):
//         _dragging = true;
//         evt.Use();
//         break;
//       case EventType.MouseUp:
//         _dragging = false; break;
//       case EventType.MouseDrag when _dragging:
//         _leftWidth = Mathf.Clamp(_leftWidth + evt.delta.x,
//           MinLeftWidth, Mathf.Max(MinLeftWidth, position.width - MinRightWidth - SplitterWidth));
//         evt.Use();
//         Repaint();
//         break;
//       }
//     }
//
//     // ---- 右栏：选中项的实时 Inspector ----
//     private void DrawRightPane() {
//       using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
//         if (_selected == null) {
//           EditorGUILayout.Space();
//           EditorGUILayout.HelpBox("从左侧列表选择一个设置项进行编辑。", MessageType.Info);
//           return;
//         }
//
//         var instance = GetInstance(_selected);
//         if (instance == null) {
//           EditorGUILayout.Space();
//           EditorGUILayout.HelpBox("无法获取单例实例：" + _selected.FullName, MessageType.Error);
//           return;
//         }
//
//         if (_cachedEditor == null || _cachedTarget != instance) {
//           DisposeEditor();
//           _cachedEditor = Editor.CreateEditor(instance);
//           _cachedTarget = instance;
//         }
//
//         using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
//           GUILayout.Label(ObjectNames.NicifyVariableName(_selected.Name), EditorStyles.boldLabel);
//           GUILayout.FlexibleSpace();
//           GUILayout.Label("auto-saved", EditorStyles.miniLabel);
//         }
//
//         var view = new EditorGUILayout.ScrollViewScope(_rightScroll, GUILayout.ExpandHeight(true));
//         _rightScroll = view.scrollPosition;
//
//         var serialized = _cachedEditor.serializedObject;
//         using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
//           EditorGUI.BeginChangeCheck();
//           _cachedEditor.DrawHeader();
//           bool hadErrors = _cachedEditor.DrawDefaultInspector();
//           bool changed = EditorGUI.EndChangeCheck() || hadErrors;
//
//           if (changed && serialized.ApplyModifiedProperties()) {
//             Save(instance);
//             Repaint();
//           }
//         }
//
//         EditorGUILayout.Space();
//         using (new EditorGUILayout.HorizontalScope()) {
//           if (GUILayout.Button(new GUIContent("Reset", "还原为字段默认值"), GUILayout.Width(80))) {
//             Undo.RecordObject(instance, "Reset Settings");
//             ResetInstance(instance);
//             Save(instance);
//             Repaint();
//           }
//           if (GUILayout.Button(new GUIContent("Save", "强制保存"), GUILayout.Width(80))) {
//             if (serialized.ApplyModifiedProperties()) Save(instance);
//           }
//         }
//       }
//     }
//
//     private static UnityEngine.Object? GetInstance(Type type) {
//       var prop = type.GetProperty("Instance",
//         BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
//       return prop == null ? null : prop.GetValue(null) as UnityEngine.Object;
//     }
//
//     private static void Save(UnityEngine.Object instance) {
//       var method = instance.GetType().GetMethod("Save",
//         BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
//       if (method != null) method.Invoke(null, null);
//     }
//
//     private static void ResetInstance(UnityEngine.Object instance) {
//       // 用 ScriptableObject 的默认构造值覆盖当前字段。
//       var fresh = ScriptableObject.CreateInstance(instance.GetType());
//       var cur = new SerializedObject(instance);
//
//       var p = new SerializedObject(fresh).GetIterator();
//       bool enter = true;
//       while (p.NextVisible(enter)) {
//         enter = false;
//         var c = cur.FindProperty(p.name);
//         if (c != null && c.propertyType != SerializedPropertyType.Generic)
//           c.serializedObject.CopyFromSerializedProperty(p);
//       }
//       cur.ApplyModifiedProperties();
//       UnityEngine.Object.DestroyImmediate(fresh);
//     }
//
//     private void DisposeEditor() {
//       if (_cachedEditor != null) {
//         UnityEngine.Object.DestroyImmediate(_cachedEditor);
//         _cachedEditor = null;
//         _cachedTarget = null;
//       }
//     }
//
//     private static IEnumerable<Type> DiscoverSettingsTypes() {
//       return ReflectionTools.FindTypes(IsSettingsType);
//     }
//
//     /// <summary>
//     /// 判断某类型是否 UserSettingsSingleton 的非抽象子类
//     /// </summary>
//     public static bool IsSettingsType(Type type) {
//       if (type.IsAbstract) return false;
//       for (var t = type.BaseType; t != null; t = t.BaseType) {
//         if (t.IsGenericType &&
//             t.GetGenericTypeDefinition() == typeof(UserSettingsSingleton<>))
//           return true;
//       }
//       return false;
//     }
//   }
//
// }
