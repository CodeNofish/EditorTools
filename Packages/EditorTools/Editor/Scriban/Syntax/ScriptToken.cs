// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Syntax {

  /// <summary>
  /// A verbatim node (use for custom parsing).
  /// </summary>
  public
    partial class ScriptToken : ScriptVerbatim {
    public static ScriptToken SemiColon() {
      return new ScriptToken(TokenType.SemiColon);
    }

    public static ScriptToken Arroba() {
      return new ScriptToken(TokenType.Arroba);
    }

    public static ScriptToken Caret() {
      return new ScriptToken(TokenType.Caret);
    }

    public static ScriptToken DoubleCaret() {
      return new ScriptToken(TokenType.DoubleCaret);
    }

    public static ScriptToken Colon() {
      return new ScriptToken(TokenType.Colon);
    }

    public static ScriptToken Equal() {
      return new ScriptToken(TokenType.Equal);
    }

    public static ScriptToken Pipe() {
      return new ScriptToken(TokenType.VerticalBar);
    }

    public static ScriptToken PipeGreater() {
      return new ScriptToken(TokenType.PipeGreater);
    }

    public static ScriptToken Exclamation() {
      return new ScriptToken(TokenType.Exclamation);
    }

    public static ScriptToken DoubleAmp() {
      return new ScriptToken(TokenType.DoubleAmp);
    }

    public static ScriptToken DoublePipe() {
      return new ScriptToken(TokenType.DoubleVerticalBar);
    }

    public static ScriptToken Amp() {
      return new ScriptToken(TokenType.Amp);
    }

    public static ScriptToken Question() {
      return new ScriptToken(TokenType.Question);
    }

    public static ScriptToken DoubleQuestion() {
      return new ScriptToken(TokenType.DoubleQuestion);
    }

    public static ScriptToken QuestionExclamation() {
      return new ScriptToken(TokenType.QuestionExclamation);
    }

    public static ScriptToken CompareEqual() {
      return new ScriptToken(TokenType.DoubleEqual);
    }

    public static ScriptToken CompareNotEqual() {
      return new ScriptToken(TokenType.ExclamationEqual);
    }

    public static ScriptToken CompareLess() {
      return new ScriptToken(TokenType.Less);
    }

    public static ScriptToken CompareGreater() {
      return new ScriptToken(TokenType.Greater);
    }

    public static ScriptToken CompareLessOrEqual() {
      return new ScriptToken(TokenType.LessEqual);
    }

    public static ScriptToken CompareGreaterOrEqual() {
      return new ScriptToken(TokenType.GreaterEqual);
    }

    public static ScriptToken Divide() {
      return new ScriptToken(TokenType.Divide);
    }

    public static ScriptToken DivideEqual() {
      return new ScriptToken(TokenType.DivideEqual);
    }

    public static ScriptToken DoubleDivide() {
      return new ScriptToken(TokenType.DoubleDivide);
    }

    public static ScriptToken DoubleDivideEqual() {
      return new ScriptToken(TokenType.DoubleDivideEqual);
    }

    public static ScriptToken Star() {
      return new ScriptToken(TokenType.Asterisk);
    }

    public static ScriptToken StarEqual() {
      return new ScriptToken(TokenType.AsteriskEqual);
    }

    public static ScriptToken Plus() {
      return new ScriptToken(TokenType.Plus);
    }

    public static ScriptToken PlusEqual() {
      return new ScriptToken(TokenType.PlusEqual);
    }

    public static ScriptToken Minus() {
      return new ScriptToken(TokenType.Minus);
    }

    public static ScriptToken MinusEqual() {
      return new ScriptToken(TokenType.MinusEqual);
    }

    public static ScriptToken Modulus() {
      return new ScriptToken(TokenType.Percent);
    }

    public static ScriptToken ModulusEqual() {
      return new ScriptToken(TokenType.PercentEqual);
    }

    public static ScriptToken DoubleLess() {
      return new ScriptToken(TokenType.DoubleLessThan);
    }

    public static ScriptToken DoubleGreater() {
      return new ScriptToken(TokenType.DoubleGreaterThan);
    }

    public static ScriptToken Comma() {
      return new ScriptToken(TokenType.Comma);
    }

    public static ScriptToken Dot() {
      return new ScriptToken(TokenType.Dot);
    }

    public static ScriptToken DoubleDot() {
      return new ScriptToken(TokenType.DoubleDot);
    }

    public static ScriptToken TripleDot() {
      return new ScriptToken(TokenType.TripleDot);
    }

    public static ScriptToken DoubleDotLess() {
      return new ScriptToken(TokenType.DoubleDotLess);
    }

    public static ScriptToken OpenParen() {
      return new ScriptToken(TokenType.OpenParen);
    }

    public static ScriptToken CloseParen() {
      return new ScriptToken(TokenType.CloseParen);
    }

    public static ScriptToken OpenBrace() {
      return new ScriptToken(TokenType.OpenBrace);
    }

    public static ScriptToken CloseBrace() {
      return new ScriptToken(TokenType.CloseBrace);
    }

    public static ScriptToken OpenBracket() {
      return new ScriptToken(TokenType.OpenBracket);
    }

    public static ScriptToken CloseBracket() {
      return new ScriptToken(TokenType.CloseBracket);
    }

    public static ScriptToken OpenInterpBrace() {
      return new ScriptToken(TokenType.OpenInterpolatedBrace);
    }

    public static ScriptToken CloseInterpBrace() {
      return new ScriptToken(TokenType.CloseInterpolatedBrace);
    }

    public ScriptToken() { }

    public ScriptToken(TokenType type) {
      TokenType = type;
      Value = type.ToText() ?? string.Empty;
    }

    public TokenType TokenType { get; set; }
  }

}
