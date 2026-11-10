#if USE_ADDRESSABLES
using System;
using UnityEditor;
using UnityEngine;

namespace EditorTools.AddressableKeyGenerator {

  [Serializable]
  [FilePath("ProjectSettings/EditorTools/AddressableKeyGenerateSettings.asset", FilePathAttribute.Location.ProjectFolder)]
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
    public string[] ignoredDirectories = { };
    
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
  }

}
#endif
