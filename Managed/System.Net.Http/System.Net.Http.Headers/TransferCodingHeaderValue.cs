using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents an accept-encoding header value.</summary>
public class TransferCodingHeaderValue : ICloneable
{
	internal string value;

	internal List<NameValueHeaderValue> parameters;

	/// <summary>Gets the transfer-coding parameters.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.The transfer-coding parameters.</returns>
	public ICollection<NameValueHeaderValue> Parameters => parameters ?? (parameters = new List<NameValueHeaderValue>());

	/// <summary>Gets the transfer-coding value.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The transfer-coding value.</returns>
	public string Value => value;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> class.</summary>
	/// <param name="value">A string used to initialize the new instance.</param>
	public TransferCodingHeaderValue(string value)
	{
		Parser.Token.Check(value);
		this.value = value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> class.</summary>
	/// <param name="source">A <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> object used to initialize the new instance. </param>
	protected TransferCodingHeaderValue(TransferCodingHeaderValue source)
	{
		value = source.value;
		if (source.parameters == null)
		{
			return;
		}
		foreach (NameValueHeaderValue parameter in source.parameters)
		{
			Parameters.Add(new NameValueHeaderValue(parameter));
		}
	}

	internal TransferCodingHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return new TransferCodingHeaderValue(this);
	}

	/// <summary>Determines whether the specified Object is equal to the current <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is TransferCodingHeaderValue transferCodingHeaderValue && string.Equals(value, transferCodingHeaderValue.value, StringComparison.OrdinalIgnoreCase))
		{
			return parameters.SequenceEqual(transferCodingHeaderValue.parameters);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int num = value.ToLowerInvariant().GetHashCode();
		if (parameters != null)
		{
			num ^= HashCodeCalculator.Calculate(parameters);
		}
		return num;
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" />.An <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents transfer-coding header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid transfer-coding header value information.</exception>
	public static TransferCodingHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		return value + CollectionExtensions.ToString(parameters);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.TransferCodingHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out TransferCodingHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<TransferCodingHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<TransferCodingHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out TransferCodingHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		TransferCodingHeaderValue transferCodingHeaderValue = new TransferCodingHeaderValue();
		transferCodingHeaderValue.value = lexer.GetStringValue(t);
		t = lexer.Scan();
		if ((Token.Type)t == Token.Type.SeparatorSemicolon && (!NameValueHeaderValue.TryParseParameters(lexer, out transferCodingHeaderValue.parameters, out t) || (Token.Type)t != Token.Type.End))
		{
			return false;
		}
		parsedValue = transferCodingHeaderValue;
		return true;
	}
}
