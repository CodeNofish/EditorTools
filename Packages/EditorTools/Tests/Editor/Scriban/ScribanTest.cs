using NUnit.Framework;
using Scriban;

namespace EditorTools.Tests {

  public class ScribanTest {
    [Test]
    public void Test_1() {
      var tpl = Template.Parse("Hello {{ name }}, your code is {{ code }}.");
      var result = tpl.Render(new { name = "Alice", code = "1234" });
      // => "Hello Alice, your code is 1234."
      Assert.That(result, Is.EqualTo("Hello Alice, your code is 1234."));
    }
  }

}
