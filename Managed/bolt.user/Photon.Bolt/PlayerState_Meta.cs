using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class PlayerState_Meta : NetworkState_Meta, ISerializerFactory, IFactory
{
	internal static PlayerState_Meta Instance;

	internal ObjectPool<PlayerState> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(108, 158, 97, 78, 248, 75, 52, 73, 176, 98, 19, 211, 174, 187, 134, 237);

	Type IFactory.TypeObject => typeof(IPlayerState);

	static PlayerState_Meta()
	{
		Instance = new PlayerState_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(16);
		CountStorage = 1;
		CountObjects = 1;
		CountProperties = 1;
		Properties = new NetworkPropertyInfo[1];
		PropertyIdBits = 1;
		PacketMaxBits = 512;
		PacketMaxProperties = 4;
		PacketMaxPropertiesBits = 3;
		InstantiationPositionCompression = PropertyVectorCompressionSettings.Create(PropertyFloatCompressionSettings.Create(1, 0f, 1f, 1f), PropertyFloatCompressionSettings.Create(1, 0f, 1f, 1f), PropertyFloatCompressionSettings.Create(1, 0f, 1f, 1f));
		InstantiationRotationCompression = PropertyQuaternionCompression.Create(PropertyFloatCompressionSettings.Create(2, 1f, 1f, 1f));
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("LatestProcessedActionID", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(0, 0);
		networkProperty_Integer.Settings_Mecanim(MecanimMode.Disabled, MecanimDirection.UsingAnimatorMethods, 0f, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create());
		AddProperty(0, 0, networkProperty_Integer, -1);
		base.InitMeta();
		_pool = new ObjectPool<PlayerState>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as PlayerState);
	}
}
