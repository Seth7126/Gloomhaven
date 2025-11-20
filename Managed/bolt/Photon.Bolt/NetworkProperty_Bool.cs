#define DEBUG
using Photon.Bolt.Utils;
using UdpKit;

namespace Photon.Bolt;

internal class NetworkProperty_Bool : NetworkProperty_Mecanim
{
	public override int BitCount(NetworkObj obj)
	{
		return 1;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		if (MecanimDirection == MecanimDirection.UsingAnimatorMethods)
		{
			BoltLog.Error("Can't call SetDynamic on a bool in 'UsingAnimatorMethods' mode");
			return;
		}
		bool flag = (bool)value;
		if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Bool, flag))
		{
			obj.Storage.Values[obj[this]].Bool = flag;
			obj.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
		}
	}

	public override object GetDynamic(NetworkObj obj)
	{
		return obj.Storage.Values[obj[this]].Bool;
	}

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return storage.Values[obj[this]].Bool;
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		packet.WriteBool(storage.Values[obj[this]].Bool);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].Bool = packet.ReadBool();
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].Bool = other.Values[obj[this]].Bool;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].Bool == storage2.Values[obj[this]].Bool;
	}

	protected override void PullMecanimValue(NetworkState state)
	{
		if (!(state.Animator == null) && state.Animator.gameObject.activeSelf)
		{
			bool flag = state.Animator.GetBool(PropertyName);
			bool b = state.Storage.Values[state[this]].Bool;
			state.Storage.Values[state[this]].Bool = flag;
			if (NetworkValue.Diff(flag, b))
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
				bool b = state.Animators[i].GetBool(PropertyName);
				bool flag = state.Storage.Values[state[this]].Bool;
				if (NetworkValue.Diff(flag, b))
				{
					state.Animators[i].SetBool(PropertyName, flag);
				}
			}
		}
	}
}
