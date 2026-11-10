#if USE_ADDRESSABLES
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools.AddressableKeyGenerator {

  [Serializable]
  [FilePath("ProjectSettings/EditorTools/AddressableKeyGenerator/GenerateSettings.asset", FilePathAttribute.Location.ProjectFolder)]
  internal class GenerateSettings : ScriptableSingleton<GenerateSettings> {
    static GenerateSettings() {
      PathTools.CreateDirectoryIfNeeded(GetFilePath());
    }
    
    /// <summary>
    /// 需要监听的内容根目录(里面全是可寻址资源)
    /// </summary>
    [SerializeField]
    public string contentRoot = "Assets/GameContent";

    /// <summary>
    /// 生成文件输出路径
    /// </summary>
    [SerializeField]
    public string outputPath = "Assets/Scripts/AddressableKeys.cs";

    /// <summary>
    /// 不生成key的目录
    /// </summary>
    [SerializeField]
    public string ignoredDirectories = string.Empty;
    
    /// <summary>
    /// 生成类的命名空间
    /// </summary>
    [SerializeField]
    public string nameSpace = "Core";

    /// <summary>
    /// 生成类的类名
    /// </summary>
    [SerializeField]
    public string classname = "AddressableKeys";

    public string[] GetIgnoredDirectories() {
      if (string.IsNullOrEmpty(ignoredDirectories)) return Array.Empty<string>();
      return ignoredDirectories.Split(',');
    }

    public void CreateIfNeeded() {
      var filePath = GetFilePath();
      if (File.Exists(filePath) == false) {
        PathTools.CreateDirectoryIfNeeded(filePath);
        Save(true);
      }
    }
  }

}
#endif
