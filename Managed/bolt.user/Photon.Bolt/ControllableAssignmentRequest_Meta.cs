using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class ControllableAssignmentRequest_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static ControllableAssignmentRequest_Meta Instance;

	internal ObjectPool<ControllableAssignmentRequest> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(164, 161, 171, 26, 93, 134, 213, 64, 164, 7, 197, 207, 45, 232, 229, 46);

	Type IFactory.TypeObject => typeof(ControllableAssignmentRequest);

	static ControllableAssignmentRequest_Meta()
	{
		Instance = new ControllableAssignmentRequest_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(2);
		CountStorage = 0;
		CountObjects = 1;
		CountProperties = 0;
		Properties = new NetworkPropertyInfo[0];
		base.InitMeta();
		_pool = new ObjectPool<ControllableAssignmentRequest>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as ControllableAssignmentRequest);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IControllableAssignmentRequestListener controllableAssignmentRequestListener)
		{
			controllableAssignmentRequestListener.OnEvent((ControllableAssignmentRequest)ev);
		}
	}
}
