using System.Security.Cryptography.Pkcs;

namespace Internal.Cryptography;

internal abstract class RecipientInfoPal
{
	public abstract byte[] EncryptedKey { get; }

	public abstract AlgorithmIdentifier KeyEncryptionAlgorithm { get; }

	public abstract SubjectIdentifier RecipientIdentifier { get; }

	public abstract int Version { get; }

	internal RecipientInfoPal()
	{
	}
}
