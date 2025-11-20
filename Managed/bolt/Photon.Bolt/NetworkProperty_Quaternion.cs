using Photon.Bolt.Collections;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal class NetworkProperty_Quaternion : NetworkProperty
{
	private PropertyQuaternionCompression Compression;

	public override bool WantsOnSimulateBefore => Interpolation.Enabled;

	public void Settings_Quaternion(PropertyFloatCompressionSettings compression, bool strict)
	{
		Compression = PropertyQuaternionCompression.Create(compression, strict);
	}

	public void Settings_QuaternionEuler(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict)
	{
		Compression = PropertyQuaternionCompression.Create(PropertyVectorCompressionSettings.Create(x, y, z, strict));
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		Quaternion quaternion = (Quaternion)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Quaternion, quaternion))
		{
			obj.Storage.Values[obj[this]].Quaternion = quaternion;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Quaternion;
	}

	public override void OnInit(NetworkObj obj)
	{
		obj.Storage.Values[obj[this]].Quaternion = Quaternion.identity;
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Quaternion;
	}

	public override int BitCount(NetworkObj obj)
	{
		return Compression.BitsRequired;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		Compression.Pack(packet, storage.Values[obj[this]].Quaternion);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		Quaternion quaternion = Compression.Read(packet);
		if (Interpolation.Enabled && obj.Root is NetworkState)
		{
			storage.Values[obj[this] + 1].Quaternion = quaternion;
		}
		else
		{
			storage.Values[obj[this]].Quaternion = quaternion;
		}
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		if (Interpolation.Enabled && obj.Root is NetworkState)
		{
			storage.Values[obj[this] + 1].Quaternion = other.Values[obj[this] + 1].Quaternion;
		}
		else
		{
			storage.Values[obj[this]].Quaternion = other.Values[obj[this]].Quaternion;
		}
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Quaternion == storage2.Values[obj[this]].Quaternion;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}

	public override void OnSimulateBefore(NetworkObj obj)
	{
		if (!Interpolation.Enabled)
		{
			return;
		}
		NetworkState networkState = (NetworkState)obj.Root;
		if (!networkState.Entity.IsOwner && (!networkState.Entity.HasControl || ToController))
		{
			BoltIterator<NetworkStorage> iterator = networkState.Frames.GetIterator();
			int num = obj[this];
			Quaternion quaternion = Math.InterpolateQuaternion(obj.RootState.Frames, num + 1, obj.RootState.Entity.Frame);
			while (iterator.Next())
			{
				iterator.val.Values[num].Quaternion = quaternion;
			}
		}
	}

	public override void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t)
	{
		if (Interpolation.Enabled)
		{
			Quaternion quaternion = from.Values[obj[this]].Quaternion;
			Quaternion quaternion2 = to.Values[obj[this]].Quaternion;
			storage.Values[obj[this]].Quaternion = Quaternion.Lerp(quaternion, quaternion2, t);
		}
		else
		{
			storage.Values[obj[this]].Quaternion = to.Values[obj[this]].Quaternion;
		}
	}
}
