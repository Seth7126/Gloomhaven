using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MathParserTK;

public class MathParser
{
	private const string NumberMaker = "#";

	private const string OperatorMarker = "$";

	private const string FunctionMarker = "@";

	private const string Plus = "$+";

	private const string UnPlus = "$un+";

	private const string Minus = "$-";

	private const string UnMinus = "$un-";

	private const string Multiply = "$*";

	private const string Divide = "$/";

	private const string Degree = "$^";

	private const string LeftParent = "$(";

	private const string RightParent = "$)";

	private const string Sqrt = "@sqrt";

	private const string Sin = "@sin";

	private const string Cos = "@cos";

	private const string Tg = "@tg";

	private const string Ctg = "@ctg";

	private const string Sh = "@sh";

	private const string Ch = "@ch";

	private const string Th = "@th";

	private const string Log = "@log";

	private const string Ln = "@ln";

	private const string Exp = "@exp";

	private const string Abs = "@abs";

	private readonly Dictionary<string, string> supportedOperators = new Dictionary<string, string>
	{
		{ "+", "$+" },
		{ "-", "$-" },
		{ "*", "$*" },
		{ "/", "$/" },
		{ "^", "$^" },
		{ "(", "$(" },
		{ ")", "$)" }
	};

	private readonly Dictionary<string, string> supportedFunctions = new Dictionary<string, string>
	{
		{ "sqrt", "@sqrt" },
		{ "?", "@sqrt" },
		{ "sin", "@sin" },
		{ "cos", "@cos" },
		{ "tg", "@tg" },
		{ "ctg", "@ctg" },
		{ "sh", "@sh" },
		{ "ch", "@ch" },
		{ "th", "@th" },
		{ "log", "@log" },
		{ "exp", "@exp" },
		{ "abs", "@abs" }
	};

	private readonly Dictionary<string, string> supportedConstants = new Dictionary<string, string>
	{
		{
			"pi",
			"#" + Math.PI
		},
		{
			"e",
			"#" + Math.E
		}
	};

	private readonly char decimalSeparator;

	private bool isRadians;

