#define DEBUG
using System;
using Photon.Bolt.Utils;
using UnityEngine;

namespace Photon.Bolt;

internal class GHControllableState : NetworkState, IGHControllableState, IControllableState, IState, IDisposable
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

	public IProtocolToken Level
	{
		get
		{
			return Storage.Values[OffsetStorage + 2].ProtocolToken;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner && !entity.HasControl)
			{
				BoltLog.Error("Only the owner and controller can modify: 'Level'");
				return;
			}
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 2].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 2].ProtocolToken = value;
			if (NetworkValue.Diff(protocolToken, value))
			{
				Storage.PropertyChanged(OffsetProperties + 2);
			}
		}
	}

	public IProtocolToken PerkPoints
	{
		get
		{
			return Storage.Values[OffsetStorage + 3].ProtocolToken;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner && !entity.HasControl)
			{
				BoltLog.Error("Only the owner and controller can modify: 'PerkPoints'");
				return;
			}
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 3].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 3].ProtocolToken = value;
			if (NetworkValue.Diff(protocolToken, value))
			{
				Storage.PropertyChanged(OffsetProperties + 3);
			}
		}
	}

	public IProtocolToken ActivePerks
	{
		get
		{
			return Storage.Values[OffsetStorage + 4].ProtocolToken;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner && !entity.HasControl)
			{
				BoltLog.Error("Only the owner and controller can modify: 'ActivePerks'");
				return;
			}
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 4].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 4].ProtocolToken = value;
			if (NetworkValue.Diff(protocolToken, value))
			{
				Storage.PropertyChanged(OffsetProperties + 4);
			}
		}
	}

	public IProtocolToken CardInventory
	{
		get
		{
			return Storage.Values[OffsetStorage + 5].ProtocolToken;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner && !entity.HasControl)
			{
				BoltLog.Error("Only the owner and controller can modify: 'CardInventory'");
				return;
			}
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 5].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 5].ProtocolToken = value;
			if (NetworkValue.Diff(protocolToken, value))
			{
				Storage.PropertyChanged(OffsetProperties + 5);
			}
		}
	}

	public IProtocolToken ItemInventory
	{
		get
		{
			return Storage.Values[OffsetStorage + 6].ProtocolToken;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner)
			{
				BoltLog.Error("Only the owner can modify: 'ItemInventory'");
				return;
			}
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 6].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 6].ProtocolToken = value;
			if (NetworkValue.Diff(protocolToken, value))
			{
				Storage.PropertyChanged(OffsetProperties + 6);
			}
		}
	}

	public IProtocolToken StartingTile
	{
		get
		{
			return Storage.Values[OffsetStorage + 7].ProtocolToken;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner)
			{
				BoltLog.Error("Only the owner can modify: 'StartingTile'");
				return;
			}
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 7].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 7].ProtocolToken = value;
			if (NetworkValue.Diff(protocolToken, value))
			{
				Storage.PropertyChanged(OffsetProperties + 7);
			}
		}
	}

	public IProtocolToken StartRoundCards
	{
		get
		{
			return Storage.Values[OffsetStorage + 8].ProtocolToken;
		}
		set
		{
			Entity entity = base.RootState.Entity;
			if (!entity.IsOwner && !entity.HasControl)
			{
				BoltLog.Error("Only the owner and controller can modify: 'StartRoundCards'");
				return;
			}
			IProtocolToken protocolToken = Storage.Values[OffsetStorage + 8].ProtocolToken;
			protocolToken.Release();
			Storage.Values[OffsetStorage + 8].ProtocolToken = value;
			if (NetworkValue.Diff(protocolToken, value))
			{
				Storage.PropertyChanged(OffsetProperties + 8);
			}
		}
	}

	public GHControllableState()
		: base(GHControllableState_Meta.Instance)
	{
	}
}
