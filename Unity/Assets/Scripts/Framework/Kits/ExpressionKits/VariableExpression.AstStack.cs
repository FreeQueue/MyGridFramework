using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Framework.Kits.ExpressionKits
{
	partial class VariableExpression
	{
		private class AstStack
		{
			private readonly List<Expression> _expressions = new List<Expression>();
			private readonly List<ExpressionType> _types = new List<ExpressionType>();

			public ExpressionType PeekType() {
				return _types[^1];
			}

			public Expression Peek(ExpressionType toType) {
				ExpressionType fromType = _types[^1];
				Expression expression = _expressions[^1];
				if (toType == fromType)
					return expression;

				switch (toType) {
					case ExpressionType.Bool:
						return IfThenElse(AsBool(expression), True, False);
					case ExpressionType.Int:
						return IfThenElse(expression, One, Zero);
				}

				throw new InvalidProgramException(
					$@"Unable to convert ExpressionType.{
						Enum<ExpressionType>.GetValues()[(int)fromType]
					} to ExpressionType.{
						Enum<ExpressionType>.GetValues()[(int)toType]
					}");
			}

			public Expression Pop(ExpressionType type) {
				Expression expression = Peek(type);
				_expressions.RemoveAt(_expressions.Count - 1);
				_types.RemoveAt(_types.Count - 1);
				return expression;
			}

			public void Push(Expression expression, ExpressionType type) {
				_expressions.Add(expression);
				switch (type) {
					case ExpressionType.Int when expression.Type != typeof(int):
						throw new InvalidOperationException(
							$"Expected System.Int type instead of {expression.Type} for {expression}");
					case ExpressionType.Bool when expression.Type != typeof(bool):
						throw new InvalidOperationException(
							$"Expected System.Boolean type instead of {expression.Type} for {expression}");
					default:
						_types.Add(type);
						break;
				}
			}

			public void Push(Expression expression) {
				_expressions.Add(expression);
				if (expression.Type == typeof(int))
					_types.Add(ExpressionType.Int);
				else if (expression.Type == typeof(bool))
					_types.Add(ExpressionType.Bool);
				else
					throw new InvalidOperationException($"Unhandled result type {expression.Type} for {expression}");
			}
		}
	}
}