using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Framework.Kits.ExpressionKits
{
	partial class VariableExpression
	{
		private class Compiler
		{
			private readonly AstStack _ast = new AstStack();

			public Expression Build(Token[] postfix, ExpressionType resultType) {
				foreach (Token t in postfix) {
					switch (t.Type) {
						case TokenType.And: {
							Expression y = _ast.Pop(ExpressionType.Bool);
							Expression x = _ast.Pop(ExpressionType.Bool);
							_ast.Push(System.Linq.Expressions.Expression.And(x, y));
							continue;
						}

						case TokenType.Or: {
							Expression y = _ast.Pop(ExpressionType.Bool);
							Expression x = _ast.Pop(ExpressionType.Bool);
							_ast.Push(System.Linq.Expressions.Expression.Or(x, y));
							continue;
						}

						case TokenType.NotEquals: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.NotEqual(x, y));
							continue;
						}

						case TokenType.Equals: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.Equal(x, y));
							continue;
						}

						case TokenType.Not: {
							_ast.Push(System.Linq.Expressions.Expression.Not(_ast.Pop(ExpressionType.Bool)));
							continue;
						}

						case TokenType.Negate: {
							_ast.Push(System.Linq.Expressions.Expression.Negate(_ast.Pop(ExpressionType.Int)));
							continue;
						}

						case TokenType.OnesComplement: {
							_ast.Push(System.Linq.Expressions.Expression.OnesComplement(_ast.Pop(ExpressionType.Int)));
							continue;
						}

						case TokenType.LessThan: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.LessThan(x, y));
							continue;
						}

						case TokenType.LessThanOrEqual: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.LessThanOrEqual(x, y));
							continue;
						}

						case TokenType.GreaterThan: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.GreaterThan(x, y));
							continue;
						}

						case TokenType.GreaterThanOrEqual: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.GreaterThanOrEqual(x, y));
							continue;
						}

						case TokenType.Add: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.Add(x, y));
							continue;
						}

						case TokenType.Subtract: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.Subtract(x, y));
							continue;
						}

						case TokenType.Multiply: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							_ast.Push(System.Linq.Expressions.Expression.Multiply(x, y));
							continue;
						}

						case TokenType.Divide: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							BinaryExpression isNotZero = System.Linq.Expressions.Expression.NotEqual(y, Zero);
							BinaryExpression divide = System.Linq.Expressions.Expression.Divide(x, y);
							_ast.Push(IfThenElse(isNotZero, divide, Zero));
							continue;
						}

						case TokenType.Modulo: {
							Expression y = _ast.Pop(ExpressionType.Int);
							Expression x = _ast.Pop(ExpressionType.Int);
							BinaryExpression isNotZero = System.Linq.Expressions.Expression.NotEqual(y, Zero);
							BinaryExpression modulo = System.Linq.Expressions.Expression.Modulo(x, y);
							_ast.Push(IfThenElse(isNotZero, modulo, Zero));
							continue;
						}

						case TokenType.False: {
							_ast.Push(False);
							continue;
						}

						case TokenType.True: {
							_ast.Push(True);
							continue;
						}

						case TokenType.Number: {
							_ast.Push(System.Linq.Expressions.Expression.Constant(((NumberToken)t).Value));
							continue;
						}

						case TokenType.Variable: {
							ConstantExpression symbol
								= System.Linq.Expressions.Expression.Constant(((VariableToken)t).Symbol);
							Func<string, IReadOnlyDictionary<string, int>, int> parseSymbol = ParseSymbol;
							_ast.Push(System.Linq.Expressions.Expression.Call(parseSymbol.Method, symbol,
								SymbolsParam));
							continue;
						}

						default:
							throw new InvalidProgramException(
								$@"ConditionExpression.Compiler.Compile() is missing an expression builder for TokenType.{
									Enum<ExpressionType>.GetValues()[(int)t.Type]}");
					}
				}

				return _ast.Pop(resultType);
			}
		}
	}
}