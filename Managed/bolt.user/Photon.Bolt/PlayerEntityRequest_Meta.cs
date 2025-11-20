using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class PlayerEntityRequest_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static PlayerEntityRequest_Meta Instance;

	internal ObjectPool<PlayerEntityRequest> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(15, 184, 145, 99, 246, 94, 228, 66, 147, 178, 84, 25, 63, 38, 170, 149);

	Type IFactory.TypeObject => typeof(PlayerEntityRequest);

	static PlayerEntityRequest_Meta()
	{
		Instance = new PlayerEntityRequest_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(11);
		CountStorage = 0;
		CountObjects = 1;
		CountProperties = 0;
		Properties = new NetworkPropertyInfo[0];
		base.InitMeta();
		_pool = new ObjectPool<PlayerEntityRequest>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as PlayerEntityRequest);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IPlayerEntityRequestListener playerEntityRequestListener)
		{
			playerEntityRequestListener.OnEvent((PlayerEntityRequest)ev);
		}
	}
}
