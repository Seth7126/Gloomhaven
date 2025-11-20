namespace System.Drawing;

/// <summary>Specifies a range of character positions within a string.</summary>
/// <filterpriority>1</filterpriority>
public struct CharacterRange
{
	private int first;

	private int length;

	/// <summary>Gets or sets the position in the string of the first character of this <see cref="T:System.Drawing.CharacterRange" />.</summary>
	/// <returns>The first position of this <see cref="T:System.Drawing.CharacterRange" />.</returns>
	/// <filterpriority>1</filterpriority>
	public int First
	{
		get
		{
			return first;
		}
		set
		{
			first = value;
		}
	}

	/// <summary>Gets or sets the number of positions in this <see cref="T:System.Drawing.CharacterRange" />.</summary>
	/// <returns>The number of positions in this <see cref="T:System.Drawing.CharacterRange" />.</returns>
	/// <filterpriority>1</filterpriority>
	public int Length
	{
		get
		{
			return length;
		}
		set
		{
			length = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.CharacterRange" /> structure, specifying a range of character positions within a string.</summary>
	/// <param name="First">The position of the first character in the range. For example, if <paramref name="First" /> is set to 0, the first position of the range is position 0 in the string. </param>
	/// <param name="Length">The number of positions in the range. </param>
	public CharacterRange(int First, int Length)
	{
		first = First;
		length = Length;
	}

	/// <summary>Gets a value indicating whether this object is equivalent to the specified object.</summary>
	/// <returns>true to indicate the specified object is an instance with the same <see cref="P:System.Drawing.CharacterRange.First" /> and <see cref="P:System.Drawing.CharacterRange.Length" /> value as this instance; otherwise, false.</returns>
	/// <param name="obj">The object to compare to for equality.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is CharacterRange characterRange))
		{
			return false;
		}
		return this == characterRange;
	}

	public override int GetHashCode()
	{
		return first ^ length;
	}

	/// <summary>Compares two <see cref="T:System.Drawing.CharacterRange" /> objects. Gets a value indicating whether the <see cref="P:System.Drawing.CharacterRange.First" /> and <see cref="P:System.Drawing.CharacterRange.Length" /> values of the two <see cref="T:System.Drawing.CharacterRange" /> objects are equal.</summary>
	/// <returns>true to indicate the two <see cref="T:System.Drawing.CharacterRange" /> objects have the same <see cref="P:System.Drawing.CharacterRange.First" /> and <see cref="P:System.Drawing.CharacterRange.Length" /> values; otherwise, false. </returns>
	/// <param name="cr1">A <see cref="T:System.Drawing.CharacterRange" /> to compare for equality.</param>
	/// <param name="cr2">A <see cref="T:System.Drawing.CharacterRange" /> to compare for equality.</param>
	public static bool operator ==(CharacterRange cr1, CharacterRange cr2)
	{
		if (cr1.first == cr2.first)
		{
			return cr1.length == cr2.length;
		}
		return false;
	}

	/// <summary>Compares two <see cref="T:System.Drawing.CharacterRange" /> objects. Gets a value indicating whether the <see cref="P:System.Drawing.CharacterRange.First" /> or <see cref="P:System.Drawing.CharacterRange.Length" /> values of the two <see cref="T:System.Drawing.CharacterRange" /> objects are not equal.</summary>
	/// <returns>true to indicate the either the <see cref="P:System.Drawing.CharacterRange.First" /> or <see cref="P:System.Drawing.CharacterRange.Length" /> values of the two <see cref="T:System.Drawing.CharacterRange" /> objects differ; otherwise, false. </returns>
	/// <param name="cr1">A <see cref="T:System.Drawing.CharacterRange" /> to compare for inequality.</param>
	/// <param name="cr2">A <see cref="T:System.Drawing.CharacterRange" /> to compare for inequality.</param>
	public static bool operator !=(CharacterRange cr1, CharacterRange cr2)
	{
		if (cr1.first == cr2.first)
		{
			return cr1.length != cr2.length;
		}
		return true;
	}
}
