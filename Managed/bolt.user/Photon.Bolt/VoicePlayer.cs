#define DEBUG
using System;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

internal class VoicePlayer : NetworkState, IVoicePlayer, IState, IDisposable
{
	public NetworkTransform Transform => Storage.Values[OffsetStorage].Transform;

	public int VoicePlayerID
	{
		get
		{
			return Storage.Values[OffsetStorage + 3].Int0;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner)
			{
				BoltLog.Error("Only the owner can modify: 'VoicePlayerID'");
				return;
			}
			if (value < 0 || value > 32)
			{
				BoltLog.Warn("Property 'VoicePlayerID' is being set to a value larger than the compression settings, it will be clamped to [+0, +32]");
			}
			value = Mathf.Clamp(value, 0, 32);
			int @int = Storage.Values[OffsetStorage + 3].Int0;
			Storage.Values[OffsetStorage + 3].Int0 = value;
			if (NetworkValue.Diff(@int, value))
			{
				Storage.PropertyChanged(OffsetProperties + 1);
			}
		}
	}

	public VoicePlayer()
		: base(VoicePlayer_Meta.Instance)
	{
	}
}
