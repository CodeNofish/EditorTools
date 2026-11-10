using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorTools {

  /// <summary>
  /// 从EditorTools包路径下加载资源
  /// </summary>
  public static class PackageAssetLoader {
    internal static T Load<T>(string packageRelativePath) where T : Object {
      var path = Const.PackagePath + packageRelativePath;
      return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public static T Load<T>(string packageName, string packageRelativePath) where T : Object {
      var path = "Packages/" + packageName + "/" + packageRelativePath;
      return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public static List<string> GetAllAssetPath(string folderPath) {
      var result = new List<string>();

      // "t:Object" 表示匹配所有资源类型
      string[] guids = AssetDatabase.FindAssets("t:Object", new[] { folderPath });

      foreach (var guid in guids) {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (!string.IsNullOrEmpty(path))
          result.Add(path);
      }

      return result;
    }
  }

}
