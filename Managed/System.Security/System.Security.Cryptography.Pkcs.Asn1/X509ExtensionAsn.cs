using System.Security.Cryptography.Asn1;
using System.Security.Cryptography.X509Certificates;
using Internal.Cryptography;

namespace System.Security.Cryptography.Pkcs.Asn1;

internal struct X509ExtensionAsn
{
	[ObjectIdentifier]
	internal string ExtnId;

	[DefaultValue(new byte[] { 1, 1, 0 })]
	internal bool Critical;

	[OctetString]
	internal ReadOnlyMemory<byte> ExtnValue;

	public X509ExtensionAsn(X509Extension extension, bool copyValue = true)
	{
		if (extension == null)
		{
			throw new ArgumentNullException("extension");
		}
		ExtnId = extension.Oid.Value;
		Critical = extension.Critical;
		ExtnValue = (copyValue ? extension.RawData.CloneByteArray() : extension.RawData);
	}
}
