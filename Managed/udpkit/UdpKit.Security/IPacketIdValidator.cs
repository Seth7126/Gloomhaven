namespace UdpKit.Security;

public interface IPacketIdValidator
{
	int PrefixPacketId(UdpEndPoint endPoint, byte[] buffer, int length);

	int ValidatePacketId(UdpEndPoint endPoint, byte[] buffer, int length);

	void RemoveEndPointReference(UdpEndPoint endPoint);
}
