namespace Framework.Kits.ExpressionKits
{
	partial class VariableExpression
	{
		private enum TokenType
		{
			// fixed values
			False,
			True,

			// varying values
			Number,
			Variable,

			// operators
			OpenParen,
			CloseParen,
			Not,
			Negate,
			OnesComplement,
			And,
			Or,
			Equals,
			NotEquals,
			LessThan,
			LessThanOrEqual,
			GreaterThan,
			GreaterThanOrEqual,
			Add,
			Subtract,
			Multiply,
			Divide,
			Modulo,

			Invalid,
		}
	}
}