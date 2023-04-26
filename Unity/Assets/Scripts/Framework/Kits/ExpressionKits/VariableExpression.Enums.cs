using System;

namespace Framework.Kits.ExpressionKits
{
	partial class VariableExpression
	{
		private enum CharClass
		{
			Whitespace,
			Operator,
			Mixed,
			Id,
			Digit,
		}

		private enum Associativity
		{
			Left,
			Right,
		}

		[Flags]
		private enum Sides
		{
			// Value type
			None = 0,

			// Postfix unary operator and/or group closer
			Left = 1,

			// Prefix unary operator and/or group opener
			Right = 2,

			// Binary+ operator
			Both = Left | Right,
		}

		private enum Grouping
		{
			None,
			Parens,
		}

		private enum Precedence
		{
			Unary = 16,
			Multiplication = 12,
			Addition = 11,
			Relation = 9,
			Equality = 8,
			And = 4,
			Or = 3,
			Binary = 0,
			Value = 0,
			Parens = -1,
			Invalid = ~0,
		}

		private enum ExpressionType
		{
			Int,
			Bool,
		}
	}
}