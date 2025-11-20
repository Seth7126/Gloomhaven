namespace UdpKit.Security;

public interface IPacketHashValidator
{
	int AppendHashToData(byte[] buffer, int length);

	bool ValidatePacket(byte[] buffer, int length);

	int GetLengthWithoutHash(byte[] buffer, int length);
}
