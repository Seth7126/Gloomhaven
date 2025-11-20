using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents a name/value pair used in various headers as defined in RFC 2616.</summary>
public class NameValueHeaderValue : ICloneable
{
	internal string value;

	/// <summary>Gets the header name.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The header name.</returns>
	public string Name { get; internal set; }

	/// <summary>Gets the header value.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The header value.</returns>
	public string Value
	{
		get
		{
			return value;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				Lexer lexer = new Lexer(value);
				Token token = lexer.Scan();
				if ((Token.Type)lexer.Scan() != Token.Type.End || ((Token.Type)token != Token.Type.Token && (Token.Type)token != Token.Type.QuotedString))
				{
					throw new FormatException();
				}
				value = lexer.GetStringValue(token);
			}
			this.value = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> class.</summary>
	/// <param name="name">The header name.</param>
	public NameValueHeaderValue(string name)
		: this(name, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> class.</summary>
	/// <param name="name">The header name.</param>
	/// <param name="value">The header value.</param>
	public NameValueHeaderValue(string name, string value)
	{
		Parser.Token.Check(name);
		Name = name;
		Value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> class.</summary>
	/// <param name="source">A <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> object used to initialize the new instance.</param>
	protected internal NameValueHeaderValue(NameValueHeaderValue source)
	{
		Name = source.Name;
		value = source.value;
	}

	internal NameValueHeaderValue()
	{
	}

	internal static NameValueHeaderValue Create(string name, string value)
	{
		return new NameValueHeaderValue
		{
			Name = name,
			value = value
		};
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return new NameValueHeaderValue(this);
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int num = Name.ToLowerInvariant().GetHashCode();
		if (!string.IsNullOrEmpty(value))
		{
			num ^= value.ToLowerInvariant().GetHashCode();
		}
		return num;
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is NameValueHeaderValue nameValueHeaderValue) || !string.Equals(nameValueHeaderValue.Name, Name, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (string.IsNullOrEmpty(value))
		{
			return string.IsNullOrEmpty(nameValueHeaderValue.value);
		}
		return string.Equals(nameValueHeaderValue.value, value, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" />.An <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents name value header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid name value header value information.</exception>
	public static NameValueHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	internal static bool TryParsePragma(string input, int minimalCount, out List<NameValueHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<NameValueHeaderValue>)TryParseElement, out result);
	}

	internal static bool TryParseParameters(Lexer lexer, out List<NameValueHeaderValue> result, out Token t)
	{
		List<NameValueHeaderValue> list = new List<NameValueHeaderValue>();
		result = null;
		do
		{
			Token token = lexer.Scan();
			if ((Token.Type)token != Token.Type.Token)
			{
				t = Token.Empty;
				return false;
			}
			string text = null;
			t = lexer.Scan();
			if ((Token.Type)t == Token.Type.SeparatorEqual)
			{
				t = lexer.Scan();
				if ((Token.Type)t != Token.Type.Token && (Token.Type)t != Token.Type.QuotedString)
				{
					return false;
				}
				text = lexer.GetStringValue(t);
				t = lexer.Scan();
			}
			list.Add(new NameValueHeaderValue
			{
				Name = lexer.GetStringValue(token),
				value = text
			});
		}
		while ((Token.Type)t == Token.Type.SeparatorSemicolon);
		result = list;
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (string.IsNullOrEmpty(value))
		{
			return Name;
		}
		return Name + "=" + value;
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.NameValueHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out NameValueHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	private static bool TryParseElement(Lexer lexer, out NameValueHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		parsedValue = new NameValueHeaderValue
		{
			Name = lexer.GetStringValue(t)
		};
		t = lexer.Scan();
		if ((Token.Type)t == Token.Type.SeparatorEqual)
		{
			t = lexer.Scan();
			if ((Token.Type)t != Token.Type.Token && (Token.Type)t != Token.Type.QuotedString)
			{
				return false;
			}
			parsedValue.value = lexer.GetStringValue(t);
			t = lexer.Scan();
		}
		return true;
	}
}
