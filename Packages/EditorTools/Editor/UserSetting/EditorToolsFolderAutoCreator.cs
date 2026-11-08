using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools {

  /// <summary>
  /// 自动创建 UserSetting下 EditorTools 目录
  /// </summary>
  [InitializeOnLoad]
  public static class EditorToolsFolderAutoCreator {
    static EditorToolsFolderAutoCreator() {
      EnsureEditorToolsFolder();
    }

    public static void EnsureEditorToolsFolder() {
      string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
      string folderPath = Path.Combine(projectRoot, Const.UserSettings, Const.EditorTools);

      if (!Directory.Exists(folderPath)) {
        Directory.CreateDirectory(folderPath);
        // Debug.Log($"已创建目录：{folderPath}");
      } else {
        // Debug.Log($"目录已存在：{folderPath}");
      }
    }
  }

}
