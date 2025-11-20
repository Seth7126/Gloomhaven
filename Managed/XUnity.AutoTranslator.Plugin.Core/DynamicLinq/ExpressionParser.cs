using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicLinq;

internal class ExpressionParser
{
	private struct Token
	{
		public TokenId id;

		public string text;

		public int pos;
	}

	private enum TokenId
	{
		Unknown,
		End,
		Identifier,
		StringLiteral,
		IntegerLiteral,
		RealLiteral,
		Exclamation,
		Percent,
		Amphersand,
		OpenParen,
		CloseParen,
		Asterisk,
		Plus,
		Comma,
		Minus,
		Dot,
		Slash,
		Colon,
		LessThan,
		Equal,
		GreaterThan,
		Question,
		OpenBracket,
		CloseBracket,
		Bar,
		ExclamationEqual,
		DoubleAmphersand,
		LessThanEqual,
		LessGreater,
		DoubleEqual,
		GreaterThanEqual,
		DoubleBar
	}

	private interface ILogicalSignatures
	{
		void F(bool x, bool y);

		void F(bool? x, bool? y);
	}

	private interface IArithmeticSignatures
	{
		void F(int x, int y);

		void F(uint x, uint y);

		void F(long x, long y);

		void F(ulong x, ulong y);

		void F(float x, float y);

		void F(double x, double y);

		void F(decimal x, decimal y);

		void F(int? x, int? y);

		void F(uint? x, uint? y);

		void F(long? x, long? y);

		void F(ulong? x, ulong? y);

		void F(float? x, float? y);

		void F(double? x, double? y);

		void F(decimal? x, decimal? y);
	}

	private interface IRelationalSignatures : IArithmeticSignatures
	{
		void F(string x, string y);

		void F(char x, char y);

		void F(DateTime x, DateTime y);

		void F(TimeSpan x, TimeSpan y);

		void F(char? x, char? y);

		void F(DateTime? x, DateTime? y);

		void F(TimeSpan? x, TimeSpan? y);
	}

	private interface IEqualitySignatures : IRelationalSignatures, IArithmeticSignatures
	{
		void F(bool x, bool y);

		void F(bool? x, bool? y);
	}

	private interface IAddSignatures : IArithmeticSignatures
	{
		void F(DateTime x, TimeSpan y);

		void F(TimeSpan x, TimeSpan y);

		void F(DateTime? x, TimeSpan? y);

		void F(TimeSpan? x, TimeSpan? y);
	}

	private interface ISubtractSignatures : IAddSignatures, IArithmeticSignatures
	{
		void F(DateTime x, DateTime y);

		void F(DateTime? x, DateTime? y);
	}

	private interface INegationSignatures
	{
		void F(int x);

		void F(long x);

		void F(float x);

		void F(double x);

		void F(decimal x);

		void F(int? x);

		void F(long? x);

		void F(float? x);

		void F(double? x);

		void F(decimal? x);
	}

	private interface INotSignatures
	{
		void F(bool x);

		void F(bool? x);
	}

	private interface IEnumerableSignatures
	{
		void Where(bool predicate);

		void Any();

		void Any(bool predicate);

		void All(bool predicate);

		void Count();

		void Count(bool predicate);

		void Min(object selector);

		void Max(object selector);

		void Sum(int selector);

		void Sum(int? selector);

		void Sum(long selector);

		void Sum(long? selector);

		void Sum(float selector);

		void Sum(float? selector);

		void Sum(double selector);

		void Sum(double? selector);

		void Sum(decimal selector);

		void Sum(decimal? selector);

		void Average(int selector);

		void Average(int? selector);

		void Average(long selector);

		void Average(long? selector);

		void Average(float selector);

		void Average(float? selector);

		void Average(double selector);

		void Average(double? selector);

		void Average(decimal selector);

		void Average(decimal? selector);
	}

	private class MethodData
	{
		public MethodBase MethodBase;

		public ParameterInfo[] Parameters;

		public Expression[] Args;
	}

	private static readonly Type[] predefinedTypes = new Type[20]
	{
		typeof(object),
		typeof(bool),
		typeof(char),
		typeof(string),
		typeof(sbyte),
		typeof(byte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(float),
		typeof(double),
		typeof(decimal),
		typeof(DateTime),
		typeof(TimeSpan),
		typeof(Guid),
		typeof(Math),
		typeof(Convert)
	};

	private static readonly Expression trueLiteral = Expression.Constant(true);

	private static readonly Expression falseLiteral = Expression.Constant(false);

	private static readonly Expression nullLiteral = Expression.Constant(null);

	private static readonly string keywordIt = "it";

	private static readonly string keywordIif = "iif";

	private static readonly string keywordNew = "new";

	private static Dictionary<string, object> keywords;

	private Dictionary<string, object> symbols;

	private IDictionary<string, object> externals;

	private Dictionary<Expression, string> literals;

	private ParameterExpression it;

	private string text;

	private int textPos;

	private int textLen;

	private char ch;

	private Token token;

	public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		if (keywords == null)
		{
			keywords = CreateKeywords();
		}
		symbols = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		literals = new Dictionary<Expression, string>();
		if (parameters != null)
		{
			ProcessParameters(parameters);
		}
		if (values != null)
		{
			ProcessValues(values);
		}
		text = expression;
		textLen = text.Length;
		SetTextPos(0);
		NextToken();
	}

	private void ProcessParameters(ParameterExpression[] parameters)
	{
		foreach (ParameterExpression parameterExpression in parameters)
		{
			if (!string.IsNullOrEmpty(parameterExpression.Name))
			{
				AddSymbol(parameterExpression.Name, parameterExpression);
			}
		}
		if (parameters.Length == 1 && string.IsNullOrEmpty(parameters[0].Name))
		{
			it = parameters[0];
		}
	}

	private void ProcessValues(object[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			object obj = values[i];
			if (i == values.Length - 1 && obj is IDictionary<string, object>)
			{
				externals = (IDictionary<string, object>)obj;
			}
			else
			{
				AddSymbol("@" + i.ToString(CultureInfo.InvariantCulture), obj);
			}
		}
	}

	private void AddSymbol(string name, object value)
	{
		if (symbols.ContainsKey(name))
		{
			throw ParseError("The identifier '{0}' was defined more than once", name);
		}
		symbols.Add(name, value);
	}

