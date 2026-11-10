using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace EditorTools {

  /// <summary>
  /// 成员信息相关的反射扩展方法：特性查询、易读名称与静态判断。
  /// </summary>
  public static class MemberInfoExtensions {
    /// <summary>
    /// 判断成员是否标记了指定特性，反射异常时返回 false。
    /// </summary>
    public static bool IsDefined<T>(this ICustomAttributeProvider member, bool inherit) where T : Attribute {
      try {
        return member.IsDefined(typeof(T), inherit);
      } catch {
        return false;
      }
    }
    
    /// <summary>
    /// 判断成员是否标记了指定特性，不沿继承链查找。
    /// </summary>
    public static bool IsDefined<T>(this ICustomAttributeProvider member) where T : Attribute {
      return IsDefined<T>(member, false);
    }
    
    /// <summary>
    /// 获取成员上第一个指定类型的特性，不存在则返回 null。
    /// </summary>
    public static T? GetAttribute<T>(this ICustomAttributeProvider member, bool inherit) where T : Attribute {
      var all = GetAttributes<T>(member, inherit).ToArray();
      return all.Length == 0 ? null : all[0];
    }

    /// <summary>
    /// 获取成员上第一个指定类型的特性，不沿继承链查找。
    /// </summary>
    public static T? GetAttribute<T>(this ICustomAttributeProvider member) where T : Attribute {
      return GetAttribute<T>(member, false);
    }

    /// <summary>
    /// 获取成员上全部指定类型的特性，不沿继承链查找。
    /// </summary>
    public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider member) where T : Attribute {
      return GetAttributes<T>(member, false);
    }

    /// <summary>
    /// 获取成员上全部指定类型的特性，反射异常时返回空集合。
    /// </summary>
    public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider member, bool inherit) where T : Attribute {
      try {
        return member.GetCustomAttributes(typeof(T), inherit).Cast<T>();
      } catch {
        return Array.Empty<T>();
      }
    }
    
    /// <summary>
    /// 获取成员上的全部特性，不沿继承链查找。
    /// </summary>
    public static Attribute[] GetAttributes(this ICustomAttributeProvider member) {
      try {
        return member.GetAttributes<Attribute>().ToArray();
      } catch {
        return Array.Empty<Attribute>();
      }
    }
    
    /// <summary>
    /// 获取成员上的全部特性，反射异常时返回空数组。
    /// </summary>
    public static Attribute[] GetAttributes(this ICustomAttributeProvider member, bool inherit) {
      try {
        return member.GetAttributes<Attribute>(inherit).ToArray();
      } catch {
        return Array.Empty<Attribute>();
      }
    }
    
    /// <summary>
    /// 获取易读的成员名称：方法使用完整签名，其余成员使用名称，并转为标题格式。
    /// </summary>
    public static string GetNiceName(this MemberInfo member) {
      var method = member as MethodBase;
      string result;
      if (method != null) {
        result = method.GetFullName();
      } else {
        result = member.Name;
      }

      return result.ToTitleCase();
    }
    
    /// <summary>
    /// 判断字段、属性、方法、事件或类型是否为静态，其他成员类型将抛出异常。
    /// </summary>
    public static bool IsStatic(this MemberInfo member) {
      // 按成员的实际类型分别取出静态标志
      var field = member as FieldInfo;
      if (field != null) {
        return field.IsStatic;
      }

      var property = member as PropertyInfo;
      if (property != null) {
        return property.CanRead ? property.GetGetMethod(true).IsStatic : property.GetSetMethod(true).IsStatic;
      }

      var method = member as MethodBase;
      if (method != null) {
        return method.IsStatic;
      }

      var @event = member as EventInfo;
      if (@event != null) {
        return @event.GetRaiseMethod(true).IsStatic;
      }

      // 类型成员：静态类同时是密封且抽象的
      var type = member as Type;
      if (type != null) {
        return type.IsSealed && type.IsAbstract;
      }

      string message = string.Format(
        CultureInfo.InvariantCulture,
        "Unable to determine IsStatic for member {0}.{1}" +
        "MemberType was {2} but only fields, properties, methods, events and types are supported.",
        member.DeclaringType?.FullName,
        member.Name,
        member.GetType().FullName);

      throw new NotSupportedException(message);
    }
  }

}
