using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class GameActionEvent_Meta : Event_Meta, IEventFactory, IFactory
{
	internal static GameActionEvent_Meta Instance;

	internal ObjectPool<GameActionEvent> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(82, 97, 212, 42, 217, 108, 175, 72, 131, 254, 88, 78, 143, 70, 162, 229);

	Type IFactory.TypeObject => typeof(GameActionEvent);

	static GameActionEvent_Meta()
	{
		Instance = new GameActionEvent_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(5);
		CountStorage = 18;
		CountObjects = 1;
		CountProperties = 18;
		Properties = new NetworkPropertyInfo[18];
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
		NetworkProperty_Integer networkProperty_Integer7 = new NetworkProperty_Integer();
		networkProperty_Integer7.PropertyMeta = this;
		networkProperty_Integer7.Settings_Property("SupplementaryDataIDMed", 1, -1073741824);
		networkProperty_Integer7.Settings_Offsets(6, 6);
		networkProperty_Integer7.Settings_Integer(PropertyIntCompressionSettings.Create(6, 0));
		AddProperty(6, 0, networkProperty_Integer7, -1);
		NetworkProperty_Integer networkProperty_Integer8 = new NetworkProperty_Integer();
		networkProperty_Integer8.PropertyMeta = this;
		networkProperty_Integer8.Settings_Property("SupplementaryDataIDMax", 1, -1073741824);
		networkProperty_Integer8.Settings_Offsets(7, 7);
		networkProperty_Integer8.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(7, 0, networkProperty_Integer8, -1);
		NetworkProperty_Bool networkProperty_Bool = new NetworkProperty_Bool();
		networkProperty_Bool.PropertyMeta = this;
		networkProperty_Bool.Settings_Property("SupplementaryDataBoolean", 1, -1073741824);
		networkProperty_Bool.Settings_Offsets(8, 8);
		AddProperty(8, 0, networkProperty_Bool, -1);
		NetworkProperty_Guid networkProperty_Guid = new NetworkProperty_Guid();
		networkProperty_Guid.PropertyMeta = this;
		networkProperty_Guid.Settings_Property("SupplementaryDataGuid", 1, -1073741824);
		networkProperty_Guid.Settings_Offsets(9, 9);
		AddProperty(9, 0, networkProperty_Guid, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken.PropertyMeta = this;
		networkProperty_ProtocolToken.Settings_Property("SupplementaryDataToken", 1, -1073741824);
		networkProperty_ProtocolToken.Settings_Offsets(10, 10);
		AddProperty(10, 0, networkProperty_ProtocolToken, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken2 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken2.PropertyMeta = this;
		networkProperty_ProtocolToken2.Settings_Property("SupplementaryDataToken2", 1, -1073741824);
		networkProperty_ProtocolToken2.Settings_Offsets(11, 11);
		AddProperty(11, 0, networkProperty_ProtocolToken2, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken3 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken3.PropertyMeta = this;
		networkProperty_ProtocolToken3.Settings_Property("SupplementaryDataToken3", 1, -1073741824);
		networkProperty_ProtocolToken3.Settings_Offsets(12, 12);
		AddProperty(12, 0, networkProperty_ProtocolToken3, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken4 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken4.PropertyMeta = this;
		networkProperty_ProtocolToken4.Settings_Property("SupplementaryDataToken4", 1, -1073741824);
		networkProperty_ProtocolToken4.Settings_Offsets(13, 13);
		AddProperty(13, 0, networkProperty_ProtocolToken4, -1);
		NetworkProperty_Bool networkProperty_Bool2 = new NetworkProperty_Bool();
		networkProperty_Bool2.PropertyMeta = this;
		networkProperty_Bool2.Settings_Property("SyncViaStateUpdate", 1, -1073741824);
		networkProperty_Bool2.Settings_Offsets(14, 14);
		AddProperty(14, 0, networkProperty_Bool2, -1);
		NetworkProperty_Bool networkProperty_Bool3 = new NetworkProperty_Bool();
		networkProperty_Bool3.PropertyMeta = this;
		networkProperty_Bool3.Settings_Property("ValidateAction", 1, -1073741824);
		networkProperty_Bool3.Settings_Offsets(15, 15);
		AddProperty(15, 0, networkProperty_Bool3, -1);
		NetworkProperty_Bool networkProperty_Bool4 = new NetworkProperty_Bool();
		networkProperty_Bool4.PropertyMeta = this;
		networkProperty_Bool4.Settings_Property("DoNotForwardAction", 1, -1073741824);
		networkProperty_Bool4.Settings_Offsets(16, 16);
		AddProperty(16, 0, networkProperty_Bool4, -1);
		NetworkProperty_Bool networkProperty_Bool5 = new NetworkProperty_Bool();
		networkProperty_Bool5.PropertyMeta = this;
		networkProperty_Bool5.Settings_Property("BinaryDataIncludesLoggingDetails", 1, -1073741824);
		networkProperty_Bool5.Settings_Offsets(17, 17);
		AddProperty(17, 0, networkProperty_Bool5, -1);
		base.InitMeta();
		_pool = new ObjectPool<GameActionEvent>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as GameActionEvent);
	}

	void IEventFactory.Dispatch(Event ev, object target)
	{
		if (target is IGameActionEventListener gameActionEventListener)
		{
			gameActionEventListener.OnEvent((GameActionEvent)ev);
		}
	}
}
