using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class NetworkActionEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static NetworkActionEvent_Meta Instance;

	internal ObjectPool<NetworkActionEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(114, 65, 65, 97, 95, 62, 249, 71, 176, 35, 1, 237, 112, 179, 17, 149);

	Type IFactory.TypeObject => typeof(NetworkActionEvent);

	static NetworkActionEvent_Meta()
	{
		Instance = new NetworkActionEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(9);
		CountStorage = 1;
		CountObjects = 1;
		CountProperties = 1;
		Properties = new NetworkPropertyInfo[1];
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken.PropertyMeta = this;
		networkProperty_ProtocolToken.Settings_Property("Token", 1, -1073741824);
		networkProperty_ProtocolToken.Settings_Offsets(0, 0);
		AddProperty(0, 0, networkProperty_ProtocolToken, -1);
		base.InitMeta();
		_pool = new ObjectPool<NetworkActionEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as NetworkActionEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is INetworkActionEventListener networkActionEventListener)
		{
			networkActionEventListener.OnEvent((NetworkActionEvent)ev);
		}
	}
}
