using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents a media type used in a Content-Type header as defined in the RFC 2616.</summary>
public class MediaTypeHeaderValue : ICloneable
{
	internal List<NameValueHeaderValue> parameters;

	internal string media_type;

	/// <summary>Gets or sets the character set.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The character set.</returns>
	public string CharSet
	{
		get
		{
			if (parameters == null)
			{
				return null;
			}
			return parameters.Find((NameValueHeaderValue l) => string.Equals(l.Name, "charset", StringComparison.OrdinalIgnoreCase))?.Value;
		}
		set
		{
			if (parameters == null)
			{
				parameters = new List<NameValueHeaderValue>();
			}
			parameters.SetValue("charset", value);
		}
	}

	/// <summary>Gets or sets the media-type header value.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The media-type header value.</returns>
	public string MediaType
	{
		get
		{
			return media_type;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("MediaType");
			}
			string media;
			Token? token = TryParseMediaType(new Lexer(value), out media);
			if (!token.HasValue || token.Value.Kind != Token.Type.End)
			{
				throw new FormatException();
			}
			media_type = media;
		}
	}

	/// <summary>Gets or sets the media-type header value parameters.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.The media-type header value parameters.</returns>
	public ICollection<NameValueHeaderValue> Parameters => parameters ?? (parameters = new List<NameValueHeaderValue>());

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> class.</summary>
	/// <param name="mediaType">The source represented as a string to initialize the new instance. </param>
	public MediaTypeHeaderValue(string mediaType)
	{
		MediaType = mediaType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> class.</summary>
	/// <param name="source"> A <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> object used to initialize the new instance.</param>
	protected MediaTypeHeaderValue(MediaTypeHeaderValue source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		media_type = source.media_type;
		if (source.parameters == null)
		{
			return;
		}
		foreach (NameValueHeaderValue parameter in source.parameters)
		{
			Parameters.Add(new NameValueHeaderValue(parameter));
		}
	}

	internal MediaTypeHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return new MediaTypeHeaderValue(this);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is MediaTypeHeaderValue mediaTypeHeaderValue))
		{
			return false;
		}
		if (string.Equals(mediaTypeHeaderValue.media_type, media_type, StringComparison.OrdinalIgnoreCase))
		{
			return mediaTypeHeaderValue.parameters.SequenceEqual(parameters);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return media_type.ToLowerInvariant().GetHashCode() ^ HashCodeCalculator.Calculate(parameters);
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" />.An <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents media type header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid media type header value information.</exception>
	public static MediaTypeHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (parameters == null)
		{
			return media_type;
		}
		return media_type + CollectionExtensions.ToString(parameters);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.MediaTypeHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out MediaTypeHeaderValue parsedValue)
	{
		parsedValue = null;
		Lexer lexer = new Lexer(input);
		List<NameValueHeaderValue> result = null;
		string media;
		Token? token = TryParseMediaType(lexer, out media);
		if (!token.HasValue)
		{
			return false;
		}
		switch (token.Value.Kind)
		{
		case Token.Type.SeparatorSemicolon:
		{
			if (!NameValueHeaderValue.TryParseParameters(lexer, out result, out var t) || (Token.Type)t != Token.Type.End)
			{
				return false;
			}
			break;
		}
		default:
			return false;
		case Token.Type.End:
			break;
		}
		parsedValue = new MediaTypeHeaderValue
		{
			media_type = media,
			parameters = result
		};
		return true;
	}

	internal static Token? TryParseMediaType(Lexer lexer, out string media)
	{
		media = null;
		Token token = lexer.Scan();
		if ((Token.Type)token != Token.Type.Token)
		{
			return null;
		}
		if ((Token.Type)lexer.Scan() != Token.Type.SeparatorSlash)
		{
			return null;
		}
		Token token2 = lexer.Scan();
		if ((Token.Type)token2 != Token.Type.Token)
		{
			return null;
		}
		media = lexer.GetStringValue(token) + "/" + lexer.GetStringValue(token2);
		return lexer.Scan();
	}
}
