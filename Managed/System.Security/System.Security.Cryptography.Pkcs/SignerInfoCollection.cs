using System.Collections;
using System.Security.Cryptography.Pkcs.Asn1;
using System.Security.Cryptography.Xml;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> class represents a collection of <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> objects. <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> implements the <see cref="T:System.Collections.ICollection" /> interface. </summary>
public sealed class SignerInfoCollection : ICollection, IEnumerable
{
	private readonly SignerInfo[] _signerInfos;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.Item(System.Int32)" /> property retrieves the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object at the specified index in the collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> object  at the specified index.</returns>
	/// <param name="index">An int value that represents the index in the collection. The index is zero based.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public SignerInfo this[int index]
	{
		get
		{
			if (index < 0 || index >= _signerInfos.Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return _signerInfos[index];
		}
	}

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.Count" /> property retrieves the number of items in the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>An int value that represents the number of items in the collection.</returns>
	public int Count => _signerInfos.Length;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.IsSynchronized" /> property retrieves whether access to the collection is synchronized, or thread safe. This property always returns false, which means the collection is not thread safe.</summary>
	/// <returns>A <see cref="T:System.Boolean" /> value of false, which means the collection is not thread safe.</returns>
	public bool IsSynchronized => false;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.SignerInfoCollection.SyncRoot" /> property retrieves an <see cref="T:System.Object" /> object is used to synchronize access to the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>An <see cref="T:System.Object" /> object is used to synchronize access to the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</returns>
	public object SyncRoot => this;

	internal SignerInfoCollection()
	{
		_signerInfos = Array.Empty<SignerInfo>();
	}

	internal SignerInfoCollection(SignerInfo[] signerInfos)
	{
		_signerInfos = signerInfos;
	}

	internal SignerInfoCollection(SignerInfoAsn[] signedDataSignerInfos, SignedCms ownerDocument)
	{
		_signerInfos = new SignerInfo[signedDataSignerInfos.Length];
		for (int i = 0; i < signedDataSignerInfos.Length; i++)
		{
			_signerInfos[i] = new SignerInfo(ref signedDataSignerInfos[i], ownerDocument);
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.GetEnumerator" /> method returns a <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object for the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object that can be used to enumerate the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</returns>
	public SignerInfoEnumerator GetEnumerator()
	{
		return new SignerInfoEnumerator(this);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.System#Collections#IEnumerable#GetEnumerator" /> method returns a <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object for the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</summary>
	/// <returns>A <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoEnumerator" /> object that can be used to enumerate the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new SignerInfoEnumerator(this);
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.CopyTo(System.Array,System.Int32)" /> method copies the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection to an array.</summary>
	/// <param name="array">An <see cref="T:System.Array" /> object to which the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is to be copied.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> where the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is copied.</param>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public void CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException("Only single dimensional arrays are supported for the requested action.", "array");
		}
		if (index < 0 || index >= array.Length)
		{
			throw new ArgumentOutOfRangeException("index", "Index was out of range. Must be non-negative and less than the size of the collection.");
		}
		if (index + Count > array.Length)
		{
			throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		}
		for (int i = 0; i < Count; i++)
		{
			array.SetValue(this[i], index + i);
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.SignerInfoCollection.CopyTo(System.Security.Cryptography.Pkcs.SignerInfo[],System.Int32)" /> method copies the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection to a <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> array.</summary>
	/// <param name="array">An array of <see cref="T:System.Security.Cryptography.Pkcs.SignerInfo" /> objects where the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is to be copied.</param>
	/// <param name="index">The zero-based index in <paramref name="array" /> where the <see cref="T:System.Security.Cryptography.Pkcs.SignerInfoCollection" /> collection is copied.</param>
	/// <exception cref="T:System.ArgumentException">One of the arguments provided to a method was not valid.</exception>
	/// <exception cref="T:System.ArgumentNullException">A null reference was passed to a method that does not accept it as a valid argument. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of an argument was outside the allowable range of values as defined by the called method.</exception>
	public void CopyTo(SignerInfo[] array, int index)
	{
		((ICollection)this).CopyTo((Array)array, index);
	}

	internal int FindIndexForSigner(SignerInfo signer)
	{
		SubjectIdentifier signerIdentifier = signer.SignerIdentifier;
		X509IssuerSerial x509IssuerSerial = default(X509IssuerSerial);
		if (signerIdentifier.Type == SubjectIdentifierType.IssuerAndSerialNumber)
		{
			x509IssuerSerial = (X509IssuerSerial)signerIdentifier.Value;
		}
		for (int i = 0; i < _signerInfos.Length; i++)
		{
			SubjectIdentifier signerIdentifier2 = _signerInfos[i].SignerIdentifier;
			if (signerIdentifier2.Type != signerIdentifier.Type)
			{
				continue;
			}
			bool flag = false;
			switch (signerIdentifier.Type)
			{
			case SubjectIdentifierType.IssuerAndSerialNumber:
			{
				X509IssuerSerial x509IssuerSerial2 = (X509IssuerSerial)signerIdentifier2.Value;
				if (x509IssuerSerial2.IssuerName == x509IssuerSerial.IssuerName && x509IssuerSerial2.SerialNumber == x509IssuerSerial.SerialNumber)
				{
					flag = true;
				}
				break;
			}
			case SubjectIdentifierType.SubjectKeyIdentifier:
				if ((string)signerIdentifier.Value == (string)signerIdentifier2.Value)
				{
					flag = true;
				}
				break;
			case SubjectIdentifierType.NoSignature:
				flag = true;
				break;
			default:
				throw new CryptographicException();
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}
}
