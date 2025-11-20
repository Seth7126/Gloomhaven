using System.Collections.Generic;
using System.Text;

namespace System.Net.Http.Headers;

/// <summary>Represents a Range header value.</summary>
public class RangeHeaderValue : ICloneable
{
	private List<RangeItemHeaderValue> ranges;

	private string unit;

	/// <summary>Gets the ranges specified from the <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.The ranges from the <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> object.</returns>
	public ICollection<RangeItemHeaderValue> Ranges => ranges ?? (ranges = new List<RangeItemHeaderValue>());

	/// <summary>Gets the unit from the <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The unit from the <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> object.</returns>
	public string Unit
	{
		get
		{
			return unit;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Unit");
			}
			Parser.Token.Check(value);
			unit = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> class.</summary>
	public RangeHeaderValue()
	{
		unit = "bytes";
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> class with a byte range.</summary>
	/// <param name="from">The position at which to start sending data.</param>
	/// <param name="to">The position at which to stop sending data.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	public RangeHeaderValue(long? from, long? to)
		: this()
	{
		Ranges.Add(new RangeItemHeaderValue(from, to));
	}

	private RangeHeaderValue(RangeHeaderValue source)
		: this()
	{
		if (source.ranges == null)
		{
			return;
		}
		foreach (RangeItemHeaderValue range in source.ranges)
		{
			Ranges.Add(range);
		}
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return new RangeHeaderValue(this);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is RangeHeaderValue rangeHeaderValue))
		{
			return false;
		}
		if (string.Equals(rangeHeaderValue.Unit, Unit, StringComparison.OrdinalIgnoreCase))
		{
			return rangeHeaderValue.ranges.SequenceEqual(ranges);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Unit.ToLowerInvariant().GetHashCode() ^ HashCodeCalculator.Calculate(ranges);
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.RangeHeaderValue" />.An <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents range header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid range header value information.</exception>
	public static RangeHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">he string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out RangeHeaderValue parsedValue)
	{
		parsedValue = null;
		Lexer lexer = new Lexer(input);
		Token token = lexer.Scan();
		if ((Token.Type)token != Token.Type.Token)
		{
			return false;
		}
		RangeHeaderValue rangeHeaderValue = new RangeHeaderValue();
		rangeHeaderValue.unit = lexer.GetStringValue(token);
		token = lexer.Scan();
		if ((Token.Type)token != Token.Type.SeparatorEqual)
		{
			return false;
		}
		do
		{
			long? num = null;
			long? num2 = null;
			bool flag = false;
			token = lexer.Scan(recognizeDash: true);
			long result;
			switch (token.Kind)
			{
			case Token.Type.SeparatorDash:
				token = lexer.Scan();
				if (!lexer.TryGetNumericValue(token, out result))
				{
					return false;
				}
				num2 = result;
				break;
			case Token.Type.Token:
			{
				string stringValue = lexer.GetStringValue(token);
				string[] array = stringValue.Split(new char[1] { '-' }, StringSplitOptions.RemoveEmptyEntries);
				if (!Parser.Long.TryParse(array[0], out result))
				{
					return false;
				}
				switch (array.Length)
				{
				case 1:
					token = lexer.Scan(recognizeDash: true);
					num = result;
					switch (token.Kind)
					{
					case Token.Type.SeparatorDash:
						token = lexer.Scan();
						if ((Token.Type)token != Token.Type.Token)
						{
							flag = true;
							break;
						}
						if (!lexer.TryGetNumericValue(token, out result))
						{
							return false;
						}
						num2 = result;
						if (!(num2 < num))
						{
							break;
						}
						return false;
					case Token.Type.End:
						if (stringValue.Length > 0 && stringValue[stringValue.Length - 1] != '-')
						{
							return false;
						}
						flag = true;
						break;
					case Token.Type.SeparatorComma:
						flag = true;
						break;
					default:
						return false;
					}
					break;
				case 2:
					num = result;
					if (!Parser.Long.TryParse(array[1], out result))
					{
						return false;
					}
					num2 = result;
					if (num2 < num)
					{
						return false;
					}
					break;
				default:
					return false;
				}
				break;
			}
			default:
				return false;
			}
			rangeHeaderValue.Ranges.Add(new RangeItemHeaderValue(num, num2));
			if (!flag)
			{
				token = lexer.Scan();
			}
		}
		while ((Token.Type)token == Token.Type.SeparatorComma);
		if ((Token.Type)token != Token.Type.End)
		{
			return false;
		}
		parsedValue = rangeHeaderValue;
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.RangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(unit);
		stringBuilder.Append("=");
		for (int i = 0; i < Ranges.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(ranges[i]);
		}
		return stringBuilder.ToString();
	}
}
