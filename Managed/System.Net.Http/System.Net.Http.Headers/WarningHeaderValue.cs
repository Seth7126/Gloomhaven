using System.Collections.Generic;
using System.Globalization;

namespace System.Net.Http.Headers;

/// <summary>Represents a warning value used by the Warning header.</summary>
public class WarningHeaderValue : ICloneable
{
	/// <summary>Gets the host that attached the warning.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The host that attached the warning.</returns>
	public string Agent { get; private set; }

	/// <summary>Gets the specific warning code.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.The specific warning code.</returns>
	public int Code { get; private set; }

	/// <summary>Gets the date/time stamp of the warning.</summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The date/time stamp of the warning.</returns>
	public DateTimeOffset? Date { get; private set; }

	/// <summary>Gets a quoted-string containing the warning text.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A quoted-string containing the warning text.</returns>
	public string Text { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> class.</summary>
	/// <param name="code">The specific warning code.</param>
	/// <param name="agent">The host that attached the warning.</param>
	/// <param name="text">A quoted-string containing the warning text.</param>
	public WarningHeaderValue(int code, string agent, string text)
	{
		if (!IsCodeValid(code))
		{
			throw new ArgumentOutOfRangeException("code");
		}
		Parser.Uri.Check(agent);
		Parser.Token.CheckQuotedString(text);
		Code = code;
		Agent = agent;
		Text = text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> class.</summary>
	/// <param name="code">The specific warning code.</param>
	/// <param name="agent">The host that attached the warning.</param>
	/// <param name="text">A quoted-string containing the warning text.</param>
	/// <param name="date">The date/time stamp of the warning.</param>
	public WarningHeaderValue(int code, string agent, string text, DateTimeOffset date)
		: this(code, agent, text)
	{
		Date = date;
	}

	private WarningHeaderValue()
	{
	}

	private static bool IsCodeValid(int code)
	{
		if (code >= 0)
		{
			return code < 1000;
		}
		return false;
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.Returns a copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is WarningHeaderValue warningHeaderValue))
		{
			return false;
		}
		if (Code == warningHeaderValue.Code && string.Equals(warningHeaderValue.Agent, Agent, StringComparison.OrdinalIgnoreCase) && Text == warningHeaderValue.Text)
		{
			DateTimeOffset? date = Date;
			DateTimeOffset? date2 = warningHeaderValue.Date;
			if (date.HasValue != date2.HasValue)
			{
				return false;
			}
			if (!date.HasValue)
			{
				return true;
			}
			return date.GetValueOrDefault() == date2.GetValueOrDefault();
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Code.GetHashCode() ^ Agent.ToLowerInvariant().GetHashCode() ^ Text.GetHashCode() ^ Date.GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> instance.</summary>
	/// <returns>Returns an <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents authentication header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid authentication header value information.</exception>
	public static WarningHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out WarningHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<WarningHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<WarningHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out WarningHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		if (!lexer.TryGetNumericValue(t, out int value) || !IsCodeValid(value))
		{
			return false;
		}
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		Token token = t;
		if (lexer.PeekChar() == 58)
		{
			lexer.EatChar();
			token = lexer.Scan();
			if ((Token.Type)token != Token.Type.Token)
			{
				return false;
			}
		}
		WarningHeaderValue warningHeaderValue = new WarningHeaderValue();
		warningHeaderValue.Code = value;
		warningHeaderValue.Agent = lexer.GetStringValue(t, token);
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.QuotedString)
		{
			return false;
		}
		warningHeaderValue.Text = lexer.GetStringValue(t);
		t = lexer.Scan();
		if ((Token.Type)t == Token.Type.QuotedString)
		{
			if (!lexer.TryGetDateValue(t, out var value2))
			{
				return false;
			}
			warningHeaderValue.Date = value2;
			t = lexer.Scan();
		}
		parsedValue = warningHeaderValue;
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.WarningHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		string text = Code.ToString("000") + " " + Agent + " " + Text;
		if (Date.HasValue)
		{
			text = text + " \"" + Date.Value.ToString("r", CultureInfo.InvariantCulture) + "\"";
		}
		return text;
	}
}
