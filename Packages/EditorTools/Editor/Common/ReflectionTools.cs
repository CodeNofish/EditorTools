using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EditorTools {

  internal static class ReflectionTools {
    public static IEnumerable<Type> FindTypes(Func<Type, bool> typePredicate) {
      return UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies()
        .SelectMany(SafeGetTypes)
        .Where(typePredicate)
        .OrderBy(t => t.FullName, StringComparer.Ordinal);
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly asm) {
      try {
        return asm.GetTypes();
      } catch (ReflectionTypeLoadException ex) {
        return ex.Types.Where(t => t != null);
      } catch {
        return Enumerable.Empty<Type>();
      }
    }
  }

}
