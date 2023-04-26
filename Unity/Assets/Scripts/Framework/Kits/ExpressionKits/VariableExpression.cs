#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using Expressions = System.Linq.Expressions;

namespace Framework.Kits.ExpressionKits
{
	public abstract partial class VariableExpression
	{
		private static readonly TokenTypeInfo[] TokenTypeInfos = CreateTokenTypeInfoEnumeration().ToArray();
		private static readonly ParameterExpression SymbolsParam =
			Expressions.Expression.Parameter(typeof(IReadOnlyDictionary<string, int>), "symbols");
		private static readonly ConstantExpression Zero = Expressions.Expression.Constant(0);
		private static readonly ConstantExpression One = Expressions.Expression.Constant(1);
		private static readonly ConstantExpression False = Expressions.Expression.Constant(false);
		private static readonly ConstantExpression True = Expressions.Expression.Constant(true);
		
		public static readonly IReadOnlyDictionary<string, int> NoVariables
			= new ReadOnlyDictionary<string, int>(new Dictionary<string, int>());

		public readonly string Expression;
		private readonly HashSet<string> _variables = new HashSet<string>();
		public IEnumerable<string> Variables => _variables;

		private static CharClass CharClassOf(char c) {
			switch (c) {
				case '~':
				case '!':
				case '%':
				case '^':
				case '&':
				case '*':
				case '(':
				case ')':
				case '+':
				case '=':
				case '[':
				case ']':
				case '{':
				case '}':
				case '|':
				case ':':
				case ';':
				case '\'':
				case '"':
				case '<':
				case '>':
				case '?':
				case ',':
				case '/':
					return CharClass.Operator;

				case '.':
				case '$':
				case '-':
				case '@':
					return CharClass.Mixed;

				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return CharClass.Digit;

				// Fast-track normal whitespace
				case ' ':
				case '\t':
				case '\n':
				case '\r':
					return CharClass.Whitespace;

				// Should other whitespace be tested?
				default:
					return char.IsWhiteSpace(c) ? CharClass.Whitespace : CharClass.Id;
			}
		}

		private static IEnumerable<TokenTypeInfo> CreateTokenTypeInfoEnumeration() {
			for (var i = 0; i <= (int)TokenType.Invalid; i++) {
				switch ((TokenType)i) {
					case TokenType.Invalid:
						yield return new TokenTypeInfo("(<INVALID>)", Precedence.Invalid);
						continue;
					case TokenType.False:
						yield return new TokenTypeInfo("false", Precedence.Value);
						continue;
					case TokenType.True:
						yield return new TokenTypeInfo("true", Precedence.Value);
						continue;
					case TokenType.Number:
						yield return new TokenTypeInfo("(<number>)", Precedence.Value);
						continue;
					case TokenType.Variable:
						yield return new TokenTypeInfo("(<variable>)", Precedence.Value);
						continue;
					case TokenType.OpenParen:
						yield return new TokenTypeInfo("(", Precedence.Parens, Grouping.Parens);
						continue;
					case TokenType.CloseParen:
						yield return new TokenTypeInfo(")", Precedence.Parens, Grouping.None, Grouping.Parens);
						continue;
					case TokenType.Not:
						yield return new TokenTypeInfo("!", Precedence.Unary, Sides.Right, Associativity.Right);
						continue;
					case TokenType.OnesComplement:
						yield return new TokenTypeInfo("~", Precedence.Unary, Sides.Right, Associativity.Right);
						continue;
					case TokenType.Negate:
						yield return new TokenTypeInfo("-", Precedence.Unary, Sides.Right, Associativity.Right);
						continue;
					case TokenType.And:
						yield return new TokenTypeInfo("&&", Precedence.And, Sides.Both, Sides.Both);
						continue;
					case TokenType.Or:
						yield return new TokenTypeInfo("||", Precedence.Or, Sides.Both, Sides.Both);
						continue;
					case TokenType.Equals:
						yield return new TokenTypeInfo("==", Precedence.Equality, Sides.Both, Sides.Both);
						continue;
					case TokenType.NotEquals:
						yield return new TokenTypeInfo("!=", Precedence.Equality, Sides.Both, Sides.Both);
						continue;
					case TokenType.LessThan:
						yield return new TokenTypeInfo("<", Precedence.Relation, Sides.Both, Sides.Both);
						continue;
					case TokenType.LessThanOrEqual:
						yield return new TokenTypeInfo("<=", Precedence.Relation, Sides.Both, Sides.Both);
						continue;
					case TokenType.GreaterThan:
						yield return new TokenTypeInfo(">", Precedence.Relation, Sides.Both, Sides.Both);
						continue;
					case TokenType.GreaterThanOrEqual:
						yield return new TokenTypeInfo(">=", Precedence.Relation, Sides.Both, Sides.Both);
						continue;
					case TokenType.Add:
						yield return new TokenTypeInfo("+", Precedence.Addition, Sides.Both, Sides.Both);
						continue;
					case TokenType.Subtract:
						yield return new TokenTypeInfo("-", Precedence.Addition, Sides.Both, Sides.Both);
						continue;
					case TokenType.Multiply:
						yield return new TokenTypeInfo("*", Precedence.Multiplication, Sides.Both, Sides.Both);
						continue;
					case TokenType.Divide:
						yield return new TokenTypeInfo("/", Precedence.Multiplication, Sides.Both, Sides.Both);
						continue;
					case TokenType.Modulo:
						yield return new TokenTypeInfo("%", Precedence.Multiplication, Sides.Both, Sides.Both);
						continue;
				}

				throw new InvalidProgramException(
					$"CreateTokenTypeInfoEnumeration is missing a TokenTypeInfo entry for TokenType.{Enum.GetValues(typeof(TokenType)).GetValue(i)}");
			}
		}


