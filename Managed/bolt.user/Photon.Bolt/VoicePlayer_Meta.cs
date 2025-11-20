using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class VoicePlayer_Meta : NetworkState_Meta, ISerializerFactory, IFactory
{
	internal static VoicePlayer_Meta Instance;

	internal ObjectPool<VoicePlayer> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(160, 218, 234, 163, 231, 239, 32, 74, 176, 82, 211, 202, 132, 138, 247, 103);

	Type IFactory.TypeObject => typeof(IVoicePlayer);

	static VoicePlayer_Meta()
	{
		Instance = new VoicePlayer_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(18);
		CountStorage = 4;
		CountObjects = 1;
		CountProperties = 2;
		Properties = new NetworkPropertyInfo[2];
		PropertyIdBits = 2;
		PacketMaxBits = 512;
		PacketMaxProperties = 16;
		PacketMaxPropertiesBits = 5;
		InstantiationPositionCompression = PropertyVectorCompressionSettings.Create(PropertyFloatCompressionSettings.Create(), PropertyFloatCompressionSettings.Create(), PropertyFloatCompressionSettings.Create());
		InstantiationRotationCompression = PropertyQuaternionCompression.Create(PropertyFloatCompressionSettings.Create());
		NetworkProperty_Transform networkProperty_Transform = new NetworkProperty_Transform();
		networkProperty_Transform.PropertyMeta = this;
		networkProperty_Transform.Settings_Property("Transform", 1, -1073741824);
		networkProperty_Transform.Settings_Offsets(0, 0);
		networkProperty_Transform.Settings_Space(TransformSpaces.Local);
		networkProperty_Transform.Settings_Vector(PropertyFloatCompressionSettings.Create(), PropertyFloatCompressionSettings.Create(), PropertyFloatCompressionSettings.Create(), strict: false);
		networkProperty_Transform.Settings_Quaternion(PropertyFloatCompressionSettings.Create(), strict: false);
		AddProperty(0, 0, networkProperty_Transform, -1);
		NetworkProperty_Integer networkProperty_Integer = new NetworkProperty_Integer();
		networkProperty_Integer.PropertyMeta = this;
		networkProperty_Integer.Settings_Property("VoicePlayerID", 1, -1073741824);
		networkProperty_Integer.Settings_Offsets(1, 3);
		networkProperty_Integer.Settings_Mecanim(MecanimMode.Disabled, MecanimDirection.UsingAnimatorMethods, 0f, 0);
		networkProperty_Integer.Settings_Integer(PropertyIntCompressionSettings.Create(6, 0));
		AddProperty(1, 0, networkProperty_Integer, -1);
		base.InitMeta();
		_pool = new ObjectPool<VoicePlayer>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as VoicePlayer);
	}
}
