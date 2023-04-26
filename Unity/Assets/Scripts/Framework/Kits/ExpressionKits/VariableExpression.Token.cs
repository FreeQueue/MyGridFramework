using System.IO;

namespace Framework.Kits.ExpressionKits
{
	public abstract partial class VariableExpression
	{
		private class Token
		{
			public readonly TokenType Type;
			public readonly int Index;

			public virtual string Symbol => TokenTypeInfos[(int)Type].Symbol;

			public int Precedence => (int)TokenTypeInfos[(int)Type].Precedence;
			public Sides OperandSides => TokenTypeInfos[(int)Type].OperandSides;
			public Associativity Associativity => TokenTypeInfos[(int)Type].Associativity;
			public bool LeftOperand => ((int)TokenTypeInfos[(int)Type].OperandSides & (int)Sides.Left) != 0;
			public bool RightOperand => ((int)TokenTypeInfos[(int)Type].OperandSides & (int)Sides.Right) != 0;

			public Grouping Opens => TokenTypeInfos[(int)Type].Opens;
			public Grouping Closes => TokenTypeInfos[(int)Type].Closes;

			public Token(TokenType type, int index) {
				Type = type;
				Index = index;
			}

			private static bool ScanIsNumber(string expression, int start, ref int i) {
				CharClass cc = CharClassOf(expression[i]);

				// Scan forwards until we find an non-digit character
				if (cc == CharClass.Digit) {
					i++;
					for (; i < expression.Length; i++) {
						cc = CharClassOf(expression[i]);
						if (cc != CharClass.Digit) {
							if (cc != CharClass.Whitespace && cc != CharClass.Operator && cc != CharClass.Mixed)
								throw new InvalidDataException(
									$"Number {int.Parse(expression.Substring(start, i - start))} and variable merged at index {start}");

							return true;
						}
					}

					return true;
				}

				return false;
			}

			private static TokenType VariableOrKeyword(string expression, int start, ref int i) {
				if (CharClassOf(expression[i - 1]) == CharClass.Mixed)
					throw new InvalidDataException(
						$"Invalid identifier end character at index {i - 1} for `{expression.Substring(start, i - start)}`");

				return VariableOrKeyword(expression, start, i - start);
			}

			private static TokenType VariableOrKeyword(string expression, int start, int length) {
				var i = start;
				if (length == 4 && expression[i++] == 't' && expression[i++] == 'r' && expression[i++] == 'u'
					&& expression[i] == 'e')
					return TokenType.True;

				if (length == 5 && expression[i++] == 'f' && expression[i++] == 'a' && expression[i++] == 'l'
					&& expression[i++] == 's' && expression[i] == 'e')
					return TokenType.False;

				return TokenType.Variable;
			}

			public static TokenType GetNextType(string expression, ref int i, TokenType lastType = TokenType.Invalid) {
				var start = i;

				switch (expression[i]) {
					case '!':
						i++;
						if (i < expression.Length && expression[i] == '=') {
							i++;
							return TokenType.NotEquals;
						}

						return TokenType.Not;

					case '<':
						i++;
						if (i < expression.Length && expression[i] == '=') {
							i++;
							return TokenType.LessThanOrEqual;
						}

						return TokenType.LessThan;

					case '>':
						i++;
						if (i < expression.Length && expression[i] == '=') {
							i++;
							return TokenType.GreaterThanOrEqual;
						}

						return TokenType.GreaterThan;

					case '=':
						i++;
						if (i < expression.Length && expression[i] == '=') {
							i++;
							return TokenType.Equals;
						}

						throw new InvalidDataException(
							$"Unexpected character '=' at index {start} - should it be `==`?");

					case '&':
						i++;
						if (i < expression.Length && expression[i] == '&') {
							i++;
							return TokenType.And;
						}

						throw new InvalidDataException(
							$"Unexpected character '&' at index {start} - should it be `&&`?");

					case '|':
						i++;
						if (i < expression.Length && expression[i] == '|') {
							i++;
							return TokenType.Or;
						}

						throw new InvalidDataException(
							$"Unexpected character '|' at index {start} - should it be `||`?");

					case '(':
						i++;
						return TokenType.OpenParen;

					case ')':
						i++;
						return TokenType.CloseParen;

					case '~':
						i++;
						return TokenType.OnesComplement;
					case '+':
						i++;
						return TokenType.Add;

					case '-':
						if (++i < expression.Length && ScanIsNumber(expression, start, ref i))
							return TokenType.Number;

						i = start + 1;
						if (IsLeftOperandOrNone(lastType))
							return TokenType.Negate;
						return TokenType.Subtract;

					case '*':
						i++;
						return TokenType.Multiply;

					case '/':
						i++;
						return TokenType.Divide;

					case '%':
						i++;
						return TokenType.Modulo;
				}

				if (ScanIsNumber(expression, start, ref i))
					return TokenType.Number;

				CharClass cc = CharClassOf(expression[start]);

				if (cc != CharClass.Id)
					throw new InvalidDataException($"Invalid character '{expression[i]}' at index {start}");

				// Scan forwards until we find an invalid name character
				for (i = start; i < expression.Length; i++) {
					cc = CharClassOf(expression[i]);
					if (cc == CharClass.Whitespace || cc == CharClass.Operator)
						return VariableOrKeyword(expression, start, ref i);
				}

				return VariableOrKeyword(expression, start, ref i);
			}

			public static Token GetNext(string expression, ref int i, TokenType lastType = TokenType.Invalid) {
				if (i == expression.Length)
					return null;

				// Check and eat whitespace
				var whitespaceBefore = false;
				if (CharClassOf(expression[i]) == CharClass.Whitespace) {
					whitespaceBefore = true;
					while (CharClassOf(expression[i]) == CharClass.Whitespace) {
						if (++i == expression.Length)
							return null;
					}
				}
				else if (lastType == TokenType.Invalid)
					whitespaceBefore = true;
				else if (RequiresWhitespaceAfter(lastType))
					throw new InvalidDataException(
						$"Missing whitespace at index {i}, after `{GetTokenSymbol(lastType)}` operator.");

				var start = i;

				TokenType type = GetNextType(expression, ref i, lastType);
				if (!whitespaceBefore && RequiresWhitespaceBefore(type))
					throw new InvalidDataException(
						$"Missing whitespace at index {i}, before `{GetTokenSymbol(type)}` operator.");

				switch (type) {
					case TokenType.Number:
						return new NumberToken(start, expression.Substring(start, i - start));

					case TokenType.Variable:
						return new VariableToken(start, expression.Substring(start, i - start));

					default:
						return new Token(type, start);
				}
			}
		}
	}
}