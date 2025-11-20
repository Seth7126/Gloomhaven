using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class GameDataRequest_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static GameDataRequest_Meta Instance;

	internal ObjectPool<GameDataRequest> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(141, 22, 5, 56, 48, 252, 4, 69, 158, 125, 89, 188, 25, 94, 72, 157);

	Type IFactory.TypeObject => typeof(GameDataRequest);

	static GameDataRequest_Meta()
	{
		Instance = new GameDataRequest_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(8);
		CountStorage = 1;
		CountObjects = 1;
		CountProperties = 1;
		Properties = new NetworkPropertyInfo[1];
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("DataActionID", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(0, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create(4, 0));
		AddProperty(0, 0, networkProperty_Integer, -1);
		base.InitMeta();
		_pool = new ObjectPool<GameDataRequest>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as GameDataRequest);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IGameDataRequestListener gameDataRequestListener)
		{
			gameDataRequestListener.OnEvent((GameDataRequest)ev);
		}
	}
}
