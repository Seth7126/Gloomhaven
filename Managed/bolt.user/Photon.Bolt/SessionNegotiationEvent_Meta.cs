using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class SessionNegotiationEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static SessionNegotiationEvent_Meta Instance;

	internal ObjectPool<SessionNegotiationEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(177, 83, 112, 198, 216, 164, 214, 76, 167, 176, 104, 177, 167, 208, 186, 76);

	Type IFactory.TypeObject => typeof(SessionNegotiationEvent);

	static SessionNegotiationEvent_Meta()
	{
		Instance = new SessionNegotiationEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(13);
		CountStorage = 3;
		CountObjects = 1;
		CountProperties = 3;
		Properties = new NetworkPropertyInfo[3];
		NetworkProperty_String networkProperty_String = new NetworkProperty_String();
		networkProperty_String.PropertyMeta = this;
		networkProperty_String.Settings_Property("Data", 1, -1073741824);
		networkProperty_String.Settings_Offsets(0, 0);
		networkProperty_String.AddStringSettings(StringEncodings.UTF8);
		AddProperty(0, 0, networkProperty_String, -1);
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("MessageType", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(1, 1);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(1, 0, networkProperty_Integer, -1);
		NetworkProperty_Integer networkProperty_Integer2 = new NetworkProperty_Integer();
		networkProperty_Integer2.PropertyMeta = this;
		networkProperty_Integer2.Settings_Property("Platform", 1, -1073741824);
		networkProperty_Integer2.Settings_Offsets(2, 2);
		networkProperty_Integer2.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(2, 0, networkProperty_Integer2, -1);
		base.InitMeta();
		_pool = new ObjectPool<SessionNegotiationEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as SessionNegotiationEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is ISessionNegotiationEventListener sessionNegotiationEventListener)
		{
			sessionNegotiationEventListener.OnEvent((SessionNegotiationEvent)ev);
		}
	}
}
