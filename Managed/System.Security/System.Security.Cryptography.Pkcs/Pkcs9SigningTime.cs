using System.Threading;
using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs;

/// <summary>The <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class defines the signing date and time of a signature. A <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> object can  be used as an authenticated attribute of a <see cref="T:System.Security.Cryptography.Pkcs.CmsSigner" />  object when an authenticated date and time are to accompany a digital signature.</summary>
public sealed class Pkcs9SigningTime : Pkcs9AttributeObject
{
	private DateTime? _lazySigningTime;

	/// <summary>The <see cref="P:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.SigningTime" /> property retrieves a <see cref="T:System.DateTime" /> structure that represents the date and time that the message was signed.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> structure that contains the date and time the document was signed.</returns>
	public DateTime SigningTime
	{
		get
		{
			if (!_lazySigningTime.HasValue)
			{
				_lazySigningTime = Decode(base.RawData);
				Interlocked.MemoryBarrier();
			}
			return _lazySigningTime.Value;
		}
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.#ctor" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class.</summary>
	public Pkcs9SigningTime()
		: this(DateTime.Now)
	{
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.#ctor(System.DateTime)" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class by using the specified signing date and time.</summary>
	/// <param name="signingTime">A <see cref="T:System.DateTime" />  structure that represents the signing date and time of the signature.</param>
	public Pkcs9SigningTime(DateTime signingTime)
		: base("1.2.840.113549.1.9.5", Encode(signingTime))
	{
		_lazySigningTime = signingTime;
	}

	/// <summary>The <see cref="M:System.Security.Cryptography.Pkcs.Pkcs9SigningTime.#ctor(System.Byte[])" /> constructor creates an instance of the <see cref="T:System.Security.Cryptography.Pkcs.Pkcs9SigningTime" /> class by using the specified array of byte values as the encoded signing date and time of the content of a CMS/PKCS #7 message.</summary>
	/// <param name="encodedSigningTime">An array of byte values that specifies the encoded signing date and time of the CMS/PKCS #7 message.</param>
	public Pkcs9SigningTime(byte[] encodedSigningTime)
		: base("1.2.840.113549.1.9.5", encodedSigningTime)
	{
	}

	/// <summary>Copies information from a <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object.</summary>
	/// <param name="asnEncodedData">The <see cref="T:System.Security.Cryptography.AsnEncodedData" /> object from which to copy information.</param>
	public override void CopyFrom(AsnEncodedData asnEncodedData)
	{
		base.CopyFrom(asnEncodedData);
		_lazySigningTime = null;
	}

	private static DateTime Decode(byte[] rawData)
	{
		if (rawData == null)
		{
			return default(DateTime);
		}
		return PkcsPal.Instance.DecodeUtcTime(rawData);
	}

	private static byte[] Encode(DateTime signingTime)
	{
		return PkcsPal.Instance.EncodeUtcTime(signingTime);
	}
}
