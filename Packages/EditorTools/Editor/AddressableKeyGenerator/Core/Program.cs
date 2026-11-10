using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scriban;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace EditorTools.AddressableKeyGenerator {

  internal static class Program {
    internal static void GenerateAddressableKey() {
      var settings = GenerateSettings.instance;

      var addresses = CollectAddresses(AddressableAssetSettingsDefaultObject.Settings, out var ignoredCount, out var keyCount);

      var renderResult = RenderTemplate(addresses);
      
      // Debug.Log(renderResult);

      PathTools.CreateDirectoryIfNeeded(settings.outputPath);
      File.WriteAllTextAsync(settings.outputPath, renderResult)
        .ContinueWith(_ => { AssetDatabase.ImportAsset(settings.outputPath); });
    }


    #region Collect Addresses

    private static SortedSet<string> CollectAddresses(AddressableAssetSettings assetSettings, out int ignoredCount, out int keyCount) {
      var entries = new List<AddressableAssetEntry>();
      assetSettings.GetAllAssets(entries, true);

      var result = new SortedSet<string>(StringComparer.Ordinal);
      ignoredCount = 0;
      keyCount = 0;

      foreach (var entry in entries) {
        if (entry == null || string.IsNullOrEmpty(entry.address)) continue;
        if (IsIgnored(entry)) {
          // Debug.Log($"Skip {entry.address}");
          ignoredCount++;
          continue;
        }

        result.Add(entry.address);
      }

      return result;
    }

    private static bool IsIgnored(AddressableAssetEntry entry) {
      var settings = GenerateSettings.instance;
      var assetPath = entry.AssetPath;

      if (string.IsNullOrEmpty(assetPath)) return false;

      foreach (var directory in settings.GetIgnoredDirectories()) {
        if (assetPath.StartsWith(directory)) return true;
      }
      
      return false;
    }
    
    #endregion


    #region Render Template

    private static string RenderTemplate(SortedSet<string> addresses) {
      var settings = GenerateSettings.instance;
      var textAsset = PackageAssetLoader.Load<TextAsset>("Editor/AddressableKeyGenerator/Templates/AddressableKey.scriban");
      var template = Template.Parse(textAsset.text);

      return template.Render(new {
        Namespace = settings.nameSpace,
        Classname = settings.classname,
        Addresses = addresses.ToArray(),
      });
    }

    #endregion
  }

}
