#define DEBUG
using System;
using Photon.Bolt.Exceptions;
using Photon.Bolt.Internal;
using Photon.Bolt.Utils;

namespace Photon.Bolt;

internal static class NetworkIdAllocator
{
	private static uint EntityId;

	private static uint ConnectionId;

	public static uint LocalConnectionId => ConnectionId;

	public static void Reset(uint connectionId)
	{
		BoltLog.Debug("Id allocator reset with id {0}", connectionId);
		EntityId = 0u;
		ConnectionId = connectionId;
	}

	public static void Assigned(uint connectionId)
	{
		BoltLog.Debug("Assigned id {0} from server", connectionId);
		Assert.True(BoltCore.isClient, "BoltCore.isClient");
		Assert.True(connectionId != 0, "connectionId > 0u");
		Assert.True(connectionId != uint.MaxValue, "connectionId != uint.MaxValue");
		Assert.True(ConnectionId == uint.MaxValue, "ConnectionId == uint.MaxValue");
		BoltLog.Debug("Assigned id {0} from server", connectionId);
		ConnectionId = connectionId;
	}

	public static NetworkId Allocate()
	{
		if (ConnectionId == 0)
		{
			throw new InvalidOperationException("Connection id not assigned");
		}
		return new NetworkId(ConnectionId, ++EntityId);
	}
}
