using System.Collections.Generic;
using Photon.Bolt;

namespace FFSNet;

public class HostPlatformData
{
	public PlatformType Platform { get; set; }

	public List<BoltConnection> BoltConnections { get; set; } = new List<BoltConnection>();

	public bool ServerLeader { get; set; }

	public BoltConnection CurrentLeader { get; set; }

	public string CurrentSession { get; set; }
}
