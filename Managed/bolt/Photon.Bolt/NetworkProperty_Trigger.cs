#define DEBUG
using System;
using Photon.Bolt.Collections;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;
using UdpKit;

namespace Photon.Bolt;

internal class NetworkProperty_Trigger : NetworkProperty_Mecanim
{
	public override bool AllowCallbacks => false;

	public override bool WantsOnFrameCloned => true;

	public override bool WantsOnSimulateAfter => true;

	public override object DebugValue(NetworkObj obj, NetworkStorage storage)
	{
		return "TRIGGER";
	}

	public override int BitCount(NetworkObj obj)
	{
		return obj.RootState.Entity.SendRate;
	}

	public override void SetDynamic(NetworkObj obj, object value)
	{
		if (MecanimDirection == MecanimDirection.UsingAnimatorMethods)
		{
			BoltLog.Error("Can't call SetDynamic on a trigger in 'UsingAnimatorMethods' mode");
		}
		else
		{
			obj.Storage.Values[obj[this]].TriggerLocal.Update(BoltCore.frame, set: true);
		}
	}

	public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].TriggerSend.Update(BoltCore.frame, set: false);
		packet.WriteInt(storage.Values[obj[this]].TriggerSend.History, obj.RootState.Entity.SendRate);
		return true;
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet)
	{
		storage.Values[obj[this]].TriggerLocal.Frame = storage.Frame;
		storage.Values[obj[this]].TriggerLocal.History = packet.ReadInt(obj.RootState.Entity.SendRate);
	}

	public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, NetworkStorage other)
	{
		storage.Values[obj[this]].TriggerLocal.Frame = storage.Frame;
		storage.Values[obj[this]].TriggerLocal.History = other.Values[obj[this]].TriggerLocal.History;
	}

	public override bool Equals(NetworkObj obj, NetworkStorage storage1, NetworkStorage storage2)
	{
		return storage1.Values[obj[this]].TriggerLocal.History == storage2.Values[obj[this]].TriggerLocal.History;
	}

	public override void OnSimulateAfter(NetworkObj obj)
	{
		if (MecanimMode == MecanimMode.Disabled)
		{
			MecanimPush(obj, push: false);
		}
		else if (ShouldPullDataFromMecanim(obj.RootState))
		{
			MecanimPull(obj, obj.Storage);
		}
		else
		{
			MecanimPush(obj, push: true);
		}
	}

	public override void OnFrameCloned(NetworkObj obj, NetworkStorage storage)
	{
		storage.Values[obj[this]].TriggerLocal.Frame = 0;
		storage.Values[obj[this]].TriggerLocal.History = 0;
	}

	private void MecanimPull(NetworkObj obj, NetworkStorage storage)
	{
		if (!(obj.RootState.Animator == null) && obj.RootState.Animator.gameObject.activeSelf && obj.RootState.Animator.GetBool(PropertyName) && !obj.RootState.Animator.IsInTransition(MecanimLayer))
		{
			storage.Values[obj[this]].TriggerSend.Update(BoltCore.frame, set: true);
			storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
			obj.Storage.Values[obj[this]].Action?.Invoke();
		}
	}

	private void MecanimPush(NetworkObj obj, bool push)
	{
		NetworkState rootState = obj.RootState;
		BoltIterator<NetworkStorage> iterator = rootState.Frames.GetIterator();
		while (iterator.Next())
		{
			NetworkStorage val = iterator.val;
			int num = obj[this];
			int frame = val.Values[num].TriggerLocal.Frame;
			int num2 = val.Values[num].TriggerLocal.History;
			Action action = val.Values[num].Action;
			int num3 = obj.RootState.Entity.SendRate - 1;
			while (num3 >= 0 && num2 != 0)
			{
				if (frame - num3 <= obj.RootState.Entity.Frame)
				{
					int num4 = 1 << num3;
					if ((num2 & num4) == num4)
					{
						num2 &= ~num4;
						val.Values[num].TriggerLocal.History = num2;
						rootState.Storage.Values[num].TriggerSend.Update(BoltCore.frame, set: true);
						rootState.Storage.PropertyChanged(obj.OffsetProperties + OffsetProperties);
						if (push)
						{
							for (int i = 0; i < obj.RootState.Animators.Count; i++)
							{
								if (obj.RootState.Animators[i].gameObject.activeSelf)
								{
									obj.RootState.Animators[i].SetTrigger(PropertyName);
								}
							}
						}
						action?.Invoke();
					}
				}
				num3--;
			}
		}
	}
}
