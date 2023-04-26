namespace Framework.Kits.ExpressionKits
{
	public abstract partial class VariableExpression
	{
		private class NumberToken : Token
		{
			public readonly int Value;

			public override string Symbol { get; }

			public NumberToken(int index, string symbol)
				: base(TokenType.Number, index) {
				Value = int.Parse(symbol);
				this.Symbol = symbol;
			}
		}
	}
}