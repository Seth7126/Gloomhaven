using Unity;

namespace System.Data.SqlClient;

public abstract class SqlColumnEncryptionKeyStoreProvider
{
	protected SqlColumnEncryptionKeyStoreProvider()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	public abstract byte[] DecryptColumnEncryptionKey(string masterKeyPath, string encryptionAlgorithm, byte[] encryptedColumnEncryptionKey);

	public abstract byte[] EncryptColumnEncryptionKey(string masterKeyPath, string encryptionAlgorithm, byte[] columnEncryptionKey);

	public virtual byte[] SignColumnMasterKeyMetadata(string masterKeyPath, bool allowEnclaveComputations)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}

	public virtual bool VerifyColumnMasterKeyMetadata(string masterKeyPath, bool allowEnclaveComputations, byte[] signature)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}
}
