#define DEBUG
using Photon.Bolt.Collections;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt;

internal class NetworkProperty_Float : NetworkProperty_Mecanim
{
	private PropertyFloatSettings Settings;

	private PropertyFloatCompressionSettings Compression;

	public override bool WantsOnSimulateBefore => Interpolation.Enabled;

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
			float @float = Math.InterpolateFloat(obj.RootState.Frames, num + 1, obj.RootState.Entity.Frame, Settings.IsAngle);
			while (iterator.Next())
			{
				iterator.val.Values[num].Float0 = @float;
			}
		}
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		if (MecanimDirection == MecanimDirection.UsingAnimatorMethods)
		{
			BoltLog.Error("Can't call SetDynamic on a float in 'UsingAnimatorMethods' mode");
			return;
		}
		float num = (float)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Float0, num))
		{
			obj.Storage.Values[obj[this]].Float0 = num;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Float0;
	}

	public void Settings_Float(PropertyFloatCompressionSettings compression)
	{
		Compression = compression;
	}

	public void Settings_Float(PropertyFloatSettings settings)
	{
		Settings = settings;
	}

	public override int BitCount(NetworkObj obj)
	{
		return Compression.BitsRequired;
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Float0;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		Compression.Pack(packet, storage.Values[obj[this]].Float0);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		if (Interpolation.Enabled && obj.Root is NetworkState)
		{
			storage.Values[obj[this] + 1].Float1 = Compression.Read(packet);
		}
		else
		{
			storage.Values[obj[this]].Float0 = Compression.Read(packet);
		}
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		if (Interpolation.Enabled && obj.Root is NetworkState)
		{
			storage.Values[obj[this] + 1].Float1 = other.Values[obj[this] + 1].Float1;
		}
		else
		{
			storage.Values[obj[this]].Float0 = other.Values[obj[this]].Float0;
		}
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Float0 == storage2.Values[obj[this]].Float0;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}

	protected override void PullMecanimValue(NetworkState state)
	{
		if (!(state.Animator == null) && state.Animator.gameObject.activeSelf)
		{
			float num = state.Animator.GetFloat(PropertyName);
			float @float = state.Storage.Values[state[this]].Float0;
			state.Storage.Values[state[this]].Float0 = num;
			if (NetworkValue.Diff(num, @float))
			{
				state.Storage.PropertyChanged(state.OffsetProperties + OffsetProperties);
			}
		}
	}

	protected override void PushMecanimValue(NetworkState state)
	{
		for (int i = 0; i < state.Animators.Count; i++)
		{
			if (state.Animators[i].gameObject.activeSelf)
			{
				float b = state.Animators[i].GetFloat(PropertyName);
				float @float = state.Storage.Values[state[this]].Float0;
				if (NetworkValue.Diff(@float, b))
				{
					state.Animators[i].SetFloat(PropertyName, @float, MecanimDamping, BoltNetwork.FrameDeltaTime);
				}
			}
		}
	}

	public override void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t)
	{
		if (Interpolation.Enabled)
		{
			float @float = from.Values[obj[this]].Float0;
			float float2 = to.Values[obj[this]].Float0;
			float num = Mathf.Abs(float2 - @float);
			if (num < Interpolation.SnapMagnitude)
			{
				storage.Values[obj[this]].Float0 = Mathf.Lerp(@float, float2, t);
			}
			else
			{
				storage.Values[obj[this]].Float0 = float2;
			}
		}
		else
		{
			storage.Values[obj[this]].Float0 = to.Values[obj[this]].Float0;
		}
	}
}