	public Expression Parse(Type resultType)
	{
		int pos = token.pos;
		Expression expression = ParseExpression();
		if (resultType != null && (expression = PromoteExpression(expression, resultType, exact: true)) == null)
		{
			throw ParseError(pos, "Expression of type '{0}' expected", GetTypeName(resultType));
		}
		ValidateToken(TokenId.End, "Syntax error");
		return expression;
	}

	public IEnumerable<DynamicOrdering> ParseOrdering()
	{
		List<DynamicOrdering> list = new List<DynamicOrdering>();
		while (true)
		{
			Expression selector = ParseExpression();
			bool flag = true;
			if (TokenIdentifierIs("asc") || TokenIdentifierIs("ascending"))
			{
				NextToken();
			}
			else if (TokenIdentifierIs("desc") || TokenIdentifierIs("descending"))
			{
				NextToken();
				flag = false;
			}
			list.Add(new DynamicOrdering
			{
				Selector = selector,
				Ascending = flag
			});
			if (token.id != TokenId.Comma)
			{
				break;
			}
			NextToken();
		}
		ValidateToken(TokenId.End, "Syntax error");
		return list;
	}

	private Expression ParseExpression()
	{
		int pos = token.pos;
		Expression expression = ParseLogicalOr();
		if (token.id == TokenId.Question)
		{
			NextToken();
			Expression expr = ParseExpression();
			ValidateToken(TokenId.Colon, "':' expected");
			NextToken();
			Expression expr2 = ParseExpression();
			expression = GenerateConditional(expression, expr, expr2, pos);
		}
		return expression;
	}

	private Expression ParseLogicalOr()
	{
		Expression left = ParseLogicalAnd();
		while (this.token.id == TokenId.DoubleBar || TokenIdentifierIs("or"))
		{
			Token token = this.token;
			NextToken();
			Expression right = ParseLogicalAnd();
			CheckAndPromoteOperands(typeof(ILogicalSignatures), token.text, ref left, ref right, token.pos);
			left = Expression.OrElse(left, right);
		}
		return left;
	}

	private Expression ParseLogicalAnd()
	{
		Expression left = ParseComparison();
		while (this.token.id == TokenId.DoubleAmphersand || TokenIdentifierIs("and"))
		{
			Token token = this.token;
			NextToken();
			Expression right = ParseComparison();
			CheckAndPromoteOperands(typeof(ILogicalSignatures), token.text, ref left, ref right, token.pos);
			left = Expression.AndAlso(left, right);
		}
		return left;
	}

	private Expression ParseComparison()
	{
		Expression left = ParseAdditive();
		while (this.token.id == TokenId.Equal || this.token.id == TokenId.DoubleEqual || this.token.id == TokenId.ExclamationEqual || this.token.id == TokenId.LessGreater || this.token.id == TokenId.GreaterThan || this.token.id == TokenId.GreaterThanEqual || this.token.id == TokenId.LessThan || this.token.id == TokenId.LessThanEqual)
		{
			Token token = this.token;
			NextToken();
			Expression right = ParseAdditive();
			bool flag = token.id == TokenId.Equal || token.id == TokenId.DoubleEqual || token.id == TokenId.ExclamationEqual || token.id == TokenId.LessGreater;
			if (flag && !left.Type.IsValueType && !right.Type.IsValueType)
			{
				if (left.Type != right.Type)
				{
					if (left.Type.IsAssignableFrom(right.Type))
					{
						right = Expression.Convert(right, left.Type);
					}
					else
					{
						if (!right.Type.IsAssignableFrom(left.Type))
						{
							throw IncompatibleOperandsError(token.text, left, right, token.pos);
						}
						left = Expression.Convert(left, right.Type);
					}
				}
			}
			else if (IsEnumType(left.Type) || IsEnumType(right.Type))
			{
				if (left.Type != right.Type)
				{
					Expression expression;
					if ((expression = PromoteExpression(right, left.Type, exact: true)) != null)
					{
						right = expression;
					}
					else
					{
						if ((expression = PromoteExpression(left, right.Type, exact: true)) == null)
						{
							throw IncompatibleOperandsError(token.text, left, right, token.pos);
						}
						left = expression;
					}
				}
			}
			else
			{
				CheckAndPromoteOperands(flag ? typeof(IEqualitySignatures) : typeof(IRelationalSignatures), token.text, ref left, ref right, token.pos);
			}
			switch (token.id)
			{
			case TokenId.Equal:
			case TokenId.DoubleEqual:
				left = GenerateEqual(left, right);
				break;
			case TokenId.ExclamationEqual:
			case TokenId.LessGreater:
				left = GenerateNotEqual(left, right);
				break;
			case TokenId.GreaterThan:
				left = GenerateGreaterThan(left, right);
				break;
			case TokenId.GreaterThanEqual:
				left = GenerateGreaterThanEqual(left, right);
				break;
			case TokenId.LessThan:
				left = GenerateLessThan(left, right);
				break;
			case TokenId.LessThanEqual:
				left = GenerateLessThanEqual(left, right);
				break;
			}
		}
		return left;
	}

	private Expression ParseAdditive()
	{
		Expression left = ParseMultiplicative();
		while (this.token.id == TokenId.Plus || this.token.id == TokenId.Minus || this.token.id == TokenId.Amphersand)
		{
			Token token = this.token;
			NextToken();
			Expression right = ParseMultiplicative();
			TokenId id = token.id;
			if (id != TokenId.Amphersand)
			{
				if (id != TokenId.Plus)
				{
					if (id == TokenId.Minus)
					{
						CheckAndPromoteOperands(typeof(ISubtractSignatures), token.text, ref left, ref right, token.pos);
						left = GenerateSubtract(left, right);
					}
					continue;
				}
				if (left.Type != typeof(string) && right.Type != typeof(string))
				{
					CheckAndPromoteOperands(typeof(IAddSignatures), token.text, ref left, ref right, token.pos);
					left = GenerateAdd(left, right);
					continue;
				}
			}
			left = GenerateStringConcat(left, right);
		}
		return left;
	}

