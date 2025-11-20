using Photon.Bolt.Collections;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal class NetworkProperty_Vector : NetworkProperty
{
	private PropertyVectorCompressionSettings Compression;

	public override bool WantsOnSimulateBefore => Interpolation.Enabled;

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Vector3;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		Vector3 vector = (Vector3)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Vector3, vector))
		{
			obj.Storage.Values[obj[this]].Vector3 = vector;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public void Settings_Vector(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z, bool strict)
	{
		Compression = PropertyVectorCompressionSettings.Create(x, y, z, strict);
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Vector3;
	}

	public override int BitCount(NetworkObj obj)
	{
		return Compression.BitsRequired;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		Compression.Pack(packet, storage.Values[obj[this]].Vector3);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		Vector3 vector = Compression.Read(packet);
		if (Interpolation.Enabled && obj.Root is NetworkState)
		{
			storage.Values[obj[this] + 1].Vector3 = vector;
		}
		else
		{
			storage.Values[obj[this]].Vector3 = vector;
		}
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		if (Interpolation.Enabled && obj.Root is NetworkState)
		{
			storage.Values[obj[this] + 1].Vector3 = other.Values[obj[this] + 1].Vector3;
		}
		else
		{
			storage.Values[obj[this]].Vector3 = other.Values[obj[this]].Vector3;
		}
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Vector3 == storage2.Values[obj[this]].Vector3;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}

	public override void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t)
	{
		if (Interpolation.Enabled)
		{
			Vector3 vector = from.Values[obj[this]].Vector3;
			Vector3 vector2 = to.Values[obj[this]].Vector3;
			float sqrMagnitude = (vector2 - vector).sqrMagnitude;
			if (sqrMagnitude < Interpolation.SnapMagnitude * Interpolation.SnapMagnitude)
			{
				storage.Values[obj[this]].Vector3 = Vector3.Lerp(vector, vector2, t);
			}
			else
			{
				storage.Values[obj[this]].Vector3 = vector2;
			}
		}
		else
		{
			storage.Values[obj[this]].Vector3 = to.Values[obj[this]].Vector3;
		}
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
			Vector3 vector = Math.InterpolateVector(obj.RootState.Frames, num + 1, obj.RootState.Entity.Frame, Interpolation.SnapMagnitude);
			while (iterator.Next())
			{
				iterator.val.Values[num].Vector3 = vector;
			}
		}
	}
}
