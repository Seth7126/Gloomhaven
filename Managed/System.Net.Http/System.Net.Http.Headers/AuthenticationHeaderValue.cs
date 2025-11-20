using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents authentication information in Authorization, ProxyAuthorization, WWW-Authenticate, and Proxy-Authenticate header values.</summary>
public class AuthenticationHeaderValue : ICloneable
{
	/// <summary>Gets the credentials containing the authentication information of the user agent for the resource being requested.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The credentials containing the authentication information.</returns>
	public string Parameter { get; private set; }

	/// <summary>Gets the scheme to use for authorization.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The scheme to use for authorization.</returns>
	public string Scheme { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> class.</summary>
	/// <param name="scheme">The scheme to use for authorization.</param>
	public AuthenticationHeaderValue(string scheme)
		: this(scheme, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> class.</summary>
	/// <param name="scheme">The scheme to use for authorization.</param>
	/// <param name="parameter">The credentials containing the authentication information of the user agent for the resource being requested.</param>
	public AuthenticationHeaderValue(string scheme, string parameter)
	{
		Parser.Token.Check(scheme);
		Scheme = scheme;
		Parameter = parameter;
	}

	private AuthenticationHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object. </param>
	public override bool Equals(object obj)
	{
		if (obj is AuthenticationHeaderValue authenticationHeaderValue && string.Equals(authenticationHeaderValue.Scheme, Scheme, StringComparison.OrdinalIgnoreCase))
		{
			return authenticationHeaderValue.Parameter == Parameter;
		}
		return false;
	}

	/// <summary>Serves as a hash function for an  <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int num = Scheme.ToLowerInvariant().GetHashCode();
		if (!string.IsNullOrEmpty(Parameter))
		{
			num ^= Parameter.ToLowerInvariant().GetHashCode();
		}
		return num;
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" />.An <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents authentication header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid authentication header value information.</exception>
	public static AuthenticationHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out AuthenticationHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<AuthenticationHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<AuthenticationHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out AuthenticationHeaderValue parsedValue, out Token t)
	{
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			parsedValue = null;
			return false;
		}
		parsedValue = new AuthenticationHeaderValue();
		parsedValue.Scheme = lexer.GetStringValue(t);
		t = lexer.Scan();
		if ((Token.Type)t == Token.Type.Token)
		{
			parsedValue.Parameter = lexer.GetRemainingStringValue(t.StartPosition);
			t = new Token(Token.Type.End, 0, 0);
		}
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.AuthenticationHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (Parameter == null)
		{
			return Scheme;
		}
		return Scheme + " " + Parameter;
	}
}