	private Expression ParseMultiplicative()
	{
		Expression left = ParseUnary();
		while (this.token.id == TokenId.Asterisk || this.token.id == TokenId.Slash || this.token.id == TokenId.Percent || TokenIdentifierIs("mod"))
		{
			Token token = this.token;
			NextToken();
			Expression right = ParseUnary();
			CheckAndPromoteOperands(typeof(IArithmeticSignatures), token.text, ref left, ref right, token.pos);
			switch (token.id)
			{
			case TokenId.Asterisk:
				left = Expression.Multiply(left, right);
				break;
			case TokenId.Slash:
				left = Expression.Divide(left, right);
				break;
			case TokenId.Identifier:
			case TokenId.Percent:
				left = Expression.Modulo(left, right);
				break;
			}
		}
		return left;
	}

	private Expression ParseUnary()
	{
		if (this.token.id == TokenId.Minus || this.token.id == TokenId.Exclamation || TokenIdentifierIs("not"))
		{
			Token token = this.token;
			NextToken();
			if (token.id == TokenId.Minus && (this.token.id == TokenId.IntegerLiteral || this.token.id == TokenId.RealLiteral))
			{
				this.token.text = "-" + this.token.text;
				this.token.pos = token.pos;
				return ParsePrimary();
			}
			Expression expr = ParseUnary();
			if (token.id == TokenId.Minus)
			{
				CheckAndPromoteOperand(typeof(INegationSignatures), token.text, ref expr, token.pos);
				return Expression.Negate(expr);
			}
			CheckAndPromoteOperand(typeof(INotSignatures), token.text, ref expr, token.pos);
			return Expression.Not(expr);
		}
		return ParsePrimary();
	}

	private Expression ParsePrimary()
	{
		Expression expression = ParsePrimaryStart();
		while (true)
		{
			if (token.id == TokenId.Dot)
			{
				NextToken();
				expression = ParseMemberAccess(null, expression);
				continue;
			}
			if (token.id != TokenId.OpenBracket)
			{
				break;
			}
			expression = ParseElementAccess(expression);
		}
		return expression;
	}

	private Expression ParsePrimaryStart()
	{
		return token.id switch
		{
			TokenId.Identifier => ParseIdentifier(), 
			TokenId.StringLiteral => ParseStringLiteral(), 
			TokenId.IntegerLiteral => ParseIntegerLiteral(), 
			TokenId.RealLiteral => ParseRealLiteral(), 
			TokenId.OpenParen => ParseParenExpression(), 
			_ => throw ParseError("Expression expected"), 
		};
	}

	private Expression ParseStringLiteral()
	{
		ValidateToken(TokenId.StringLiteral);
		char c = token.text[0];
		string text = token.text.Substring(1, token.text.Length - 2);
		int startIndex = 0;
		while (true)
		{
			int num = text.IndexOf(c, startIndex);
			if (num < 0)
			{
				break;
			}
			text = text.Remove(num, 1);
			startIndex = num + 1;
		}
		if (c == '\'')
		{
			if (text.Length != 1)
			{
				throw ParseError("Character literal must contain exactly one character");
			}
			NextToken();
			return CreateLiteral(text[0], text);
		}
		NextToken();
		return CreateLiteral(text, text);
	}

	private Expression ParseIntegerLiteral()
	{
		ValidateToken(TokenId.IntegerLiteral);
		string text = token.text;
		if (text[0] != '-')
		{
			if (!ulong.TryParse(text, out var result))
			{
				throw ParseError("Invalid integer literal '{0}'", text);
			}
			NextToken();
			if (result <= int.MaxValue)
			{
				return CreateLiteral((int)result, text);
			}
			if (result <= uint.MaxValue)
			{
				return CreateLiteral((uint)result, text);
			}
			if (result <= long.MaxValue)
			{
				return CreateLiteral((long)result, text);
			}
			return CreateLiteral(result, text);
		}
		if (!long.TryParse(text, out var result2))
		{
			throw ParseError("Invalid integer literal '{0}'", text);
		}
		NextToken();
		if (result2 >= int.MinValue && result2 <= int.MaxValue)
		{
			return CreateLiteral((int)result2, text);
		}
		return CreateLiteral(result2, text);
	}

	private Expression ParseRealLiteral()
	{
		ValidateToken(TokenId.RealLiteral);
		string text = token.text;
		object obj = null;
		char c = text[text.Length - 1];
		double result2;
		if (c == 'F' || c == 'f')
		{
			if (float.TryParse(text.Substring(0, text.Length - 1), out var result))
			{
				obj = result;
			}
		}
		else if (double.TryParse(text, out result2))
		{
			obj = result2;
		}
		if (obj == null)
		{
			throw ParseError("Invalid real literal '{0}'", text);
		}
		NextToken();
		return CreateLiteral(obj, text);
	}

	private Expression CreateLiteral(object value, string text)
	{
		ConstantExpression constantExpression = Expression.Constant(value);
		literals.Add(constantExpression, text);
		return constantExpression;
	}

	private Expression ParseParenExpression()
	{
		ValidateToken(TokenId.OpenParen, "'(' expected");
		NextToken();
		Expression result = ParseExpression();
		ValidateToken(TokenId.CloseParen, "')' or operator expected");
		NextToken();
		return result;
	}

	private Expression ParseIdentifier()
	{
		ValidateToken(TokenId.Identifier);
		if (keywords.TryGetValue(token.text, out var value))
		{
			if (value is Type)
			{
				return ParseTypeAccess((Type)value);
			}
			if (value == keywordIt)
			{
				return ParseIt();
			}
			if (value == keywordIif)
			{
				return ParseIif();
			}
			if (value == keywordNew)
			{
				return ParseNew();
			}
			NextToken();
			return (Expression)value;
		}
		if (symbols.TryGetValue(token.text, out value) || (externals != null && externals.TryGetValue(token.text, out value)))
		{
			Expression expression = value as Expression;
			if (expression == null)
			{
				expression = Expression.Constant(value);
			}
			else if (expression is LambdaExpression lambda)
			{
				return ParseLambdaInvocation(lambda);
			}
			NextToken();
			return expression;
		}
		if (it != null)
		{
			return ParseMemberAccess(null, it);
		}
		throw ParseError("Unknown identifier '{0}'", token.text);
	}

	private Expression ParseIt()
	{
		if (it == null)
		{
			throw ParseError("No 'it' is in scope");
		}
		NextToken();
		return it;
	}

