using System;
using Photon.Bolt.Internal;

namespace Photon.Bolt;

internal class ControllableState_Meta : NetworkState_Meta, ISerializerFactory, IFactory
{
	internal static ControllableState_Meta Instance;

	internal ObjectPool<ControllableState> _pool;

	TypeId IFactory.TypeId => TypeId;

	UniqueId IFactory.TypeKey => new UniqueId(79, 116, 210, 112, 253, 229, 27, 70, 181, 253, 207, 129, 105, 245, 123, 164);

	Type IFactory.TypeObject => typeof(IControllableState);

	static ControllableState_Meta()
	{
		Instance = new ControllableState_Meta();
		Instance.InitMeta();
	}

	internal override void InitObject(NetworkObj obj, Offsets offsets)
	{
	}

	internal override void InitMeta()
	{
		TypeId = new TypeId(17);
		CountStorage = 2;
		CountObjects = 1;
		CountProperties = 2;
		Properties = new NetworkPropertyInfo[2];
		PropertyIdBits = 2;
		PacketMaxBits = 512;
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
		base.InitMeta();
		_pool = new ObjectPool<ControllableState>();
	}

	object IFactory.Create()
	{
		return _pool.Get();
	}

	void IFactory.Return(object objToReturn)
	{
		_pool.Return(objToReturn as ControllableState);
	}
}
