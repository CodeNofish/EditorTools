using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace EditorTools {

  /// <summary>
  /// 方法信息相关的反射扩展方法：生成方法签名与判断扩展方法。
  /// </summary>
  public static class MethodInfoExtensions {
    /// <summary>
    /// 拼接方法的完整名称，包含扩展方法前缀、泛型实参与形参列表。
    /// </summary>
    public static string GetFullName(this MethodBase method, string extensionMethodPrefix = "[ext] ") {
      var builder = new StringBuilder();
      bool isExtensionMethod = method.IsExtensionMethod();

      if (isExtensionMethod) {
        builder.Append(extensionMethodPrefix);
      }

      builder.Append(method.Name);

      if (method.IsGenericMethod) {
        var genArgs = method.GetGenericArguments();
        builder.Append("<");
        for (int i = 0; i < genArgs.Length; i++) {
          if (i != 0) builder.Append(", ");

          builder.Append(genArgs[i].GetNiceName());
        }
        builder.Append(">");
      }

      builder.Append("(");
      builder.Append(method.GetParamsNames());
      builder.Append(")");
      return builder.ToString();
    }

    /// <summary>
    /// 拼接形参列表（类型 + 名称），扩展方法会跳过首个 this 参数。
    /// </summary>
    public static string GetParamsNames(this MethodBase method) {
      ParameterInfo[] pinfos = method.IsExtensionMethod() ? method.GetParameters().Skip(1).ToArray() : method.GetParameters();
      var builder = new StringBuilder();

      for (int i = 0, len = pinfos.Length; i < len; i++) {
        var param = pinfos[i];
        var paramTypeName = param.ParameterType.GetNiceName();
        builder.Append(paramTypeName);
        builder.Append(" ");
        builder.Append(param.Name);

        if (i < len - 1) {
          builder.Append(", ");
        }
      }

      return builder.ToString();
    }

    /// <summary>
    /// 判断是否为扩展方法：声明在非泛型、非嵌套的静态类中且带 ExtensionAttribute。
    /// </summary>
    public static bool IsExtensionMethod(this MethodBase method) {
      var type = method.DeclaringType;
      return type != null &&
             type.IsSealed &&
             !type.IsGenericType &&
             !type.IsNested &&
             method.IsDefined(typeof(ExtensionAttribute), false);
    }
  }

}
