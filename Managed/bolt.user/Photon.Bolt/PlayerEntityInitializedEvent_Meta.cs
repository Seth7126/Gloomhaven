using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class PlayerEntityInitializedEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static PlayerEntityInitializedEvent_Meta Instance;

	internal ObjectPool<PlayerEntityInitializedEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(235, 47, 74, 216, 170, 173, 121, 72, 185, 15, 182, 218, 114, 92, 98, 49);

	Type IFactory.TypeObject => typeof(PlayerEntityInitializedEvent);

	static PlayerEntityInitializedEvent_Meta()
	{
		Instance = new PlayerEntityInitializedEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(10);
		CountStorage = 0;
		CountObjects = 1;
		CountProperties = 0;
		Properties = new NetworkPropertyInfo[0];
		base.InitMeta();
		_pool = new ObjectPool<PlayerEntityInitializedEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as PlayerEntityInitializedEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IPlayerEntityInitializedEventListener playerEntityInitializedEventListener)
		{
			playerEntityInitializedEventListener.OnEvent((PlayerEntityInitializedEvent)ev);
		}
	}
}
