namespace Epic.OnlineServices.P2P;

public struct GetNextReceivedPacketSizeOptions
{
	public ProductUserId LocalUserId { get; set; }

	public byte? RequestedChannel { get; set; }
}
