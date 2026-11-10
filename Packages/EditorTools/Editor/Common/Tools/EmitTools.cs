#pragma warning disable

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace EditorTools {

  // 字段/属性访问委托：实例以 ref 传入以兼容结构体；Weak 表示参数为 object，Value 表示参数为强类型
  public delegate object WeakValueGetter(ref object instance);
  public delegate void WeakValueSetter(ref object instance, object value);
  public delegate FieldType WeakValueGetter<FieldType>(ref object instance);
  public delegate void WeakValueSetter<FieldType>(ref object instance, FieldType value);
  public delegate FieldType ValueGetter<InstanceType, FieldType>(ref InstanceType instance);
  public delegate void ValueSetter<InstanceType, FieldType>(ref InstanceType instance, FieldType value);

  /// <summary>
  /// 基于 Reflection.Emit 生成字段、属性、方法的访问与调用委托；Unity 编辑器与引擎程序集的成员自动回退为反射调用。
  /// </summary>
  public static class EmitTools {
    // 这两个程序集中的成员不允许用 Emit 直接访问
    private static Assembly EditorAssembly = typeof(UnityEditor.Editor).Assembly;
    private static Assembly EngineAssembly = typeof(UnityEngine.Object).Assembly;

    // 判断成员是否禁止 Emit：声明类型位于上述程序集且不是 Attribute 派生类
    private static bool EmitIsIllegalForMember(MemberInfo member) {
      return member.DeclaringType != null && (member.DeclaringType.Assembly == EditorAssembly || member.DeclaringType.Assembly == EngineAssembly) &&
             !typeof(Attribute).IsAssignableFrom(member.DeclaringType);
    }

    /// <summary>
    /// 创建静态字段的强类型读取委托；常量字段直接返回缓存值。
    /// </summary>
    public static Func<FieldType> CreateStaticFieldGetter<FieldType>(FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (!fieldInfo.IsStatic) {
        throw new ArgumentException("Field must be static.");
      }

      if (fieldInfo.IsLiteral) {
        FieldType value = (FieldType)fieldInfo.GetValue(null);
        return () => value;
      }

      // 禁止 Emit 的成员改用反射读取
      if (EmitIsIllegalForMember(fieldInfo)) {
        return () => (FieldType)fieldInfo.GetValue(null);
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".get_" + fieldInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(FieldType), new Type[0], true);
      ILGenerator gen = getterMethod.GetILGenerator();

      gen.Emit(OpCodes.Ldsfld, fieldInfo);
      gen.Emit(OpCodes.Ret);

      return (Func<FieldType>)getterMethod.CreateDelegate(typeof(Func<FieldType>));
    }

    /// <summary>
    /// 创建静态字段的弱类型读取委托，值类型结果装箱为 object。
    /// </summary>
    public static Func<object> CreateWeakStaticFieldGetter(FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (!fieldInfo.IsStatic) {
        throw new ArgumentException("Field must be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return () => fieldInfo.GetValue(null);
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".get_" + fieldInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(object), new Type[0], true);
      ILGenerator gen = getterMethod.GetILGenerator();

      gen.Emit(OpCodes.Ldsfld, fieldInfo);

      // 值类型需装箱后才能作为 object 返回
      if (fieldInfo.FieldType.IsValueType) {
        gen.Emit(OpCodes.Box, fieldInfo.FieldType);
      }

      gen.Emit(OpCodes.Ret);

      return (Func<object>)getterMethod.CreateDelegate(typeof(Func<object>));
    }


    /// <summary>
    /// 创建静态字段的强类型写入委托；常量字段不可写入。
    /// </summary>
    public static Action<FieldType> CreateStaticFieldSetter<FieldType>(FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (!fieldInfo.IsStatic) {
        throw new ArgumentException("Field must be static.");
      }

      if (fieldInfo.IsLiteral) {
        throw new ArgumentException("Field cannot be constant.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return value => { fieldInfo.SetValue(null, value); };
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".set_" + fieldInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(FieldType) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Stsfld, fieldInfo);
      gen.Emit(OpCodes.Ret);

      return (Action<FieldType>)setterMethod.CreateDelegate(typeof(Action<FieldType>));
    }

    /// <summary>
    /// 创建静态字段的弱类型写入委托，按字段类型拆箱或转型。
    /// </summary>
    public static Action<object> CreateWeakStaticFieldSetter(FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (!fieldInfo.IsStatic) {
        throw new ArgumentException("Field must be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return value => { fieldInfo.SetValue(null, value); };
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".set_" + fieldInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(object) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      gen.Emit(OpCodes.Ldarg_0);

      // object 参数按字段实际类型还原：值类型拆箱，引用类型转型
      if (fieldInfo.FieldType.IsValueType) {
        gen.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
      } else {
        gen.Emit(OpCodes.Castclass, fieldInfo.FieldType);
      }

      gen.Emit(OpCodes.Stsfld, fieldInfo);
      gen.Emit(OpCodes.Ret);

      return (Action<object>)setterMethod.CreateDelegate(typeof(Action<object>));
    }


    /// <summary>
    /// 创建实例字段的强类型读取委托，兼容结构体与引用类型实例。
    /// </summary>
    public static ValueGetter<InstanceType, FieldType> CreateInstanceFieldGetter<InstanceType, FieldType>(FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (fieldInfo.IsStatic) {
        throw new ArgumentException("Field cannot be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return (ref InstanceType classInstance) => (FieldType)fieldInfo.GetValue(classInstance);
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".get_" + fieldInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(FieldType), new[] { typeof(InstanceType).MakeByRefType() }, true);
      ILGenerator gen = getterMethod.GetILGenerator();

      // 结构体实例参数本身就是地址，可直读取字段；引用类型需先解引用
      if (typeof(InstanceType).IsValueType) {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldfld, fieldInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Ldfld, fieldInfo);
      }

      gen.Emit(OpCodes.Ret);

      return (ValueGetter<InstanceType, FieldType>)getterMethod.CreateDelegate(typeof(ValueGetter<InstanceType, FieldType>));
    }

    /// <summary>
    /// 创建实例字段的弱类型读取委托（返回值强类型，实例为 object）。
    /// </summary>
    public static WeakValueGetter<FieldType> CreateWeakInstanceFieldGetter<FieldType>(Type instanceType, FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (instanceType == null) {
        throw new ArgumentNullException(nameof(instanceType));
      }

      if (fieldInfo.IsStatic) {
        throw new ArgumentException("Field cannot be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return (ref object classInstance) => (FieldType)fieldInfo.GetValue(classInstance);
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".get_" + fieldInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(FieldType), new[] { typeof(object).MakeByRefType() }, true);
      ILGenerator gen = getterMethod.GetILGenerator();

      // 结构体需拆箱后才能读取字段，引用类型只需转型
      if (instanceType.IsValueType) {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Unbox_Any, instanceType);
        gen.Emit(OpCodes.Ldfld, fieldInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Castclass, instanceType);
        gen.Emit(OpCodes.Ldfld, fieldInfo);
      }

      gen.Emit(OpCodes.Ret);

      return (WeakValueGetter<FieldType>)getterMethod.CreateDelegate(typeof(WeakValueGetter<FieldType>));
    }

    /// <summary>
    /// 创建实例字段的完全弱类型读取委托，字段值装箱为 object。
    /// </summary>
    public static WeakValueGetter CreateWeakInstanceFieldGetter(Type instanceType, FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (instanceType == null) {
        throw new ArgumentNullException(nameof(instanceType));
      }

      if (fieldInfo.IsStatic) {
        throw new ArgumentException("Field cannot be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return (ref object classInstance) => fieldInfo.GetValue(classInstance);
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".get_" + fieldInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(object), new[] { typeof(object).MakeByRefType() }, true);
      ILGenerator gen = getterMethod.GetILGenerator();

      if (instanceType.IsValueType) {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Unbox_Any, instanceType);
        gen.Emit(OpCodes.Ldfld, fieldInfo);

        if (fieldInfo.FieldType.IsValueType) {
          gen.Emit(OpCodes.Box, fieldInfo.FieldType);
        }
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Castclass, instanceType);
        gen.Emit(OpCodes.Ldfld, fieldInfo);

        if (fieldInfo.FieldType.IsValueType) {
          gen.Emit(OpCodes.Box, fieldInfo.FieldType);
        }
      }

      gen.Emit(OpCodes.Ret);

      return (WeakValueGetter)getterMethod.CreateDelegate(typeof(WeakValueGetter));
    }

    /// <summary>
    /// 创建实例字段的强类型写入委托；结构体实例通过 ref 写回。
    /// </summary>
    public static ValueSetter<InstanceType, FieldType> CreateInstanceFieldSetter<InstanceType, FieldType>(FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (fieldInfo.IsStatic) {
        throw new ArgumentException("Field cannot be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return (ref InstanceType classInstance, FieldType value) => {
          if (typeof(InstanceType).IsValueType) {
            // 结构体先装箱，否则反射无法把字段修改写回
            object obj = classInstance;
            fieldInfo.SetValue(obj, value);
            // 再把装箱后的结果拆箱写回 ref 参数
            classInstance = (InstanceType)obj;
          } else {
            fieldInfo.SetValue(classInstance, value);
          }
        };
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".set_" + fieldInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(InstanceType).MakeByRefType(), typeof(FieldType) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      if (typeof(InstanceType).IsValueType) {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Stfld, fieldInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Stfld, fieldInfo);
      }

      gen.Emit(OpCodes.Ret);

      return (ValueSetter<InstanceType, FieldType>)setterMethod.CreateDelegate(typeof(ValueSetter<InstanceType, FieldType>));
    }

    /// <summary>
    /// 创建实例字段的弱类型写入委托（值强类型）；结构体修改后重新装箱写回 ref 参数。
    /// </summary>
    public static WeakValueSetter<FieldType> CreateWeakInstanceFieldSetter<FieldType>(Type instanceType, FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (instanceType == null) {
        throw new ArgumentNullException(nameof(instanceType));
      }

      if (fieldInfo.IsStatic) {
        throw new ArgumentException("Field cannot be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return (ref object classInstance, FieldType value) => { fieldInfo.SetValue(classInstance, value); };
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".set_" + fieldInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(object).MakeByRefType(), typeof(FieldType) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      // 结构体：拆箱到局部变量，设置字段后重新装箱写回
      if (instanceType.IsValueType) {
        var local = gen.DeclareLocal(instanceType);

        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldind_Ref); // Load reference
        gen.Emit(OpCodes.Unbox_Any, instanceType); // Unbox to struct
        gen.Emit(OpCodes.Stloc, local); // Set local to struct value
        gen.Emit(OpCodes.Ldloca_S, local); // Load address to local value
        gen.Emit(OpCodes.Ldarg_1); // Load FieldType value
        gen.Emit(OpCodes.Stfld, fieldInfo); // Set field on local struct value
        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldloc, local); // Load local struct value
        gen.Emit(OpCodes.Box, instanceType); // Box local struct
        gen.Emit(OpCodes.Stind_Ref); // Set object reference argument
      } else {
        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldind_Ref); // Load reference
        gen.Emit(OpCodes.Castclass, instanceType); // Cast to instance type
        gen.Emit(OpCodes.Ldarg_1); // Load value argument
        gen.Emit(OpCodes.Stfld, fieldInfo); // Set field
      }

      gen.Emit(OpCodes.Ret);

      return (WeakValueSetter<FieldType>)setterMethod.CreateDelegate(typeof(WeakValueSetter<FieldType>));
    }

    /// <summary>
    /// 创建实例字段的完全弱类型写入委托；结构体修改后重新装箱写回 ref 参数。
    /// </summary>
    public static WeakValueSetter CreateWeakInstanceFieldSetter(Type instanceType, FieldInfo fieldInfo) {
      if (fieldInfo == null) {
        throw new ArgumentNullException(nameof(fieldInfo));
      }

      if (instanceType == null) {
        throw new ArgumentNullException(nameof(instanceType));
      }

      if (fieldInfo.IsStatic) {
        throw new ArgumentException("Field cannot be static.");
      }

      if (EmitIsIllegalForMember(fieldInfo)) {
        return (ref object classInstance, object value) => { fieldInfo.SetValue(classInstance, value); };
      }

      string methodName = fieldInfo.ReflectedType?.FullName + ".set_" + fieldInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(object).MakeByRefType(), typeof(object) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      // 结构体：拆箱到局部变量，写入字段后重新装箱写回
      if (instanceType.IsValueType) {
        var local = gen.DeclareLocal(instanceType);

        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldind_Ref); // Load reference
        gen.Emit(OpCodes.Unbox_Any, instanceType); // Unbox to struct
        gen.Emit(OpCodes.Stloc, local); // Set local to struct value
        gen.Emit(OpCodes.Ldloca_S, local); // Load address to local value
        gen.Emit(OpCodes.Ldarg_1); // Load FieldType value

        if (fieldInfo.FieldType.IsValueType) {
          gen.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
        } else {
          gen.Emit(OpCodes.Castclass, fieldInfo.FieldType);
        }

        gen.Emit(OpCodes.Stfld, fieldInfo); // Set field on local struct value
        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldloc, local); // Load local struct value
        gen.Emit(OpCodes.Box, instanceType); // Box local struct
        gen.Emit(OpCodes.Stind_Ref); // Set object reference argument
      } else {
        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldind_Ref); // Load reference
        gen.Emit(OpCodes.Castclass, instanceType); // Cast to instance type
        gen.Emit(OpCodes.Ldarg_1); // Load value argument

        if (fieldInfo.FieldType.IsValueType) {
          gen.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
        } else {
          gen.Emit(OpCodes.Castclass, fieldInfo.FieldType);
        }

        gen.Emit(OpCodes.Stfld, fieldInfo); // Set field
      }

      gen.Emit(OpCodes.Ret);

      return (WeakValueSetter)setterMethod.CreateDelegate(typeof(WeakValueSetter));
    }

    /// <summary>
    /// 创建实例属性的弱类型读取委托，不支持索引器与静态属性。
    /// </summary>
    public static WeakValueGetter CreateWeakInstancePropertyGetter(Type instanceType, PropertyInfo propertyInfo) {
      if (propertyInfo == null) {
        throw new ArgumentNullException(nameof(propertyInfo));
      }

      if (instanceType == null) {
        throw new ArgumentNullException(nameof(instanceType));
      }

      if (propertyInfo.GetIndexParameters().Length > 0) {
        throw new ArgumentException("Property must not have any index parameters");
      }

      var getMethod = propertyInfo.GetGetMethod(true);

      if (getMethod == null) {
        throw new ArgumentException("Property must have a getter.");
      }

      if (getMethod.IsStatic) {
        throw new ArgumentException("Property cannot be static.");
      }

      if (EmitIsIllegalForMember(propertyInfo)) {
        return (ref object classInstance) => propertyInfo.GetValue(classInstance, null);
      }

      string methodName = propertyInfo.ReflectedType?.FullName + ".get_" + propertyInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(object), new[] { typeof(object).MakeByRefType() }, true);
      ILGenerator gen = getterMethod.GetILGenerator();

      if (instanceType.IsValueType) {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Unbox_Any, instanceType);

        // 虚方法/抽象方法用 Callvirt，否则直接 Call
        if (getMethod.IsVirtual || getMethod.IsAbstract) {
          gen.Emit(OpCodes.Callvirt, getMethod);
        } else {
          gen.Emit(OpCodes.Call, getMethod);
        }

        if (propertyInfo.PropertyType.IsValueType) {
          gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
        }
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Castclass, instanceType);

        if (getMethod.IsVirtual || getMethod.IsAbstract) {
          gen.Emit(OpCodes.Callvirt, getMethod);
        } else {
          gen.Emit(OpCodes.Call, getMethod);
        }

        if (propertyInfo.PropertyType.IsValueType) {
          gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
        }
      }

      gen.Emit(OpCodes.Ret);

      return (WeakValueGetter)getterMethod.CreateDelegate(typeof(WeakValueGetter));
    }

    /// <summary>
    /// 创建实例属性的弱类型写入委托，不支持索引器与静态属性。
    /// </summary>
    public static WeakValueSetter CreateWeakInstancePropertySetter(Type instanceType, PropertyInfo propertyInfo) {
      if (propertyInfo == null) {
        throw new ArgumentNullException(nameof(propertyInfo));
      }

      if (instanceType == null) {
        throw new ArgumentNullException(nameof(instanceType));
      }

      if (propertyInfo.GetIndexParameters().Length > 0) {
        throw new ArgumentException("Property must not have any index parameters");
      }

      var setMethod = propertyInfo.GetSetMethod(true);

      if (setMethod.IsStatic) {
        throw new ArgumentException("Property cannot be static.");
      }

      if (EmitIsIllegalForMember(propertyInfo)) {
        return (ref object classInstance, object value) => { propertyInfo.SetValue(classInstance, value, null); };
      }

      string methodName = propertyInfo.ReflectedType?.FullName + ".set_" + propertyInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(object).MakeByRefType(), typeof(object) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      // 结构体：拆箱到局部变量，调用 setter 后重新装箱写回
      if (instanceType.IsValueType) {
        var local = gen.DeclareLocal(instanceType);

        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldind_Ref); // Load reference
        gen.Emit(OpCodes.Unbox_Any, instanceType); // Unbox to struct
        gen.Emit(OpCodes.Stloc, local); // Set local to struct value
        gen.Emit(OpCodes.Ldloca_S, local); // Load address to local value
        gen.Emit(OpCodes.Ldarg_1); // Load PropertyInfo value

        if (propertyInfo.PropertyType.IsValueType) {
          gen.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
        } else {
          gen.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
        }

        if (setMethod.IsVirtual || setMethod.IsAbstract) {
          gen.Emit(OpCodes.Callvirt, setMethod); // Set property on local struct value
        } else {
          gen.Emit(OpCodes.Call, setMethod); // Set property on local struct value
        }

        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldloc, local); // Load local struct value
        gen.Emit(OpCodes.Box, instanceType); // Box local struct
        gen.Emit(OpCodes.Stind_Ref); // Set object reference argument
      } else {
        gen.Emit(OpCodes.Ldarg_0); // Load object reference argument
        gen.Emit(OpCodes.Ldind_Ref); // Load reference
        gen.Emit(OpCodes.Castclass, instanceType); // Cast to instance type
        gen.Emit(OpCodes.Ldarg_1); // Load value argument

        if (propertyInfo.PropertyType.IsValueType) {
          gen.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
        } else {
          gen.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
        }

        if (setMethod.IsVirtual || setMethod.IsAbstract) {
          gen.Emit(OpCodes.Callvirt, setMethod); // Set property on local struct value
        } else {
          gen.Emit(OpCodes.Call, setMethod); // Set property on local struct value
        }
      }

      gen.Emit(OpCodes.Ret);

      return (WeakValueSetter)setterMethod.CreateDelegate(typeof(WeakValueSetter));
    }

    /// <summary>
    /// 创建静态属性的强类型写入委托。
    /// </summary>
    public static Action<PropType> CreateStaticPropertySetter<PropType>(PropertyInfo propertyInfo) {
      if (propertyInfo == null) {
        throw new ArgumentNullException(nameof(propertyInfo));
      }

      if (propertyInfo.GetIndexParameters().Length > 0) {
        throw new ArgumentException("Property must not have any index parameters");
      }

      MethodInfo setMethod = propertyInfo.GetSetMethod(true);

      if (setMethod == null) {
        throw new ArgumentException("Property must have a set method.");
      }

      if (!setMethod.IsStatic) {
        throw new ArgumentException("Property must be static.");
      }

      if (EmitIsIllegalForMember(propertyInfo)) {
        return value => { propertyInfo.SetValue(null, value, null); };
      }

      string methodName = propertyInfo.ReflectedType?.FullName + ".set_" + propertyInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(PropType) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Call, setMethod);
      gen.Emit(OpCodes.Ret);

      return (Action<PropType>)setterMethod.CreateDelegate(typeof(Action<PropType>));
    }

    /// <summary>
    /// 创建静态属性的强类型读取委托，必要时对返回值装箱。
    /// </summary>
    public static Func<PropType> CreateStaticPropertyGetter<PropType>(PropertyInfo propertyInfo) {
      if (propertyInfo == null) {
        throw new ArgumentNullException(nameof(propertyInfo));
      }

      if (propertyInfo.GetIndexParameters().Length > 0) {
        throw new ArgumentException("Property must not have any index parameters");
      }

      MethodInfo getMethod = propertyInfo.GetGetMethod(true);

      if (getMethod == null) {
        throw new ArgumentException("Property must have a get method.");
      }

      if (!getMethod.IsStatic) {
        throw new ArgumentException("Property must be static.");
      }

      if (EmitIsIllegalForMember(propertyInfo)) {
        return () => (PropType)propertyInfo.GetValue(null, null);
      }

      string methodName = propertyInfo.ReflectedType?.FullName + ".get_" + propertyInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(PropType), new Type[0], true);
      ILGenerator gen = getterMethod.GetILGenerator();

      gen.Emit(OpCodes.Call, getMethod);

      var returnType = propertyInfo.GetReturnType();
      // 属性返回值为值类型而委托返回引用类型时需装箱
      if (returnType.IsValueType && !typeof(PropType).IsValueType) {
        gen.Emit(OpCodes.Box, returnType);
      }

      gen.Emit(OpCodes.Ret);

      return (Func<PropType>)getterMethod.CreateDelegate(typeof(Func<PropType>));
    }

    /// <summary>
    /// 创建实例属性的强类型写入委托。
    /// </summary>
    public static ValueSetter<InstanceType, PropType> CreateInstancePropertySetter<InstanceType, PropType>(PropertyInfo propertyInfo) {
      if (propertyInfo == null) {
        throw new ArgumentNullException(nameof(propertyInfo));
      }

      if (propertyInfo.GetIndexParameters().Length > 0) {
        throw new ArgumentException("Property must not have any index parameters");
      }

      MethodInfo setMethod = propertyInfo.GetSetMethod(true);

      if (setMethod == null) {
        throw new ArgumentException("Property must have a set method.");
      }

      if (setMethod.IsStatic) {
        throw new ArgumentException("Property cannot be static.");
      }

      if (EmitIsIllegalForMember(propertyInfo)) {
        return (ref InstanceType classInstance, PropType value) => {
          if (typeof(InstanceType).IsValueType) {
            // 结构体先装箱，否则反射无法把属性修改写回
            object obj = classInstance;
            propertyInfo.SetValue(obj, value, null);
            // 再把装箱后的结果拆箱写回 ref 参数
            classInstance = (InstanceType)obj;
          } else {
            propertyInfo.SetValue(classInstance, value, null);
          }
        };
      }

      string methodName = propertyInfo.ReflectedType?.FullName + ".set_" + propertyInfo.Name;

      DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(InstanceType).MakeByRefType(), typeof(PropType) }, true);
      ILGenerator gen = setterMethod.GetILGenerator();

      if (typeof(InstanceType).IsValueType) {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Callvirt, setMethod);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Callvirt, setMethod);
      }

      gen.Emit(OpCodes.Ret);

      return (ValueSetter<InstanceType, PropType>)setterMethod.CreateDelegate(typeof(ValueSetter<InstanceType, PropType>));
    }

    /// <summary>
    /// 创建实例属性的强类型读取委托。
    /// </summary>
    public static ValueGetter<InstanceType, PropType> CreateInstancePropertyGetter<InstanceType, PropType>(PropertyInfo propertyInfo) {
      if (propertyInfo == null) {
        throw new ArgumentNullException(nameof(propertyInfo));
      }
      if (propertyInfo.GetIndexParameters().Length > 0) {
        throw new ArgumentException("Property must not have any index parameters");
      }

      MethodInfo getMethod = propertyInfo.GetGetMethod(true);

      if (getMethod == null) {
        throw new ArgumentException("Property must have a get method.");
      }

      if (getMethod.IsStatic) {
        throw new ArgumentException("Property cannot be static.");
      }

      if (EmitIsIllegalForMember(propertyInfo)) {
        return (ref InstanceType classInstance) => (PropType)propertyInfo.GetValue(classInstance, null);
      }

      string methodName = propertyInfo.ReflectedType?.FullName + ".get_" + propertyInfo.Name;

      DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(PropType), new[] { typeof(InstanceType).MakeByRefType() }, true);
      ILGenerator gen = getterMethod.GetILGenerator();

      if (typeof(InstanceType).IsValueType) {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Callvirt, getMethod);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldind_Ref);
        gen.Emit(OpCodes.Callvirt, getMethod);
      }

      gen.Emit(OpCodes.Ret);

      return (ValueGetter<InstanceType, PropType>)getterMethod.CreateDelegate(typeof(ValueGetter<InstanceType, PropType>));
    }

    /// <summary>
    /// 为无参实例方法创建带返回值的强类型委托（直接绑定委托，无需 Emit）。
    /// </summary>
    public static Func<InstanceType, ReturnType> CreateMethodReturner<InstanceType, ReturnType>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      // 无需 Emit：直接创建委托仅比直接调用慢约 10%，远快于 MethodInfo.Invoke（约 600 倍）
      return (Func<InstanceType, ReturnType>)Delegate.CreateDelegate(typeof(Func<InstanceType, ReturnType>), methodInfo);
    }

    /// <summary>
    /// 为无参静态无返回值方法创建调用委托（直接绑定委托，无需 Emit）。
    /// </summary>
    public static Action CreateStaticMethodCaller(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (!methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is an instance method when it has to be static.");
      }

      if (methodInfo.GetParameters().Length > 0) {
        throw new ArgumentException("Given method cannot have any parameters.");
      }

      // 无需 Emit：直接创建委托仅比直接调用慢约 10%，远快于 MethodInfo.Invoke（约 600 倍）
      return (Action)Delegate.CreateDelegate(typeof(Action), methodInfo);
    }

    /// <summary>
    /// 为无参静态有返回值方法创建调用委托（直接绑定委托，无需 Emit）。
    /// </summary>
    public static Func<TResult> CreateStaticMethodCaller<TResult>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (!methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is an instance method when it has to be static.");
      }

      if (methodInfo.GetParameters().Length > 0) {
        throw new ArgumentException("Given method cannot have any parameters.");
      }

      if (methodInfo.ReturnType != typeof(TResult)) {
        throw new ArgumentException($"Given method returns '{methodInfo.ReturnType.Name}' but expected '{typeof(TResult).Name}'.");
      }

      return (Func<TResult>)Delegate.CreateDelegate(typeof(Func<TResult>), methodInfo);
    }

    /// <summary>
    /// 为单参数实例方法创建弱类型调用委托，无返回值。
    /// </summary>
    public static Action<object, TArg1> CreateWeakInstanceMethodCaller<TArg1>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      var parameters = methodInfo.GetParameters();

      if (parameters.Length != 1) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' must have exactly one parameter.");
      }

      if (parameters[0].ParameterType != typeof(TArg1)) {
        throw new ArgumentException("The first parameter of the method '" + methodInfo.Name + "' must be of type " + typeof(TArg1) + ".");
      }

      if (EmitIsIllegalForMember(methodInfo)) {
        return (classInstance, arg) => { methodInfo.Invoke(classInstance, new object[] { arg }); };
      }

      Type declaringType = methodInfo.DeclaringType!;
      string methodName = methodInfo.ReflectedType?.FullName + ".call_" + methodInfo.Name;

      DynamicMethod method = new DynamicMethod(methodName, null, new[] { typeof(object), typeof(TArg1) }, true);
      ILGenerator gen = method.GetILGenerator();

      // 结构体需拆箱并取地址调用，引用类型直接转型后虚调用
      if (declaringType.IsValueType) {
        var loc = gen.DeclareLocal(declaringType);

        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Unbox_Any, declaringType);
        gen.Emit(OpCodes.Stloc, loc);
        gen.Emit(OpCodes.Ldloca_S, loc);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, methodInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Castclass, declaringType);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Callvirt, methodInfo);
      }

      gen.Emit(OpCodes.Ret);

      return (Action<object, TArg1>)method.CreateDelegate(typeof(Action<object, TArg1>));
    }

    /// <summary>
    /// 为无参实例方法创建弱类型调用委托，忽略返回值。
    /// </summary>
    public static Action<object> CreateWeakInstanceMethodCaller(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      if (methodInfo.GetParameters().Length > 0) {
        throw new ArgumentException("Given method cannot have any parameters.");
      }

      if (EmitIsIllegalForMember(methodInfo)) {
        return classInstance => { methodInfo.Invoke(classInstance, null); };
      }

      Type declaringType = methodInfo.DeclaringType!;
      string methodName = methodInfo.ReflectedType?.FullName + ".call_" + methodInfo.Name;

      DynamicMethod method = new DynamicMethod(methodName, null, new[] { typeof(object) }, true);
      ILGenerator gen = method.GetILGenerator();

      if (declaringType.IsValueType) {
        var loc = gen.DeclareLocal(declaringType);

        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Unbox_Any, declaringType);
        gen.Emit(OpCodes.Stloc, loc);
        gen.Emit(OpCodes.Ldloca_S, loc);
        gen.Emit(OpCodes.Call, methodInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Castclass, declaringType);
        gen.Emit(OpCodes.Callvirt, methodInfo);
      }

      // 有返回值时从栈上弹出，因为当前委托不返回结果
      if (methodInfo.ReturnType != typeof(void)) {
        gen.Emit(OpCodes.Pop);
      }

      gen.Emit(OpCodes.Ret);

      return (Action<object>)method.CreateDelegate(typeof(Action<object>));
    }

    /// <summary>
    /// 为单参数实例方法创建弱类型调用委托，返回指定类型结果。
    /// </summary>
    public static Func<object, TArg1, TResult> CreateWeakInstanceMethodCaller<TResult, TArg1>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      if (methodInfo.ReturnType != typeof(TResult)) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' must return type " + typeof(TResult) + ".");
      }

      var parameters = methodInfo.GetParameters();

      if (parameters.Length != 1) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' must have exactly one parameter.");
      }

      if (typeof(TArg1).InheritsFrom(parameters[0].ParameterType) == false) {
        throw new ArgumentException("The first parameter of the method '" + methodInfo.Name + "' must be of type " + typeof(TArg1) + ".");
      }

      if (EmitIsIllegalForMember(methodInfo)) {
        return (classInstance, arg1) => { return (TResult)methodInfo.Invoke(classInstance, new object[] { arg1 }); };
      }

      Type declaringType = methodInfo.DeclaringType!;
      string methodName = methodInfo.ReflectedType?.FullName + ".call_" + methodInfo.Name;

      DynamicMethod method = new DynamicMethod(methodName, typeof(TResult), new[] { typeof(object), typeof(TArg1) }, true);
      ILGenerator gen = method.GetILGenerator();

      if (declaringType.IsValueType) {
        var loc = gen.DeclareLocal(declaringType);

        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Unbox_Any, declaringType);
        gen.Emit(OpCodes.Stloc, loc);
        gen.Emit(OpCodes.Ldloca_S, loc);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, methodInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Castclass, declaringType);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Callvirt, methodInfo);
      }

      gen.Emit(OpCodes.Ret);

      return (Func<object, TArg1, TResult>)method.CreateDelegate(typeof(Func<object, TArg1, TResult>));
    }

    /// <summary>
    /// 为无参实例方法创建弱类型调用委托，返回指定类型结果。
    /// </summary>
    public static Func<object, TResult> CreateWeakInstanceMethodCallerFunc<TResult>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      if (methodInfo.ReturnType != typeof(TResult)) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' must return type " + typeof(TResult) + ".");
      }

      var parameters = methodInfo.GetParameters();

      if (parameters.Length != 0) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' must have no parameter.");
      }

      if (EmitIsIllegalForMember(methodInfo)) {
        return classInstance => (TResult)methodInfo.Invoke(classInstance, null);
      }

      Type declaringType = methodInfo.DeclaringType!;
      string methodName = methodInfo.ReflectedType?.FullName + ".call_" + methodInfo.Name;

      DynamicMethod method = new DynamicMethod(methodName, typeof(TResult), new[] { typeof(object) }, true);
      ILGenerator gen = method.GetILGenerator();

      if (declaringType.IsValueType) {
        var loc = gen.DeclareLocal(declaringType);

        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Unbox_Any, declaringType);
        gen.Emit(OpCodes.Stloc, loc);
        gen.Emit(OpCodes.Ldloca_S, loc);
        gen.Emit(OpCodes.Call, methodInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Castclass, declaringType);
        gen.Emit(OpCodes.Callvirt, methodInfo);
      }

      gen.Emit(OpCodes.Ret);

      return (Func<object, TResult>)method.CreateDelegate(typeof(Func<object, TResult>));
    }

    /// <summary>
    /// 为单参数实例方法创建弱类型调用委托，返回指定类型结果。
    /// </summary>
    public static Func<object, TArg, TResult> CreateWeakInstanceMethodCallerFunc<TArg, TResult>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      if (methodInfo.ReturnType != typeof(TResult)) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' must return type " + typeof(TResult) + ".");
      }

      var parameters = methodInfo.GetParameters();

      if (parameters.Length != 1) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' must have one parameter.");
      }

      if (!parameters[0].ParameterType.IsAssignableFrom(typeof(TArg))) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' has an invalid parameter type.");
      }

      if (EmitIsIllegalForMember(methodInfo)) {
        return (classInstance, arg) => { return (TResult)methodInfo.Invoke(classInstance, new object[] { arg }); };
      }

      Type declaringType = methodInfo.DeclaringType!;
      string methodName = methodInfo.ReflectedType?.FullName + ".call_" + methodInfo.Name;

      DynamicMethod method = new DynamicMethod(methodName, typeof(TResult), new[] { typeof(object), typeof(TArg) }, true);
      ILGenerator gen = method.GetILGenerator();

      if (declaringType.IsValueType) {
        var loc = gen.DeclareLocal(declaringType);

        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Unbox_Any, declaringType);
        gen.Emit(OpCodes.Stloc, loc);
        gen.Emit(OpCodes.Ldloca_S, loc);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, methodInfo);
      } else {
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Castclass, declaringType);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Callvirt, methodInfo);
      }

      gen.Emit(OpCodes.Ret);

      return (Func<object, TArg, TResult>)method.CreateDelegate(typeof(Func<object, TArg, TResult>));
    }

    /// <summary>
    /// 为无参实例方法创建强类型调用委托（直接绑定委托，无需 Emit）。
    /// </summary>
    public static Action<InstanceType> CreateInstanceMethodCaller<InstanceType>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      if (methodInfo.GetParameters().Length > 0) {
        throw new ArgumentException("Given method cannot have any parameters.");
      }

      // 无需 Emit：直接创建委托仅比直接调用慢约 10%，远快于 MethodInfo.Invoke（约 600 倍）
      return (Action<InstanceType>)Delegate.CreateDelegate(typeof(Action<InstanceType>), methodInfo);
    }

    /// <summary>
    /// 为单参数实例方法创建强类型调用委托（直接绑定委托，无需 Emit）。
    /// </summary>
    public static Action<InstanceType, Arg1> CreateInstanceMethodCaller<InstanceType, Arg1>(MethodInfo methodInfo) {
      if (methodInfo == null) {
        throw new ArgumentNullException(nameof(methodInfo));
      }

      if (methodInfo.IsStatic) {
        throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
      }

      if (methodInfo.GetParameters().Length != 1) {
        throw new ArgumentException("Given method must have only one parameter.");
      }

      // 无需 Emit：直接创建委托仅比直接调用慢约 10%，远快于 MethodInfo.Invoke（约 600 倍）
      return (Action<InstanceType, Arg1>)Delegate.CreateDelegate(typeof(Action<InstanceType, Arg1>), methodInfo);
    }
  }

}
