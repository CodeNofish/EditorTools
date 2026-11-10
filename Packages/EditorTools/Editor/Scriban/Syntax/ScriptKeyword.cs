// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax {

  /// <summary>
  /// A verbatim node (use for custom parsing).
  /// </summary>
  public
    partial class ScriptKeyword : ScriptVerbatim {
    public static ScriptKeyword This() {
      return new ScriptKeyword("this");
    }

    public static ScriptKeyword Func() {
      return new ScriptKeyword("func");
    }

    public static ScriptKeyword Do() {
      return new ScriptKeyword("do");
    }

    public static ScriptKeyword Break() {
      return new ScriptKeyword("break");
    }

    public static ScriptKeyword Capture() {
      return new ScriptKeyword("capture");
    }

    public static ScriptKeyword Case() {
      return new ScriptKeyword("case");
    }

    public static ScriptKeyword Continue() {
      return new ScriptKeyword("continue");
    }

    public static ScriptKeyword Else() {
      return new ScriptKeyword("else");
    }

    public static ScriptKeyword End() {
      return new ScriptKeyword("end");
    }

    public static ScriptKeyword If() {
      return new ScriptKeyword("if");
    }

    public static ScriptKeyword In() {
      return new ScriptKeyword("in");
    }

    public static ScriptKeyword For() {
      return new ScriptKeyword("for");
    }

    public static ScriptKeyword Import() {
      return new ScriptKeyword("import");
    }

    public static ScriptKeyword ReadOnly() {
      return new ScriptKeyword("readonly");
    }

    public static ScriptKeyword Ret() {
      return new ScriptKeyword("ret");
    }

    public static ScriptKeyword TableRow() {
      return new ScriptKeyword("tablerow");
    }

    public static ScriptKeyword When() {
      return new ScriptKeyword("when");
    }

    public static ScriptKeyword While() {
      return new ScriptKeyword("while");
    }

    public static ScriptKeyword With() {
      return new ScriptKeyword("with");
    }

    public static ScriptKeyword Wrap() {
      return new ScriptKeyword("wrap");
    }

    public ScriptKeyword() { }

    public ScriptKeyword(string value) : base(value) {
      Value = value;
    }
  }

}
