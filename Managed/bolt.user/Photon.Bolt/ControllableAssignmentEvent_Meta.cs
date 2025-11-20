using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class ControllableAssignmentEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static ControllableAssignmentEvent_Meta Instance;

	internal ObjectPool<ControllableAssignmentEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(51, 49, 222, 80, 119, 46, 134, 67, 134, 107, 201, 70, 186, 122, 17, 58);

	Type IFactory.TypeObject => typeof(ControllableAssignmentEvent);

	static ControllableAssignmentEvent_Meta()
	{
		Instance = new ControllableAssignmentEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(1);
		CountStorage = 3;
		CountObjects = 1;
		CountProperties = 3;
		Properties = new NetworkPropertyInfo[3];
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
		NetworkProperty_Bool networkProperty_Bool = new NetworkProperty_Bool();
		networkProperty_Bool.PropertyMeta = this;
		networkProperty_Bool.Settings_Property("ReleaseFirst", 1, -1073741824);
		networkProperty_Bool.Settings_Offsets(2, 2);
		AddProperty(2, 0, networkProperty_Bool, -1);
		base.InitMeta();
		_pool = new ObjectPool<ControllableAssignmentEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as ControllableAssignmentEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IControllableAssignmentEventListener controllableAssignmentEventListener)
		{
			controllableAssignmentEventListener.OnEvent((ControllableAssignmentEvent)ev);
		}
	}
}
