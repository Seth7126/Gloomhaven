using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class BlockedUsersStateChangedEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static BlockedUsersStateChangedEvent_Meta Instance;

	internal ObjectPool<BlockedUsersStateChangedEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(123, 45, 78, 71, 194, 199, 37, 71, 165, 122, 14, 230, 135, 177, 152, 95);

	Type IFactory.TypeObject => typeof(BlockedUsersStateChangedEvent);

	static BlockedUsersStateChangedEvent_Meta()
	{
		Instance = new BlockedUsersStateChangedEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(14);
		CountStorage = 1;
		CountObjects = 1;
		CountProperties = 1;
		Properties = new NetworkPropertyInfo[1];
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken.PropertyMeta = this;
		networkProperty_ProtocolToken.Settings_Property("Data", 1, -1073741824);
		networkProperty_ProtocolToken.Settings_Offsets(0, 0);
		AddProperty(0, 0, networkProperty_ProtocolToken, -1);
		base.InitMeta();
		_pool = new ObjectPool<BlockedUsersStateChangedEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as BlockedUsersStateChangedEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IBlockedUsersStateChangedEventListener blockedUsersStateChangedEventListener)
		{
			blockedUsersStateChangedEventListener.OnEvent((BlockedUsersStateChangedEvent)ev);
		}
	}
}
