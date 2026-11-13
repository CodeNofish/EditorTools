using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI {

  internal static class Helper {
    internal static void AttachSheet(VisualElement element, StyleSheet? overrideSheet, string defaultResourceName) {
      var sheet = overrideSheet ? overrideSheet : Resources.Load<StyleSheet>(defaultResourceName);
      if (sheet != null && !element.styleSheets.Contains(sheet)) {
        element.styleSheets.Add(sheet);
      }
    }
  }

}
