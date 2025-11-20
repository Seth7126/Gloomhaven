using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class GameDataEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static GameDataEvent_Meta Instance;

	internal ObjectPool<GameDataEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(26, 189, 122, 69, 81, 191, 218, 76, 141, 21, 177, 38, 80, 11, 31, 47);

	Type IFactory.TypeObject => typeof(GameDataEvent);

	static GameDataEvent_Meta()
	{
		Instance = new GameDataEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(7);
		CountStorage = 5;
		CountObjects = 1;
		CountProperties = 5;
		Properties = new NetworkPropertyInfo[5];
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("DataActionID", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(0, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create(4, 0));
		AddProperty(0, 0, networkProperty_Integer, -1);
		NetworkProperty_Integer networkProperty_Integer2 = new NetworkProperty_Integer();
		networkProperty_Integer2.PropertyMeta = this;
		networkProperty_Integer2.Settings_Property("ChunkSize", 1, -1073741824);
		networkProperty_Integer2.Settings_Offsets(1, 1);
		networkProperty_Integer2.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(1, 0, networkProperty_Integer2, -1);
		NetworkProperty_Integer networkProperty_Integer3 = new NetworkProperty_Integer();
		networkProperty_Integer3.PropertyMeta = this;
		networkProperty_Integer3.Settings_Property("ChunkIndex", 1, -1073741824);
		networkProperty_Integer3.Settings_Offsets(2, 2);
		networkProperty_Integer3.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(2, 0, networkProperty_Integer3, -1);
		NetworkProperty_Integer networkProperty_Integer4 = new NetworkProperty_Integer();
		networkProperty_Integer4.PropertyMeta = this;
		networkProperty_Integer4.Settings_Property("TotalSize", 1, -1073741824);
		networkProperty_Integer4.Settings_Offsets(3, 3);
		networkProperty_Integer4.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(3, 0, networkProperty_Integer4, -1);
		NetworkProperty_Bool networkProperty_Bool = new NetworkProperty_Bool();
		networkProperty_Bool.PropertyMeta = this;
		networkProperty_Bool.Settings_Property("Complete", 1, -1073741824);
		networkProperty_Bool.Settings_Offsets(4, 4);
		AddProperty(4, 0, networkProperty_Bool, -1);
		base.InitMeta();
		_pool = new ObjectPool<GameDataEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as GameDataEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IGameDataEventListener gameDataEventListener)
		{
			gameDataEventListener.OnEvent((GameDataEvent)ev);
		}
	}
}
