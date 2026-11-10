// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

namespace Scriban.Syntax {

  [ScriptSyntax("increment/decrement expression", "<operator> <expression> or <expression> <operator>")]
  public
    partial class ScriptIncrementDecrementExpression : ScriptUnaryExpression {
    public bool Post { get; set; }

    public override object? Evaluate(TemplateContext context) {
      if (Right is null) return null;

      var increment = Operator == ScriptUnaryOperator.Increment ? 1 : -1;
      var value = Evaluate(context, Right.Span, ScriptUnaryOperator.Plus, context.Evaluate(Right));
      var incrementedValue = ScriptBinaryExpression.Evaluate(context, Right.Span, ScriptBinaryOperator.Add, value, increment);
      context.SetValue(Right, incrementedValue);
      return Post ? value : incrementedValue;
    }

    public override void PrintTo(ScriptPrinter printer) {
      if (Post) {
        printer.Write(Right);
        PrintOperator(printer);
      } else {
        PrintOperator(printer);
        printer.Write(Right);
      }

    }

    private void PrintOperator(ScriptPrinter printer) {
      if (OperatorToken is not null)
        printer.Write(OperatorToken);
      else
        printer.Write(Operator.ToText());
    }
  }

}
