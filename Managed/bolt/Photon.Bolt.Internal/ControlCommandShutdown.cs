using System;
using System.Collections.Generic;
using Photon.Bolt.Utils;
using UdpKit;
using UnityEngine;

namespace Photon.Bolt.Internal;

internal class ControlCommandShutdown : ControlCommand
{
	public List<Action> Callbacks = new List<Action>();

	public UdpConnectionDisconnectReason disconnectReason = UdpConnectionDisconnectReason.Disconnected;

	public override void Run()
	{
		BoltCore.BeginShutdown(this);
	}

	public override void Done()
	{
		BoltCore._mode = BoltNetworkModes.None;
		for (int i = 0; i < Callbacks.Count; i++)
		{
			try
			{
				if (Application.isPlaying)
				{
					Callbacks[i]();
				}
			}
			catch (Exception exception)
			{
				BoltLog.Exception(exception);
			}
		}
	}
}
