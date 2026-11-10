using System.Globalization;
using System.Text;

namespace EditorTools {

  /// <summary>
  /// 字符串相关的扩展方法。
  /// </summary>
  public static class StringExtensions {
    /// <summary>
    /// 移除下划线并将紧随其后的字母转为大写，用于把下划线命名转为驼峰形式。
    /// </summary>
    public static string ToTitleCase(this string input) {
      var builder = new StringBuilder();
      for (int i = 0; i < input.Length; i++) {
        var current = input[i];
        // 遇到下划线时跳过它，并把下一个字符转为大写
        if (current == '_' && i + 1 < input.Length) {
          var next = input[i + 1];
          if (char.IsLower(next)) {
            next = char.ToUpper(next, CultureInfo.InvariantCulture);
          }

          builder.Append(next);
          i++;
        } else {
          builder.Append(current);
        }
      }

      return builder.ToString();
    }
  }

}
