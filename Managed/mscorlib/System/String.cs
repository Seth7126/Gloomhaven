using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace System;

/// <summary>Represents text as a series of Unicode characters.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public sealed class String : IComparable, IEnumerable, IEnumerable<char>, IComparable<string>, IEquatable<string>, IConvertible, ICloneable
{
	private enum TrimType
	{
		Head,
		Tail,
		Both
	}

	[StructLayout(LayoutKind.Explicit, Size = 32)]
	private struct ProbabilisticMap
	{
	}

	private const int StackallocIntBufferSizeLimit = 128;

	private const int PROBABILISTICMAP_BLOCK_INDEX_MASK = 7;

	private const int PROBABILISTICMAP_BLOCK_INDEX_SHIFT = 3;

	private const int PROBABILISTICMAP_SIZE = 8;

	[NonSerialized]
	private int _stringLength;

	[NonSerialized]
	private char _firstChar;

	/// <summary>Represents the empty string. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly string Empty;

	/// <summary>Gets the number of characters in the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The number of characters in the current string.</returns>
	/// <filterpriority>1</filterpriority>
	public int Length => _stringLength;

	/// <summary>Gets the <see cref="T:System.Char" /> object at a specified position in the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The object at position <paramref name="index" />.</returns>
	/// <param name="index">A position in the current string. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is greater than or equal to the length of this object or less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	[IndexerName("Chars")]
	public char this[int index]
	{
		[Intrinsic]
		get
		{
			if ((uint)index >= _stringLength)
			{
				ThrowHelper.ThrowIndexOutOfRangeException();
			}
			return Unsafe.Add(ref _firstChar, index);
		}
	}

	private unsafe static int CompareOrdinalIgnoreCaseHelper(string strA, string strB)
	{
		int num = Math.Min(strA.Length, strB.Length);
		fixed (char* firstChar = &strA._firstChar)
		{
			fixed (char* firstChar2 = &strB._firstChar)
			{
				char* ptr = firstChar;
				char* ptr2 = firstChar2;
				int num2 = 0;
				int num3 = 0;
				while (num != 0)
				{
					num2 = *ptr;
					num3 = *ptr2;
					if ((uint)(num2 - 97) <= 25u)
					{
						num2 -= 32;
					}
					if ((uint)(num3 - 97) <= 25u)
					{
						num3 -= 32;
					}
					if (num2 != num3)
					{
						return num2 - num3;
					}
					ptr++;
					ptr2++;
					num--;
				}
				return strA.Length - strB.Length;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool EqualsHelper(string strA, string strB)
	{
		return SpanHelpers.SequenceEqual(ref Unsafe.As<char, byte>(ref strA.GetRawStringData()), ref Unsafe.As<char, byte>(ref strB.GetRawStringData()), (ulong)strA.Length * 2uL);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int CompareOrdinalHelper(string strA, int indexA, int countA, string strB, int indexB, int countB)
	{
		return SpanHelpers.SequenceCompareTo(ref Unsafe.Add(ref strA.GetRawStringData(), indexA), countA, ref Unsafe.Add(ref strB.GetRawStringData(), indexB), countB);
	}

	private unsafe static bool EqualsIgnoreCaseAsciiHelper(string strA, string strB)
	{
		int num = strA.Length;
		fixed (char* firstChar = &strA._firstChar)
		{
			fixed (char* firstChar2 = &strB._firstChar)
			{
				char* ptr = firstChar;
				char* ptr2 = firstChar2;
				while (num != 0)
				{
					int num2 = *ptr;
					int num3 = *ptr2;
					if (num2 == num3 || ((num2 | 0x20) == (num3 | 0x20) && (uint)((num2 | 0x20) - 97) <= 25u))
					{
						ptr++;
						ptr2++;
						num--;
						continue;
					}
					return false;
				}
				return true;
			}
		}
	}

	private unsafe static int CompareOrdinalHelper(string strA, string strB)
	{
		int num = Math.Min(strA.Length, strB.Length);
		fixed (char* firstChar = &strA._firstChar)
		{
			fixed (char* firstChar2 = &strB._firstChar)
			{
				char* ptr = firstChar;
				char* ptr2 = firstChar2;
				if (ptr[1] == ptr2[1])
				{
					num -= 2;
					ptr += 2;
					ptr2 += 2;
					while (true)
					{
						if (num >= 12)
						{
							if (*(long*)ptr == *(long*)ptr2)
							{
								if (*(long*)(ptr + 4) == *(long*)(ptr2 + 4))
								{
									if (*(long*)(ptr + 8) == *(long*)(ptr2 + 8))
									{
										num -= 12;
										ptr += 12;
										ptr2 += 12;
										continue;
									}
									ptr += 4;
									ptr2 += 4;
								}
								ptr += 4;
								ptr2 += 4;
							}
							if (*(int*)ptr == *(int*)ptr2)
							{
								ptr += 2;
								ptr2 += 2;
							}
							break;
						}
						while (true)
						{
							if (num > 0)
							{
								if (*(int*)ptr != *(int*)ptr2)
								{
									break;
								}
								num -= 2;
								ptr += 2;
								ptr2 += 2;
								continue;
							}
							return strA.Length - strB.Length;
						}
						break;
					}
					if (*ptr != *ptr2)
					{
						return *ptr - *ptr2;
					}
				}
				return ptr[1] - ptr2[1];
			}
		}
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, string strB)
	{
		return Compare(strA, strB, StringComparison.CurrentCulture);
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, string strB, bool ignoreCase)
	{
		StringComparison comparisonType = (ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
		return Compare(strA, strB, comparisonType);
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare.</param>
	/// <param name="strB">The second string to compare. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="T:System.StringComparison" /> is not supported.</exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, string strB, StringComparison comparisonType)
	{
		if ((object)strA == strB)
		{
			CheckStringComparison(comparisonType);
			return 0;
		}
		if ((object)strA == null)
		{
			CheckStringComparison(comparisonType);
			return -1;
		}
		if ((object)strB == null)
		{
			CheckStringComparison(comparisonType);
			return 1;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CompareInfo.Invariant.Compare(strA, strB, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CompareInfo.Invariant.Compare(strA, strB, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			if (strA._firstChar != strB._firstChar)
			{
				return strA._firstChar - strB._firstChar;
			}
			return CompareOrdinalHelper(strA, strB);
		case StringComparison.OrdinalIgnoreCase:
			return CompareInfo.CompareOrdinalIgnoreCase(strA, 0, strA.Length, strB, 0, strB.Length);
		default:
			throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType");
		}
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two strings to each other in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between <paramref name="strA" /> and <paramref name="strB" />, as shown in the following tableValueConditionLess than zero<paramref name="strA" /> is less than <paramref name="strB" />.Zero<paramref name="strA" /> equals <paramref name="strB" />.Greater than zero<paramref name="strA" /> is greater than <paramref name="strB" />.</returns>
	/// <param name="strA">The first string to compare.  </param>
	/// <param name="strB">The second string to compare.</param>
	/// <param name="culture">The culture that supplies culture-specific comparison information.</param>
	/// <param name="options">Options to use when performing the comparison (such as ignoring case or symbols).  </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a <see cref="T:System.Globalization.CompareOptions" /> value.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null.</exception>
	public static int Compare(string strA, string strB, CultureInfo culture, CompareOptions options)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.CompareInfo.Compare(strA, strB, options);
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> equals <paramref name="strB" />. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false. </param>
	/// <param name="culture">An object that supplies culture-specific comparison information. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, string strB, bool ignoreCase, CultureInfo culture)
	{
		CompareOptions options = (ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		return Compare(strA, strB, culture, options);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.Value Condition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length)
	{
		return Compare(strA, indexA, strB, indexB, length, ignoreCase: false);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.ValueCondition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase)
	{
		int num = length;
		int num2 = length;
		if ((object)strA != null)
		{
			num = Math.Min(num, strA.Length - indexA);
		}
		if ((object)strB != null)
		{
			num2 = Math.Min(num2, strB.Length - indexB);
		}
		CompareOptions options = (ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, options);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects, ignoring or honoring their case and using culture-specific information to influence the comparison, and returns an integer that indicates their relative position in the sort order.</summary>
	/// <returns>An integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false. </param>
	/// <param name="culture">An object that supplies culture-specific comparison information. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="strA" /> or <paramref name="strB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
	{
		CompareOptions options = (ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		return Compare(strA, indexA, strB, indexB, length, culture, options);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified comparison options and culture-specific information to influence the comparison, and returns an integer that indicates the relationship of the two substrings to each other in the sort order.</summary>
	/// <returns>An integer that indicates the lexical relationship between the two substrings, as shown in the following table.ValueConditionLess than zeroThe substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />.ZeroThe substrings are equal or <paramref name="length" /> is zero.Greater than zeroThe substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />.</returns>
	/// <param name="strA">The first string to use in the comparison.   </param>
	/// <param name="indexA">The starting position of the substring within <paramref name="strA" />.</param>
	/// <param name="strB">The second string to use in the comparison.</param>
	/// <param name="indexB">The starting position of the substring within <paramref name="strB" />.</param>
	/// <param name="length">The maximum number of characters in the substrings to compare.</param>
	/// <param name="culture">An object that supplies culture-specific comparison information.</param>
	/// <param name="options">Options to use when performing the comparison (such as ignoring case or symbols).  </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not a <see cref="T:System.Globalization.CompareOptions" /> value.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.Length.-or-<paramref name="indexB" /> is greater than <paramref name="strB" />.Length.-or-<paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative.-or-Either <paramref name="strA" /> or <paramref name="strB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null.</exception>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, CultureInfo culture, CompareOptions options)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		int num = length;
		int num2 = length;
		if ((object)strA != null)
		{
			num = Math.Min(num, strA.Length - indexA);
		}
		if ((object)strB != null)
		{
			num2 = Math.Min(num2, strB.Length - indexB);
		}
		return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, options);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects using the specified rules, and returns an integer that indicates their relative position in the sort order. </summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.Value Condition Less than zero The substring in the <paramref name="strA" /> parameter is less than the substring in the <paramref name="strB" /> parameter.Zero The substrings are equal, or the <paramref name="length" /> parameter is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The position of the substring within <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison.</param>
	/// <param name="indexB">The position of the substring within <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. -or-Either <paramref name="indexA" /> or <paramref name="indexB" /> is null, and <paramref name="length" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <filterpriority>1</filterpriority>
	public static int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType)
	{
		CheckStringComparison(comparisonType);
		if ((object)strA == null || (object)strB == null)
		{
			if ((object)strA == strB)
			{
				return 0;
			}
			if ((object)strA != null)
			{
				return 1;
			}
			return -1;
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
		}
		if (indexA < 0 || indexB < 0)
		{
			throw new ArgumentOutOfRangeException((indexA < 0) ? "indexA" : "indexB", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (strA.Length - indexA < 0 || strB.Length - indexB < 0)
		{
			throw new ArgumentOutOfRangeException((strA.Length - indexA < 0) ? "indexA" : "indexB", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (length == 0 || ((object)strA == strB && indexA == indexB))
		{
			return 0;
		}
		int num = Math.Min(length, strA.Length - indexA);
		int num2 = Math.Min(length, strB.Length - indexB);
		return comparisonType switch
		{
			StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None), 
			StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase), 
			StringComparison.InvariantCulture => CompareInfo.Invariant.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None), 
			StringComparison.InvariantCultureIgnoreCase => CompareInfo.Invariant.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase), 
			StringComparison.Ordinal => CompareOrdinalHelper(strA, indexA, num, strB, indexB, num2), 
			StringComparison.OrdinalIgnoreCase => CompareInfo.CompareOrdinalIgnoreCase(strA, indexA, num, strB, indexB, num2), 
			_ => throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType"), 
		};
	}

	/// <summary>Compares two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each string.</summary>
	/// <returns>An integer that indicates the lexical relationship between the two comparands.ValueCondition Less than zero <paramref name="strA" /> is less than <paramref name="strB" />. Zero <paramref name="strA" /> and <paramref name="strB" /> are equal. Greater than zero <paramref name="strA" /> is greater than <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to compare. </param>
	/// <param name="strB">The second string to compare. </param>
	/// <filterpriority>2</filterpriority>
	public static int CompareOrdinal(string strA, string strB)
	{
		if ((object)strA == strB)
		{
			return 0;
		}
		if ((object)strA == null)
		{
			return -1;
		}
		if ((object)strB == null)
		{
			return 1;
		}
		if (strA._firstChar != strB._firstChar)
		{
			return strA._firstChar - strB._firstChar;
		}
		return CompareOrdinalHelper(strA, strB);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int CompareOrdinal(ReadOnlySpan<char> strA, ReadOnlySpan<char> strB)
	{
		return SpanHelpers.SequenceCompareTo(ref MemoryMarshal.GetReference(strA), strA.Length, ref MemoryMarshal.GetReference(strB), strB.Length);
	}

	/// <summary>Compares substrings of two specified <see cref="T:System.String" /> objects by evaluating the numeric values of the corresponding <see cref="T:System.Char" /> objects in each substring. </summary>
	/// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.ValueCondition Less than zero The substring in <paramref name="strA" /> is less than the substring in <paramref name="strB" />. Zero The substrings are equal, or <paramref name="length" /> is zero. Greater than zero The substring in <paramref name="strA" /> is greater than the substring in <paramref name="strB" />. </returns>
	/// <param name="strA">The first string to use in the comparison. </param>
	/// <param name="indexA">The starting index of the substring in <paramref name="strA" />. </param>
	/// <param name="strB">The second string to use in the comparison. </param>
	/// <param name="indexB">The starting index of the substring in <paramref name="strB" />. </param>
	/// <param name="length">The maximum number of characters in the substrings to compare. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="strA" /> is not null and <paramref name="indexA" /> is greater than <paramref name="strA" />.<see cref="P:System.String.Length" />.-or- <paramref name="strB" /> is not null and<paramref name="indexB" /> is greater than <paramref name="strB" />.<see cref="P:System.String.Length" />.-or- <paramref name="indexA" />, <paramref name="indexB" />, or <paramref name="length" /> is negative. </exception>
	/// <filterpriority>2</filterpriority>
	public static int CompareOrdinal(string strA, int indexA, string strB, int indexB, int length)
	{
		if ((object)strA == null || (object)strB == null)
		{
			if ((object)strA == strB)
			{
				return 0;
			}
			if ((object)strA != null)
			{
				return 1;
			}
			return -1;
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Count cannot be less than zero.");
		}
		if (indexA < 0 || indexB < 0)
		{
			throw new ArgumentOutOfRangeException((indexA < 0) ? "indexA" : "indexB", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		int num = Math.Min(length, strA.Length - indexA);
		int num2 = Math.Min(length, strB.Length - indexB);
		if (num < 0 || num2 < 0)
		{
			throw new ArgumentOutOfRangeException((num < 0) ? "indexA" : "indexB", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (length == 0 || ((object)strA == strB && indexA == indexB))
		{
			return 0;
		}
		return CompareOrdinalHelper(strA, indexA, num, strB, indexB, num2);
	}

	/// <summary>Compares this instance with a specified <see cref="T:System.Object" /> and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
	/// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="value" /> parameter.Value Condition Less than zero This instance precedes <paramref name="value" />. Zero This instance has the same position in the sort order as <paramref name="value" />. Greater than zero This instance follows <paramref name="value" />.-or- <paramref name="value" /> is null. </returns>
	/// <param name="value">An object that evaluates to a <see cref="T:System.String" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> is not a <see cref="T:System.String" />. </exception>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(object value)
	{
		if (value == null)
		{
			return 1;
		}
		if (!(value is string strB))
		{
			throw new ArgumentException("Object must be of type String.");
		}
		return CompareTo(strB);
	}

	/// <summary>Compares this instance with a specified <see cref="T:System.String" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.String" />.</summary>
	/// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="value" /> parameter.Value Condition Less than zero This instance precedes <paramref name="strB" />. Zero This instance has the same position in the sort order as <paramref name="strB" />. Greater than zero This instance follows <paramref name="strB" />.-or- <paramref name="strB" /> is null. </returns>
	/// <param name="strB">The string to compare with this instance. </param>
	/// <filterpriority>2</filterpriority>
	public int CompareTo(string strB)
	{
		return Compare(this, strB, StringComparison.CurrentCulture);
	}

	/// <summary>Determines whether the end of this string instance matches the specified string.</summary>
	/// <returns>true if <paramref name="value" /> matches the end of this instance; otherwise, false.</returns>
	/// <param name="value">The string to compare to the substring at the end of this instance. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool EndsWith(string value)
	{
		return EndsWith(value, StringComparison.CurrentCulture);
	}

	/// <summary>Determines whether the end of this string instance matches the specified string when compared using the specified comparison option.</summary>
	/// <returns>true if the <paramref name="value" /> parameter matches the end of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare to the substring at the end of this instance. </param>
	/// <param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
	public bool EndsWith(string value, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((object)this == value)
		{
			CheckStringComparison(comparisonType);
			return true;
		}
		if (value.Length == 0)
		{
			CheckStringComparison(comparisonType);
			return true;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CompareInfo.Invariant.IsSuffix(this, value, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CompareInfo.Invariant.IsSuffix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			if (Length >= value.Length)
			{
				return CompareOrdinalHelper(this, Length - value.Length, value.Length, value, 0, value.Length) == 0;
			}
			return false;
		case StringComparison.OrdinalIgnoreCase:
			if (Length >= value.Length)
			{
				return CompareInfo.CompareOrdinalIgnoreCase(this, Length - value.Length, value.Length, value, 0, value.Length) == 0;
			}
			return false;
		default:
			throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType");
		}
	}

	/// <summary>Determines whether the end of this string instance matches the specified string when compared using the specified culture.</summary>
	/// <returns>true if the <paramref name="value" /> parameter matches the end of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare to the substring at the end of this instance. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <param name="culture">Cultural information that determines how this instance and <paramref name="value" /> are compared. If <paramref name="culture" /> is null, the current culture is used.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((object)this == value)
		{
			return true;
		}
		return (culture ?? CultureInfo.CurrentCulture).CompareInfo.IsSuffix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
	}

	public bool EndsWith(char value)
	{
		int length = Length;
		if (length != 0)
		{
			return this[length - 1] == value;
		}
		return false;
	}

	/// <summary>Determines whether this instance and a specified object, which must also be a <see cref="T:System.String" /> object, have the same value.</summary>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:System.String" /> and its value is the same as this instance; otherwise, false.</returns>
	/// <param name="obj">The string to compare to this instance. </param>
	/// <filterpriority>2</filterpriority>
	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is string text))
		{
			return false;
		}
		if (Length != text.Length)
		{
			return false;
		}
		return EqualsHelper(this, text);
	}

	/// <summary>Determines whether this instance and another specified <see cref="T:System.String" /> object have the same value.</summary>
	/// <returns>true if the value of the <paramref name="value" /> parameter is the same as this instance; otherwise, false.</returns>
	/// <param name="value">The string to compare to this instance. </param>
	/// <filterpriority>2</filterpriority>
	public bool Equals(string value)
	{
		if ((object)this == value)
		{
			return true;
		}
		if ((object)value == null)
		{
			return false;
		}
		if (Length != value.Length)
		{
			return false;
		}
		return EqualsHelper(this, value);
	}

	/// <summary>Determines whether this string and a specified <see cref="T:System.String" /> object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	/// <returns>true if the value of the <paramref name="value" /> parameter is the same as this string; otherwise, false.</returns>
	/// <param name="value">The string to compare to this instance.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies how the strings will be compared. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <filterpriority>2</filterpriority>
	public bool Equals(string value, StringComparison comparisonType)
	{
		if ((object)this == value)
		{
			CheckStringComparison(comparisonType);
			return true;
		}
		if ((object)value == null)
		{
			CheckStringComparison(comparisonType);
			return false;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0;
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0;
		case StringComparison.InvariantCulture:
			return CompareInfo.Invariant.Compare(this, value, CompareOptions.None) == 0;
		case StringComparison.InvariantCultureIgnoreCase:
			return CompareInfo.Invariant.Compare(this, value, CompareOptions.IgnoreCase) == 0;
		case StringComparison.Ordinal:
			if (Length != value.Length)
			{
				return false;
			}
			return EqualsHelper(this, value);
		case StringComparison.OrdinalIgnoreCase:
			if (Length != value.Length)
			{
				return false;
			}
			return CompareInfo.CompareOrdinalIgnoreCase(this, 0, Length, value, 0, value.Length) == 0;
		default:
			throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType");
		}
	}

	/// <summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value.</summary>
	/// <returns>true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, false. If both <paramref name="a" /> and <paramref name="b" /> are null, the method returns true.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <filterpriority>1</filterpriority>
	public static bool Equals(string a, string b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null || a.Length != b.Length)
		{
			return false;
		}
		return EqualsHelper(a, b);
	}

	/// <summary>Determines whether two specified <see cref="T:System.String" /> objects have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.</summary>
	/// <returns>true if the value of the <paramref name="a" /> parameter is equal to the value of the <paramref name="b" /> parameter; otherwise, false.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the comparison. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value. </exception>
	/// <filterpriority>1</filterpriority>
	public static bool Equals(string a, string b, StringComparison comparisonType)
	{
		if ((object)a == b)
		{
			CheckStringComparison(comparisonType);
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			CheckStringComparison(comparisonType);
			return false;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0;
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0;
		case StringComparison.InvariantCulture:
			return CompareInfo.Invariant.Compare(a, b, CompareOptions.None) == 0;
		case StringComparison.InvariantCultureIgnoreCase:
			return CompareInfo.Invariant.Compare(a, b, CompareOptions.IgnoreCase) == 0;
		case StringComparison.Ordinal:
			if (a.Length != b.Length)
			{
				return false;
			}
			return EqualsHelper(a, b);
		case StringComparison.OrdinalIgnoreCase:
			if (a.Length != b.Length)
			{
				return false;
			}
			return CompareInfo.CompareOrdinalIgnoreCase(a, 0, a.Length, b, 0, b.Length) == 0;
		default:
			throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType");
		}
	}

	/// <summary>Determines whether two specified strings have the same value.</summary>
	/// <returns>true if the value of <paramref name="a" /> is the same as the value of <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator ==(string a, string b)
	{
		return Equals(a, b);
	}

	/// <summary>Determines whether two specified strings have different values.</summary>
	/// <returns>true if the value of <paramref name="a" /> is different from the value of <paramref name="b" />; otherwise, false.</returns>
	/// <param name="a">The first string to compare, or null. </param>
	/// <param name="b">The second string to compare, or null. </param>
	/// <filterpriority>3</filterpriority>
	public static bool operator !=(string a, string b)
	{
		return !Equals(a, b);
	}

	/// <summary>Returns the hash code for this string.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>2</filterpriority>
	public override int GetHashCode()
	{
		return GetLegacyNonRandomizedHashCode();
	}

	public int GetHashCode(StringComparison comparisonType)
	{
		return StringComparer.FromComparison(comparisonType).GetHashCode(this);
	}

	internal unsafe int GetLegacyNonRandomizedHashCode()
	{
		fixed (char* firstChar = &_firstChar)
		{
			int num = 5381;
			int num2 = num;
			char* ptr = firstChar;
			int num3;
			while ((num3 = *ptr) != 0)
			{
				num = ((num << 5) + num) ^ num3;
				num3 = ptr[1];
				if (num3 == 0)
				{
					break;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
				ptr += 2;
			}
			return num + num2 * 1566083941;
		}
	}

	/// <summary>Determines whether the beginning of this string instance matches the specified string.</summary>
	/// <returns>true if <paramref name="value" /> matches the beginning of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool StartsWith(string value)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		return StartsWith(value, StringComparison.CurrentCulture);
	}

	/// <summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified comparison option.</summary>
	/// <returns>true if this instance begins with <paramref name="value" />; otherwise, false.</returns>
	/// <param name="value">The string to compare. </param>
	/// <param name="comparisonType">One of the enumeration values that determines how this string and <paramref name="value" /> are compared. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a <see cref="T:System.StringComparison" /> value.</exception>
	public bool StartsWith(string value, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((object)this == value)
		{
			CheckStringComparison(comparisonType);
			return true;
		}
		if (value.Length == 0)
		{
			CheckStringComparison(comparisonType);
			return true;
		}
		switch (comparisonType)
		{
		case StringComparison.CurrentCulture:
			return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);
		case StringComparison.CurrentCultureIgnoreCase:
			return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.InvariantCulture:
			return CompareInfo.Invariant.IsPrefix(this, value, CompareOptions.None);
		case StringComparison.InvariantCultureIgnoreCase:
			return CompareInfo.Invariant.IsPrefix(this, value, CompareOptions.IgnoreCase);
		case StringComparison.Ordinal:
			if (Length < value.Length || _firstChar != value._firstChar)
			{
				return false;
			}
			if (value.Length != 1)
			{
				return SpanHelpers.SequenceEqual(ref Unsafe.As<char, byte>(ref GetRawStringData()), ref Unsafe.As<char, byte>(ref value.GetRawStringData()), (ulong)value.Length * 2uL);
			}
			return true;
		case StringComparison.OrdinalIgnoreCase:
			if (Length < value.Length)
			{
				return false;
			}
			return CompareInfo.CompareOrdinalIgnoreCase(this, 0, value.Length, value, 0, value.Length) == 0;
		default:
			throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType");
		}
	}

	/// <summary>Determines whether the beginning of this string instance matches the specified string when compared using the specified culture.</summary>
	/// <returns>true if the <paramref name="value" /> parameter matches the beginning of this string; otherwise, false.</returns>
	/// <param name="value">The string to compare. </param>
	/// <param name="ignoreCase">true to ignore case during the comparison; otherwise, false.</param>
	/// <param name="culture">Cultural information that determines how this string and <paramref name="value" /> are compared. If <paramref name="culture" /> is null, the current culture is used.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool StartsWith(string value, bool ignoreCase, CultureInfo culture)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((object)this == value)
		{
			return true;
		}
		return (culture ?? CultureInfo.CurrentCulture).CompareInfo.IsPrefix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
	}

	public bool StartsWith(char value)
	{
		if (Length != 0)
		{
			return _firstChar == value;
		}
		return false;
	}

	internal static void CheckStringComparison(StringComparison comparisonType)
	{
		if ((uint)(comparisonType - 0) > 5u)
		{
			ThrowHelper.ThrowArgumentException(ExceptionResource.NotSupported_StringComparison, ExceptionArgument.comparisonType);
		}
	}

	private unsafe static void FillStringChecked(string dest, int destPos, string src)
	{
		if (src.Length > dest.Length - destPos)
		{
			throw new IndexOutOfRangeException();
		}
		fixed (char* firstChar = &dest._firstChar)
		{
			fixed (char* firstChar2 = &src._firstChar)
			{
				wstrcpy(firstChar + destPos, firstChar2, src.Length);
			}
		}
	}

	/// <summary>Creates the string  representation of a specified object.</summary>
	/// <returns>The string representation of the value of <paramref name="arg0" />, or <see cref="F:System.String.Empty" /> if <paramref name="arg0" /> is null.</returns>
	/// <param name="arg0">The object to represent, or null. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(object arg0)
	{
		if (arg0 == null)
		{
			return Empty;
		}
		return arg0.ToString();
	}

	/// <summary>Concatenates the string representations of two specified objects.</summary>
	/// <returns>The concatenated string representations of the values of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
	/// <param name="arg0">The first object to concatenate. </param>
	/// <param name="arg1">The second object to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(object arg0, object arg1)
	{
		if (arg0 == null)
		{
			arg0 = Empty;
		}
		if (arg1 == null)
		{
			arg1 = Empty;
		}
		return arg0.ToString() + arg1.ToString();
	}

	/// <summary>Concatenates the string representations of three specified objects.</summary>
	/// <returns>The concatenated string representations of the values of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
	/// <param name="arg0">The first object to concatenate. </param>
	/// <param name="arg1">The second object to concatenate. </param>
	/// <param name="arg2">The third object to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(object arg0, object arg1, object arg2)
	{
		if (arg0 == null)
		{
			arg0 = Empty;
		}
		if (arg1 == null)
		{
			arg1 = Empty;
		}
		if (arg2 == null)
		{
			arg2 = Empty;
		}
		return arg0.ToString() + arg1.ToString() + arg2.ToString();
	}

	/// <summary>Concatenates the string representations of the elements in a specified <see cref="T:System.Object" /> array.</summary>
	/// <returns>The concatenated string representations of the values of the elements in <paramref name="args" />.</returns>
	/// <param name="args">An object array that contains the elements to concatenate. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	public static string Concat(params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException("args");
		}
		if (args.Length <= 1)
		{
			object obj;
			if (args.Length != 0)
			{
				obj = args[0]?.ToString();
				if (obj == null)
				{
					return Empty;
				}
			}
			else
			{
				obj = Empty;
			}
			return (string)obj;
		}
		string[] array = new string[args.Length];
		int num = 0;
		for (int i = 0; i < args.Length; i++)
		{
			num += (array[i] = args[i]?.ToString() ?? Empty).Length;
			if (num < 0)
			{
				throw new OutOfMemoryException();
			}
		}
		if (num == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(num);
		int num2 = 0;
		foreach (string text2 in array)
		{
			FillStringChecked(text, num2, text2);
			num2 += text2.Length;
		}
		return text;
	}

	/// <summary>Concatenates the members of an <see cref="T:System.Collections.Generic.IEnumerable`1" /> implementation.</summary>
	/// <returns>The concatenated members in <paramref name="values" />.</returns>
	/// <param name="values">A collection object that implements the <see cref="T:System.Collections.Generic.IEnumerable`1" /> interface.</param>
	/// <typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	public static string Concat<T>(IEnumerable<T> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (typeof(T) == typeof(char))
		{
			using (IEnumerator<char> enumerator = Unsafe.As<IEnumerable<char>>(values).GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					return Empty;
				}
				char current = enumerator.Current;
				if (!enumerator.MoveNext())
				{
					return CreateFromChar(current);
				}
				StringBuilder stringBuilder = StringBuilderCache.Acquire();
				stringBuilder.Append(current);
				do
				{
					current = enumerator.Current;
					stringBuilder.Append(current);
				}
				while (enumerator.MoveNext());
				return StringBuilderCache.GetStringAndRelease(stringBuilder);
			}
		}
		using IEnumerator<T> enumerator2 = values.GetEnumerator();
		if (!enumerator2.MoveNext())
		{
			return Empty;
		}
		string text = enumerator2.Current?.ToString();
		if (!enumerator2.MoveNext())
		{
			return text ?? Empty;
		}
		StringBuilder stringBuilder2 = StringBuilderCache.Acquire();
		stringBuilder2.Append(text);
		do
		{
			T current2 = enumerator2.Current;
			if (current2 != null)
			{
				stringBuilder2.Append(current2.ToString());
			}
		}
		while (enumerator2.MoveNext());
		return StringBuilderCache.GetStringAndRelease(stringBuilder2);
	}

	/// <summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />.</summary>
	/// <returns>The concatenated strings in <paramref name="values" />.</returns>
	/// <param name="values">A collection object that implements <see cref="T:System.Collections.Generic.IEnumerable`1" /> and whose generic type argument is <see cref="T:System.String" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	public static string Concat(IEnumerable<string> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		using IEnumerator<string> enumerator = values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return Empty;
		}
		string current = enumerator.Current;
		if (!enumerator.MoveNext())
		{
			return current ?? Empty;
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		stringBuilder.Append(current);
		do
		{
			stringBuilder.Append(enumerator.Current);
		}
		while (enumerator.MoveNext());
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Concatenates two specified instances of <see cref="T:System.String" />.</summary>
	/// <returns>The concatenation of <paramref name="str0" /> and <paramref name="str1" />.</returns>
	/// <param name="str0">The first string to concatenate. </param>
	/// <param name="str1">The second string to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(string str0, string str1)
	{
		if (IsNullOrEmpty(str0))
		{
			if (IsNullOrEmpty(str1))
			{
				return Empty;
			}
			return str1;
		}
		if (IsNullOrEmpty(str1))
		{
			return str0;
		}
		int length = str0.Length;
		string text = FastAllocateString(length + str1.Length);
		FillStringChecked(text, 0, str0);
		FillStringChecked(text, length, str1);
		return text;
	}

	/// <summary>Concatenates three specified instances of <see cref="T:System.String" />.</summary>
	/// <returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, and <paramref name="str2" />.</returns>
	/// <param name="str0">The first string to concatenate. </param>
	/// <param name="str1">The second string to concatenate. </param>
	/// <param name="str2">The third string to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(string str0, string str1, string str2)
	{
		if (IsNullOrEmpty(str0))
		{
			return str1 + str2;
		}
		if (IsNullOrEmpty(str1))
		{
			return str0 + str2;
		}
		if (IsNullOrEmpty(str2))
		{
			return str0 + str1;
		}
		string text = FastAllocateString(str0.Length + str1.Length + str2.Length);
		FillStringChecked(text, 0, str0);
		FillStringChecked(text, str0.Length, str1);
		FillStringChecked(text, str0.Length + str1.Length, str2);
		return text;
	}

	/// <summary>Concatenates four specified instances of <see cref="T:System.String" />.</summary>
	/// <returns>The concatenation of <paramref name="str0" />, <paramref name="str1" />, <paramref name="str2" />, and <paramref name="str3" />.</returns>
	/// <param name="str0">The first string to concatenate. </param>
	/// <param name="str1">The second string to concatenate. </param>
	/// <param name="str2">The third string to concatenate. </param>
	/// <param name="str3">The fourth string to concatenate. </param>
	/// <filterpriority>1</filterpriority>
	public static string Concat(string str0, string str1, string str2, string str3)
	{
		if (IsNullOrEmpty(str0))
		{
			return str1 + str2 + str3;
		}
		if (IsNullOrEmpty(str1))
		{
			return str0 + str2 + str3;
		}
		if (IsNullOrEmpty(str2))
		{
			return str0 + str1 + str3;
		}
		if (IsNullOrEmpty(str3))
		{
			return str0 + str1 + str2;
		}
		string text = FastAllocateString(str0.Length + str1.Length + str2.Length + str3.Length);
		FillStringChecked(text, 0, str0);
		FillStringChecked(text, str0.Length, str1);
		FillStringChecked(text, str0.Length + str1.Length, str2);
		FillStringChecked(text, str0.Length + str1.Length + str2.Length, str3);
		return text;
	}

	/// <summary>Concatenates the elements of a specified <see cref="T:System.String" /> array.</summary>
	/// <returns>The concatenated elements of <paramref name="values" />.</returns>
	/// <param name="values">An array of string instances. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	public static string Concat(params string[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length <= 1)
		{
			object obj;
			if (values.Length != 0)
			{
				obj = values[0];
				if (obj == null)
				{
					return Empty;
				}
			}
			else
			{
				obj = Empty;
			}
			return (string)obj;
		}
		long num = 0L;
		foreach (string text in values)
		{
			if ((object)text != null)
			{
				num += text.Length;
			}
		}
		if (num > int.MaxValue)
		{
			throw new OutOfMemoryException();
		}
		int num2 = (int)num;
		if (num2 == 0)
		{
			return Empty;
		}
		string text2 = FastAllocateString(num2);
		int num3 = 0;
		foreach (string text3 in values)
		{
			if (!IsNullOrEmpty(text3))
			{
				int length = text3.Length;
				if (length > num2 - num3)
				{
					num3 = -1;
					break;
				}
				FillStringChecked(text2, num3, text3);
				num3 += length;
			}
		}
		if (num3 != num2)
		{
			return Concat((string[])values.Clone());
		}
		return text2;
	}

	/// <summary>Replaces one or more format items in a specified string with the string representation of a specified object.</summary>
	/// <returns>A copy of <paramref name="format" /> in which any format items are replaced by the string representation of <paramref name="arg0" />.</returns>
	/// <param name="format">A composite format string. </param>
	/// <param name="arg0">The object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">The format item in <paramref name="format" /> is invalid.-or- The index of a format item is not zero. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, object arg0)
	{
		return FormatHelper(null, format, new ParamsArray(arg0));
	}

	/// <summary>Replaces the format items in a specified string with the string representation of two specified objects.</summary>
	/// <returns>A copy of <paramref name="format" /> in which format items are replaced by the string representations of <paramref name="arg0" /> and <paramref name="arg1" />.</returns>
	/// <param name="format">A composite format string. </param>
	/// <param name="arg0">The first object to format. </param>
	/// <param name="arg1">The second object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is not zero or one. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, object arg0, object arg1)
	{
		return FormatHelper(null, format, new ParamsArray(arg0, arg1));
	}

	/// <summary>Replaces the format items in a specified string with the string representation of three specified objects.</summary>
	/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representations of <paramref name="arg0" />, <paramref name="arg1" />, and <paramref name="arg2" />.</returns>
	/// <param name="format">A composite format string.</param>
	/// <param name="arg0">The first object to format. </param>
	/// <param name="arg1">The second object to format. </param>
	/// <param name="arg2">The third object to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is less than zero, or greater than two. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, object arg0, object arg1, object arg2)
	{
		return FormatHelper(null, format, new ParamsArray(arg0, arg1, arg2));
	}

	/// <summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array.</summary>
	/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
	/// <param name="format">A composite format string.</param>
	/// <param name="args">An object array that contains zero or more objects to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(string format, params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException(((object)format == null) ? "format" : "args");
		}
		return FormatHelper(null, format, new ParamsArray(args));
	}

	public static string Format(IFormatProvider provider, string format, object arg0)
	{
		return FormatHelper(provider, format, new ParamsArray(arg0));
	}

	public static string Format(IFormatProvider provider, string format, object arg0, object arg1)
	{
		return FormatHelper(provider, format, new ParamsArray(arg0, arg1));
	}

	public static string Format(IFormatProvider provider, string format, object arg0, object arg1, object arg2)
	{
		return FormatHelper(provider, format, new ParamsArray(arg0, arg1, arg2));
	}

	/// <summary>Replaces the format items in a specified string with the string representations of corresponding objects in a specified array. A parameter supplies culture-specific formatting information.</summary>
	/// <returns>A copy of <paramref name="format" /> in which the format items have been replaced by the string representation of the corresponding objects in <paramref name="args" />.</returns>
	/// <param name="provider">An object that supplies culture-specific formatting information. </param>
	/// <param name="format">A composite format string. </param>
	/// <param name="args">An object array that contains zero or more objects to format. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> or <paramref name="args" /> is null. </exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="format" /> is invalid.-or- The index of a format item is less than zero, or greater than or equal to the length of the <paramref name="args" /> array. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Format(IFormatProvider provider, string format, params object[] args)
	{
		if (args == null)
		{
			throw new ArgumentNullException(((object)format == null) ? "format" : "args");
		}
		return FormatHelper(provider, format, new ParamsArray(args));
	}

	private static string FormatHelper(IFormatProvider provider, string format, ParamsArray args)
	{
		if ((object)format == null)
		{
			throw new ArgumentNullException("format");
		}
		return StringBuilderCache.GetStringAndRelease(StringBuilderCache.Acquire(format.Length + args.Length * 8).AppendFormatHelper(provider, format, args));
	}

	/// <summary>Returns a new string in which a specified string is inserted at a specified index position in this instance. </summary>
	/// <returns>A new string that is equivalent to this instance, but with <paramref name="value" /> inserted at position <paramref name="startIndex" />.</returns>
	/// <param name="startIndex">The zero-based index position of the insertion. </param>
	/// <param name="value">The string to insert. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is negative or greater than the length of this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public unsafe string Insert(int startIndex, string value)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		int length = Length;
		int length2 = value.Length;
		if (length == 0)
		{
			return value;
		}
		if (length2 == 0)
		{
			return this;
		}
		string text = FastAllocateString(length + length2);
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* firstChar2 = &value._firstChar)
			{
				fixed (char* firstChar3 = &text._firstChar)
				{
					wstrcpy(firstChar3, firstChar, startIndex);
					wstrcpy(firstChar3 + startIndex, firstChar2, length2);
					wstrcpy(firstChar3 + startIndex + length2, firstChar + startIndex, length - startIndex);
				}
			}
		}
		return text;
	}

	public static string Join(char separator, params string[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return Join(separator, value, 0, value.Length);
	}

	public unsafe static string Join(char separator, params object[] values)
	{
		return JoinCore(&separator, 1, values);
	}

	public unsafe static string Join<T>(char separator, IEnumerable<T> values)
	{
		return JoinCore(&separator, 1, values);
	}

	public unsafe static string Join(char separator, string[] value, int startIndex, int count)
	{
		return JoinCore(&separator, 1, value, startIndex, count);
	}

	/// <summary>Concatenates all the elements of a string array, using the specified separator between each element. </summary>
	/// <returns>A string that consists of the elements in <paramref name="value" /> delimited by the <paramref name="separator" /> string. If <paramref name="value" /> is an empty array, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
	/// <param name="value">An array that contains the elements to concatenate. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public static string Join(string separator, params string[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return Join(separator, value, 0, value.Length);
	}

	/// <summary>Concatenates the elements of an object array, using the specified separator between each element.</summary>
	/// <returns>A string that consists of the elements of <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> is an empty array, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
	/// <param name="values">An array that contains the elements to concatenate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	public unsafe static string Join(string separator, params object[] values)
	{
		separator = separator ?? Empty;
		fixed (char* firstChar = &separator._firstChar)
		{
			return JoinCore(firstChar, separator.Length, values);
		}
	}

	/// <summary>Concatenates the members of a collection, using the specified separator between each member.</summary>
	/// <returns>A string that consists of the members of <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> has no members, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
	/// <param name="values">A collection that contains the objects to concatenate.</param>
	/// <typeparam name="T">The type of the members of <paramref name="values" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	public unsafe static string Join<T>(string separator, IEnumerable<T> values)
	{
		separator = separator ?? Empty;
		fixed (char* firstChar = &separator._firstChar)
		{
			return JoinCore(firstChar, separator.Length, values);
		}
	}

	/// <summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
	/// <returns>A string that consists of the members of <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> has no members, the method returns <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="values" /> has more than one element.</param>
	/// <param name="values">A collection that contains the strings to concatenate.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="values" /> is null. </exception>
	public static string Join(string separator, IEnumerable<string> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		using IEnumerator<string> enumerator = values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return Empty;
		}
		string current = enumerator.Current;
		if (!enumerator.MoveNext())
		{
			return current ?? Empty;
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		stringBuilder.Append(current);
		do
		{
			stringBuilder.Append(separator);
			stringBuilder.Append(enumerator.Current);
		}
		while (enumerator.MoveNext());
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Concatenates the specified elements of a string array, using the specified separator between each element. </summary>
	/// <returns>A string that consists of the strings in <paramref name="value" /> delimited by the <paramref name="separator" /> string. -or-<see cref="F:System.String.Empty" /> if <paramref name="count" /> is zero, <paramref name="value" /> has no elements, or <paramref name="separator" /> and all the elements of <paramref name="value" /> are <see cref="F:System.String.Empty" />.</returns>
	/// <param name="separator">The string to use as a separator. <paramref name="separator" /> is included in the returned string only if <paramref name="value" /> has more than one element.</param>
	/// <param name="value">An array that contains the elements to concatenate. </param>
	/// <param name="startIndex">The first element in <paramref name="value" /> to use. </param>
	/// <param name="count">The number of elements of <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="count" /> is less than 0.-or- <paramref name="startIndex" /> plus <paramref name="count" /> is greater than the number of elements in <paramref name="value" />. </exception>
	/// <exception cref="T:System.OutOfMemoryException">Out of memory.</exception>
	/// <filterpriority>1</filterpriority>
	public unsafe static string Join(string separator, string[] value, int startIndex, int count)
	{
		separator = separator ?? Empty;
		fixed (char* firstChar = &separator._firstChar)
		{
			return JoinCore(firstChar, separator.Length, value, startIndex, count);
		}
	}

	private unsafe static string JoinCore(char* separator, int separatorLength, object[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length == 0)
		{
			return Empty;
		}
		string text = values[0]?.ToString();
		if (values.Length == 1)
		{
			return text ?? Empty;
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		stringBuilder.Append(text);
		for (int i = 1; i < values.Length; i++)
		{
			stringBuilder.Append(separator, separatorLength);
			object obj = values[i];
			if (obj != null)
			{
				stringBuilder.Append(obj.ToString());
			}
		}
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	private unsafe static string JoinCore<T>(char* separator, int separatorLength, IEnumerable<T> values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		using IEnumerator<T> enumerator = values.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return Empty;
		}
		string text = enumerator.Current?.ToString();
		if (!enumerator.MoveNext())
		{
			return text ?? Empty;
		}
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		stringBuilder.Append(text);
		do
		{
			T current = enumerator.Current;
			stringBuilder.Append(separator, separatorLength);
			if (current != null)
			{
				stringBuilder.Append(current.ToString());
			}
		}
		while (enumerator.MoveNext());
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	private unsafe static string JoinCore(char* separator, int separatorLength, string[] value, int startIndex, int count)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero.");
		}
		if (startIndex > value.Length - count)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index and count must refer to a location within the buffer.");
		}
		if (count <= 1)
		{
			object obj;
			if (count != 0)
			{
				obj = value[startIndex];
				if (obj == null)
				{
					return Empty;
				}
			}
			else
			{
				obj = Empty;
			}
			return (string)obj;
		}
		long num = (long)(count - 1) * (long)separatorLength;
		if (num > int.MaxValue)
		{
			throw new OutOfMemoryException();
		}
		int num2 = (int)num;
		int i = startIndex;
		for (int num3 = startIndex + count; i < num3; i++)
		{
			string text = value[i];
			if ((object)text != null)
			{
				num2 += text.Length;
				if (num2 < 0)
				{
					throw new OutOfMemoryException();
				}
			}
		}
		string text2 = FastAllocateString(num2);
		int num4 = 0;
		int j = startIndex;
		for (int num5 = startIndex + count; j < num5; j++)
		{
			string text3 = value[j];
			if ((object)text3 != null)
			{
				int length = text3.Length;
				if (length > num2 - num4)
				{
					num4 = -1;
					break;
				}
				FillStringChecked(text2, num4, text3);
				num4 += length;
			}
			if (j >= num5 - 1)
			{
				continue;
			}
			fixed (char* firstChar = &text2._firstChar)
			{
				if (separatorLength == 1)
				{
					firstChar[num4] = *separator;
				}
				else
				{
					wstrcpy(firstChar + num4, separator, separatorLength);
				}
			}
			num4 += separatorLength;
		}
		if (num4 != num2)
		{
			return JoinCore(separator, separatorLength, (string[])value.Clone(), startIndex, count);
		}
		return text2;
	}

	/// <summary>Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public string PadLeft(int totalWidth)
	{
		return PadLeft(totalWidth, ' ');
	}

	/// <summary>Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified Unicode character, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but right-aligned and padded on the left with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <param name="paddingChar">A Unicode padding character. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public unsafe string PadLeft(int totalWidth, char paddingChar)
	{
		if (totalWidth < 0)
		{
			throw new ArgumentOutOfRangeException("totalWidth", "Non-negative number required.");
		}
		int length = Length;
		int num = totalWidth - length;
		if (num <= 0)
		{
			return this;
		}
		string text = FastAllocateString(totalWidth);
		fixed (char* firstChar = &text._firstChar)
		{
			for (int i = 0; i < num; i++)
			{
				firstChar[i] = paddingChar;
			}
			fixed (char* firstChar2 = &_firstChar)
			{
				wstrcpy(firstChar + num, firstChar2, length);
			}
		}
		return text;
	}

	/// <summary>Returns a new string that left-aligns the characters in this string by padding them with spaces on the right, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many spaces as needed to create a length of <paramref name="totalWidth" />. However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public string PadRight(int totalWidth)
	{
		return PadRight(totalWidth, ' ');
	}

	/// <summary>Returns a new string that left-aligns the characters in this string by padding them on the right with a specified Unicode character, for a specified total length.</summary>
	/// <returns>A new string that is equivalent to this instance, but left-aligned and padded on the right with as many <paramref name="paddingChar" /> characters as needed to create a length of <paramref name="totalWidth" />.  However, if <paramref name="totalWidth" /> is less than the length of this instance, the method returns a reference to the existing instance. If <paramref name="totalWidth" /> is equal to the length of this instance, the method returns a new string that is identical to this instance.</returns>
	/// <param name="totalWidth">The number of characters in the resulting string, equal to the number of original characters plus any additional padding characters. </param>
	/// <param name="paddingChar">A Unicode padding character. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="totalWidth" /> is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public unsafe string PadRight(int totalWidth, char paddingChar)
	{
		if (totalWidth < 0)
		{
			throw new ArgumentOutOfRangeException("totalWidth", "Non-negative number required.");
		}
		int length = Length;
		int num = totalWidth - length;
		if (num <= 0)
		{
			return this;
		}
		string text = FastAllocateString(totalWidth);
		fixed (char* firstChar = &text._firstChar)
		{
			fixed (char* firstChar2 = &_firstChar)
			{
				wstrcpy(firstChar, firstChar2, length);
			}
			for (int i = 0; i < num; i++)
			{
				firstChar[length + i] = paddingChar;
			}
		}
		return text;
	}

	/// <summary>Returns a new string in which a specified number of characters in the current this instance beginning at a specified position have been deleted.</summary>
	/// <returns>A new string that is equivalent to this instance except for the removed characters.</returns>
	/// <param name="startIndex">The zero-based position to begin deleting characters. </param>
	/// <param name="count">The number of characters to delete. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Either <paramref name="startIndex" /> or <paramref name="count" /> is less than zero.-or- <paramref name="startIndex" /> plus <paramref name="count" /> specify a position outside this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public unsafe string Remove(int startIndex, int count)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero.");
		}
		int length = Length;
		if (count > length - startIndex)
		{
			throw new ArgumentOutOfRangeException("count", "Index and count must refer to a location within the string.");
		}
		if (count == 0)
		{
			return this;
		}
		int num = length - count;
		if (num == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(num);
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* firstChar2 = &text._firstChar)
			{
				wstrcpy(firstChar2, firstChar, startIndex);
				wstrcpy(firstChar2 + startIndex, firstChar + startIndex + count, num - startIndex);
			}
		}
		return text;
	}

	/// <summary>Returns a new string in which all the characters in the current instance, beginning at a specified position and continuing through the last position, have been deleted.</summary>
	/// <returns>A new string that is equivalent to this string except for the removed characters.</returns>
	/// <param name="startIndex">The zero-based position to begin deleting characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero.-or- <paramref name="startIndex" /> specifies a position that is not within this string. </exception>
	/// <filterpriority>1</filterpriority>
	public string Remove(int startIndex)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		if (startIndex >= Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "startIndex must be less than length of string.");
		}
		return Substring(0, startIndex);
	}

	public string Replace(string oldValue, string newValue, bool ignoreCase, CultureInfo culture)
	{
		return ReplaceCore(oldValue, newValue, culture, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
	}

	public string Replace(string oldValue, string newValue, StringComparison comparisonType)
	{
		return comparisonType switch
		{
			StringComparison.CurrentCulture => ReplaceCore(oldValue, newValue, CultureInfo.CurrentCulture, CompareOptions.None), 
			StringComparison.CurrentCultureIgnoreCase => ReplaceCore(oldValue, newValue, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase), 
			StringComparison.InvariantCulture => ReplaceCore(oldValue, newValue, CultureInfo.InvariantCulture, CompareOptions.None), 
			StringComparison.InvariantCultureIgnoreCase => ReplaceCore(oldValue, newValue, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase), 
			StringComparison.Ordinal => Replace(oldValue, newValue), 
			StringComparison.OrdinalIgnoreCase => ReplaceCore(oldValue, newValue, CultureInfo.InvariantCulture, CompareOptions.OrdinalIgnoreCase), 
			_ => throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType"), 
		};
	}

	private unsafe string ReplaceCore(string oldValue, string newValue, CultureInfo culture, CompareOptions options)
	{
		if ((object)oldValue == null)
		{
			throw new ArgumentNullException("oldValue");
		}
		if (oldValue.Length == 0)
		{
			throw new ArgumentException("String cannot be of zero length.", "oldValue");
		}
		if ((object)newValue == null)
		{
			newValue = Empty;
		}
		CultureInfo obj = culture ?? CultureInfo.CurrentCulture;
		StringBuilder stringBuilder = StringBuilderCache.Acquire();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		CompareInfo compareInfo = obj.CompareInfo;
		do
		{
			num2 = compareInfo.IndexOf(this, oldValue, num, Length - num, options, &num3);
			if (num2 >= 0)
			{
				stringBuilder.Append(this, num, num2 - num);
				stringBuilder.Append(newValue);
				num = num2 + num3;
				flag = true;
				continue;
			}
			if (!flag)
			{
				StringBuilderCache.Release(stringBuilder);
				return this;
			}
			stringBuilder.Append(this, num, Length - num);
		}
		while (num2 >= 0);
		return StringBuilderCache.GetStringAndRelease(stringBuilder);
	}

	/// <summary>Returns a new string in which all occurrences of a specified Unicode character in this instance are replaced with another specified Unicode character.</summary>
	/// <returns>A string that is equivalent to this instance except that all instances of <paramref name="oldChar" /> are replaced with <paramref name="newChar" />. If <paramref name="oldChar" /> is not found in the current instance, the method returns the current instance unchanged. </returns>
	/// <param name="oldChar">The Unicode character to be replaced. </param>
	/// <param name="newChar">The Unicode character to replace all occurrences of <paramref name="oldChar" />. </param>
	/// <filterpriority>1</filterpriority>
	public unsafe string Replace(char oldChar, char newChar)
	{
		if (oldChar == newChar)
		{
			return this;
		}
		int num = Length;
		fixed (char* firstChar = &_firstChar)
		{
			char* ptr = firstChar;
			while (num > 0 && *ptr != oldChar)
			{
				num--;
				ptr++;
			}
		}
		if (num == 0)
		{
			return this;
		}
		string text = FastAllocateString(Length);
		fixed (char* firstChar2 = &_firstChar)
		{
			fixed (char* firstChar3 = &text._firstChar)
			{
				int num2 = Length - num;
				if (num2 > 0)
				{
					wstrcpy(firstChar3, firstChar2, num2);
				}
				char* ptr2 = firstChar2 + num2;
				char* ptr3 = firstChar3 + num2;
				do
				{
					char c = *ptr2;
					if (c == oldChar)
					{
						c = newChar;
					}
					*ptr3 = c;
					num--;
					ptr2++;
					ptr3++;
				}
				while (num > 0);
			}
		}
		return text;
	}

	/// <summary>Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.</summary>
	/// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue" /> are replaced with <paramref name="newValue" />. If <paramref name="oldValue" /> is not found in the current instance, the method returns the current instance unchanged. </returns>
	/// <param name="oldValue">The string to be replaced. </param>
	/// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="oldValue" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="oldValue" /> is the empty string (""). </exception>
	/// <filterpriority>1</filterpriority>
	public unsafe string Replace(string oldValue, string newValue)
	{
		if ((object)oldValue == null)
		{
			throw new ArgumentNullException("oldValue");
		}
		if (oldValue.Length == 0)
		{
			throw new ArgumentException("String cannot be of zero length.", "oldValue");
		}
		if ((object)newValue == null)
		{
			newValue = Empty;
		}
		Span<int> initialSpan = stackalloc int[128];
		ValueListBuilder<int> valueListBuilder = new ValueListBuilder<int>(initialSpan);
		fixed (char* firstChar = &_firstChar)
		{
			int num = 0;
			int num2 = Length - oldValue.Length;
			while (num <= num2)
			{
				char* ptr = firstChar + num;
				int num3 = 0;
				while (true)
				{
					if (num3 < oldValue.Length)
					{
						if (ptr[num3] == oldValue[num3])
						{
							num3++;
							continue;
						}
						num++;
						break;
					}
					valueListBuilder.Append(num);
					num += oldValue.Length;
					break;
				}
			}
		}
		if (valueListBuilder.Length == 0)
		{
			return this;
		}
		string result = ReplaceHelper(oldValue.Length, newValue, valueListBuilder.AsSpan());
		valueListBuilder.Dispose();
		return result;
	}

	private string ReplaceHelper(int oldValueLength, string newValue, ReadOnlySpan<int> indices)
	{
		long num = Length + (long)(newValue.Length - oldValueLength) * (long)indices.Length;
		if (num > int.MaxValue)
		{
			throw new OutOfMemoryException();
		}
		string text = FastAllocateString((int)num);
		Span<char> span = new Span<char>(ref text.GetRawStringData(), text.Length);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < indices.Length; i++)
		{
			int num4 = indices[i];
			int num5 = num4 - num2;
			if (num5 != 0)
			{
				this.AsSpan(num2, num5).CopyTo(span.Slice(num3));
				num3 += num5;
			}
			num2 = num4 + oldValueLength;
			newValue.AsSpan().CopyTo(span.Slice(num3));
			num3 += newValue.Length;
		}
		this.AsSpan(num2).CopyTo(span.Slice(num3));
		return text;
	}

	public string[] Split(char separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return SplitInternal(new ReadOnlySpan<char>(ref separator, 1), int.MaxValue, options);
	}

	public string[] Split(char separator, int count, StringSplitOptions options = StringSplitOptions.None)
	{
		return SplitInternal(new ReadOnlySpan<char>(ref separator, 1), count, options);
	}

	/// <summary>Returns a string array that contains the substrings in this instance that are delimited by elements of a specified Unicode character array.</summary>
	/// <returns>An array whose elements contain the substrings in this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this instance, an empty array that contains no delimiters, or null. </param>
	/// <filterpriority>1</filterpriority>
	public string[] Split(params char[] separator)
	{
		return SplitInternal(separator, int.MaxValue, StringSplitOptions.None);
	}

	/// <summary>Returns a string array that contains the substrings in this instance that are delimited by elements of a specified Unicode character array. A parameter specifies the maximum number of substrings to return.</summary>
	/// <returns>An array whose elements contain the substrings in this instance that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this instance, an empty array that contains no delimiters, or null. </param>
	/// <param name="count">The maximum number of substrings to return. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <filterpriority>1</filterpriority>
	public string[] Split(char[] separator, int count)
	{
		return SplitInternal(separator, count, StringSplitOptions.None);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified Unicode character array. A parameter specifies whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	public string[] Split(char[] separator, StringSplitOptions options)
	{
		return SplitInternal(separator, int.MaxValue, options);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified Unicode character array. Parameters specify the maximum number of substrings to return and whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more characters in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of Unicode characters that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="count">The maximum number of substrings to return. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	public string[] Split(char[] separator, int count, StringSplitOptions options)
	{
		return SplitInternal(separator, count, options);
	}

	private string[] SplitInternal(ReadOnlySpan<char> separators, int count, StringSplitOptions options)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero.");
		}
		if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
		{
			throw new ArgumentException(SR.Format("Illegal enum value: {0}.", options));
		}
		bool flag = options == StringSplitOptions.RemoveEmptyEntries;
		if (count == 0 || (flag && Length == 0))
		{
			return Array.Empty<string>();
		}
		if (count != 1)
		{
			Span<int> initialSpan = stackalloc int[128];
			ValueListBuilder<int> sepListBuilder = new ValueListBuilder<int>(initialSpan);
			MakeSeparatorList(separators, ref sepListBuilder);
			ReadOnlySpan<int> sepList = sepListBuilder.AsSpan();
			if (sepList.Length != 0)
			{
				string[] result = (flag ? SplitOmitEmptyEntries(sepList, default(ReadOnlySpan<int>), 1, count) : SplitKeepEmptyEntries(sepList, default(ReadOnlySpan<int>), 1, count));
				sepListBuilder.Dispose();
				return result;
			}
			return new string[1] { this };
		}
		return new string[1] { this };
	}

	public string[] Split(string separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return SplitInternal(separator ?? Empty, null, int.MaxValue, options);
	}

	public string[] Split(string separator, int count, StringSplitOptions options = StringSplitOptions.None)
	{
		return SplitInternal(separator ?? Empty, null, count, options);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified string array. A parameter specifies whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	public string[] Split(string[] separator, StringSplitOptions options)
	{
		return SplitInternal(null, separator, int.MaxValue, options);
	}

	/// <summary>Returns a string array that contains the substrings in this string that are delimited by elements of a specified string array. Parameters specify the maximum number of substrings to return and whether to return empty array elements.</summary>
	/// <returns>An array whose elements contain the substrings in this string that are delimited by one or more strings in <paramref name="separator" />. For more information, see the Remarks section.</returns>
	/// <param name="separator">An array of strings that delimit the substrings in this string, an empty array that contains no delimiters, or null. </param>
	/// <param name="count">The maximum number of substrings to return. </param>
	/// <param name="options">
	///   <see cref="F:System.StringSplitOptions.RemoveEmptyEntries" /> to omit empty array elements from the array returned; or <see cref="F:System.StringSplitOptions.None" /> to include empty array elements in the array returned. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="options" /> is not one of the <see cref="T:System.StringSplitOptions" /> values.</exception>
	/// <filterpriority>1</filterpriority>
	public string[] Split(string[] separator, int count, StringSplitOptions options)
	{
		return SplitInternal(null, separator, count, options);
	}

	private string[] SplitInternal(string separator, string[] separators, int count, StringSplitOptions options)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero.");
		}
		if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
		{
			throw new ArgumentException(SR.Format("Illegal enum value: {0}.", (int)options));
		}
		bool flag = options == StringSplitOptions.RemoveEmptyEntries;
		bool flag2 = (object)separator != null;
		if (!flag2 && (separators == null || separators.Length == 0))
		{
			return SplitInternal((ReadOnlySpan<char>)null, count, options);
		}
		if (count == 0 || (flag && Length == 0))
		{
			return Array.Empty<string>();
		}
		if (count == 1 || (flag2 && separator.Length == 0))
		{
			return new string[1] { this };
		}
		if (flag2)
		{
			return SplitInternal(separator, count, options);
		}
		Span<int> initialSpan = stackalloc int[128];
		ValueListBuilder<int> sepListBuilder = new ValueListBuilder<int>(initialSpan);
		Span<int> initialSpan2 = stackalloc int[128];
		ValueListBuilder<int> lengthListBuilder = new ValueListBuilder<int>(initialSpan2);
		MakeSeparatorList(separators, ref sepListBuilder, ref lengthListBuilder);
		ReadOnlySpan<int> sepList = sepListBuilder.AsSpan();
		ReadOnlySpan<int> lengthList = lengthListBuilder.AsSpan();
		if (sepList.Length != 0)
		{
			string[] result = (flag ? SplitOmitEmptyEntries(sepList, lengthList, 0, count) : SplitKeepEmptyEntries(sepList, lengthList, 0, count));
			sepListBuilder.Dispose();
			lengthListBuilder.Dispose();
			return result;
		}
		return new string[1] { this };
	}

	private string[] SplitInternal(string separator, int count, StringSplitOptions options)
	{
		Span<int> initialSpan = stackalloc int[128];
		ValueListBuilder<int> sepListBuilder = new ValueListBuilder<int>(initialSpan);
		MakeSeparatorList(separator, ref sepListBuilder);
		ReadOnlySpan<int> sepList = sepListBuilder.AsSpan();
		if (sepList.Length != 0)
		{
			string[] result = ((options == StringSplitOptions.RemoveEmptyEntries) ? SplitOmitEmptyEntries(sepList, default(ReadOnlySpan<int>), separator.Length, count) : SplitKeepEmptyEntries(sepList, default(ReadOnlySpan<int>), separator.Length, count));
			sepListBuilder.Dispose();
			return result;
		}
		return new string[1] { this };
	}

	private string[] SplitKeepEmptyEntries(ReadOnlySpan<int> sepList, ReadOnlySpan<int> lengthList, int defaultLength, int count)
	{
		int num = 0;
		int num2 = 0;
		count--;
		int num3 = ((sepList.Length < count) ? sepList.Length : count);
		string[] array = new string[num3 + 1];
		for (int i = 0; i < num3; i++)
		{
			if (num >= Length)
			{
				break;
			}
			array[num2++] = Substring(num, sepList[i] - num);
			num = sepList[i] + (lengthList.IsEmpty ? defaultLength : lengthList[i]);
		}
		if (num < Length && num3 >= 0)
		{
			array[num2] = Substring(num);
		}
		else if (num2 == num3)
		{
			array[num2] = Empty;
		}
		return array;
	}

	private string[] SplitOmitEmptyEntries(ReadOnlySpan<int> sepList, ReadOnlySpan<int> lengthList, int defaultLength, int count)
	{
		int length = sepList.Length;
		int num = ((length < count) ? (length + 1) : count);
		string[] array = new string[num];
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < length; i++)
		{
			if (num2 >= Length)
			{
				break;
			}
			if (sepList[i] - num2 > 0)
			{
				array[num3++] = Substring(num2, sepList[i] - num2);
			}
			num2 = sepList[i] + (lengthList.IsEmpty ? defaultLength : lengthList[i]);
			if (num3 == count - 1)
			{
				while (i < length - 1 && num2 == sepList[++i])
				{
					num2 += (lengthList.IsEmpty ? defaultLength : lengthList[i]);
				}
				break;
			}
		}
		if (num2 < Length)
		{
			array[num3++] = Substring(num2);
		}
		string[] array2 = array;
		if (num3 != num)
		{
			array2 = new string[num3];
			for (int j = 0; j < num3; j++)
			{
				array2[j] = array[j];
			}
		}
		return array2;
	}

	private unsafe void MakeSeparatorList(ReadOnlySpan<char> separators, ref ValueListBuilder<int> sepListBuilder)
	{
		switch (separators.Length)
		{
		case 0:
		{
			for (int i = 0; i < Length; i++)
			{
				if (char.IsWhiteSpace(this[i]))
				{
					sepListBuilder.Append(i);
				}
			}
			return;
		}
		case 1:
		{
			char c = separators[0];
			for (int k = 0; k < Length; k++)
			{
				if (this[k] == c)
				{
					sepListBuilder.Append(k);
				}
			}
			return;
		}
		case 2:
		{
			char c = separators[0];
			char c2 = separators[1];
			for (int l = 0; l < Length; l++)
			{
				char c5 = this[l];
				if (c5 == c || c5 == c2)
				{
					sepListBuilder.Append(l);
				}
			}
			return;
		}
		case 3:
		{
			char c = separators[0];
			char c2 = separators[1];
			char c3 = separators[2];
			for (int j = 0; j < Length; j++)
			{
				char c4 = this[j];
				if (c4 == c || c4 == c2 || c4 == c3)
				{
					sepListBuilder.Append(j);
				}
			}
			return;
		}
		}
		ProbabilisticMap probabilisticMap = default(ProbabilisticMap);
		uint* charMap = (uint*)(&probabilisticMap);
		InitializeProbabilisticMap(charMap, separators);
		for (int m = 0; m < Length; m++)
		{
			char c6 = this[m];
			if (IsCharBitSet(charMap, (byte)c6) && IsCharBitSet(charMap, (byte)((int)c6 >> 8)) && separators.Contains(c6))
			{
				sepListBuilder.Append(m);
			}
		}
	}

	private void MakeSeparatorList(string separator, ref ValueListBuilder<int> sepListBuilder)
	{
		int length = separator.Length;
		for (int i = 0; i < Length; i++)
		{
			if (this[i] == separator[0] && length <= Length - i && (length == 1 || this.AsSpan(i, length).SequenceEqual(separator)))
			{
				sepListBuilder.Append(i);
				i += length - 1;
			}
		}
	}

	private void MakeSeparatorList(string[] separators, ref ValueListBuilder<int> sepListBuilder, ref ValueListBuilder<int> lengthListBuilder)
	{
		_ = separators.Length;
		for (int i = 0; i < Length; i++)
		{
			foreach (string text in separators)
			{
				if (!IsNullOrEmpty(text))
				{
					int length = text.Length;
					if (this[i] == text[0] && length <= Length - i && (length == 1 || this.AsSpan(i, length).SequenceEqual(text)))
					{
						sepListBuilder.Append(i);
						lengthListBuilder.Append(length);
						i += length - 1;
						break;
					}
				}
			}
		}
	}

	/// <summary>Retrieves a substring from this instance. The substring starts at a specified character position and continues to the end of the string.</summary>
	/// <returns>A string that is equivalent to the substring that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance.</returns>
	/// <param name="startIndex">The zero-based starting character position of a substring in this instance. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than zero or greater than the length of this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public string Substring(int startIndex)
	{
		return Substring(startIndex, Length - startIndex);
	}

	/// <summary>Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.</summary>
	/// <returns>A string that is equivalent to the substring of length <paramref name="length" /> that begins at <paramref name="startIndex" /> in this instance, or <see cref="F:System.String.Empty" /> if <paramref name="startIndex" /> is equal to the length of this instance and <paramref name="length" /> is zero.</returns>
	/// <param name="startIndex">The zero-based starting character position of a substring in this instance. </param>
	/// <param name="length">The number of characters in the substring. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> plus <paramref name="length" /> indicates a position not within this instance.-or- <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	public string Substring(int startIndex, int length)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		if (startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "startIndex cannot be larger than length of string.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
		}
		if (startIndex > Length - length)
		{
			throw new ArgumentOutOfRangeException("length", "Index and length must refer to a location within the string.");
		}
		if (length == 0)
		{
			return Empty;
		}
		if (startIndex == 0 && length == Length)
		{
			return this;
		}
		return InternalSubString(startIndex, length);
	}

	private unsafe string InternalSubString(int startIndex, int length)
	{
		string text = FastAllocateString(length);
		fixed (char* firstChar = &text._firstChar)
		{
			fixed (char* firstChar2 = &_firstChar)
			{
				wstrcpy(firstChar, firstChar2 + startIndex, length);
			}
		}
		return text;
	}

	/// <summary>Returns a copy of this string converted to lowercase.</summary>
	/// <returns>A string in lowercase.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToLower()
	{
		return CultureInfo.CurrentCulture.TextInfo.ToLower(this);
	}

	/// <summary>Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.</summary>
	/// <returns>The lowercase equivalent of the current string.</returns>
	/// <param name="culture">An object that supplies culture-specific casing rules. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToLower(CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.TextInfo.ToLower(this);
	}

	/// <summary>Returns a copy of this <see cref="T:System.String" /> object converted to lowercase using the casing rules of the invariant culture.</summary>
	/// <returns>The lowercase equivalent of the current string.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToLowerInvariant()
	{
		return CultureInfo.InvariantCulture.TextInfo.ToLower(this);
	}

	/// <summary>Returns a copy of this string converted to uppercase.</summary>
	/// <returns>The uppercase equivalent of the current string.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToUpper()
	{
		return CultureInfo.CurrentCulture.TextInfo.ToUpper(this);
	}

	/// <summary>Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.</summary>
	/// <returns>The uppercase equivalent of the current string.</returns>
	/// <param name="culture">An object that supplies culture-specific casing rules. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToUpper(CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return culture.TextInfo.ToUpper(this);
	}

	/// <summary>Returns a copy of this <see cref="T:System.String" /> object converted to uppercase using the casing rules of the invariant culture.</summary>
	/// <returns>The uppercase equivalent of the current string.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string ToUpperInvariant()
	{
		return CultureInfo.InvariantCulture.TextInfo.ToUpper(this);
	}

	/// <summary>Removes all leading and trailing white-space characters from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all white-space characters are removed from the start and end of the current string.</returns>
	/// <filterpriority>1</filterpriority>
	public string Trim()
	{
		return TrimWhiteSpaceHelper(TrimType.Both);
	}

	public unsafe string Trim(char trimChar)
	{
		return TrimHelper(&trimChar, 1, TrimType.Both);
	}

	/// <summary>Removes all leading and trailing occurrences of a set of characters specified in an array from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the start and end of the current string. If <paramref name="trimChars" /> is null or an empty array, white-space characters are removed instead.</returns>
	/// <param name="trimChars">An array of Unicode characters to remove, or null. </param>
	/// <filterpriority>1</filterpriority>
	public unsafe string Trim(params char[] trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
		{
			return TrimWhiteSpaceHelper(TrimType.Both);
		}
		fixed (char* trimChars2 = &trimChars[0])
		{
			return TrimHelper(trimChars2, trimChars.Length, TrimType.Both);
		}
	}

	public string TrimStart()
	{
		return TrimWhiteSpaceHelper(TrimType.Head);
	}

	public unsafe string TrimStart(char trimChar)
	{
		return TrimHelper(&trimChar, 1, TrimType.Head);
	}

	/// <summary>Removes all leading occurrences of a set of characters specified in an array from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all occurrences of characters in the <paramref name="trimChars" /> parameter are removed from the start of the current string. If <paramref name="trimChars" /> is null or an empty array, white-space characters are removed instead.</returns>
	/// <param name="trimChars">An array of Unicode characters to remove, or null. </param>
	/// <filterpriority>2</filterpriority>
	public unsafe string TrimStart(params char[] trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
		{
			return TrimWhiteSpaceHelper(TrimType.Head);
		}
		fixed (char* trimChars2 = &trimChars[0])
		{
			return TrimHelper(trimChars2, trimChars.Length, TrimType.Head);
		}
	}

	public string TrimEnd()
	{
		return TrimWhiteSpaceHelper(TrimType.Tail);
	}

	public unsafe string TrimEnd(char trimChar)
	{
		return TrimHelper(&trimChar, 1, TrimType.Tail);
	}

	/// <summary>Removes all trailing occurrences of a set of characters specified in an array from the current <see cref="T:System.String" /> object.</summary>
	/// <returns>The string that remains after all occurrences of the characters in the <paramref name="trimChars" /> parameter are removed from the end of the current string. If <paramref name="trimChars" /> is null or an empty array, Unicode white-space characters are removed instead.</returns>
	/// <param name="trimChars">An array of Unicode characters to remove, or null. </param>
	/// <filterpriority>2</filterpriority>
	public unsafe string TrimEnd(params char[] trimChars)
	{
		if (trimChars == null || trimChars.Length == 0)
		{
			return TrimWhiteSpaceHelper(TrimType.Tail);
		}
		fixed (char* trimChars2 = &trimChars[0])
		{
			return TrimHelper(trimChars2, trimChars.Length, TrimType.Tail);
		}
	}

	private string TrimWhiteSpaceHelper(TrimType trimType)
	{
		int num = Length - 1;
		int i = 0;
		if (trimType != TrimType.Tail)
		{
			for (i = 0; i < Length && char.IsWhiteSpace(this[i]); i++)
			{
			}
		}
		if (trimType != TrimType.Head)
		{
			num = Length - 1;
			while (num >= i && char.IsWhiteSpace(this[num]))
			{
				num--;
			}
		}
		return CreateTrimmedString(i, num);
	}

	private unsafe string TrimHelper(char* trimChars, int trimCharsLength, TrimType trimType)
	{
		int num = Length - 1;
		int i = 0;
		if (trimType != TrimType.Tail)
		{
			for (i = 0; i < Length; i++)
			{
				int num2 = 0;
				char c = this[i];
				for (num2 = 0; num2 < trimCharsLength && trimChars[num2] != c; num2++)
				{
				}
				if (num2 == trimCharsLength)
				{
					break;
				}
			}
		}
		if (trimType != TrimType.Head)
		{
			for (num = Length - 1; num >= i; num--)
			{
				int num3 = 0;
				char c2 = this[num];
				for (num3 = 0; num3 < trimCharsLength && trimChars[num3] != c2; num3++)
				{
				}
				if (num3 == trimCharsLength)
				{
					break;
				}
			}
		}
		return CreateTrimmedString(i, num);
	}

	private string CreateTrimmedString(int start, int end)
	{
		int num = end - start + 1;
		if (num != Length)
		{
			if (num != 0)
			{
				return InternalSubString(start, num);
			}
			return Empty;
		}
		return this;
	}

	/// <summary>Returns a value indicating whether the specified <see cref="T:System.String" /> object occurs within this string.</summary>
	/// <returns>true if the <paramref name="value" /> parameter occurs within this string, or if <paramref name="value" /> is the empty string (""); otherwise, false.</returns>
	/// <param name="value">The string to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public bool Contains(string value)
	{
		return IndexOf(value, StringComparison.Ordinal) >= 0;
	}

	public bool Contains(string value, StringComparison comparisonType)
	{
		return IndexOf(value, comparisonType) >= 0;
	}

	public bool Contains(char value)
	{
		return IndexOf(value) != -1;
	}

	public bool Contains(char value, StringComparison comparisonType)
	{
		return IndexOf(value, comparisonType) != -1;
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">A Unicode character to seek. </param>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(char value)
	{
		return SpanHelpers.IndexOf(ref _firstChar, value, Length);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this string. The search starts at a specified character position.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">A Unicode character to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of the string. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(char value, int startIndex)
	{
		return IndexOf(value, startIndex, Length - startIndex);
	}

	public int IndexOf(char value, StringComparison comparisonType)
	{
		return comparisonType switch
		{
			StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, CompareOptions.None), 
			StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, CompareOptions.IgnoreCase), 
			StringComparison.InvariantCulture => CompareInfo.Invariant.IndexOf(this, value, CompareOptions.None), 
			StringComparison.InvariantCultureIgnoreCase => CompareInfo.Invariant.IndexOf(this, value, CompareOptions.IgnoreCase), 
			StringComparison.Ordinal => CompareInfo.Invariant.IndexOf(this, value, CompareOptions.Ordinal), 
			StringComparison.OrdinalIgnoreCase => CompareInfo.Invariant.IndexOf(this, value, CompareOptions.OrdinalIgnoreCase), 
			_ => throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType"), 
		};
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified character in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">A Unicode character to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(char value, int startIndex, int count)
	{
		if ((uint)startIndex > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if ((uint)count > (uint)(Length - startIndex))
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		int num = SpanHelpers.IndexOf(ref Unsafe.Add(ref _firstChar, startIndex), value, count);
		if (num != -1)
		{
			return num + startIndex;
		}
		return num;
	}

	/// <summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters.</summary>
	/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public int IndexOfAny(char[] anyOf)
	{
		return IndexOfAny(anyOf, 0, Length);
	}

	/// <summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position.</summary>
	/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the number of characters in this instance. </exception>
	/// <filterpriority>2</filterpriority>
	public int IndexOfAny(char[] anyOf, int startIndex)
	{
		return IndexOfAny(anyOf, startIndex, Length - startIndex);
	}

	/// <summary>Reports the zero-based index of the first occurrence in this instance of any character in a specified array of Unicode characters. The search starts at a specified character position and examines a specified number of character positions.</summary>
	/// <returns>The zero-based index position of the first occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="count" /> + <paramref name="startIndex" /> is greater than the number of characters in this instance. </exception>
	/// <filterpriority>2</filterpriority>
	public int IndexOfAny(char[] anyOf, int startIndex, int count)
	{
		if (anyOf == null)
		{
			throw new ArgumentNullException("anyOf");
		}
		if ((uint)startIndex > (uint)Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if ((uint)count > (uint)(Length - startIndex))
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		if (anyOf.Length == 2)
		{
			return IndexOfAny(anyOf[0], anyOf[1], startIndex, count);
		}
		if (anyOf.Length == 3)
		{
			return IndexOfAny(anyOf[0], anyOf[1], anyOf[2], startIndex, count);
		}
		if (anyOf.Length > 3)
		{
			return IndexOfCharArray(anyOf, startIndex, count);
		}
		if (anyOf.Length == 1)
		{
			return IndexOf(anyOf[0], startIndex, count);
		}
		return -1;
	}

	private unsafe int IndexOfAny(char value1, char value2, int startIndex, int count)
	{
		fixed (char* firstChar = &_firstChar)
		{
			char* ptr = firstChar + startIndex;
			while (count > 0)
			{
				char c = *ptr;
				if (c == value1 || c == value2)
				{
					return (int)(ptr - firstChar);
				}
				c = ptr[1];
				if (c == value1 || c == value2)
				{
					if (count != 1)
					{
						return (int)(ptr - firstChar) + 1;
					}
					return -1;
				}
				ptr += 2;
				count -= 2;
			}
			return -1;
		}
	}

	private unsafe int IndexOfAny(char value1, char value2, char value3, int startIndex, int count)
	{
		fixed (char* firstChar = &_firstChar)
		{
			char* ptr = firstChar + startIndex;
			while (count > 0)
			{
				char c = *ptr;
				if (c == value1 || c == value2 || c == value3)
				{
					return (int)(ptr - firstChar);
				}
				ptr++;
				count--;
			}
			return -1;
		}
	}

	private unsafe int IndexOfCharArray(char[] anyOf, int startIndex, int count)
	{
		ProbabilisticMap probabilisticMap = default(ProbabilisticMap);
		uint* charMap = (uint*)(&probabilisticMap);
		InitializeProbabilisticMap(charMap, anyOf);
		fixed (char* firstChar = &_firstChar)
		{
			char* ptr = firstChar + startIndex;
			while (count > 0)
			{
				int num = *ptr;
				if (IsCharBitSet(charMap, (byte)num) && IsCharBitSet(charMap, (byte)(num >> 8)) && ArrayContains((char)num, anyOf))
				{
					return (int)(ptr - firstChar);
				}
				count--;
				ptr++;
			}
			return -1;
		}
	}

	private unsafe static void InitializeProbabilisticMap(uint* charMap, ReadOnlySpan<char> anyOf)
	{
		bool flag = false;
		for (int i = 0; i < anyOf.Length; i++)
		{
			int num = anyOf[i];
			SetCharBit(charMap, (byte)num);
			num >>= 8;
			if (num == 0)
			{
				flag = true;
			}
			else
			{
				SetCharBit(charMap, (byte)num);
			}
		}
		if (flag)
		{
			*charMap |= 1u;
		}
	}

	private static bool ArrayContains(char searchChar, char[] anyOf)
	{
		for (int i = 0; i < anyOf.Length; i++)
		{
			if (anyOf[i] == searchChar)
			{
				return true;
			}
		}
		return false;
	}

	private unsafe static bool IsCharBitSet(uint* charMap, byte value)
	{
		return (charMap[value & 7] & (uint)(1 << (value >> 3))) != 0;
	}

	private unsafe static void SetCharBit(uint* charMap, byte value)
	{
		charMap[value & 7] |= (uint)(1 << (value >> 3));
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in this instance.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
	/// <param name="value">The string to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string value)
	{
		return IndexOf(value, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string.</exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string value, int startIndex)
	{
		return IndexOf(value, startIndex, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in this instance. The search starts at a specified character position and examines a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the length of this string.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />. </exception>
	/// <filterpriority>1</filterpriority>
	public int IndexOf(string value, int startIndex, int count)
	{
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || count > Length - startIndex)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		return IndexOf(value, startIndex, count, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is 0.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int IndexOf(string value, StringComparison comparisonType)
	{
		return IndexOf(value, 0, Length, comparisonType);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string and the type of search to use for the specified string.</summary>
	/// <returns>The zero-based index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> is less than 0 (zero) or greater than the length of this string. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int IndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		return IndexOf(value, startIndex, Length - startIndex, comparisonType);
	}

	/// <summary>Reports the zero-based index of the first occurrence of the specified string in the current <see cref="T:System.String" /> object. Parameters specify the starting search position in the current string, the number of characters in the current string to search, and the type of search to use for the specified string.</summary>
	/// <returns>The zero-based index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is <paramref name="startIndex" />.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. </param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- <paramref name="startIndex" /> is greater than the length of this instance.-or-<paramref name="count" /> is greater than the length of this string minus <paramref name="startIndex" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || startIndex > Length - count)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		return comparisonType switch
		{
			StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None), 
			StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
			StringComparison.InvariantCulture => CompareInfo.Invariant.IndexOf(this, value, startIndex, count, CompareOptions.None), 
			StringComparison.InvariantCultureIgnoreCase => CompareInfo.Invariant.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
			StringComparison.Ordinal => CompareInfo.Invariant.IndexOfOrdinal(this, value, startIndex, count, ignoreCase: false), 
			StringComparison.OrdinalIgnoreCase => CompareInfo.Invariant.IndexOfOrdinal(this, value, startIndex, count, ignoreCase: true), 
			_ => throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType"), 
		};
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not.</returns>
	/// <param name="value">The Unicode character to seek. </param>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(char value)
	{
		return SpanHelpers.LastIndexOf(ref _firstChar, value, Length);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified Unicode character within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="value">The Unicode character to seek. </param>
	/// <param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.</exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(char value, int startIndex)
	{
		return LastIndexOf(value, startIndex, startIndex + 1);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of the specified Unicode character in a substring within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that character is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="value">The Unicode character to seek. </param>
	/// <param name="startIndex">The starting position of the search. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than or equal to the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" /> + 1 is less than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(char value, int startIndex, int count)
	{
		if (Length == 0)
		{
			return -1;
		}
		if ((uint)startIndex >= (uint)Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if ((uint)count > (uint)(startIndex + 1))
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		int num = startIndex + 1 - count;
		int num2 = SpanHelpers.LastIndexOf(ref Unsafe.Add(ref _firstChar, num), value, count);
		if (num2 != -1)
		{
			return num2 + num;
		}
		return num2;
	}

	/// <summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array.</summary>
	/// <returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public int LastIndexOfAny(char[] anyOf)
	{
		return LastIndexOfAny(anyOf, Length - 1, Length);
	}

	/// <summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	/// <returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> specifies a position that is not within this instance. </exception>
	/// <filterpriority>2</filterpriority>
	public int LastIndexOfAny(char[] anyOf, int startIndex)
	{
		return LastIndexOfAny(anyOf, startIndex, startIndex + 1);
	}

	/// <summary>Reports the zero-based index position of the last occurrence in this instance of one or more characters specified in a Unicode array. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	/// <returns>The index position of the last occurrence in this instance where any character in <paramref name="anyOf" /> was found; -1 if no character in <paramref name="anyOf" /> was found or if the current instance equals <see cref="F:System.String.Empty" />.</returns>
	/// <param name="anyOf">A Unicode character array containing one or more characters to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="anyOf" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="count" /> or <paramref name="startIndex" /> is negative.-or- The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> minus <paramref name="count" /> + 1 is less than zero. </exception>
	/// <filterpriority>2</filterpriority>
	public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
	{
		if (anyOf == null)
		{
			throw new ArgumentNullException("anyOf");
		}
		if (Length == 0)
		{
			return -1;
		}
		if ((uint)startIndex >= (uint)Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count < 0 || count - 1 > startIndex)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		if (anyOf.Length > 1)
		{
			return LastIndexOfCharArray(anyOf, startIndex, count);
		}
		if (anyOf.Length == 1)
		{
			return LastIndexOf(anyOf[0], startIndex, count);
		}
		return -1;
	}

	private unsafe int LastIndexOfCharArray(char[] anyOf, int startIndex, int count)
	{
		ProbabilisticMap probabilisticMap = default(ProbabilisticMap);
		uint* charMap = (uint*)(&probabilisticMap);
		InitializeProbabilisticMap(charMap, anyOf);
		fixed (char* firstChar = &_firstChar)
		{
			char* ptr = firstChar + startIndex;
			while (count > 0)
			{
				int num = *ptr;
				if (IsCharBitSet(charMap, (byte)num) && IsCharBitSet(charMap, (byte)(num >> 8)) && ArrayContains((char)num, anyOf))
				{
					return (int)(ptr - firstChar);
				}
				count--;
				ptr--;
			}
			return -1;
		}
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(string value)
	{
		return LastIndexOf(value, Length - 1, Length, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance. -or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than zero.</exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(string value, int startIndex)
	{
		return LastIndexOf(value, startIndex, startIndex + 1, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for a specified number of character positions.</summary>
	/// <returns>The zero-based index position of <paramref name="value" /> if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and  <paramref name="startIndex" /> is negative.-or- The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> - <paramref name="count" /> + 1 specifies a position that is not within this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public int LastIndexOf(string value, int startIndex, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		return LastIndexOf(value, startIndex, count, StringComparison.CurrentCulture);
	}

	/// <summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. A parameter specifies the type of search to use for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int LastIndexOf(string value, StringComparison comparisonType)
	{
		return LastIndexOf(value, Length - 1, Length, comparisonType);
	}

	/// <summary>Reports the zero-based index of the last occurrence of a specified string within the current <see cref="T:System.String" /> object. The search starts at a specified character position and proceeds backward toward the beginning of the string. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is less than zero or greater than the length of the current instance. -or-The current instance equals <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than zero.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
	{
		return LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
	}

	/// <summary>Reports the zero-based index position of the last occurrence of a specified string within this instance. The search starts at a specified character position and proceeds backward toward the beginning of the string for the specified number of character positions. A parameter specifies the type of comparison to perform when searching for the specified string.</summary>
	/// <returns>The index position of the <paramref name="value" /> parameter if that string is found, or -1 if it is not found or if the current instance equals <see cref="F:System.String.Empty" />. If <paramref name="value" /> is <see cref="F:System.String.Empty" />, the return value is the smaller of <paramref name="startIndex" /> and the last index position in this instance.</returns>
	/// <param name="value">The string to seek. </param>
	/// <param name="startIndex">The search starting position. The search proceeds from <paramref name="startIndex" /> toward the beginning of this instance.</param>
	/// <param name="count">The number of character positions to examine. </param>
	/// <param name="comparisonType">One of the enumeration values that specifies the rules for the search. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is negative.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is negative.-or- The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> is greater than the length of this instance.-or-The current instance does not equal <see cref="F:System.String.Empty" />, and <paramref name="startIndex" /> + 1 - <paramref name="count" /> specifies a position that is not within this instance. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
	public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
	{
		if ((object)value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (Length == 0 && (startIndex == -1 || startIndex == 0))
		{
			if (value.Length != 0)
			{
				return -1;
			}
			return 0;
		}
		if (startIndex < 0 || startIndex > Length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (startIndex == Length)
		{
			startIndex--;
			if (count > 0)
			{
				count--;
			}
		}
		if (count < 0 || startIndex - count + 1 < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count must be positive and count must refer to a location within the string/array/collection.");
		}
		if (value.Length == 0)
		{
			return startIndex;
		}
		return comparisonType switch
		{
			StringComparison.CurrentCulture => CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None), 
			StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
			StringComparison.InvariantCulture => CompareInfo.Invariant.LastIndexOf(this, value, startIndex, count, CompareOptions.None), 
			StringComparison.InvariantCultureIgnoreCase => CompareInfo.Invariant.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase), 
			StringComparison.Ordinal => CompareInfo.Invariant.LastIndexOfOrdinal(this, value, startIndex, count, ignoreCase: false), 
			StringComparison.OrdinalIgnoreCase => CompareInfo.Invariant.LastIndexOfOrdinal(this, value, startIndex, count, ignoreCase: true), 
			_ => throw new ArgumentException("The string comparison type passed in is currently not supported.", "comparisonType"), 
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters.</summary>
	/// <param name="value">An array of Unicode characters. </param>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[PreserveDependency("CreateString(System.Char[])", "System.String")]
	public extern String(char[] value);

	private unsafe static string Ctor(char[] value)
	{
		if (value == null || value.Length == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(value.Length);
		fixed (char* firstChar = &text._firstChar)
		{
			fixed (char* smem = value)
			{
				wstrcpy(firstChar, smem, value.Length);
			}
		}
		return text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by an array of Unicode characters, a starting character position within that array, and a length.</summary>
	/// <param name="value">An array of Unicode characters. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or- The sum of <paramref name="startIndex" /> and <paramref name="length" /> is greater than the number of elements in <paramref name="value" />. </exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[PreserveDependency("CreateString(System.Char[], System.Int32, System.Int32)", "System.String")]
	public extern String(char[] value, int startIndex, int length);

	private unsafe static string Ctor(char[] value, int startIndex, int length)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
		}
		if (startIndex > value.Length - length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (length == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(length);
		fixed (char* firstChar = &text._firstChar)
		{
			fixed (char* ptr = value)
			{
				wstrcpy(firstChar, ptr + startIndex, length);
			}
		}
		return text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of Unicode characters.</summary>
	/// <param name="value">A pointer to a null-terminated array of Unicode characters. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The current process does not have read access to all the addressed characters.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> specifies an array that contains an invalid Unicode character, or <paramref name="value" /> specifies an address less than 64000.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[PreserveDependency("CreateString(System.Char*)", "System.String")]
	public unsafe extern String(char* value);

	private unsafe static string Ctor(char* ptr)
	{
		if (ptr == null)
		{
			return Empty;
		}
		int num = wcslen(ptr);
		if (num == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(num);
		fixed (char* firstChar = &text._firstChar)
		{
			wstrcpy(firstChar, ptr, num);
		}
		return text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of Unicode characters, a starting character position within that array, and a length.</summary>
	/// <param name="value">A pointer to an array of Unicode characters. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero, <paramref name="value" /> + <paramref name="startIndex" /> cause a pointer overflow, or the current process does not have read access to all the addressed characters.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="value" /> specifies an array that contains an invalid Unicode character, or <paramref name="value" /> + <paramref name="startIndex" /> specifies an address less than 64000.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[PreserveDependency("CreateString(System.Char*, System.Int32, System.Int32)", "System.String")]
	[CLSCompliant(false)]
	public unsafe extern String(char* value, int startIndex, int length);

	private unsafe static string Ctor(char* ptr, int startIndex, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		char* ptr2 = ptr + startIndex;
		if (ptr2 < ptr)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Pointer startIndex and length do not refer to a valid string.");
		}
		if (length == 0)
		{
			return Empty;
		}
		if (ptr == null)
		{
			throw new ArgumentOutOfRangeException("ptr", "Pointer startIndex and length do not refer to a valid string.");
		}
		string text = FastAllocateString(length);
		fixed (char* firstChar = &text._firstChar)
		{
			wstrcpy(firstChar, ptr2, length);
		}
		return text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a pointer to an array of 8-bit signed integers.</summary>
	/// <param name="value">A pointer to a null-terminated array of 8-bit signed integers. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded in ANSI. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The length of the new string to initialize, which is determined by the null termination character of <paramref name="value" />, is too large to allocate. </exception>
	/// <exception cref="T:System.AccessViolationException">
	///   <paramref name="value" /> specifies an invalid address.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[PreserveDependency("CreateString(System.SByte*)", "System.String")]
	[CLSCompliant(false)]
	public unsafe extern String(sbyte* value);

	private unsafe static string Ctor(sbyte* value)
	{
		if (value == null)
		{
			return Empty;
		}
		int num = new ReadOnlySpan<byte>(value, int.MaxValue).IndexOf<byte>(0);
		if (num < 0)
		{
			throw new ArgumentException("The string must be null-terminated.");
		}
		return CreateStringForSByteConstructor((byte*)value, num);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of 8-bit signed integers, a starting position within that array, and a length.</summary>
	/// <param name="value">A pointer to an array of 8-bit signed integers. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. -or-The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is too large for the current platform; that is, the address calculation overflowed. -or-The length of the new string to initialize is too large to allocate.</exception>
	/// <exception cref="T:System.ArgumentException">The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is less than 64K.-or- A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded in ANSI. </exception>
	/// <exception cref="T:System.AccessViolationException">
	///   <paramref name="value" />, <paramref name="startIndex" />, and <paramref name="length" /> collectively specify an invalid address.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[PreserveDependency("CreateString(System.SByte*, System.Int32, System.Int32)", "System.String")]
	public unsafe extern String(sbyte* value, int startIndex, int length);

	private unsafe static string Ctor(sbyte* value, int startIndex, int length)
	{
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
		}
		if (value == null)
		{
			if (length == 0)
			{
				return Empty;
			}
			throw new ArgumentNullException("value");
		}
		byte* ptr = (byte*)(value + startIndex);
		if (ptr < value)
		{
			throw new ArgumentOutOfRangeException("value", "Pointer startIndex and length do not refer to a valid string.");
		}
		return CreateStringForSByteConstructor(ptr, length);
	}

	private unsafe static string CreateStringForSByteConstructor(byte* pb, int numBytes)
	{
		if (numBytes == 0)
		{
			return Empty;
		}
		return Encoding.UTF8.GetString(pb, numBytes);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified pointer to an array of 8-bit signed integers, a starting position within that array, a length, and an <see cref="T:System.Text.Encoding" /> object.</summary>
	/// <param name="value">A pointer to an array of 8-bit signed integers. </param>
	/// <param name="startIndex">The starting position within <paramref name="value" />. </param>
	/// <param name="length">The number of characters within <paramref name="value" /> to use. </param>
	/// <param name="enc">An object that specifies how the array referenced by <paramref name="value" /> is encoded. If <paramref name="enc" /> is null, ANSI encoding is assumed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero. -or-The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is too large for the current platform; that is, the address calculation overflowed. -or-The length of the new string to initialize is too large to allocate.</exception>
	/// <exception cref="T:System.ArgumentException">The address specified by <paramref name="value" /> + <paramref name="startIndex" /> is less than 64K.-or- A new instance of <see cref="T:System.String" /> could not be initialized using <paramref name="value" />, assuming <paramref name="value" /> is encoded as specified by <paramref name="enc" />. </exception>
	/// <exception cref="T:System.AccessViolationException">
	///   <paramref name="value" />, <paramref name="startIndex" />, and <paramref name="length" /> collectively specify an invalid address.</exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	[PreserveDependency("CreateString(System.SByte*, System.Int32, System.Int32, System.Text.Encoding)", "System.String")]
	public unsafe extern String(sbyte* value, int startIndex, int length, Encoding enc);

	private unsafe static string Ctor(sbyte* value, int startIndex, int length, Encoding enc)
	{
		if (enc == null)
		{
			return new string(value, startIndex, length);
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Non-negative number required.");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex", "StartIndex cannot be less than zero.");
		}
		if (value == null)
		{
			if (length == 0)
			{
				return Empty;
			}
			throw new ArgumentNullException("value");
		}
		byte* ptr = (byte*)(value + startIndex);
		if (ptr < value)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Pointer startIndex and length do not refer to a valid string.");
		}
		return enc.GetString(new ReadOnlySpan<byte>(ptr, length));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.String" /> class to the value indicated by a specified Unicode character repeated a specified number of times.</summary>
	/// <param name="c">A Unicode character. </param>
	/// <param name="count">The number of times <paramref name="c" /> occurs. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> is less than zero. </exception>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[PreserveDependency("CreateString(System.Char, System.Int32)", "System.String")]
	public extern String(char c, int count);

	private unsafe static string Ctor(char c, int count)
	{
		if (count <= 0)
		{
			if (count == 0)
			{
				return Empty;
			}
			throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero.");
		}
		string text = FastAllocateString(count);
		if (c != 0)
		{
			fixed (char* firstChar = &text._firstChar)
			{
				uint num = ((uint)c << 16) | c;
				uint* ptr = (uint*)firstChar;
				if (count >= 4)
				{
					count -= 4;
					do
					{
						*ptr = num;
						ptr[1] = num;
						ptr += 2;
						count -= 4;
					}
					while (count >= 0);
				}
				if ((count & 2) != 0)
				{
					*ptr = num;
					ptr++;
				}
				if ((count & 1) != 0)
				{
					*(char*)ptr = c;
				}
			}
		}
		return text;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[PreserveDependency("CreateString(System.ReadOnlySpan`1<System.Char>)", "System.String")]
	public extern String(ReadOnlySpan<char> value);

	private unsafe static string Ctor(ReadOnlySpan<char> value)
	{
		if (value.Length == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(value.Length);
		fixed (char* firstChar = &text._firstChar)
		{
			fixed (char* reference = &MemoryMarshal.GetReference(value))
			{
				wstrcpy(firstChar, reference, value.Length);
			}
		}
		return text;
	}

	public static string Create<TState>(int length, TState state, SpanAction<char, TState> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (length <= 0)
		{
			if (length == 0)
			{
				return Empty;
			}
			throw new ArgumentOutOfRangeException("length");
		}
		string text = FastAllocateString(length);
		action(new Span<char>(ref text.GetRawStringData(), length), state);
		return text;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator ReadOnlySpan<char>(string value)
	{
		if ((object)value == null)
		{
			return default(ReadOnlySpan<char>);
		}
		return new ReadOnlySpan<char>(ref value.GetRawStringData(), value.Length);
	}

	/// <summary>Returns a reference to this instance of <see cref="T:System.String" />.</summary>
	/// <returns>This instance of <see cref="T:System.String" />.</returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return this;
	}

	/// <summary>Creates a new instance of <see cref="T:System.String" /> with the same value as a specified <see cref="T:System.String" />.</summary>
	/// <returns>A new string with the same value as <paramref name="str" />.</returns>
	/// <param name="str">The string to copy. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public unsafe static string Copy(string str)
	{
		if ((object)str == null)
		{
			throw new ArgumentNullException("str");
		}
		string text = FastAllocateString(str.Length);
		fixed (char* firstChar = &text._firstChar)
		{
			fixed (char* firstChar2 = &str._firstChar)
			{
				wstrcpy(firstChar, firstChar2, str.Length);
			}
		}
		return text;
	}

	/// <summary>Copies a specified number of characters from a specified position in this instance to a specified position in an array of Unicode characters.</summary>
	/// <param name="sourceIndex">The index of the first character in this instance to copy. </param>
	/// <param name="destination">An array of Unicode characters to which characters in this instance are copied. </param>
	/// <param name="destinationIndex">The index in <paramref name="destination" /> at which the copy operation begins. </param>
	/// <param name="count">The number of characters in this instance to copy to <paramref name="destination" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destination" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="sourceIndex" />, <paramref name="destinationIndex" />, or <paramref name="count" /> is negative -or- <paramref name="count" /> is greater than the length of the substring from <paramref name="startIndex" /> to the end of this instance -or- <paramref name="count" /> is greater than the length of the subarray from <paramref name="destinationIndex" /> to the end of <paramref name="destination" /></exception>
	/// <filterpriority>2</filterpriority>
	public unsafe void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Count cannot be less than zero.");
		}
		if (sourceIndex < 0)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (count > Length - sourceIndex)
		{
			throw new ArgumentOutOfRangeException("sourceIndex", "Index and count must refer to a location within the string.");
		}
		if (destinationIndex > destination.Length - count || destinationIndex < 0)
		{
			throw new ArgumentOutOfRangeException("destinationIndex", "Index and count must refer to a location within the string.");
		}
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* ptr = destination)
			{
				wstrcpy(ptr + destinationIndex, firstChar + sourceIndex, count);
			}
		}
	}

	/// <summary>Copies the characters in this instance to a Unicode character array.</summary>
	/// <returns>A Unicode character array whose elements are the individual characters of this instance. If this instance is an empty string, the returned array is empty and has a zero length.</returns>
	/// <filterpriority>1</filterpriority>
	public unsafe char[] ToCharArray()
	{
		if (Length == 0)
		{
			return Array.Empty<char>();
		}
		char[] array = new char[Length];
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* dmem = &array[0])
			{
				wstrcpy(dmem, firstChar, Length);
			}
		}
		return array;
	}

	/// <summary>Copies the characters in a specified substring in this instance to a Unicode character array.</summary>
	/// <returns>A Unicode character array whose elements are the <paramref name="length" /> number of characters in this instance starting from character position <paramref name="startIndex" />.</returns>
	/// <param name="startIndex">The starting position of a substring in this instance. </param>
	/// <param name="length">The length of the substring in this instance. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startIndex" /> or <paramref name="length" /> is less than zero.-or- <paramref name="startIndex" /> plus <paramref name="length" /> is greater than the length of this instance. </exception>
	/// <filterpriority>1</filterpriority>
	public unsafe char[] ToCharArray(int startIndex, int length)
	{
		if (startIndex < 0 || startIndex > Length || startIndex > Length - length)
		{
			throw new ArgumentOutOfRangeException("startIndex", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (length <= 0)
		{
			if (length == 0)
			{
				return Array.Empty<char>();
			}
			throw new ArgumentOutOfRangeException("length", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		char[] array = new char[length];
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* dmem = &array[0])
			{
				wstrcpy(dmem, firstChar + startIndex, length);
			}
		}
		return array;
	}

	/// <summary>Indicates whether the specified string is null or an <see cref="F:System.String.Empty" /> string.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is null or an empty string (""); otherwise, false.</returns>
	/// <param name="value">The string to test. </param>
	/// <filterpriority>1</filterpriority>
	[NonVersionable]
	public static bool IsNullOrEmpty(string value)
	{
		if ((object)value != null && 0u < (uint)value.Length)
		{
			return false;
		}
		return true;
	}

	/// <summary>Indicates whether a specified string is null, empty, or consists only of white-space characters.</summary>
	/// <returns>true if the <paramref name="value" /> parameter is null or <see cref="F:System.String.Empty" />, or if <paramref name="value" /> consists exclusively of white-space characters. </returns>
	/// <param name="value">The string to test.</param>
	public static bool IsNullOrWhiteSpace(string value)
	{
		if ((object)value == null)
		{
			return true;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (!char.IsWhiteSpace(value[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal ref char GetRawStringData()
	{
		return ref _firstChar;
	}

	internal unsafe static string CreateStringFromEncoding(byte* bytes, int byteLength, Encoding encoding)
	{
		int charCount = encoding.GetCharCount(bytes, byteLength, null);
		if (charCount == 0)
		{
			return Empty;
		}
		string text = FastAllocateString(charCount);
		fixed (char* firstChar = &text._firstChar)
		{
			encoding.GetChars(bytes, byteLength, firstChar, charCount, null);
		}
		return text;
	}

	internal static string CreateFromChar(char c)
	{
		string text = FastAllocateString(1);
		text._firstChar = c;
		return text;
	}

	internal unsafe static void wstrcpy(char* dmem, char* smem, int charCount)
	{
		Buffer.Memmove((byte*)dmem, (byte*)smem, (uint)(charCount * 2));
	}

	/// <summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
	/// <returns>The current string.</returns>
	/// <filterpriority>1</filterpriority>
	public override string ToString()
	{
		return this;
	}

	/// <summary>Returns this instance of <see cref="T:System.String" />; no actual conversion is performed.</summary>
	/// <returns>The current string.</returns>
	/// <param name="provider">(Reserved) An object that supplies culture-specific formatting information. </param>
	/// <filterpriority>1</filterpriority>
	public string ToString(IFormatProvider provider)
	{
		return this;
	}

	/// <summary>Retrieves an object that can iterate through the individual characters in this string.</summary>
	/// <returns>An enumerator object.</returns>
	/// <filterpriority>2</filterpriority>
	public CharEnumerator GetEnumerator()
	{
		return new CharEnumerator(this);
	}

	IEnumerator<char> IEnumerable<char>.GetEnumerator()
	{
		return new CharEnumerator(this);
	}

	/// <summary>Returns an enumerator that iterates through the current <see cref="T:System.String" /> object. </summary>
	/// <returns>An enumerator that can be used to iterate through the current string.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new CharEnumerator(this);
	}

	internal unsafe static int wcslen(char* ptr)
	{
		char* ptr2 = ptr;
		int num = IntPtr.Size - 1;
		while (true)
		{
			if (((int)ptr2 & num) != 0)
			{
				if (*ptr2 == '\0')
				{
					break;
				}
				ptr2++;
				continue;
			}
			while (true)
			{
				if (((*(long*)ptr2 + 9223231297218904063L) | 0x7FFF7FFF7FFF7FFFL) == -1)
				{
					ptr2 += 4;
					continue;
				}
				if (*ptr2 == '\0')
				{
					break;
				}
				if (ptr2[1] != 0)
				{
					if (ptr2[2] != 0)
					{
						if (ptr2[3] != 0)
						{
							ptr2 += 4;
							continue;
						}
						ptr2++;
					}
					ptr2++;
				}
				ptr2++;
				break;
			}
			break;
		}
		int num2 = (int)(ptr2 - ptr);
		if (ptr + num2 != ptr2)
		{
			throw new ArgumentException("The string must be null-terminated.");
		}
		return num2;
	}

	/// <summary>Returns the <see cref="T:System.TypeCode" /> for class <see cref="T:System.String" />.</summary>
	/// <returns>The enumerated constant, <see cref="F:System.TypeCode.String" />.</returns>
	/// <filterpriority>2</filterpriority>
	public TypeCode GetTypeCode()
	{
		return TypeCode.String;
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToBoolean(System.IFormatProvider)" />. </summary>
	/// <returns>true if the value of the current string is <see cref="F:System.Boolean.TrueString" />; false if the value of the current string is <see cref="F:System.Boolean.FalseString" />.</returns>
	/// <param name="provider">This parameter is ignored.</param>
	/// <exception cref="T:System.FormatException">The value of the current string is not <see cref="F:System.Boolean.TrueString" /> or <see cref="F:System.Boolean.FalseString" />.</exception>
	bool IConvertible.ToBoolean(IFormatProvider provider)
	{
		return Convert.ToBoolean(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToChar(System.IFormatProvider)" />.</summary>
	/// <returns>The character at index 0 in the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	char IConvertible.ToChar(IFormatProvider provider)
	{
		return Convert.ToChar(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSByte(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.SByte.MaxValue" /> or less than <see cref="F:System.SByte.MinValue" />. </exception>
	sbyte IConvertible.ToSByte(IFormatProvider provider)
	{
		return Convert.ToSByte(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToByte(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.Byte.MaxValue" /> or less than <see cref="F:System.Byte.MinValue" />. </exception>
	byte IConvertible.ToByte(IFormatProvider provider)
	{
		return Convert.ToByte(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.Int16.MaxValue" /> or less than <see cref="F:System.Int16.MinValue" />.</exception>
	short IConvertible.ToInt16(IFormatProvider provider)
	{
		return Convert.ToInt16(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt16(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater than <see cref="F:System.UInt16.MaxValue" /> or less than <see cref="F:System.UInt16.MinValue" />.</exception>
	ushort IConvertible.ToUInt16(IFormatProvider provider)
	{
		return Convert.ToUInt16(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	int IConvertible.ToInt32(IFormatProvider provider)
	{
		return Convert.ToInt32(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt32(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number greater <see cref="F:System.UInt32.MaxValue" /> or less than <see cref="F:System.UInt32.MinValue" /></exception>
	uint IConvertible.ToUInt32(IFormatProvider provider)
	{
		return Convert.ToUInt32(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	long IConvertible.ToInt64(IFormatProvider provider)
	{
		return Convert.ToInt64(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToUInt64(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	ulong IConvertible.ToUInt64(IFormatProvider provider)
	{
		return Convert.ToUInt64(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToSingle(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	float IConvertible.ToSingle(IFormatProvider provider)
	{
		return Convert.ToSingle(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDouble(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number less than <see cref="F:System.Double.MinValue" /> or greater than <see cref="F:System.Double.MaxValue" />. </exception>
	double IConvertible.ToDouble(IFormatProvider provider)
	{
		return Convert.ToDouble(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDecimal(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	/// <exception cref="T:System.FormatException">The value of the current <see cref="T:System.String" /> object cannot be parsed. </exception>
	/// <exception cref="T:System.OverflowException">The value of the current <see cref="T:System.String" /> object is a number less than <see cref="F:System.Decimal.MinValue" /> or than <see cref="F:System.Decimal.MaxValue" /> greater. </exception>
	decimal IConvertible.ToDecimal(IFormatProvider provider)
	{
		return Convert.ToDecimal(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToDateTime(System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="provider">An object that provides culture-specific formatting information. </param>
	DateTime IConvertible.ToDateTime(IFormatProvider provider)
	{
		return Convert.ToDateTime(this, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IConvertible.ToType(System.Type,System.IFormatProvider)" />.</summary>
	/// <returns>The converted value of the current <see cref="T:System.String" /> object.</returns>
	/// <param name="type">The type of the returned object. </param>
	/// <param name="provider">An object that provides culture-specific formatting information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	/// <exception cref="T:System.InvalidCastException">The value of the current <see cref="T:System.String" /> object cannot be converted to the type specified by the <paramref name="type" /> parameter. </exception>
	object IConvertible.ToType(Type type, IFormatProvider provider)
	{
		return Convert.DefaultToType(this, type, provider);
	}

	/// <summary>Indicates whether this string is in Unicode normalization form C.</summary>
	/// <returns>true if this string is in normalization form C; otherwise, false.</returns>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public bool IsNormalized()
	{
		return IsNormalized(NormalizationForm.FormC);
	}

	/// <summary>Indicates whether this string is in the specified Unicode normalization form.</summary>
	/// <returns>true if this string is in the normalization form specified by the <paramref name="normalizationForm" /> parameter; otherwise, false.</returns>
	/// <param name="normalizationForm">A Unicode normalization form. </param>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public bool IsNormalized(NormalizationForm normalizationForm)
	{
		return Normalization.IsNormalized(this, normalizationForm);
	}

	/// <summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in Unicode normalization form C.</summary>
	/// <returns>A new, normalized string whose textual value is the same as this string, but whose binary representation is in normalization form C.</returns>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string Normalize()
	{
		return Normalize(NormalizationForm.FormC);
	}

	/// <summary>Returns a new string whose textual value is the same as this string, but whose binary representation is in the specified Unicode normalization form.</summary>
	/// <returns>A new string whose textual value is the same as this string, but whose binary representation is in the normalization form specified by the <paramref name="normalizationForm" /> parameter.</returns>
	/// <param name="normalizationForm">A Unicode normalization form. </param>
	/// <exception cref="T:System.ArgumentException">The current instance contains invalid Unicode characters.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string Normalize(NormalizationForm normalizationForm)
	{
		return Normalization.Normalize(this, normalizationForm);
	}

	internal unsafe int IndexOfUnchecked(string value, int startIndex, int count)
	{
		int length = value.Length;
		if (count < length)
		{
			return -1;
		}
		if (length == 0)
		{
			return startIndex;
		}
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* ptr = value)
			{
				char* ptr2 = firstChar + startIndex;
				for (char* ptr3 = ptr2 + count - length + 1; ptr2 != ptr3; ptr2++)
				{
					if (*ptr2 != *ptr)
					{
						continue;
					}
					int num = 1;
					while (true)
					{
						if (num < length)
						{
							if (ptr2[num] != ptr[num])
							{
								break;
							}
							num++;
							continue;
						}
						return (int)(ptr2 - firstChar);
					}
				}
			}
		}
		return -1;
	}

	[CLSCompliant(false)]
	public static string Concat(object arg0, object arg1, object arg2, object arg3, __arglist)
	{
		throw new PlatformNotSupportedException();
	}

	internal unsafe int IndexOfUncheckedIgnoreCase(string value, int startIndex, int count)
	{
		int length = value.Length;
		if (count < length)
		{
			return -1;
		}
		if (length == 0)
		{
			return startIndex;
		}
		TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* ptr = value)
			{
				char* ptr2 = firstChar + startIndex;
				char* ptr3 = ptr2 + count - length + 1;
				char c = textInfo.ToUpper(*ptr);
				for (; ptr2 != ptr3; ptr2++)
				{
					if (textInfo.ToUpper(*ptr2) != c)
					{
						continue;
					}
					int num = 1;
					while (true)
					{
						if (num < length)
						{
							if (textInfo.ToUpper(ptr2[num]) != textInfo.ToUpper(ptr[num]))
							{
								break;
							}
							num++;
							continue;
						}
						return (int)(ptr2 - firstChar);
					}
				}
			}
		}
		return -1;
	}

	internal unsafe int LastIndexOfUnchecked(string value, int startIndex, int count)
	{
		int length = value.Length;
		if (count < length)
		{
			return -1;
		}
		if (length == 0)
		{
			return startIndex;
		}
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* ptr = value)
			{
				char* ptr2 = firstChar + startIndex;
				char* ptr3 = ptr2 - count + length - 1;
				char* ptr4 = ptr + length - 1;
				while (ptr2 != ptr3)
				{
					if (*ptr2 == *ptr4)
					{
						char* ptr5 = ptr2;
						do
						{
							if (ptr != ptr4)
							{
								ptr4--;
								ptr2--;
								continue;
							}
							return (int)(ptr2 - firstChar);
						}
						while (*ptr2 == *ptr4);
						ptr4 = ptr + length - 1;
						ptr2 = ptr5;
					}
					ptr2--;
				}
			}
		}
		return -1;
	}

	internal unsafe int LastIndexOfUncheckedIgnoreCase(string value, int startIndex, int count)
	{
		int length = value.Length;
		if (count < length)
		{
			return -1;
		}
		if (length == 0)
		{
			return startIndex;
		}
		TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
		fixed (char* firstChar = &_firstChar)
		{
			fixed (char* ptr = value)
			{
				char* ptr2 = firstChar + startIndex;
				char* ptr3 = ptr2 - count + length - 1;
				char* ptr4 = ptr + length - 1;
				char c = textInfo.ToUpper(*ptr4);
				while (ptr2 != ptr3)
				{
					if (textInfo.ToUpper(*ptr2) == c)
					{
						char* ptr5 = ptr2;
						do
						{
							if (ptr != ptr4)
							{
								ptr4--;
								ptr2--;
								continue;
							}
							return (int)(ptr2 - firstChar);
						}
						while (textInfo.ToUpper(*ptr2) == textInfo.ToUpper(*ptr4));
						ptr4 = ptr + length - 1;
						ptr2 = ptr5;
					}
					ptr2--;
				}
			}
		}
		return -1;
	}

	internal bool StartsWithOrdinalUnchecked(string value)
	{
		if (Length < value.Length || _firstChar != value._firstChar)
		{
			return false;
		}
		if (value.Length != 1)
		{
			return SpanHelpers.SequenceEqual(ref Unsafe.As<char, byte>(ref GetRawStringData()), ref Unsafe.As<char, byte>(ref value.GetRawStringData()), (ulong)value.Length * 2uL);
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string FastAllocateString(int length);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string InternalIsInterned(string str);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string InternalIntern(string str);

	private unsafe static int FastCompareStringHelper(uint* strAChars, int countA, uint* strBChars, int countB)
	{
		char* ptr = (char*)strAChars;
		char* ptr2 = (char*)strBChars;
		char* ptr3 = ptr + Math.Min(countA, countB);
		while (ptr < ptr3)
		{
			if (*ptr != *ptr2)
			{
				return *ptr - *ptr2;
			}
			ptr++;
			ptr2++;
		}
		return countA - countB;
	}

	private unsafe static void memset(byte* dest, int val, int len)
	{
		if (len < 8)
		{
			while (len != 0)
			{
				*dest = (byte)val;
				dest++;
				len--;
			}
			return;
		}
		if (val != 0)
		{
			val |= val << 8;
			val |= val << 16;
		}
		int num = (int)dest & 3;
		if (num != 0)
		{
			num = 4 - num;
			len -= num;
			do
			{
				*dest = (byte)val;
				dest++;
				num--;
			}
			while (num != 0);
		}
		while (len >= 16)
		{
			*(int*)dest = val;
			((int*)dest)[1] = val;
			((int*)dest)[2] = val;
			((int*)dest)[3] = val;
			dest += 16;
			len -= 16;
		}
		while (len >= 4)
		{
			*(int*)dest = val;
			dest += 4;
			len -= 4;
		}
		while (len > 0)
		{
			*dest = (byte)val;
			dest++;
			len--;
		}
	}

	private unsafe static void memcpy(byte* dest, byte* src, int size)
	{
		Buffer.Memcpy(dest, src, size);
	}

	internal unsafe static void bzero(byte* dest, int len)
	{
		memset(dest, 0, len);
	}

	internal unsafe static void bzero_aligned_1(byte* dest, int len)
	{
		*dest = 0;
	}

	internal unsafe static void bzero_aligned_2(byte* dest, int len)
	{
		*(short*)dest = 0;
	}

	internal unsafe static void bzero_aligned_4(byte* dest, int len)
	{
		*(int*)dest = 0;
	}

	internal unsafe static void bzero_aligned_8(byte* dest, int len)
	{
		*(long*)dest = 0L;
	}

	internal unsafe static void memcpy_aligned_1(byte* dest, byte* src, int size)
	{
		*dest = *src;
	}

	internal unsafe static void memcpy_aligned_2(byte* dest, byte* src, int size)
	{
		*(short*)dest = *(short*)src;
	}

	internal unsafe static void memcpy_aligned_4(byte* dest, byte* src, int size)
	{
		*(int*)dest = *(int*)src;
	}

	internal unsafe static void memcpy_aligned_8(byte* dest, byte* src, int size)
	{
		*(long*)dest = *(long*)src;
	}

	private unsafe string CreateString(sbyte* value)
	{
		return Ctor(value);
	}

	private unsafe string CreateString(sbyte* value, int startIndex, int length)
	{
		return Ctor(value, startIndex, length);
	}

	private unsafe string CreateString(char* value)
	{
		return Ctor(value);
	}

	private unsafe string CreateString(char* value, int startIndex, int length)
	{
		return Ctor(value, startIndex, length);
	}

	private string CreateString(char[] val, int startIndex, int length)
	{
		return Ctor(val, startIndex, length);
	}

	private string CreateString(char[] val)
	{
		return Ctor(val);
	}

	private string CreateString(char c, int count)
	{
		return Ctor(c, count);
	}

	private unsafe string CreateString(sbyte* value, int startIndex, int length, Encoding enc)
	{
		return Ctor(value, startIndex, length, enc);
	}

	private string CreateString(ReadOnlySpan<char> value)
	{
		return Ctor(value);
	}

	/// <summary>Retrieves the system's reference to the specified <see cref="T:System.String" />.</summary>
	/// <returns>The system's reference to <paramref name="str" />, if it is interned; otherwise, a new reference to a string with the value of <paramref name="str" />.</returns>
	/// <param name="str">A string to search for in the intern pool. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public static string Intern(string str)
	{
		if ((object)str == null)
		{
			throw new ArgumentNullException("str");
		}
		return InternalIntern(str);
	}

	/// <summary>Retrieves a reference to a specified <see cref="T:System.String" />.</summary>
	/// <returns>A reference to <paramref name="str" /> if it is in the common language runtime intern pool; otherwise, null.</returns>
	/// <param name="str">The string to search for in the intern pool. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="str" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public static string IsInterned(string str)
	{
		if ((object)str == null)
		{
			throw new ArgumentNullException("str");
		}
		return InternalIsInterned(str);
	}

	private unsafe int LegacyStringGetHashCode()
	{
		int num = 5381;
		int num2 = num;
		fixed (char* ptr = this)
		{
			char* ptr2 = ptr;
			int num3;
			while ((num3 = *ptr2) != 0)
			{
				num = ((num << 5) + num) ^ num3;
				num3 = ptr2[1];
				if (num3 == 0)
				{
					break;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
				ptr2 += 2;
			}
		}
		return num + num2 * 1566083941;
	}
}
