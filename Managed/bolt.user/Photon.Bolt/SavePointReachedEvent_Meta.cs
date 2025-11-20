using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class SavePointReachedEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static SavePointReachedEvent_Meta Instance;

	internal ObjectPool<SavePointReachedEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(157, 188, 44, 107, 71, 36, 130, 65, 151, 67, 37, 207, 82, 149, 49, 37);

	Type IFactory.TypeObject => typeof(SavePointReachedEvent);

	static SavePointReachedEvent_Meta()
	{
		Instance = new SavePointReachedEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(12);
		CountStorage = 0;
		CountObjects = 1;
		CountProperties = 0;
		Properties = new NetworkPropertyInfo[0];
		base.InitMeta();
		_pool = new ObjectPool<SavePointReachedEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as SavePointReachedEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is ISavePointReachedEventListener savePointReachedEventListener)
		{
			savePointReachedEventListener.OnEvent((SavePointReachedEvent)ev);
		}
	}
}
