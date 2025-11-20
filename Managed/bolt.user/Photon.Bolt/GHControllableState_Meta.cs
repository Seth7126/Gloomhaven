using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class GHControllableState_Meta : NetworkState_Meta, ISerializerFactory, IFactory
{
	internal static GHControllableState_Meta Instance;

	internal ObjectPool<GHControllableState> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(233, 64, 164, 202, 154, 103, 149, 65, 173, 223, 208, 15, 251, 42, 144, 246);

	Type IFactory.TypeObject => typeof(IGHControllableState);

	static GHControllableState_Meta()
	{
		Instance = new GHControllableState_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(19);
		CountStorage = 9;
		CountObjects = 1;
		CountProperties = 9;
		Properties = new NetworkPropertyInfo[9];
		PropertyIdBits = 4;
		PacketMaxBits = 1024;
		PacketMaxProperties = 16;
		PacketMaxPropertiesBits = 5;
		InstantiationPositionCompression = PropertyVectorCompressionSettings.Create(PropertyFloatCompressionSettings.Create(1, 0f, 1f, 1f), PropertyFloatCompressionSettings.Create(1, 0f, 1f, 1f), PropertyFloatCompressionSettings.Create(1, 0f, 1f, 1f));
		InstantiationRotationCompression = PropertyQuaternionCompression.Create(PropertyFloatCompressionSettings.Create(2, 1f, 1f, 1f));
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("ControllableID", 1, 0);
		networkProperty_Integer.Settings_Offsets(0, 0);
		networkProperty_Integer.Settings_Mecanim(MecanimMode.Disabled, MecanimDirection.UsingAnimatorMethods, 0f, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(0, 0, networkProperty_Integer, -1);
		NetworkProperty_Integer networkProperty_Integer2 = new NetworkProperty_Integer();
		networkProperty_Integer2.PropertyMeta = this;
		networkProperty_Integer2.Settings_Property("ControllerID", 1, -1073741824);
		networkProperty_Integer2.Settings_Offsets(1, 1);
		networkProperty_Integer2.Settings_Mecanim(MecanimMode.Disabled, MecanimDirection.UsingAnimatorMethods, 0f, 0);
		networkProperty_Integer2.Settings_Integer(PropertyIntCompressionSettings.Create(8, 0));
		AddProperty(1, 0, networkProperty_Integer2, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken.PropertyMeta = this;
		networkProperty_ProtocolToken.Settings_Property("Level", 3, 1073741824);
		networkProperty_ProtocolToken.Settings_Offsets(2, 2);
		AddProperty(2, 0, networkProperty_ProtocolToken, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken2 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken2.PropertyMeta = this;
		networkProperty_ProtocolToken2.Settings_Property("PerkPoints", 2, 1073741824);
		networkProperty_ProtocolToken2.Settings_Offsets(3, 3);
		AddProperty(3, 0, networkProperty_ProtocolToken2, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken3 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken3.PropertyMeta = this;
		networkProperty_ProtocolToken3.Settings_Property("ActivePerks", 1, 1073741824);
		networkProperty_ProtocolToken3.Settings_Offsets(4, 4);
		AddProperty(4, 0, networkProperty_ProtocolToken3, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken4 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken4.PropertyMeta = this;
		networkProperty_ProtocolToken4.Settings_Property("CardInventory", 1, 1073741824);
		networkProperty_ProtocolToken4.Settings_Offsets(5, 5);
		AddProperty(5, 0, networkProperty_ProtocolToken4, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken5 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken5.PropertyMeta = this;
		networkProperty_ProtocolToken5.Settings_Property("ItemInventory", 1, -1073741824);
		networkProperty_ProtocolToken5.Settings_Offsets(6, 6);
		AddProperty(6, 0, networkProperty_ProtocolToken5, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken6 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken6.PropertyMeta = this;
		networkProperty_ProtocolToken6.Settings_Property("StartingTile", 1, -1073741824);
		networkProperty_ProtocolToken6.Settings_Offsets(7, 7);
		AddProperty(7, 0, networkProperty_ProtocolToken6, -1);
		NetworkProperty_ProtocolToken networkProperty_ProtocolToken7 = new NetworkProperty_ProtocolToken();
		networkProperty_ProtocolToken7.PropertyMeta = this;
		networkProperty_ProtocolToken7.Settings_Property("StartRoundCards", 1, 1073741824);
		networkProperty_ProtocolToken7.Settings_Offsets(8, 8);
		AddProperty(8, 0, networkProperty_ProtocolToken7, -1);
		base.InitMeta();
		_pool = new ObjectPool<GHControllableState>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as GHControllableState);
	}
}
