namespace Framework.Kits.ExpressionKits
{
	partial class VariableExpression
	{
		private readonly struct TokenTypeInfo
		{
			public readonly string Symbol;
			public readonly Precedence Precedence;
			public readonly Sides OperandSides;
			public readonly Sides WhitespaceSides;
			public readonly Associativity Associativity;
			public readonly Grouping Opens;
			public readonly Grouping Closes;

			public TokenTypeInfo(
				string symbol, Precedence precedence, Sides operandSides = Sides.None,
				Associativity associativity = Associativity.Left,
				Grouping opens = Grouping.None, Grouping closes = Grouping.None
			) {
				Symbol = symbol;
				Precedence = precedence;
				OperandSides = operandSides;
				WhitespaceSides = Sides.None;
				Associativity = associativity;
				Opens = opens;
				Closes = closes;
			}

			public TokenTypeInfo(
				string symbol, Precedence precedence, Sides operandSides,
				Sides whitespaceSides,
				Associativity associativity = Associativity.Left,
				Grouping opens = Grouping.None, Grouping closes = Grouping.None
			) {
				Symbol = symbol;
				Precedence = precedence;
				OperandSides = operandSides;
				WhitespaceSides = whitespaceSides;
				Associativity = associativity;
				Opens = opens;
				Closes = closes;
			}

			public TokenTypeInfo(
				string symbol, Precedence precedence, Grouping opens, Grouping closes = Grouping.None,
				Associativity associativity = Associativity.Left
			) {
				Symbol = symbol;
				Precedence = precedence;
				WhitespaceSides = Sides.None;
				OperandSides = opens == Grouping.None
					? (closes == Grouping.None ? Sides.None : Sides.Left)
					: (closes == Grouping.None ? Sides.Right : Sides.Both);
				Associativity = associativity;
				Opens = opens;
				Closes = closes;
			}
		}
	}
}