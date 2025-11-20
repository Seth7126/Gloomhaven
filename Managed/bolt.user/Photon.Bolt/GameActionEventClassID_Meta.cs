using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class GameActionEventClassID_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static GameActionEventClassID_Meta Instance;

	internal ObjectPool<GameActionEventClassID> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(156, 34, 173, 168, 92, 61, 41, 65, 173, 84, 227, 170, 141, 89, 131, 174);

	Type IFactory.TypeObject => typeof(GameActionEventClassID);

	static GameActionEventClassID_Meta()
	{
		Instance = new GameActionEventClassID_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(6);
		CountStorage = 8;
		CountObjects = 1;
		CountProperties = 8;
		Properties = new NetworkPropertyInfo[8];
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("ActionID", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(0, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(0, 0, networkProperty_Integer, -1);
		NetworkProperty_Integer networkProperty_Integer2 = new NetworkProperty_Integer();
		networkProperty_Integer2.PropertyMeta = this;
		networkProperty_Integer2.Settings_Property("ActionTypeID", 1, -1073741824);
		networkProperty_Integer2.Settings_Offsets(1, 1);
		networkProperty_Integer2.Settings_Integer(PropertyIntCompressionSettings.Create(7, 0));
		AddProperty(1, 0, networkProperty_Integer2, -1);
		NetworkProperty_Integer networkProperty_Integer3 = new NetworkProperty_Integer();
		networkProperty_Integer3.PropertyMeta = this;
		networkProperty_Integer3.Settings_Property("PlayerID", 1, -1073741824);
		networkProperty_Integer3.Settings_Offsets(2, 2);
		networkProperty_Integer3.Settings_Integer(PropertyIntCompressionSettings.Create(8, 0));
		AddProperty(2, 0, networkProperty_Integer3, -1);
		NetworkProperty_Integer networkProperty_Integer4 = new NetworkProperty_Integer();
		networkProperty_Integer4.PropertyMeta = this;
		networkProperty_Integer4.Settings_Property("ActorID", 1, -1073741824);
		networkProperty_Integer4.Settings_Offsets(3, 3);
		networkProperty_Integer4.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(3, 0, networkProperty_Integer4, -1);
		NetworkProperty_Integer networkProperty_Integer5 = new NetworkProperty_Integer();
		networkProperty_Integer5.PropertyMeta = this;
		networkProperty_Integer5.Settings_Property("TargetPhaseID", 1, -1073741824);
		networkProperty_Integer5.Settings_Offsets(4, 4);
		networkProperty_Integer5.Settings_Integer(PropertyIntCompressionSettings.Create(7, 0));
		AddProperty(4, 0, networkProperty_Integer5, -1);
		NetworkProperty_Integer networkProperty_Integer6 = new NetworkProperty_Integer();
		networkProperty_Integer6.PropertyMeta = this;
		networkProperty_Integer6.Settings_Property("SupplementaryDataIDMin", 1, -1073741824);
		networkProperty_Integer6.Settings_Offsets(5, 5);
		networkProperty_Integer6.Settings_Integer(PropertyIntCompressionSettings.Create(3, 0));
		AddProperty(5, 0, networkProperty_Integer6, -1);
		NetworkProperty_Bool networkProperty_Bool = new NetworkProperty_Bool();
		networkProperty_Bool.PropertyMeta = this;
		networkProperty_Bool.Settings_Property("SupplementaryDataBoolean", 1, -1073741824);
		networkProperty_Bool.Settings_Offsets(6, 6);
		AddProperty(6, 0, networkProperty_Bool, -1);
		NetworkProperty_String networkProperty_String = new NetworkProperty_String();
		networkProperty_String.PropertyMeta = this;
		networkProperty_String.Settings_Property("ClassID", 1, -1073741824);
		networkProperty_String.Settings_Offsets(7, 7);
		networkProperty_String.AddStringSettings(StringEncodings.ASCII);
		AddProperty(7, 0, networkProperty_String, -1);
		base.InitMeta();
		_pool = new ObjectPool<GameActionEventClassID>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as GameActionEventClassID);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IGameActionEventClassIDListener gameActionEventClassIDListener)
		{
			gameActionEventClassIDListener.OnEvent((GameActionEventClassID)ev);
		}
	}
}
