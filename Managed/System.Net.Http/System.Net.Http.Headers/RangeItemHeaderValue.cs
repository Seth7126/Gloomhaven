namespace System.Net.Http.Headers;

/// <summary>Represents a byte range in a Range header value.</summary>
public class RangeItemHeaderValue : ICloneable
{
	/// <summary>Gets the position at which to start sending data.</summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The position at which to start sending data.</returns>
	public long? From { get; private set; }

	/// <summary>Gets the position at which to stop sending data. </summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The position at which to stop sending data. </returns>
	public long? To { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.RangeItemHeaderValue" /> class.</summary>
	/// <param name="from">The position at which to start sending data.</param>
	/// <param name="to">The position at which to stop sending data.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="from" /> is greater than <paramref name="to" />-or- <paramref name="from" /> or <paramref name="to" /> is less than 0. </exception>
	public RangeItemHeaderValue(long? from, long? to)
	{
		if (!from.HasValue && !to.HasValue)
		{
			throw new ArgumentException();
		}
		if (from.HasValue && to.HasValue && from > to)
		{
			throw new ArgumentOutOfRangeException("from");
		}
		if (from < 0)
		{
			throw new ArgumentOutOfRangeException("from");
		}
		if (to < 0)
		{
			throw new ArgumentOutOfRangeException("to");
		}
		From = from;
		To = to;
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.RangeItemHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.RangeItemHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is RangeItemHeaderValue { From: var num } rangeItemHeaderValue && num == From)
		{
			return rangeItemHeaderValue.To == To;
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.RangeItemHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return From.GetHashCode() ^ To.GetHashCode();
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.RangeItemHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (!From.HasValue)
		{
			return "-" + To.Value;
		}
		if (!To.HasValue)
		{
			return From.Value + "-";
		}
		return From.Value + "-" + To.Value;
	}
}
