namespace Epic.OnlineServices.P2P;

public struct ReceivePacketOptions
{
	public ProductUserId LocalUserId { get; set; }

	public uint MaxDataSizeBytes { get; set; }

	public byte? RequestedChannel { get; set; }
}
