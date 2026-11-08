using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EditorTools {

  /// <summary>
  /// 用户设置中心。
  /// </summary>
  public class UserSettingsHubWindow : EditorWindow {
    private List<Type> _types = new();
    private Vector2 _scroll;
    private string _search = "";

    [MenuItem("Tools/User Settings", false, 1)]
    private static void OpenHub() {
      var w = GetWindow<UserSettingsHubWindow>();
      w.titleContent = new GUIContent("User Settings", "所有 UserSettingsSingleton 子类");
      w.minSize = new Vector2(300, 220);
      w.LoadTypes();
      w.Show();
    }

    private void OnEnable() => LoadTypes();

    private void LoadTypes() {
      _types = DiscoverSettingsTypes().ToList();
      Repaint();
    }

    private void OnGUI() {
      // 顶栏：搜索框 + 刷新
      using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar)) {
        GUILayout.Label("User Settings", EditorStyles.toolbarButton, GUILayout.Width(110));
        _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField,
          GUILayout.MinWidth(80));

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
          LoadTypes();
      }

      using (var sv = new EditorGUILayout.ScrollViewScope(_scroll)) {
        _scroll = sv.scrollPosition;

        // if (_types.Count == 0) {
        //   EditorGUILayout.HelpBox(
        //     "当前工程中没有找到 UserSettingsSingleton<> 的非抽象子类。\n" +
        //     "新建一个 : UserSettingsSingleton<MySettings> 的类后，点上方 Refresh 即可。",
        //     MessageType.Info);
        //   return;
        // }

        const StringComparison flag = StringComparison.Ordinal;
        foreach (var type in _types) {
          var nice = ObjectNames.NicifyVariableName(type.Name);
          if (!string.IsNullOrEmpty(_search) &&
              nice.IndexOf(_search, flag) < 0 &&
              (type.FullName?.IndexOf(_search, flag) ?? -1) < 0)
            continue;

          using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) {
            EditorGUILayout.LabelField(nice, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            if (GUILayout.Button("Open", GUILayout.Width(60)))
              SettingsEditorWindow.Open(type);
          }
        }
      }
    }

    private static IEnumerable<Type> DiscoverSettingsTypes() {
      return ReflectionTools.FindTypes(SettingsEditorWindow.IsSettingsType);
    }
  }

}
