using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EditorTools {

  /// <summary>
  /// 类型相关的反射扩展方法：成员类型、泛型接口与继承关系判断。
  /// </summary>
  public static class TypeExtensions {
    // 常用的泛型集合接口，数组需要按这两个接口特殊处理
    private static readonly Type GenericListInterface = typeof(IList<>);
    private static readonly Type GenericCollectionInterface = typeof(ICollection<>);

    /// <summary>
    /// 获取成员的值/返回类型：字段、属性、方法、事件各取对应类型，无法识别时返回 null。
    /// </summary>
    public static Type? GetReturnType(this MemberInfo memberInfo) {
      var fieldInfo = memberInfo as FieldInfo;
      if (fieldInfo != null) {
        return fieldInfo.FieldType;
      }

      var propertyInfo = memberInfo as PropertyInfo;
      if (propertyInfo != null) {
        return propertyInfo.PropertyType;
      }

      var methodInfo = memberInfo as MethodInfo;
      if (methodInfo != null) {
        return methodInfo.ReturnType;
      }

      var eventInfo = memberInfo as EventInfo;
      if (eventInfo != null) {
        return eventInfo.EventHandlerType;
      }
      return null;
    }

    /// <summary>
    /// 判断类型是否实现了指定的开放泛型接口（如 IList 泛型接口定义）。
    /// </summary>
    public static bool ImplementsOpenGenericInterface(this Type candidateType, Type openGenericInterfaceType) {
      if (candidateType == openGenericInterfaceType)
        return true;

      if (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericInterfaceType)
        return true;

      var interfaces = candidateType.GetInterfaces();

      for (int i = 0; i < interfaces.Length; i++) {
        if (interfaces[i].ImplementsOpenGenericInterface(openGenericInterfaceType))
          return true;
      }

      return false;
    }

    /// <summary>
    /// 获取类型从指定开放泛型接口继承的泛型实参，数组按列表/集合接口特殊处理。
    /// </summary>
    public static Type[] GetArgumentsOfInheritedOpenGenericInterface(this Type candidateType, Type openGenericInterfaceType) {
      if ((openGenericInterfaceType == GenericListInterface || openGenericInterfaceType == GenericCollectionInterface) && candidateType.IsArray) {
        return new[] { candidateType.GetElementType() };
      }

      if (candidateType == openGenericInterfaceType)
        return candidateType.GetGenericArguments();

      if (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericInterfaceType)
        return candidateType.GetGenericArguments();

      var interfaces = candidateType.GetInterfaces();

      for (int i = 0; i < interfaces.Length; i++) {
        var @interface = interfaces[i];
        if (!@interface.IsGenericType) continue;
        return @interface.GetArgumentsOfInheritedOpenGenericInterface(openGenericInterfaceType);
      }

      return new Type[] { };
    }

    /// <summary>
    /// 沿继承链向上枚举全部成员；指定 DeclaredOnly 时只枚举当前类型声明的成员。
    /// </summary>
    public static IEnumerable<MemberInfo> GetAllMembers(this Type type, BindingFlags flags = BindingFlags.Default) {
      Type currentType = type;

      if ((flags & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly) {
        foreach (var member in currentType.GetMembers(flags)) {
          yield return member;
        }
      } else {
        // 逐层向基类遍历，配合 DeclaredOnly 避免成员重复
        flags |= BindingFlags.DeclaredOnly;

        do {
          foreach (var member in currentType.GetMembers(flags)) {
            yield return member;
          }

          currentType = currentType.BaseType;
        } while (currentType != null);
      }
    }

    /// <summary>
    /// 沿继承链枚举指定名称的成员。
    /// </summary>
    public static IEnumerable<MemberInfo> GetAllMembers(this Type type, string name, BindingFlags flags = BindingFlags.Default) {
      foreach (var member in type.GetAllMembers(flags)) {
        if (member.Name != name) continue;
        yield return member;
      }
    }

    /// <summary>
    /// 沿继承链枚举指定类型的成员，忽略 object 基类。
    /// </summary>
    public static IEnumerable<T> GetAllMembers<T>(this Type type, BindingFlags flags = BindingFlags.Default) where T : MemberInfo {
      if (type == null) throw new ArgumentNullException(nameof(type));
      if (type == typeof(object)) yield break;

      Type currentType = type;

      if ((flags & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly) {
        foreach (var member in currentType.GetMembers(flags)) {
          var found = member as T;

          if (found != null) {
            yield return found;
          }
        }
      } else {
        flags |= BindingFlags.DeclaredOnly;

        do {
          foreach (var member in currentType.GetMembers(flags)) {
            var found = member as T;

            if (found != null) {
              yield return found;
            }
          }

          currentType = currentType.BaseType;
        } while (currentType != null);
      }
    }

    /// <summary>
    /// 判断类型是否派生自或实现了 TBase。
    /// </summary>
    public static bool InheritsFrom<TBase>(this Type type) {
      return type.InheritsFrom(typeof(TBase));
    }

    /// <summary>
    /// 判断类型是否派生自或实现了 baseType，并支持开放泛型定义与接口的匹配。
    /// </summary>
    public static bool InheritsFrom(this Type type, Type baseType) {
      if (baseType.IsAssignableFrom(type)) {
        return true;
      }

      if (type.IsInterface && baseType.IsInterface == false) {
        return false;
      }

      if (baseType.IsInterface) {
        return type.GetInterfaces().Contains(baseType);
      }

      // 沿基类链比较，并兼容开放泛型类型定义
      var t = type;
      while (t != null) {
        if (t == baseType) {
          return true;
        }

        if (baseType.IsGenericTypeDefinition && t.IsGenericType && t.GetGenericTypeDefinition() == baseType) {
          return true;
        }

        t = t.BaseType;
      }

      return false;
    }
  }

}
