namespace System.Collections;

/// <summary>Exposes an enumerator, which supports a simple iteration over a non-generic collection.</summary>
/// <filterpriority>1</filterpriority>
public interface IEnumerable
{
	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
	/// <filterpriority>2</filterpriority>
	IEnumerator GetEnumerator();
}
