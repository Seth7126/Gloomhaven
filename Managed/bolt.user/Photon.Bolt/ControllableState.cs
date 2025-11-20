#define DEBUG
using System;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

internal class ControllableState : NetworkState, IControllableState, IState, IDisposable
{
	public int ControllableID
	{
		get
		{
			return Storage.Values[OffsetStorage].Int0;
		}
		set
		{
			int @int = Storage.Values[OffsetStorage].Int0;
			Storage.Values[OffsetStorage].Int0 = value;
			if (NetworkValue.Diff(@int, value))
			{
				Storage.PropertyChanged(OffsetProperties);
			}
		}
	}

	public int ControllerID
	{
		get
		{
			return Storage.Values[OffsetStorage + 1].Int0;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner)
			{
				BoltLog.Error("Only the owner can modify: 'ControllerID'");
				return;
			}
			if (value < 0 || value > 255)
			{
				BoltLog.Warn("Property 'ControllerID' is being set to a value larger than the compression settings, it will be clamped to [+0, +255]");
			}
			value = Mathf.Clamp(value, 0, 255);
			int @int = Storage.Values[OffsetStorage + 1].Int0;
			Storage.Values[OffsetStorage + 1].Int0 = value;
			if (NetworkValue.Diff(@int, value))
			{
				Storage.PropertyChanged(OffsetProperties + 1);
			}
		}
	}

	public ControllableState()
		: base(ControllableState_Meta.Instance)
	{
	}
}
