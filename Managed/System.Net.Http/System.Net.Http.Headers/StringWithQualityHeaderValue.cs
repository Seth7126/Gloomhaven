using System.Collections.Generic;
using System.Globalization;

namespace System.Net.Http.Headers;

/// <summary>Represents a string header value with an optional quality.</summary>
public class StringWithQualityHeaderValue : ICloneable
{
	/// <summary>Gets the quality factor from the <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Double" />.The quality factor from the <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> object.</returns>
	public double? Quality { get; private set; }

	/// <summary>Gets the string value from the <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The string value from the <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> object.</returns>
	public string Value { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> class.</summary>
	/// <param name="value">The string used to initialize the new instance.</param>
	public StringWithQualityHeaderValue(string value)
	{
		Parser.Token.Check(value);
		Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> class.</summary>
	/// <param name="value">A string used to initialize the new instance.</param>
	/// <param name="quality">A quality factor used to initialize the new instance.</param>
	public StringWithQualityHeaderValue(string value, double quality)
		: this(value)
	{
		if (quality < 0.0 || quality > 1.0)
		{
			throw new ArgumentOutOfRangeException("quality");
		}
		Quality = quality;
	}

	private StringWithQualityHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified Object is equal to the current <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is StringWithQualityHeaderValue stringWithQualityHeaderValue && string.Equals(stringWithQualityHeaderValue.Value, Value, StringComparison.OrdinalIgnoreCase))
		{
			return stringWithQualityHeaderValue.Quality == Quality;
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Value.ToLowerInvariant().GetHashCode() ^ Quality.GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" />.An <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents quality header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid string with quality header value information.</exception>
	public static StringWithQualityHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out StringWithQualityHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<StringWithQualityHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<StringWithQualityHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out StringWithQualityHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		StringWithQualityHeaderValue stringWithQualityHeaderValue = new StringWithQualityHeaderValue();
		stringWithQualityHeaderValue.Value = lexer.GetStringValue(t);
		t = lexer.Scan();
		if ((Token.Type)t == Token.Type.SeparatorSemicolon)
		{
			t = lexer.Scan();
			if ((Token.Type)t != Token.Type.Token)
			{
				return false;
			}
			string stringValue = lexer.GetStringValue(t);
			if (stringValue != "q" && stringValue != "Q")
			{
				return false;
			}
			t = lexer.Scan();
			if ((Token.Type)t != Token.Type.SeparatorEqual)
			{
				return false;
			}
			t = lexer.Scan();
			if (!lexer.TryGetDoubleValue(t, out var value))
			{
				return false;
			}
			if (value > 1.0)
			{
				return false;
			}
			stringWithQualityHeaderValue.Quality = value;
			t = lexer.Scan();
		}
		parsedValue = stringWithQualityHeaderValue;
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.StringWithQualityHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (Quality.HasValue)
		{
			return Value + "; q=" + Quality.Value.ToString("0.0##", CultureInfo.InvariantCulture);
		}
		return Value;
	}
}
