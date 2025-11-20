using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents a product token value in a User-Agent header.</summary>
public class ProductHeaderValue : ICloneable
{
	/// <summary>Gets the name of the product token.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The name of the product token.</returns>
	public string Name { get; internal set; }

	/// <summary>Gets the version of the product token.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The version of the product token. </returns>
	public string Version { get; internal set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> class.</summary>
	/// <param name="name">The product name.</param>
	public ProductHeaderValue(string name)
	{
		Parser.Token.Check(name);
		Name = name;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> class.</summary>
	/// <param name="name">The product name value.</param>
	/// <param name="version">The product version value.</param>
	public ProductHeaderValue(string name, string version)
		: this(name)
	{
		if (!string.IsNullOrEmpty(version))
		{
			Parser.Token.Check(version);
		}
		Version = version;
	}

	internal ProductHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is ProductHeaderValue productHeaderValue))
		{
			return false;
		}
		if (string.Equals(productHeaderValue.Name, Name, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(productHeaderValue.Version, Version, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int num = Name.ToLowerInvariant().GetHashCode();
		if (Version != null)
		{
			num ^= Version.ToLowerInvariant().GetHashCode();
		}
		return num;
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ProductHeaderValue" />.An <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents product header value information.</param>
	public static ProductHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out ProductHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<ProductHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<ProductHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out ProductHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		parsedValue = new ProductHeaderValue();
		parsedValue.Name = lexer.GetStringValue(t);
		t = lexer.Scan();
		if ((Token.Type)t == Token.Type.SeparatorSlash)
		{
			t = lexer.Scan();
			if ((Token.Type)t != Token.Type.Token)
			{
				return false;
			}
			parsedValue.Version = lexer.GetStringValue(t);
			t = lexer.Scan();
		}
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.ProductHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (Version != null)
		{
			return Name + "/" + Version;
		}
		return Name;
	}
}
