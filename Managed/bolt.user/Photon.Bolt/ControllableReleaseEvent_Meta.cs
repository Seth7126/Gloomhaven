using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class ControllableReleaseEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static ControllableReleaseEvent_Meta Instance;

	internal ObjectPool<ControllableReleaseEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(7, 138, 41, 159, 236, 142, 109, 67, 178, 214, 193, 8, 112, 65, 110, 142);

	Type IFactory.TypeObject => typeof(ControllableReleaseEvent);

	static ControllableReleaseEvent_Meta()
	{
		Instance = new ControllableReleaseEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(4);
		CountStorage = 2;
		CountObjects = 1;
		CountProperties = 2;
		Properties = new NetworkPropertyInfo[2];
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("PlayerID", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(0, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create(8, 0));
		AddProperty(0, 0, networkProperty_Integer, -1);
		NetworkProperty_Integer networkProperty_Integer2 = new NetworkProperty_Integer();
		networkProperty_Integer2.PropertyMeta = this;
		networkProperty_Integer2.Settings_Property("ControllableID", 1, -1073741824);
		networkProperty_Integer2.Settings_Offsets(1, 1);
		networkProperty_Integer2.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(1, 0, networkProperty_Integer2, -1);
		base.InitMeta();
		_pool = new ObjectPool<ControllableReleaseEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as ControllableReleaseEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IControllableReleaseEventListener controllableReleaseEventListener)
		{
			controllableReleaseEventListener.OnEvent((ControllableReleaseEvent)ev);
		}
	}
}
