using System.Collections;
using Unity;

namespace System.Security.Cryptography;

/// <summary>Provides enumeration functionality for the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection. This class cannot be inherited. </summary>
public sealed class CryptographicAttributeObjectEnumerator : IEnumerator
{
	private readonly CryptographicAttributeObjectCollection _attributes;

	private int _current;

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object from the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object that represents the current cryptographic attribute in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	public CryptographicAttributeObject Current => _attributes[_current];

	/// <summary>Gets the current <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object from the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object that represents the current cryptographic attribute in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</returns>
	object IEnumerator.Current => _attributes[_current];

	internal CryptographicAttributeObjectEnumerator(CryptographicAttributeObjectCollection attributes)
	{
		_attributes = attributes;
		_current = -1;
	}

	/// <summary>Advances the enumeration to the next <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	/// <returns>true if the enumeration successfully moved to the next <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object; false if the enumerator is at the end of the enumeration.</returns>
	public bool MoveNext()
	{
		if (_current >= _attributes.Count - 1)
		{
			return false;
		}
		_current++;
		return true;
	}

	/// <summary>Resets the enumeration to the first <see cref="T:System.Security.Cryptography.CryptographicAttributeObject" /> object in the <see cref="T:System.Security.Cryptography.CryptographicAttributeObjectCollection" /> collection.</summary>
	public void Reset()
	{
		_current = -1;
	}

	internal CryptographicAttributeObjectEnumerator()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