	private Expression ParseIif()
	{
		int pos = token.pos;
		NextToken();
		Expression[] array = ParseArgumentList();
		if (array.Length != 3)
		{
			throw ParseError(pos, "The 'iif' function requires three arguments");
		}
		return GenerateConditional(array[0], array[1], array[2], pos);
	}

	private Expression GenerateConditional(Expression test, Expression expr1, Expression expr2, int errorPos)
	{
		if (test.Type != typeof(bool))
		{
			throw ParseError(errorPos, "The first expression must be of type 'Boolean'");
		}
		if (expr1.Type != expr2.Type)
		{
			Expression expression = ((expr2 != nullLiteral) ? PromoteExpression(expr1, expr2.Type, exact: true) : null);
			Expression expression2 = ((expr1 != nullLiteral) ? PromoteExpression(expr2, expr1.Type, exact: true) : null);
			if (expression != null && expression2 == null)
			{
				expr1 = expression;
			}
			else
			{
				if (expression2 == null || expression != null)
				{
					string text = ((expr1 != nullLiteral) ? expr1.Type.Name : "null");
					string text2 = ((expr2 != nullLiteral) ? expr2.Type.Name : "null");
					if (expression != null && expression2 != null)
					{
						throw ParseError(errorPos, "Both of the types '{0}' and '{1}' convert to the other", text, text2);
					}
					throw ParseError(errorPos, "Neither of the types '{0}' and '{1}' converts to the other", text, text2);
				}
				expr2 = expression2;
			}
		}
		return Expression.Condition(test, expr1, expr2);
	}

	private Expression ParseNew()
	{
		NextToken();
		ValidateToken(TokenId.OpenParen, "'(' expected");
		NextToken();
		List<DynamicProperty> list = new List<DynamicProperty>();
		List<Expression> list2 = new List<Expression>();
		while (true)
		{
			int pos = token.pos;
			Expression expression = ParseExpression();
			string name;
			if (TokenIdentifierIs("as"))
			{
				NextToken();
				name = GetIdentifier();
				NextToken();
			}
			else
			{
				if (!(expression is MemberExpression memberExpression))
				{
					throw ParseError(pos, "Expression is missing an 'as' clause");
				}
				name = memberExpression.Member.Name;
			}
			list2.Add(expression);
			list.Add(new DynamicProperty(name, expression.Type));
			if (token.id != TokenId.Comma)
			{
				break;
			}
			NextToken();
		}
		ValidateToken(TokenId.CloseParen, "')' or ',' expected");
		NextToken();
		Type type = DynamicExpression.CreateClass(list);
		MemberBinding[] array = new MemberBinding[list.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Expression.Bind(type.GetProperty(list[i].Name), list2[i]);
		}
		return Expression.MemberInit(Expression.New(type), array);
	}

	private Expression ParseLambdaInvocation(LambdaExpression lambda)
	{
		int pos = token.pos;
		NextToken();
		Expression[] array = ParseArgumentList();
		if (FindMethod(lambda.Type, "Invoke", staticAccess: false, array, out var _) != 1)
		{
			throw ParseError(pos, "Argument list incompatible with lambda expression");
		}
		return Expression.Invoke(lambda, array);
	}

	private Expression ParseTypeAccess(Type type)
	{
		int pos = token.pos;
		NextToken();
		if (token.id == TokenId.Question)
		{
			if (!type.IsValueType || IsNullableType(type))
			{
				throw ParseError(pos, "Type '{0}' has no nullable form", GetTypeName(type));
			}
			type = typeof(Nullable<>).MakeGenericType(type);
			NextToken();
		}
		if (token.id == TokenId.OpenParen)
		{
			Expression[] array = ParseArgumentList();
			MethodBase method;
			switch (FindBestMethod(type.GetConstructors(), array, out method))
			{
			case 0:
				if (array.Length == 1)
				{
					return GenerateConversion(array[0], type, pos);
				}
				throw ParseError(pos, "No matching constructor in type '{0}'", GetTypeName(type));
			case 1:
				return Expression.New((ConstructorInfo)method, array);
			default:
				throw ParseError(pos, "Ambiguous invocation of '{0}' constructor", GetTypeName(type));
			}
		}
		ValidateToken(TokenId.Dot, "'.' or '(' expected");
		NextToken();
		return ParseMemberAccess(type, null);
	}

	private Expression GenerateConversion(Expression expr, Type type, int errorPos)
	{
		Type type2 = expr.Type;
		if (type2 == type)
		{
			return expr;
		}
		if (type2.IsValueType && type.IsValueType)
		{
			if ((IsNullableType(type2) || IsNullableType(type)) && GetNonNullableType(type2) == GetNonNullableType(type))
			{
				return Expression.Convert(expr, type);
			}
			if (((IsNumericType(type2) || IsEnumType(type2)) && IsNumericType(type)) || IsEnumType(type))
			{
				return Expression.ConvertChecked(expr, type);
			}
		}
		if (type2.IsAssignableFrom(type) || type.IsAssignableFrom(type2) || type2.IsInterface || type.IsInterface)
		{
			return Expression.Convert(expr, type);
		}
		throw ParseError(errorPos, "A value of type '{0}' cannot be converted to type '{1}'", GetTypeName(type2), GetTypeName(type));
	}

