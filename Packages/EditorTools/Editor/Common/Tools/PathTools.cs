using System;
using System.IO;

namespace EditorTools {

  public static class PathTools {
    public static void CreateDirectoryIfNeeded(string path) {
      string directory = Path.GetDirectoryName(path) ?? string.Empty;
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
        Directory.CreateDirectory(directory);
      }
    }

    public static string Normalize(string path) {
      return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/').Trim('/');
    }

    public static bool IsUnder(string path, string dir) {
      var normalizedPath = Normalize(path);
      var normalizedDir = Normalize(dir);
      if (normalizedPath.Length == 0 || normalizedDir.Length == 0) return false;

      return normalizedPath.Equals(normalizedDir, StringComparison.OrdinalIgnoreCase)
             || normalizedPath.StartsWith(normalizedDir + "/", StringComparison.OrdinalIgnoreCase);
    }
  }

}
