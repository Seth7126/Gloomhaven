using System.Globalization;
using System.Text;

namespace System.Net.Http.Headers;

/// <summary>Represents the value of the Content-Range header.</summary>
public class ContentRangeHeaderValue : ICloneable
{
	private string unit = "bytes";

	/// <summary>Gets the position at which to start sending data.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The position, in bytes, at which to start sending data.</returns>
	public long? From { get; private set; }

	/// <summary>Gets whether the Content-Range header has a length specified.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the Content-Range has a length specified; otherwise, false.</returns>
	public bool HasLength => Length.HasValue;

	/// <summary>Gets whether the Content-Range has a range specified. </summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the Content-Range has a range specified; otherwise, false.</returns>
	public bool HasRange => From.HasValue;

	/// <summary>Gets the length of the full entity-body.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The length of the full entity-body.</returns>
	public long? Length { get; private set; }

	/// <summary>Gets the position at which to stop sending data.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The position at which to stop sending data.</returns>
	public long? To { get; private set; }

	/// <summary>The range units used.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A <see cref="T:System.String" /> that contains range units. </returns>
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

	private ContentRangeHeaderValue()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> class.</summary>
	/// <param name="length">The starting or ending point of the range, in bytes.</param>
	public ContentRangeHeaderValue(long length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		Length = length;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> class.</summary>
	/// <param name="from">The position, in bytes, at which to start sending data.</param>
	/// <param name="to">The position, in bytes, at which to stop sending data.</param>
	public ContentRangeHeaderValue(long from, long to)
	{
		if (from < 0 || from > to)
		{
			throw new ArgumentOutOfRangeException("from");
		}
		From = from;
		To = to;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> class.</summary>
	/// <param name="from">The position, in bytes, at which to start sending data.</param>
	/// <param name="to">The position, in bytes, at which to stop sending data.</param>
	/// <param name="length">The starting or ending point of the range, in bytes.</param>
	public ContentRangeHeaderValue(long from, long to, long length)
		: this(from, to)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (to > length)
		{
			throw new ArgumentOutOfRangeException("to");
		}
		Length = length;
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified Object is equal to the current <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is ContentRangeHeaderValue { Length: var length } contentRangeHeaderValue))
		{
			return false;
		}
		if (length == Length && contentRangeHeaderValue.From == From && contentRangeHeaderValue.To == To)
		{
			return string.Equals(contentRangeHeaderValue.unit, unit, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Unit.GetHashCode() ^ Length.GetHashCode() ^ From.GetHashCode() ^ To.GetHashCode() ^ unit.ToLowerInvariant().GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" />.An <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents content range header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid content range header value information.</exception>
	public static ContentRangeHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out ContentRangeHeaderValue parsedValue)
	{
		parsedValue = null;
		Lexer lexer = new Lexer(input);
		Token token = lexer.Scan();
		if ((Token.Type)token != Token.Type.Token)
		{
			return false;
		}
		ContentRangeHeaderValue contentRangeHeaderValue = new ContentRangeHeaderValue();
		contentRangeHeaderValue.unit = lexer.GetStringValue(token);
		token = lexer.Scan();
		if ((Token.Type)token != Token.Type.Token)
		{
			return false;
		}
		if (!lexer.IsStarStringValue(token))
		{
			if (!lexer.TryGetNumericValue(token, out long value))
			{
				string stringValue = lexer.GetStringValue(token);
				if (stringValue.Length < 3)
				{
					return false;
				}
				string[] array = stringValue.Split('-');
				if (array.Length != 2)
				{
					return false;
				}
				if (!long.TryParse(array[0], NumberStyles.None, CultureInfo.InvariantCulture, out value))
				{
					return false;
				}
				contentRangeHeaderValue.From = value;
				if (!long.TryParse(array[1], NumberStyles.None, CultureInfo.InvariantCulture, out value))
				{
					return false;
				}
				contentRangeHeaderValue.To = value;
			}
			else
			{
				contentRangeHeaderValue.From = value;
				token = lexer.Scan(recognizeDash: true);
				if ((Token.Type)token != Token.Type.SeparatorDash)
				{
					return false;
				}
				token = lexer.Scan();
				if (!lexer.TryGetNumericValue(token, out value))
				{
					return false;
				}
				contentRangeHeaderValue.To = value;
			}
		}
		token = lexer.Scan();
		if ((Token.Type)token != Token.Type.SeparatorSlash)
		{
			return false;
		}
		token = lexer.Scan();
		if (!lexer.IsStarStringValue(token))
		{
			if (!lexer.TryGetNumericValue(token, out long value2))
			{
				return false;
			}
			contentRangeHeaderValue.Length = value2;
		}
		token = lexer.Scan();
		if ((Token.Type)token != Token.Type.End)
		{
			return false;
		}
		parsedValue = contentRangeHeaderValue;
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.ContentRangeHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(unit);
		stringBuilder.Append(" ");
		if (!From.HasValue)
		{
			stringBuilder.Append("*");
		}
		else
		{
			stringBuilder.Append(From.Value.ToString(CultureInfo.InvariantCulture));
			stringBuilder.Append("-");
			stringBuilder.Append(To.Value.ToString(CultureInfo.InvariantCulture));
		}
		stringBuilder.Append("/");
		stringBuilder.Append((!Length.HasValue) ? "*" : Length.Value.ToString(CultureInfo.InvariantCulture));
		return stringBuilder.ToString();
	}
}
