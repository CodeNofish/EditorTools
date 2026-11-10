#if UNITY_EDITOR
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Scriban {

  /// <summary>
  /// Scriban导入器，让 .scriban 作为 TextAsset 导入
  /// </summary>
  [ScriptedImporter(2, "scriban")]
  public class ScribanTextImporter : ScriptedImporter {
    public override void OnImportAsset(AssetImportContext ctx) {
      var text = File.ReadAllText(ctx.assetPath);
      var asset = new TextAsset(text);
      ctx.AddObjectToAsset("main", asset);
      ctx.SetMainObject(asset);
    }
  }

}
#endif
