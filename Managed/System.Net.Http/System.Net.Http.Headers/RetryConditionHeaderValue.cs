using System.Globalization;

namespace System.Net.Http.Headers;

/// <summary>Represents a Retry-After header value which can either be a date/time or a timespan value.</summary>
public class RetryConditionHeaderValue : ICloneable
{
	/// <summary>Gets the date and time offset from the <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The date and time offset from the <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> object.</returns>
	public DateTimeOffset? Date { get; private set; }

	/// <summary>Gets the delta in seconds from the <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.TimeSpan" />.The delta in seconds from the <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> object.</returns>
	public TimeSpan? Delta { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> class.</summary>
	/// <param name="date">The date and time offset used to initialize the new instance.</param>
	public RetryConditionHeaderValue(DateTimeOffset date)
	{
		Date = date;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> class.</summary>
	/// <param name="delta">The delta, in seconds, used to initialize the new instance.</param>
	public RetryConditionHeaderValue(TimeSpan delta)
	{
		if (delta.TotalSeconds > 4294967295.0)
		{
			throw new ArgumentOutOfRangeException("delta");
		}
		Delta = delta;
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is RetryConditionHeaderValue { Date: var date } retryConditionHeaderValue && date == Date)
		{
			TimeSpan? delta = retryConditionHeaderValue.Delta;
			TimeSpan? delta2 = Delta;
			if (delta.HasValue != delta2.HasValue)
			{
				return false;
			}
			if (!delta.HasValue)
			{
				return true;
			}
			return delta.GetValueOrDefault() == delta2.GetValueOrDefault();
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return Date.GetHashCode() ^ Delta.GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" />.An <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents retry condition header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid retry condition header value information.</exception>
	public static RetryConditionHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out RetryConditionHeaderValue parsedValue)
	{
		parsedValue = null;
		Lexer lexer = new Lexer(input);
		Token token = lexer.Scan();
		if ((Token.Type)token != Token.Type.Token)
		{
			return false;
		}
		TimeSpan? timeSpan = lexer.TryGetTimeSpanValue(token);
		if (timeSpan.HasValue)
		{
			if ((Token.Type)lexer.Scan() != Token.Type.End)
			{
				return false;
			}
			parsedValue = new RetryConditionHeaderValue(timeSpan.Value);
		}
		else
		{
			if (!Lexer.TryGetDateValue(input, out var value))
			{
				return false;
			}
			parsedValue = new RetryConditionHeaderValue(value);
		}
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.RetryConditionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (!Delta.HasValue)
		{
			return Date.Value.ToString("r", CultureInfo.InvariantCulture);
		}
		return Delta.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture);
	}
}
