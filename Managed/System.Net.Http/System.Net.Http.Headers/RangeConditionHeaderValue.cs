using System.Globalization;

namespace System.Net.Http.Headers;

/// <summary>Represents an If-Range header value which can either be a date/time or an entity-tag value.</summary>
public class RangeConditionHeaderValue : ICloneable
{
	/// <summary>Gets the date from the <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The date from the <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> object.</returns>
	public DateTimeOffset? Date { get; private set; }

	/// <summary>Gets the entity tag from the <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" />.The entity tag from the <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> object.</returns>
	public EntityTagHeaderValue EntityTag { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> class.</summary>
	/// <param name="date">A date value used to initialize the new instance.</param>
	public RangeConditionHeaderValue(DateTimeOffset date)
	{
		Date = date;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> class.</summary>
	/// <param name="entityTag">An <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> object used to initialize the new instance.</param>
	public RangeConditionHeaderValue(EntityTagHeaderValue entityTag)
	{
		if (entityTag == null)
		{
			throw new ArgumentNullException("entityTag");
		}
		EntityTag = entityTag;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> class.</summary>
	/// <param name="entityTag">An entity tag represented as a string used to initialize the new instance.</param>
	public RangeConditionHeaderValue(string entityTag)
		: this(new EntityTagHeaderValue(entityTag))
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is RangeConditionHeaderValue rangeConditionHeaderValue))
		{
			return false;
		}
		if (EntityTag == null)
		{
			DateTimeOffset? date = Date;
			DateTimeOffset? date2 = rangeConditionHeaderValue.Date;
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
		return EntityTag.Equals(rangeConditionHeaderValue.EntityTag);
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		if (EntityTag == null)
		{
			return Date.GetHashCode();
		}
		return EntityTag.GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" />.An <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents range condition header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid range Condition header value information.</exception>
	public static RangeConditionHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out RangeConditionHeaderValue parsedValue)
	{
		parsedValue = null;
		Lexer lexer = new Lexer(input);
		Token token = lexer.Scan();
		bool isWeak;
		if ((Token.Type)token == Token.Type.Token)
		{
			if (lexer.GetStringValue(token) != "W")
			{
				if (!Lexer.TryGetDateValue(input, out var value))
				{
					return false;
				}
				parsedValue = new RangeConditionHeaderValue(value);
				return true;
			}
			if (lexer.PeekChar() != 47)
			{
				return false;
			}
			isWeak = true;
			lexer.EatChar();
			token = lexer.Scan();
		}
		else
		{
			isWeak = false;
		}
		if ((Token.Type)token != Token.Type.QuotedString)
		{
			return false;
		}
		if ((Token.Type)lexer.Scan() != Token.Type.End)
		{
			return false;
		}
		parsedValue = new RangeConditionHeaderValue(new EntityTagHeaderValue
		{
			Tag = lexer.GetStringValue(token),
			IsWeak = isWeak
		});
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.RangeConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (EntityTag != null)
		{
			return EntityTag.ToString();
		}
		return Date.Value.ToString("r", CultureInfo.InvariantCulture);
	}
}
