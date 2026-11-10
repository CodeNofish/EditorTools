using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace EditorTools.AddressableKeyGenerator {

  internal static class Program {
    private const string FallbackGroupName = "Ungrouped";

    internal static void GenerateAddressableKey() {
      var settings = GenerateSettings.instance;

      var collectResult = CollectKeys(AddressableAssetSettingsDefaultObject.Settings, out var ignoredCount, out var keyCount);

      var renderResult = RenderTemplate(collectResult);

      PathTools.CreateDirectoryIfNeeded(settings.outputPath);
      File.WriteAllTextAsync(settings.outputPath, renderResult)
        .ContinueWith(_ => { AssetDatabase.ImportAsset(settings.outputPath); });

      Debug.Log(renderResult);
    }


    #region Collect Entry

    private static SortedDictionary<string, SortedSet<string>> CollectKeys(AddressableAssetSettings assetSettings, out int ignoredCount, out int keyCount) {
      var entries = new List<AddressableAssetEntry>();
      assetSettings.GetAllAssets(entries, true);

      var groups = new SortedDictionary<string, SortedSet<string>>();
      ignoredCount = 0;
      keyCount = 0;

      foreach (var entry in entries) {
        if (entry == null || string.IsNullOrEmpty(entry.address)) continue;
        if (IsIgnored(entry)) {
          ignoredCount++;
          continue;
        }

        var groupName = entry.parentGroup == null ? FallbackGroupName : entry.parentGroup.Name;
        if (string.IsNullOrEmpty(groupName)) {
          groupName = FallbackGroupName;
        }

        if (!groups.TryGetValue(groupName, out var keys)) {
          keys = new SortedSet<string>(StringComparer.Ordinal);
          groups.Add(groupName, keys);
        }

        if (keys.Add(entry.address)) {
          keyCount++;
        }
      }

      return groups;
    }

    private static bool IsIgnored(AddressableAssetEntry entry) {
      var settings = GenerateSettings.instance;
      var assetPath = ResolveAssetPath(entry);
      if (!string.IsNullOrEmpty(assetPath)) {
        foreach (var directory in settings.ignoredDirectories) {
          if (PathTools.IsUnder(assetPath, directory)) return true;
        }
      }
      return false;
    }

    private static string ResolveAssetPath(AddressableAssetEntry entry) {
      var current = entry;
      for (var i = 0; current != null && i < 8; i++) {
        if (!string.IsNullOrEmpty(current.AssetPath)) return current.AssetPath;
        current = current.ParentEntry;
      }
      return string.Empty;
    }

    #endregion


    #region Render Template

    private static string RenderTemplate(SortedDictionary<string, SortedSet<string>> groups) {
      var settings = GenerateSettings.instance;
      var textAsset = PackageAssetLoader.Load<TextAsset>("Editor/AddressableKeyGenerator/Templates/AddressableKey.scriban");
      var template = Template.Parse(textAsset.text);

      var model = new ScriptObject();
      model["namespace"] = settings.nameSpace;
      model["classname"] = settings.classname;
      model["groups"] = groups.Select(kv => new {
        name = kv.Key,
        values = kv.Value.ToList(),
        count = kv.Value.Count,
      }).ToList();

      return template.Render(model);
    }

    #endregion
  }

}
