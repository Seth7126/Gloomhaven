#define DEBUG
using System;
using Photon.Bolt.Utils;

namespace Photon.Bolt;

internal class PlayerState : NetworkState, IPlayerState, IState, IDisposable
{
	public int LatestProcessedActionID
	{
		get
		{
			return Storage.Values[OffsetStorage].Int0;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner)
			{
				BoltLog.Error("Only the owner can modify: 'LatestProcessedActionID'");
				return;
			}
			int @int = Storage.Values[OffsetStorage].Int0;
			Storage.Values[OffsetStorage].Int0 = value;
			if (NetworkValue.Diff(@int, value))
			{
				Storage.PropertyChanged(OffsetProperties);
			}
		}
	}

	public PlayerState()
		: base(PlayerState_Meta.Instance)
	{
	}
}
