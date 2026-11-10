// using System;
// using NUnit.Framework;
//
// namespace EditorTools.Tests {
//
//   [Serializable]
//   public class CustomUserSettings : UserSettingsSingleton<CustomUserSettings> {
//     public int intValue = 0;
//     public float floatValue = 0;
//     public string stringValue = "";
//   }
//
//   public class UserSettingsSingletonTests {
//     [Test]
//     public void Test_AutoCreate() {
//       var userSettings = CustomUserSettings.Instance;
//       Assert.That(userSettings, Is.Not.Null);
//       Assert.That(userSettings, Is.TypeOf<CustomUserSettings>());
//  
//     }
//
//     [Test]
//     public void Test_Save() {
//       var userSettings = CustomUserSettings.Instance;
//       userSettings.intValue = 1;
//       userSettings.floatValue = 1;
//       userSettings.stringValue = "Hello World";
//       CustomUserSettings.Save();
//       
//       userSettings = CustomUserSettings.Instance;
//       Assert.That(userSettings.intValue, Is.EqualTo(1));
//       Assert.That(userSettings.floatValue, Is.EqualTo(1));
//       Assert.That(userSettings.stringValue, Is.EqualTo("Hello World"));
//     }
//   }
//
// }
