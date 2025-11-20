namespace Photon.Bolt;

internal abstract class NetworkState_Meta : NetworkObj_Meta
{
	public int PropertyIdBits;

	public int PacketMaxBits;

	public int PacketMaxProperties;

	public int PacketMaxPropertiesBits;

	public PropertyVectorCompressionSettings InstantiationPositionCompression;

	public PropertyQuaternionCompression InstantiationRotationCompression;
}
