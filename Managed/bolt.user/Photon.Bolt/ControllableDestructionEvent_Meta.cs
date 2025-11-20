using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class ControllableDestructionEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static ControllableDestructionEvent_Meta Instance;

	internal ObjectPool<ControllableDestructionEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(4, 143, 162, 128, 74, 4, 196, 79, 135, 230, 127, 234, 81, 150, 252, 248);

	Type IFactory.TypeObject => typeof(ControllableDestructionEvent);

	static ControllableDestructionEvent_Meta()
	{
		Instance = new ControllableDestructionEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(3);
		CountStorage = 1;
		CountObjects = 1;
		CountProperties = 1;
		Properties = new NetworkPropertyInfo[1];
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("ControllableID", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(0, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(0, 0, networkProperty_Integer, -1);
		base.InitMeta();
		_pool = new ObjectPool<ControllableDestructionEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as ControllableDestructionEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IControllableDestructionEventListener controllableDestructionEventListener)
		{
			controllableDestructionEventListener.OnEvent((ControllableDestructionEvent)ev);
		}
	}
}