	private Expression ParseMemberAccess(Type type, Expression instance)
	{
		if (instance != null)
		{
			type = instance.Type;
		}
		int pos = token.pos;
		string identifier = GetIdentifier();
		NextToken();
		if (token.id == TokenId.OpenParen)
		{
			if (instance != null && type != typeof(string))
			{
				Type type2 = FindGenericType(typeof(IEnumerable<>), type);
				if (type2 != null)
				{
					Type elementType = type2.GetGenericArguments()[0];
					return ParseAggregate(instance, elementType, identifier, pos);
				}
			}
			Expression[] array = ParseArgumentList();
			MethodBase method;
			switch (FindMethod(type, identifier, instance == null, array, out method))
			{
			case 0:
				throw ParseError(pos, "No applicable method '{0}' exists in type '{1}'", identifier, GetTypeName(type));
			case 1:
			{
				MethodInfo methodInfo = (MethodInfo)method;
				if (!IsPredefinedType(methodInfo.DeclaringType))
				{
					throw ParseError(pos, "Methods on type '{0}' are not accessible", GetTypeName(methodInfo.DeclaringType));
				}
				if (methodInfo.ReturnType == typeof(void))
				{
					throw ParseError(pos, "Method '{0}' in type '{1}' does not return a value", identifier, GetTypeName(methodInfo.DeclaringType));
				}
				return Expression.Call(instance, methodInfo, array);
			}
			default:
				throw ParseError(pos, "Ambiguous invocation of method '{0}' in type '{1}'", identifier, GetTypeName(type));
			}
		}
		MemberInfo memberInfo = FindPropertyOrField(type, identifier, instance == null);
		if (memberInfo == null)
		{
			throw ParseError(pos, "No property or field '{0}' exists in type '{1}'", identifier, GetTypeName(type));
		}
		if (!(memberInfo is PropertyInfo))
		{
			return Expression.Field(instance, (FieldInfo)memberInfo);
		}
		return Expression.Property(instance, (PropertyInfo)memberInfo);
	}

