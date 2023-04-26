using System;
using System.Collections.Generic;

namespace Framework.Kits.ExpressionKits
{
	public class BooleanExpression : VariableExpression
	{
		private readonly Func<IReadOnlyDictionary<string, int>, bool> _asFunction;

		public BooleanExpression(string expression)
			: base(expression) {
			_asFunction = Compile<bool>();
		}

		public bool Evaluate(IReadOnlyDictionary<string, int> symbols) {
			return _asFunction(symbols);
		}
	}
}