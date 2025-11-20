using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class LogNetworkMessageEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static LogNetworkMessageEvent_Meta Instance;

	internal ObjectPool<LogNetworkMessageEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(104, 137, 115, 132, 145, 185, 107, 65, 145, 238, 44, 206, 142, 54, 236, 41);

	Type IFactory.TypeObject => typeof(LogNetworkMessageEvent);

	static LogNetworkMessageEvent_Meta()
	{
		Instance = new LogNetworkMessageEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(15);
		CountStorage = 1;
		CountObjects = 1;
		CountProperties = 1;
		Properties = new NetworkPropertyInfo[1];
		NetworkProperty_String networkProperty_String = new NetworkProperty_String();
		networkProperty_String.PropertyMeta = this;
		networkProperty_String.Settings_Property("Message", 1, -1073741824);
		networkProperty_String.Settings_Offsets(0, 0);
		networkProperty_String.AddStringSettings(StringEncodings.UTF8);
		AddProperty(0, 0, networkProperty_String, -1);
		base.InitMeta();
		_pool = new ObjectPool<LogNetworkMessageEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as LogNetworkMessageEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is ILogNetworkMessageEventListener logNetworkMessageEventListener)
		{
			logNetworkMessageEventListener.OnEvent((LogNetworkMessageEvent)ev);
		}
	}
}