		private static bool HasRightOperand(TokenType type) {
			return ((int)TokenTypeInfos[(int)type].OperandSides & (int)Sides.Right) != 0;
		}

		private static bool IsLeftOperandOrNone(TokenType type) {
			return type == TokenType.Invalid || HasRightOperand(type);
		}

		private static bool RequiresWhitespaceAfter(TokenType type) {
			return ((int)TokenTypeInfos[(int)type].WhitespaceSides & (int)Sides.Right) != 0;
		}

		private static bool RequiresWhitespaceBefore(TokenType type) {
			return ((int)TokenTypeInfos[(int)type].WhitespaceSides & (int)Sides.Left) != 0;
		}

		private static string GetTokenSymbol(TokenType type) {
			return TokenTypeInfos[(int)type].Symbol;
		}

		public VariableExpression(string expression) {
			Expression = expression;
		}

		private Expression Build(ExpressionType resultType) {
			var tokens = new List<Token>();
			var currentOpeners = new Stack<Token>();
			Token lastToken = null;
			for (var i = 0;;) {
				Token token = Token.GetNext(Expression, ref i, lastToken?.Type ?? TokenType.Invalid);
				if (token == null) {
					// Sanity check parsed tree
					if (lastToken == null)
						throw new InvalidDataException("Empty expression");

					// Expressions can't end with a binary or unary prefix operation
					if (lastToken.RightOperand)
						throw new InvalidDataException(
							$"Missing value or sub-expression at end for `{lastToken.Symbol}` operator");

					break;
				}

				if (token.Closes != Grouping.None) {
					if (currentOpeners.Count == 0)
						throw new InvalidDataException($"Unmatched closing parenthesis at index {token.Index}");

					currentOpeners.Pop();
				}

				if (token.Opens != Grouping.None)
					currentOpeners.Push(token);

				if (lastToken == null) {
					// Expressions can't start with a binary or unary postfix operation or closer
					if (token.LeftOperand)
						throw new InvalidDataException(
							$"Missing value or sub-expression at beginning for `{token.Symbol}` operator");
				}
				else {
					// Disallow empty parentheses
					if (lastToken.Opens != Grouping.None && token.Closes != Grouping.None)
						throw new InvalidDataException($"Empty parenthesis at index {lastToken.Index}");

					// Exactly one of two consecutive tokens must take the other's sub-expression evaluation as an operand
					if (lastToken.RightOperand == token.LeftOperand) {
						if (lastToken.RightOperand)
							throw new InvalidDataException(
								$"Missing value or sub-expression or there is an extra operator `{lastToken.Symbol}` at index {lastToken.Index} or `{token.Symbol}` at index {token.Index}");
						throw new InvalidDataException(
							$"Missing binary operation before `{token.Symbol}` at index {token.Index}");
					}
				}

				if (token.Type == TokenType.Variable)
					_variables.Add(token.Symbol);

				tokens.Add(token);
				lastToken = token;
			}

			if (currentOpeners.Count > 0)
				throw new InvalidDataException($"Unclosed opening parenthesis at index {currentOpeners.Peek().Index}");

			return new Compiler().Build(ToPostfix(tokens).ToArray(), resultType);
		}

		protected Func<IReadOnlyDictionary<string, int>, T> Compile<T>() {
			ExpressionType resultType;
			if (typeof(T) == typeof(int))
				resultType = ExpressionType.Int;
			else if (typeof(T) == typeof(bool))
				resultType = ExpressionType.Bool;
			else
				throw new InvalidCastException("Variable expressions can only be int or bool.");

			return Expressions.Expression
				.Lambda<Func<IReadOnlyDictionary<string, int>, T>>(Build(resultType), SymbolsParam).Compile();
		}

		private static int ParseSymbol(string symbol, IReadOnlyDictionary<string, int> symbols) {
			symbols.TryGetValue(symbol, out var value);
			return value;
		}

		private static IEnumerable<Token> ToPostfix(IEnumerable<Token> tokens) {
			var s = new Stack<Token>();
			foreach (Token t in tokens) {
				if (t.Opens != Grouping.None)
					s.Push(t);
				else if (t.Closes != Grouping.None) {
					Token temp;
					while ((temp = s.Pop()).Opens == Grouping.None)
						yield return temp;
				}
				else if (t.OperandSides == Sides.None)
					yield return t;
				else {
					while (s.Count > 0 &&
							((t.Associativity == Associativity.Right && t.Precedence < s.Peek().Precedence)
							|| (t.Associativity == Associativity.Left && t.Precedence <= s.Peek().Precedence)))
						yield return s.Pop();

					s.Push(t);
				}
			}

			while (s.Count > 0)
				yield return s.Pop();
		}


		private static Expression AsBool(Expression expression) {
			return Expressions.Expression.NotEqual(expression, Zero);
		}

		private static Expression IfThenElse(Expression test, Expression ifTrue, Expression ifFalse) {
			return Expressions.Expression.Condition(test, ifTrue, ifFalse);
		}
	}
}