	public MathParser()
	{
		try
		{
			decimalSeparator = char.Parse(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		}
		catch (FormatException innerException)
		{
			throw new FormatException("Error: can't read char decimal separator from system, check your regional settings.", innerException);
		}
	}

	public MathParser(char decimalSeparator)
	{
		this.decimalSeparator = decimalSeparator;
	}

	public double Parse(string expression, bool isRadians = true)
	{
		this.isRadians = isRadians;
		try
		{
			return Calculate(ConvertToRPN(FormatString(expression)));
		}
		catch (DivideByZeroException ex)
		{
			throw ex;
		}
		catch (FormatException ex2)
		{
			throw ex2;
		}
		catch (InvalidOperationException ex3)
		{
			throw ex3;
		}
		catch (ArgumentOutOfRangeException ex4)
		{
			throw ex4;
		}
		catch (ArgumentException ex5)
		{
			throw ex5;
		}
		catch (Exception ex6)
		{
			throw ex6;
		}
	}

	private string FormatString(string expression)
	{
		if (string.IsNullOrEmpty(expression))
		{
			throw new ArgumentNullException("Expression is null or empty");
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (char c in expression)
		{
			switch (c)
			{
			case '(':
				num++;
				break;
			case ')':
				num--;
				break;
			}
			if (!char.IsWhiteSpace(c))
			{
				if (char.IsUpper(c))
				{
					stringBuilder.Append(char.ToLower(c));
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
		}
		if (num != 0)
		{
			throw new FormatException("Number of left and right parenthesis is not equal");
		}
		return stringBuilder.ToString();
	}

	private string ConvertToRPN(string expression)
	{
		int pos = 0;
		StringBuilder stringBuilder = new StringBuilder();
		Stack<string> stack = new Stack<string>();
		while (pos < expression.Length)
		{
			string token = LexicalAnalysisInfixNotation(expression, ref pos);
			stringBuilder = SyntaxAnalysisInfixNotation(token, stringBuilder, stack);
		}
		while (stack.Count > 0)
		{
			if (stack.Peek()[0] == "$"[0])
			{
				stringBuilder.Append(stack.Pop());
				continue;
			}
			throw new FormatException("Format exception, there is function without parenthesis");
		}
		return stringBuilder.ToString();
	}

	private string LexicalAnalysisInfixNotation(string expression, ref int pos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(expression[pos]);
		if (supportedOperators.ContainsKey(stringBuilder.ToString()))
		{
			bool flag = pos == 0 || expression[pos - 1] == '(';
			pos++;
			string text = stringBuilder.ToString();
			if (!(text == "+"))
			{
				if (text == "-")
				{
					if (!flag)
					{
						return "$-";
					}
					return "$un-";
				}
				return supportedOperators[stringBuilder.ToString()];
			}
			if (!flag)
			{
				return "$+";
			}
			return "$un+";
		}
		if (char.IsLetter(stringBuilder[0]) || supportedFunctions.ContainsKey(stringBuilder.ToString()) || supportedConstants.ContainsKey(stringBuilder.ToString()))
		{
			while (++pos < expression.Length && char.IsLetter(expression[pos]))
			{
				stringBuilder.Append(expression[pos]);
			}
			if (supportedFunctions.ContainsKey(stringBuilder.ToString()))
			{
				return supportedFunctions[stringBuilder.ToString()];
			}
			if (supportedConstants.ContainsKey(stringBuilder.ToString()))
			{
				return supportedConstants[stringBuilder.ToString()];
			}
			throw new ArgumentException("Unknown token");
		}
		if (char.IsDigit(stringBuilder[0]) || stringBuilder[0] == decimalSeparator)
		{
			if (char.IsDigit(stringBuilder[0]))
			{
				while (++pos < expression.Length && char.IsDigit(expression[pos]))
				{
					stringBuilder.Append(expression[pos]);
				}
			}
			else
			{
				stringBuilder.Length = 0;
			}
			if (pos < expression.Length && expression[pos] == decimalSeparator)
			{
				stringBuilder.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
				while (++pos < expression.Length && char.IsDigit(expression[pos]))
				{
					stringBuilder.Append(expression[pos]);
				}
			}
			if (pos + 1 < expression.Length && expression[pos] == 'e' && (char.IsDigit(expression[pos + 1]) || (pos + 2 < expression.Length && (expression[pos + 1] == '+' || expression[pos + 1] == '-') && char.IsDigit(expression[pos + 2]))))
			{
				stringBuilder.Append(expression[pos++]);
				if (expression[pos] == '+' || expression[pos] == '-')
				{
					stringBuilder.Append(expression[pos++]);
				}
				while (pos < expression.Length && char.IsDigit(expression[pos]))
				{
					stringBuilder.Append(expression[pos++]);
				}
				return "#" + Convert.ToDouble(stringBuilder.ToString());
			}
			return "#" + stringBuilder.ToString();
		}
		throw new ArgumentException("Unknown token in expression");
	}

	private StringBuilder SyntaxAnalysisInfixNotation(string token, StringBuilder outputString, Stack<string> stack)
	{
		if (token[0] == "#"[0])
		{
			outputString.Append(token);
		}
		else if (token[0] == "@"[0])
		{
			stack.Push(token);
		}
		else if (token == "$(")
		{
			stack.Push(token);
		}
		else if (token == "$)")
		{
			string value;
			while ((value = stack.Pop()) != "$(")
			{
				outputString.Append(value);
			}
			if (stack.Count > 0 && stack.Peek()[0] == "@"[0])
			{
				outputString.Append(stack.Pop());
			}
		}
		else
		{
			while (stack.Count > 0 && Priority(token, stack.Peek()))
			{
				outputString.Append(stack.Pop());
			}
			stack.Push(token);
		}
		return outputString;
	}

	private bool Priority(string token, string p)
	{
		if (!IsRightAssociated(token))
		{
			return GetPriority(token) <= GetPriority(p);
		}
		return GetPriority(token) < GetPriority(p);
	}

	private bool IsRightAssociated(string token)
	{
		return token == "$^";
	}

	private int GetPriority(string token)
	{
		switch (token)
		{
		case "$(":
			return 0;
		case "$+":
		case "$-":
			return 2;
		case "$un+":
		case "$un-":
			return 6;
		case "$*":
		case "$/":
			return 4;
		case "$^":
		case "@sqrt":
			return 8;
		case "@sin":
		case "@cos":
		case "@tg":
		case "@ctg":
		case "@sh":
		case "@ch":
		case "@th":
		case "@log":
		case "@ln":
		case "@exp":
		case "@abs":
			return 10;
		default:
			throw new ArgumentException("Unknown operator");
		}
	}

	private double Calculate(string expression)
	{
		int pos = 0;
		Stack<double> stack = new Stack<double>();
		while (pos < expression.Length)
		{
			string token = LexicalAnalysisRPN(expression, ref pos);
			stack = SyntaxAnalysisRPN(stack, token);
		}
		if (stack.Count > 1)
		{
			throw new ArgumentException("Excess operand");
		}
		return stack.Pop();
	}

	private string LexicalAnalysisRPN(string expression, ref int pos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(expression[pos++]);
		while (pos < expression.Length && expression[pos] != "#"[0] && expression[pos] != "$"[0] && expression[pos] != "@"[0])
		{
			stringBuilder.Append(expression[pos++]);
		}
		return stringBuilder.ToString();
	}

	private Stack<double> SyntaxAnalysisRPN(Stack<double> stack, string token)
	{
		if (token[0] == "#"[0])
		{
			stack.Push(double.Parse(token.Remove(0, 1)));
		}
		else if (NumberOfArguments(token) == 1)
		{
			double num = stack.Pop();
			double num2;
			stack.Push(token switch
			{
				"$un+" => num, 
				"$un-" => 0.0 - num, 
				"@sqrt" => Math.Sqrt(num), 
				"@sin" => ApplyTrigFunction(Math.Sin, num), 
				"@cos" => ApplyTrigFunction(Math.Cos, num), 
				"@tg" => ApplyTrigFunction(Math.Tan, num), 
				"@ctg" => 1.0 / ApplyTrigFunction(Math.Tan, num), 
				"@sh" => Math.Sinh(num), 
				"@ch" => num2 = Math.Cosh(num), 
				"@th" => Math.Tanh(num), 
				"@ln" => Math.Log(num), 
				"@exp" => Math.Exp(num), 
				"@abs" => Math.Abs(num), 
				_ => throw new ArgumentException("Unknown operator"), 
			});
		}
		else
		{
			double num3 = stack.Pop();
			double num4 = stack.Pop();
			double item;
			switch (token)
			{
			case "$+":
				item = num4 + num3;
				break;
			case "$-":
				item = num4 - num3;
				break;
			case "$*":
				item = num4 * num3;
				break;
			case "$/":
				if (num3 == 0.0)
				{
					throw new DivideByZeroException("Second argument is zero");
				}
				item = num4 / num3;
				break;
			case "$^":
				item = Math.Pow(num4, num3);
				break;
			case "@log":
				item = Math.Log(num3, num4);
				break;
			default:
				throw new ArgumentException("Unknown operator");
			}
			stack.Push(item);
		}
		return stack;
	}

	private double ApplyTrigFunction(Func<double, double> func, double arg)
	{
		if (!isRadians)
		{
			arg = arg * Math.PI / 180.0;
		}
		return func(arg);
	}

	private int NumberOfArguments(string token)
	{
		switch (token)
		{
		case "$un+":
		case "$un-":
		case "@sqrt":
		case "@tg":
		case "@sh":
		case "@ch":
		case "@th":
		case "@ln":
		case "@ctg":
		case "@sin":
		case "@cos":
		case "@exp":
		case "@abs":
			return 1;
		case "$+":
		case "$-":
		case "$*":
		case "$/":
		case "$^":
		case "@log":
			return 2;
		default:
			throw new ArgumentException("Unknown operator");
		}
	}
}
