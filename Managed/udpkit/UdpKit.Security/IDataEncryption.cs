namespace UdpKit.Security;

public interface IDataEncryption
{
	int DecryptData(byte[] buffer, int length);

	byte[] DecryptDataAlloc(byte[] buffer, int length);

	int EncryptData(byte[] buffer, int length);

	byte[] EncryptDataAlloc(byte[] buffer, int length);
}
