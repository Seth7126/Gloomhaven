using System.Globalization;
using System.Runtime.Serialization;
using System.Security;

namespace System.Collections;

/// <summary>Compares two objects for equivalence, where string comparisons are case-sensitive.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class Comparer : IComparer, ISerializable
{
	private CompareInfo _compareInfo;

	/// <summary>Represents an instance of <see cref="T:System.Collections.Comparer" /> that is associated with the <see cref="P:System.Threading.Thread.CurrentCulture" /> of the current thread. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Comparer Default = new Comparer(CultureInfo.CurrentCulture);

	/// <summary>Represents an instance of <see cref="T:System.Collections.Comparer" /> that is associated with <see cref="P:System.Globalization.CultureInfo.InvariantCulture" />. This field is read-only.</summary>
	/// <filterpriority>1</filterpriority>
	public static readonly Comparer DefaultInvariant = new Comparer(CultureInfo.InvariantCulture);

	/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Comparer" /> class using the specified <see cref="T:System.Globalization.CultureInfo" />.</summary>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use for the new <see cref="T:System.Collections.Comparer" />. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null. </exception>
	public Comparer(CultureInfo culture)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		_compareInfo = culture.CompareInfo;
	}

	private Comparer(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		_compareInfo = (CompareInfo)info.GetValue("CompareInfo", typeof(CompareInfo));
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the data required for serialization.  </summary>
	/// <param name="info">The object to populate with data.</param>
	/// <param name="context">The context information about the source or destination of the serialization.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	[SecurityCritical]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("CompareInfo", _compareInfo);
	}

	/// <summary>Performs a case-sensitive comparison of two objects of the same type and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
	/// <returns>A signed integer that indicates the relative values of <paramref name="a" /> and <paramref name="b" />, as shown in the following table.Value Meaning Less than zero <paramref name="a" /> is less than <paramref name="b" />. Zero <paramref name="a" /> equals <paramref name="b" />. Greater than zero <paramref name="a" /> is greater than <paramref name="b" />. </returns>
	/// <param name="a">The first object to compare. </param>
	/// <param name="b">The second object to compare. </param>
	/// <exception cref="T:System.ArgumentException">Neither <paramref name="a" /> nor <paramref name="b" /> implements the <see cref="T:System.IComparable" /> interface.-or- <paramref name="a" /> and <paramref name="b" /> are of different types and neither one can handle comparisons with the other. </exception>
	/// <filterpriority>2</filterpriority>
	public int Compare(object a, object b)
	{
		if (a == b)
		{
			return 0;
		}
		if (a == null)
		{
			return -1;
		}
		if (b == null)
		{
			return 1;
		}
		string text = a as string;
		string text2 = b as string;
		if (text != null && text2 != null)
		{
			return _compareInfo.Compare(text, text2);
		}
		if (a is IComparable comparable)
		{
			return comparable.CompareTo(b);
		}
		if (b is IComparable comparable2)
		{
			return -comparable2.CompareTo(a);
		}
		throw new ArgumentException("At least one object must implement IComparable.");
	}
}
