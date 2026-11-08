using UnityEditorInternal;
using UnityEngine;

namespace EditorTools {

  public abstract class UserSettingsSingleton<T> : ScriptableObject where T : UserSettingsSingleton<T> {
    private static T _instance = null!;

    public static T Instance {
      get {
        if (!_instance) {
          var objects = InternalEditorUtility.LoadSerializedFileAndForget(GetFilePath());

          if (objects.Length > 0 && objects[0] is T settings)
            _instance = settings;

          if (!_instance) {
            _instance = CreateInstance<T>();
            Save();
          }
        }

        return _instance;
      }
    }

    public static void Save() {
      if (!_instance) return;

      InternalEditorUtility.SaveToSerializedFileAndForget(
        new Object[] { _instance },
        GetFilePath(),
        true);
    }

    protected static string GetFilePath() {
      return "UserSettings/" + typeof(T).Name + ".asset";
    }
  }

}
