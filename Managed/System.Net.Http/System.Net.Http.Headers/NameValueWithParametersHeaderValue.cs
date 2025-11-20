using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents a name/value pair with parameters used in various headers as defined in RFC 2616.</summary>
public class NameValueWithParametersHeaderValue : NameValueHeaderValue, ICloneable
{
	private List<NameValueHeaderValue> parameters;

	/// <summary>Gets the parameters from the <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.A collection containing the parameters.</returns>
	public ICollection<NameValueHeaderValue> Parameters => parameters ?? (parameters = new List<NameValueHeaderValue>());

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> class.</summary>
	/// <param name="name">The header name.</param>
	public NameValueWithParametersHeaderValue(string name)
		: base(name)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> class.</summary>
	/// <param name="name">The header name.</param>
	/// <param name="value">The header value.</param>
	public NameValueWithParametersHeaderValue(string name, string value)
		: base(name, value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> class.</summary>
	/// <param name="source">A <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> object used to initialize the new instance.</param>
	protected NameValueWithParametersHeaderValue(NameValueWithParametersHeaderValue source)
		: base(source)
	{
		if (source.parameters == null)
		{
			return;
		}
		foreach (NameValueHeaderValue parameter in source.parameters)
		{
			Parameters.Add(parameter);
		}
	}

	private NameValueWithParametersHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return new NameValueWithParametersHeaderValue(this);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is NameValueWithParametersHeaderValue nameValueWithParametersHeaderValue))
		{
			return false;
		}
		if (base.Equals(obj))
		{
			return nameValueWithParametersHeaderValue.parameters.SequenceEqual(parameters);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode() ^ HashCodeCalculator.Calculate(parameters);
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" />.An <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents name value with parameter header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid name value with parameter header value information.</exception>
	public new static NameValueWithParametersHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (parameters == null || parameters.Count == 0)
		{
			return base.ToString();
		}
		return base.ToString() + CollectionExtensions.ToString(parameters);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.NameValueWithParametersHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out NameValueWithParametersHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<NameValueWithParametersHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<NameValueWithParametersHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out NameValueWithParametersHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		parsedValue = new NameValueWithParametersHeaderValue
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
		if ((Token.Type)t == Token.Type.SeparatorSemicolon)
		{
			if (!NameValueHeaderValue.TryParseParameters(lexer, out var result, out t))
			{
				return false;
			}
			parsedValue.parameters = result;
		}
		return true;
	}
}