	private static Type FindGenericType(Type generic, Type type)
	{
		while (type != null && type != typeof(object))
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
			{
				return type;
			}
			if (generic.IsInterface)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					Type type3 = FindGenericType(generic, type2);
					if (type3 != null)
					{
						return type3;
					}
				}
			}
			type = type.BaseType;
		}
		return null;
	}

	private Expression ParseAggregate(Expression instance, Type elementType, string methodName, int errorPos)
	{
		ParameterExpression parameterExpression = it;
		ParameterExpression parameterExpression2 = (it = Expression.Parameter(elementType, ""));
		Expression[] array = ParseArgumentList();
		it = parameterExpression;
		if (FindMethod(typeof(IEnumerableSignatures), methodName, staticAccess: false, array, out var method) != 1)
		{
			throw ParseError(errorPos, "No applicable aggregate method '{0}' exists", methodName);
		}
		return Expression.Call(typeArguments: (!(method.Name == "Min") && !(method.Name == "Max")) ? new Type[1] { elementType } : new Type[2]
		{
			elementType,
			array[0].Type
		}, arguments: (array.Length != 0) ? new Expression[2]
		{
			instance,
			Expression.Lambda(array[0], parameterExpression2)
		} : new Expression[1] { instance }, type: typeof(Enumerable), methodName: method.Name);
	}

	private Expression[] ParseArgumentList()
	{
		ValidateToken(TokenId.OpenParen, "'(' expected");
		NextToken();
		Expression[] result = ((token.id != TokenId.CloseParen) ? ParseArguments() : new Expression[0]);
		ValidateToken(TokenId.CloseParen, "')' or ',' expected");
		NextToken();
		return result;
	}

	private Expression[] ParseArguments()
	{
		List<Expression> list = new List<Expression>();
		while (true)
		{
			list.Add(ParseExpression());
			if (token.id != TokenId.Comma)
			{
				break;
			}
			NextToken();
		}
		return list.ToArray();
	}

	private Expression ParseElementAccess(Expression expr)
	{
		int pos = token.pos;
		ValidateToken(TokenId.OpenBracket, "'(' expected");
		NextToken();
		Expression[] array = ParseArguments();
		ValidateToken(TokenId.CloseBracket, "']' or ',' expected");
		NextToken();
		if (expr.Type.IsArray)
		{
			if (expr.Type.GetArrayRank() != 1 || array.Length != 1)
			{
				throw ParseError(pos, "Indexing of multi-dimensional arrays is not supported");
			}
			Expression expression = PromoteExpression(array[0], typeof(int), exact: true);
			if (expression == null)
			{
				throw ParseError(pos, "Array index must be an integer expression");
			}
			return Expression.ArrayIndex(expr, expression);
		}
		MethodBase method;
		return FindIndexer(expr.Type, array, out method) switch
		{
			0 => throw ParseError(pos, "No applicable indexer exists in type '{0}'", GetTypeName(expr.Type)), 
			1 => Expression.Call(expr, (MethodInfo)method, array), 
			_ => throw ParseError(pos, "Ambiguous invocation of indexer in type '{0}'", GetTypeName(expr.Type)), 
		};
	}

	private static bool IsPredefinedType(Type type)
	{
		Type[] array = predefinedTypes;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == type)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsNullableType(Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}

	private static Type GetNonNullableType(Type type)
	{
		if (!IsNullableType(type))
		{
			return type;
		}
		return type.GetGenericArguments()[0];
	}

	private static string GetTypeName(Type type)
	{
		Type nonNullableType = GetNonNullableType(type);
		string text = nonNullableType.Name;
		if (type != nonNullableType)
		{
			text += "?";
		}
		return text;
	}

	private static bool IsNumericType(Type type)
	{
		return GetNumericTypeKind(type) != 0;
	}

	private static bool IsSignedIntegralType(Type type)
	{
		return GetNumericTypeKind(type) == 2;
	}

	private static bool IsUnsignedIntegralType(Type type)
	{
		return GetNumericTypeKind(type) == 3;
	}

	private static int GetNumericTypeKind(Type type)
	{
		type = GetNonNullableType(type);
		if (type.IsEnum)
		{
			return 0;
		}
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.Char:
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
			return 1;
		case TypeCode.SByte:
		case TypeCode.Int16:
		case TypeCode.Int32:
		case TypeCode.Int64:
			return 2;
		case TypeCode.Byte:
		case TypeCode.UInt16:
		case TypeCode.UInt32:
		case TypeCode.UInt64:
			return 3;
		default:
			return 0;
		}
	}

	private static bool IsEnumType(Type type)
	{
		return GetNonNullableType(type).IsEnum;
	}

	private void CheckAndPromoteOperand(Type signatures, string opName, ref Expression expr, int errorPos)
	{
		Expression[] array = new Expression[1] { expr };
		if (FindMethod(signatures, "F", staticAccess: false, array, out var _) != 1)
		{
			throw ParseError(errorPos, "Operator '{0}' incompatible with operand type '{1}'", opName, GetTypeName(array[0].Type));
		}
		expr = array[0];
	}

	private void CheckAndPromoteOperands(Type signatures, string opName, ref Expression left, ref Expression right, int errorPos)
	{
		Expression[] array = new Expression[2] { left, right };
		if (FindMethod(signatures, "F", staticAccess: false, array, out var _) != 1)
		{
			throw IncompatibleOperandsError(opName, left, right, errorPos);
		}
		left = array[0];
		right = array[1];
	}

	private Exception IncompatibleOperandsError(string opName, Expression left, Expression right, int pos)
	{
		return ParseError(pos, "Operator '{0}' incompatible with operand types '{1}' and '{2}'", opName, GetTypeName(left.Type), GetTypeName(right.Type));
	}

	private MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
	{
		BindingFlags bindingAttr = (BindingFlags)(0x12 | (staticAccess ? 8 : 4));
		foreach (Type item in SelfAndBaseTypes(type))
		{
			MemberInfo[] array = item.FindMembers(MemberTypes.Field | MemberTypes.Property, bindingAttr, Type.FilterNameIgnoreCase, memberName);
			if (array.Length != 0)
			{
				return array[0];
			}
		}
		return null;
	}

	private int FindMethod(Type type, string methodName, bool staticAccess, Expression[] args, out MethodBase method)
	{
		BindingFlags bindingAttr = (BindingFlags)(0x12 | (staticAccess ? 8 : 4));
		foreach (Type item in SelfAndBaseTypes(type))
		{
			MemberInfo[] source = item.FindMembers(MemberTypes.Method, bindingAttr, Type.FilterNameIgnoreCase, methodName);
			int num = FindBestMethod(source.Cast<MethodBase>(), args, out method);
			if (num != 0)
			{
				return num;
			}
		}
		method = null;
		return 0;
	}

	private int FindIndexer(Type type, Expression[] args, out MethodBase method)
	{
		foreach (Type item in SelfAndBaseTypes(type))
		{
			MemberInfo[] defaultMembers = item.GetDefaultMembers();
			if (defaultMembers.Length != 0)
			{
				IEnumerable<MethodBase> methods = from m in defaultMembers.OfType<PropertyInfo>().Select((Func<PropertyInfo, MethodBase>)((PropertyInfo p) => p.GetGetMethod()))
					where m != null
					select m;
				int num = FindBestMethod(methods, args, out method);
				if (num != 0)
				{
					return num;
				}
			}
		}
		method = null;
		return 0;
	}

	private static IEnumerable<Type> SelfAndBaseTypes(Type type)
	{
		if (type.IsInterface)
		{
			List<Type> list = new List<Type>();
			AddInterface(list, type);
			return list;
		}
		return SelfAndBaseClasses(type);
	}

	private static IEnumerable<Type> SelfAndBaseClasses(Type type)
	{
		while (type != null)
		{
			yield return type;
			type = type.BaseType;
		}
	}

	private static void AddInterface(List<Type> types, Type type)
	{
		if (!types.Contains(type))
		{
			types.Add(type);
			Type[] interfaces = type.GetInterfaces();
			foreach (Type type2 in interfaces)
			{
				AddInterface(types, type2);
			}
		}
	}

	private int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
	{
		MethodData[] applicable = (from m in methods
			select new MethodData
			{
				MethodBase = m,
				Parameters = m.GetParameters()
			} into m
			where IsApplicable(m, args)
			select m).ToArray();
		if (applicable.Length > 1)
		{
			applicable = applicable.Where((MethodData m) => applicable.All((MethodData n) => m == n || IsBetterThan(args, m, n))).ToArray();
		}
		if (applicable.Length == 1)
		{
			MethodData methodData = applicable[0];
			for (int num = 0; num < args.Length; num++)
			{
				args[num] = methodData.Args[num];
			}
			method = methodData.MethodBase;
		}
		else
		{
			method = null;
		}
		return applicable.Length;
	}

	private bool IsApplicable(MethodData method, Expression[] args)
	{
		if (method.Parameters.Length != args.Length)
		{
			return false;
		}
		Expression[] array = new Expression[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			ParameterInfo parameterInfo = method.Parameters[i];
			if (parameterInfo.IsOut)
			{
				return false;
			}
			Expression expression = PromoteExpression(args[i], parameterInfo.ParameterType, exact: false);
			if (expression == null)
			{
				return false;
			}
			array[i] = expression;
		}
		method.Args = array;
		return true;
	}

	private Expression PromoteExpression(Expression expr, Type type, bool exact)
	{
		if (expr.Type == type)
		{
			return expr;
		}
		if (expr is ConstantExpression)
		{
			ConstantExpression constantExpression = (ConstantExpression)expr;
			string value;
			if (constantExpression == nullLiteral)
			{
				if (!type.IsValueType || IsNullableType(type))
				{
					return Expression.Constant(null, type);
				}
			}
			else if (literals.TryGetValue(constantExpression, out value))
			{
				Type nonNullableType = GetNonNullableType(type);
				object obj = null;
				switch (Type.GetTypeCode(constantExpression.Type))
				{
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
					obj = ParseNumber(value, nonNullableType);
					break;
				case TypeCode.Double:
					if (nonNullableType == typeof(decimal))
					{
						obj = ParseNumber(value, nonNullableType);
					}
					break;
				case TypeCode.String:
					obj = ParseEnum(value, nonNullableType);
					break;
				}
				if (obj != null)
				{
					return Expression.Constant(obj, type);
				}
			}
		}
		if (IsCompatibleWith(expr.Type, type))
		{
			if (type.IsValueType || exact)
			{
				return Expression.Convert(expr, type);
			}
			return expr;
		}
		return null;
	}

	private static object ParseNumber(string text, Type type)
	{
		switch (Type.GetTypeCode(GetNonNullableType(type)))
		{
		case TypeCode.SByte:
		{
			if (sbyte.TryParse(text, out var result6))
			{
				return result6;
			}
			break;
		}
		case TypeCode.Byte:
		{
			if (byte.TryParse(text, out var result10))
			{
				return result10;
			}
			break;
		}
		case TypeCode.Int16:
		{
			if (short.TryParse(text, out var result2))
			{
				return result2;
			}
			break;
		}
		case TypeCode.UInt16:
		{
			if (ushort.TryParse(text, out var result8))
			{
				return result8;
			}
			break;
		}
		case TypeCode.Int32:
		{
			if (int.TryParse(text, out var result4))
			{
				return result4;
			}
			break;
		}
		case TypeCode.UInt32:
		{
			if (uint.TryParse(text, out var result11))
			{
				return result11;
			}
			break;
		}
		case TypeCode.Int64:
		{
			if (long.TryParse(text, out var result9))
			{
				return result9;
			}
			break;
		}
		case TypeCode.UInt64:
		{
			if (ulong.TryParse(text, out var result7))
			{
				return result7;
			}
			break;
		}
		case TypeCode.Single:
		{
			if (float.TryParse(text, out var result5))
			{
				return result5;
			}
			break;
		}
		case TypeCode.Double:
		{
			if (double.TryParse(text, out var result3))
			{
				return result3;
			}
			break;
		}
		case TypeCode.Decimal:
		{
			if (decimal.TryParse(text, out var result))
			{
				return result;
			}
			break;
		}
		}
		return null;
	}

	private static object ParseEnum(string name, Type type)
	{
		if (type.IsEnum)
		{
			MemberInfo[] array = type.FindMembers(MemberTypes.Field, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public, Type.FilterNameIgnoreCase, name);
			if (array.Length != 0)
			{
				return ((FieldInfo)array[0]).GetValue(null);
			}
		}
		return null;
	}

	private static bool IsCompatibleWith(Type source, Type target)
	{
		if (source == target)
		{
			return true;
		}
		if (!target.IsValueType)
		{
			return target.IsAssignableFrom(source);
		}
		Type nonNullableType = GetNonNullableType(source);
		Type nonNullableType2 = GetNonNullableType(target);
		if (nonNullableType != source && nonNullableType2 == target)
		{
			return false;
		}
		TypeCode typeCode = (nonNullableType.IsEnum ? TypeCode.Object : Type.GetTypeCode(nonNullableType));
		TypeCode typeCode2 = (nonNullableType2.IsEnum ? TypeCode.Object : Type.GetTypeCode(nonNullableType2));
		switch (typeCode)
		{
		case TypeCode.SByte:
			switch (typeCode2)
			{
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			}
			break;
		case TypeCode.Byte:
			if ((uint)(typeCode2 - 6) <= 9u)
			{
				return true;
			}
			break;
		case TypeCode.Int16:
			switch (typeCode2)
			{
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			}
			break;
		case TypeCode.UInt16:
			if ((uint)(typeCode2 - 8) <= 7u)
			{
				return true;
			}
			break;
		case TypeCode.Int32:
			switch (typeCode2)
			{
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Single:
			case TypeCode.Double:
			case TypeCode.Decimal:
				return true;
			}
			break;
		case TypeCode.UInt32:
			if ((uint)(typeCode2 - 10) <= 5u)
			{
				return true;
			}
			break;
		case TypeCode.Int64:
			if (typeCode2 == TypeCode.Int64 || (uint)(typeCode2 - 13) <= 2u)
			{
				return true;
			}
			break;
		case TypeCode.UInt64:
			if ((uint)(typeCode2 - 12) <= 3u)
			{
				return true;
			}
			break;
		case TypeCode.Single:
			if ((uint)(typeCode2 - 13) <= 1u)
			{
				return true;
			}
			break;
		default:
			if (nonNullableType == nonNullableType2)
			{
				return true;
			}
			break;
		}
		return false;
	}

	private static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
	{
		bool result = false;
		for (int i = 0; i < args.Length; i++)
		{
			int num = CompareConversions(args[i].Type, m1.Parameters[i].ParameterType, m2.Parameters[i].ParameterType);
			if (num < 0)
			{
				return false;
			}
			if (num > 0)
			{
				result = true;
			}
		}
		return result;
	}

	private static int CompareConversions(Type s, Type t1, Type t2)
	{
		if (t1 == t2)
		{
			return 0;
		}
		if (s == t1)
		{
			return 1;
		}
		if (s == t2)
		{
			return -1;
		}
		bool flag = IsCompatibleWith(t1, t2);
		bool flag2 = IsCompatibleWith(t2, t1);
		if (flag && !flag2)
		{
			return 1;
		}
		if (flag2 && !flag)
		{
			return -1;
		}
		if (IsSignedIntegralType(t1) && IsUnsignedIntegralType(t2))
		{
			return 1;
		}
		if (IsSignedIntegralType(t2) && IsUnsignedIntegralType(t1))
		{
			return -1;
		}
		return 0;
	}

	private Expression GenerateEqual(Expression left, Expression right)
	{
		return Expression.Equal(left, right);
	}

	private Expression GenerateNotEqual(Expression left, Expression right)
	{
		return Expression.NotEqual(left, right);
	}

	private Expression GenerateGreaterThan(Expression left, Expression right)
	{
		if (left.Type == typeof(string))
		{
			return Expression.GreaterThan(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
		}
		return Expression.GreaterThan(left, right);
	}

	private Expression GenerateGreaterThanEqual(Expression left, Expression right)
	{
		if (left.Type == typeof(string))
		{
			return Expression.GreaterThanOrEqual(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
		}
		return Expression.GreaterThanOrEqual(left, right);
	}

	private Expression GenerateLessThan(Expression left, Expression right)
	{
		if (left.Type == typeof(string))
		{
			return Expression.LessThan(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
		}
		return Expression.LessThan(left, right);
	}

	private Expression GenerateLessThanEqual(Expression left, Expression right)
	{
		if (left.Type == typeof(string))
		{
			return Expression.LessThanOrEqual(GenerateStaticMethodCall("Compare", left, right), Expression.Constant(0));
		}
		return Expression.LessThanOrEqual(left, right);
	}

	private Expression GenerateAdd(Expression left, Expression right)
	{
		if (left.Type == typeof(string) && right.Type == typeof(string))
		{
			return GenerateStaticMethodCall("Concat", left, right);
		}
		return Expression.Add(left, right);
	}

	private Expression GenerateSubtract(Expression left, Expression right)
	{
		return Expression.Subtract(left, right);
	}

	private Expression GenerateStringConcat(Expression left, Expression right)
	{
		return Expression.Call(null, typeof(string).GetMethod("Concat", new Type[2]
		{
			typeof(object),
			typeof(object)
		}), left, right);
	}

	private MethodInfo GetStaticMethod(string methodName, Expression left, Expression right)
	{
		return left.Type.GetMethod(methodName, new Type[2] { left.Type, right.Type });
	}

	private Expression GenerateStaticMethodCall(string methodName, Expression left, Expression right)
	{
		return Expression.Call(null, GetStaticMethod(methodName, left, right), left, right);
	}

	private void SetTextPos(int pos)
	{
		textPos = pos;
		ch = ((textPos < textLen) ? text[textPos] : '\0');
	}

	private void NextChar()
	{
		if (textPos < textLen)
		{
			textPos++;
		}
		ch = ((textPos < textLen) ? text[textPos] : '\0');
	}

	private void NextToken()
	{
		while (char.IsWhiteSpace(ch))
		{
			NextChar();
		}
		int num = textPos;
		TokenId id;
		switch (ch)
		{
		case '!':
			NextChar();
			if (ch == '=')
			{
				NextChar();
				id = TokenId.ExclamationEqual;
			}
			else
			{
				id = TokenId.Exclamation;
			}
			break;
		case '%':
			NextChar();
			id = TokenId.Percent;
			break;
		case '&':
			NextChar();
			if (ch == '&')
			{
				NextChar();
				id = TokenId.DoubleAmphersand;
			}
			else
			{
				id = TokenId.Amphersand;
			}
			break;
		case '(':
			NextChar();
			id = TokenId.OpenParen;
			break;
		case ')':
			NextChar();
			id = TokenId.CloseParen;
			break;
		case '*':
			NextChar();
			id = TokenId.Asterisk;
			break;
		case '+':
			NextChar();
			id = TokenId.Plus;
			break;
		case ',':
			NextChar();
			id = TokenId.Comma;
			break;
		case '-':
			NextChar();
			id = TokenId.Minus;
			break;
		case '.':
			NextChar();
			id = TokenId.Dot;
			break;
		case '/':
			NextChar();
			id = TokenId.Slash;
			break;
		case ':':
			NextChar();
			id = TokenId.Colon;
			break;
		case '<':
			NextChar();
			if (ch == '=')
			{
				NextChar();
				id = TokenId.LessThanEqual;
			}
			else if (ch == '>')
			{
				NextChar();
				id = TokenId.LessGreater;
			}
			else
			{
				id = TokenId.LessThan;
			}
			break;
		case '=':
			NextChar();
			if (ch == '=')
			{
				NextChar();
				id = TokenId.DoubleEqual;
			}
			else
			{
				id = TokenId.Equal;
			}
			break;
		case '>':
			NextChar();
			if (ch == '=')
			{
				NextChar();
				id = TokenId.GreaterThanEqual;
			}
			else
			{
				id = TokenId.GreaterThan;
			}
			break;
		case '?':
			NextChar();
			id = TokenId.Question;
			break;
		case '[':
			NextChar();
			id = TokenId.OpenBracket;
			break;
		case ']':
			NextChar();
			id = TokenId.CloseBracket;
			break;
		case '|':
			NextChar();
			if (ch == '|')
			{
				NextChar();
				id = TokenId.DoubleBar;
			}
			else
			{
				id = TokenId.Bar;
			}
			break;
		case '"':
		case '\'':
		{
			char c = ch;
			do
			{
				NextChar();
				while (textPos < textLen && ch != c)
				{
					NextChar();
				}
				if (textPos == textLen)
				{
					throw ParseError(textPos, "Unterminated string literal");
				}
				NextChar();
			}
			while (ch == c);
			id = TokenId.StringLiteral;
			break;
		}
		default:
			if (char.IsLetter(ch) || ch == '@' || ch == '_')
			{
				do
				{
					NextChar();
				}
				while (char.IsLetterOrDigit(ch) || ch == '_');
				id = TokenId.Identifier;
			}
			else if (char.IsDigit(ch))
			{
				id = TokenId.IntegerLiteral;
				do
				{
					NextChar();
				}
				while (char.IsDigit(ch));
				if (ch == '.')
				{
					id = TokenId.RealLiteral;
					NextChar();
					ValidateDigit();
					do
					{
						NextChar();
					}
					while (char.IsDigit(ch));
				}
				if (ch == 'E' || ch == 'e')
				{
					id = TokenId.RealLiteral;
					NextChar();
					if (ch == '+' || ch == '-')
					{
						NextChar();
					}
					ValidateDigit();
					do
					{
						NextChar();
					}
					while (char.IsDigit(ch));
				}
				if (ch == 'F' || ch == 'f')
				{
					NextChar();
				}
			}
			else
			{
				if (textPos != textLen)
				{
					throw ParseError(textPos, "Syntax error '{0}'", ch);
				}
				id = TokenId.End;
			}
			break;
		}
		token.id = id;
		token.text = text.Substring(num, textPos - num);
		token.pos = num;
	}

	private bool TokenIdentifierIs(string id)
	{
		if (token.id == TokenId.Identifier)
		{
			return string.Equals(id, token.text, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private string GetIdentifier()
	{
		ValidateToken(TokenId.Identifier, "Identifier expected");
		string text = token.text;
		if (text.Length > 1 && text[0] == '@')
		{
			text = text.Substring(1);
		}
		return text;
	}

	private void ValidateDigit()
	{
		if (!char.IsDigit(ch))
		{
			throw ParseError(textPos, "Digit expected");
		}
	}

	private void ValidateToken(TokenId t, string errorMessage)
	{
		if (token.id != t)
		{
			throw ParseError(errorMessage);
		}
	}

	private void ValidateToken(TokenId t)
	{
		if (token.id != t)
		{
			throw ParseError("Syntax error");
		}
	}

	private Exception ParseError(string format, params object[] args)
	{
		return ParseError(token.pos, format, args);
	}

	private Exception ParseError(int pos, string format, params object[] args)
	{
		return new ParseException(string.Format(CultureInfo.CurrentCulture, format, args), pos);
	}

	private static Dictionary<string, object> CreateKeywords()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		dictionary.Add("true", trueLiteral);
		dictionary.Add("false", falseLiteral);
		dictionary.Add("null", nullLiteral);
		dictionary.Add(keywordIt, keywordIt);
		dictionary.Add(keywordIif, keywordIif);
		dictionary.Add(keywordNew, keywordNew);
		Type[] array = predefinedTypes;
		foreach (Type type in array)
		{
			dictionary.Add(type.Name, type);
		}
		return dictionary;
	}
}
