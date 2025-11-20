using Photon.Bolt.Internal;
using UdpKit;

namespace Photon.Bolt;

internal interface IEntitySerializer
{
	TypeId TypeId { get; }

	void OnRender();

	void OnInitialized();

	void OnCreated(Entity entity);

	void OnParentChanging(Entity newParent, Entity oldParent);

	void OnSimulateBefore();

	void OnSimulateAfter();

	void OnControlGained();

	void OnControlLost();

	BitSet GetDefaultMask();

	BitSet GetFilter(BoltConnection connection, EntityProxy proxy);

	void DebugInfo();

	void InitProxy(EntityProxy p);

	int Pack(BoltConnection connection, UdpPacket stream, EntityProxyEnvelope proxy);

	void Read(BoltConnection connection, UdpPacket stream, int frame);
}
internal interface IEntitySerializer<TState> : IEntitySerializer where TState : IState
{
	TState state { get; }
}
