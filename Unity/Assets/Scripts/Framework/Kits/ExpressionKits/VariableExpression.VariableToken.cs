namespace Framework.Kits.ExpressionKits
{
	public abstract partial class VariableExpression
	{
		private class VariableToken : Token
		{
			public readonly string Name;

			public override string Symbol => Name;

			public VariableToken(int index, string symbol)
				: base(TokenType.Variable, index) {
				Name = symbol;
			}
		}
	}
}