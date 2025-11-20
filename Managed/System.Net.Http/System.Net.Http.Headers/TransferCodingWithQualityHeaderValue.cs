using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents an Accept-Encoding header value.with optional quality factor.</summary>
public sealed class TransferCodingWithQualityHeaderValue : TransferCodingHeaderValue
{
	/// <summary>Gets the quality factor from the <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" />.</summary>
	/// <returns>Returns <see cref="T:System.Double" />.The quality factor from the <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" />.</returns>
	public double? Quality
	{
		get
		{
			return QualityValue.GetValue(parameters);
		}
		set
		{
			QualityValue.SetValue(ref parameters, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" /> class.</summary>
	/// <param name="value">A string used to initialize the new instance.</param>
	public TransferCodingWithQualityHeaderValue(string value)
		: base(value)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" /> class.</summary>
	/// <param name="value">A string used to initialize the new instance.</param>
	/// <param name="quality">A value for the quality factor.</param>
	public TransferCodingWithQualityHeaderValue(string value, double quality)
		: this(value)
	{
		Quality = quality;
	}

	private TransferCodingWithQualityHeaderValue()
	{
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" />.An <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents transfer-coding value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid transfer-coding with quality header value information.</exception>
	public new static TransferCodingWithQualityHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException();
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.TransferCodingWithQualityHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out TransferCodingWithQualityHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<TransferCodingWithQualityHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<TransferCodingWithQualityHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out TransferCodingWithQualityHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		TransferCodingWithQualityHeaderValue transferCodingWithQualityHeaderValue = new TransferCodingWithQualityHeaderValue();
		transferCodingWithQualityHeaderValue.value = lexer.GetStringValue(t);
		t = lexer.Scan();
		if ((Token.Type)t == Token.Type.SeparatorSemicolon && (!NameValueHeaderValue.TryParseParameters(lexer, out transferCodingWithQualityHeaderValue.parameters, out t) || (Token.Type)t != Token.Type.End))
		{
			return false;
		}
		parsedValue = transferCodingWithQualityHeaderValue;
		return true;
	}
}
