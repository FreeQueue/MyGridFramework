using System;
using System.Collections.Generic;

namespace Framework.Kits.ExpressionKits
{
	public class IntegerExpression : VariableExpression
	{
		private readonly Func<IReadOnlyDictionary<string, int>, int> _asFunction;

		public IntegerExpression(string expression)
			: base(expression) {
			_asFunction = Compile<int>();
		}

		public int Evaluate(IReadOnlyDictionary<string, int> symbols) {
			return _asFunction(symbols);
		}
	}
}