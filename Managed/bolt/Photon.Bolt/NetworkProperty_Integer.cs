#define DEBUG
using Photon.Bolt.Utils;
using UdpKit;

namespace Photon.Bolt;

internal class NetworkProperty_Integer : NetworkProperty_Mecanim
{
	private PropertyIntCompressionSettings Compression;

	public void Settings_Integer(PropertyIntCompressionSettings compression)
	{
		Compression = compression;
	}

	public override int BitCount(NetworkObj obj)
	{
		return Compression.BitsRequired;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		if (MecanimDirection == MecanimDirection.UsingAnimatorMethods)
		{
			BoltLog.Error("Can't call SetDynamic on an integer in 'UsingAnimatorMethods' mode");
			return;
		}
		int num = (int)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Int0, num))
		{
			obj.Storage.Values[obj[this]].Int0 = num;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Int0;
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Int0;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		Compression.Pack(packet, storage.Values[obj[this]].Int0);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].Int0 = Compression.Read(packet);
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].Int0 = other.Values[obj[this]].Int0;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Int0 == storage2.Values[obj[this]].Int0;
	}

	public override bool SupportsDeltaCompression()
	{
		return true;
	}

	protected override void PullMecanimValue(NetworkState state)
	{
		if (!(state.Animator == null) && state.Animator.gameObject.activeSelf)
		{
			int integer = state.Animator.GetInteger(PropertyName);
			int @int = state.Storage.Values[state[this]].Int0;
			state.Storage.Values[state[this]].Int0 = integer;
			if (NetworkValue.Diff(integer, @int))
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
				int integer = state.Animators[i].GetInteger(PropertyName);
				int @int = state.Storage.Values[state[this]].Int0;
				if (NetworkValue.Diff(@int, integer))
				{
					state.Animators[i].SetInteger(PropertyName, @int);
				}
			}
		}
	}
}
