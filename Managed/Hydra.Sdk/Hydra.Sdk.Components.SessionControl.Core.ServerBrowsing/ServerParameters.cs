using System.Collections.Generic;

namespace Hydra.Sdk.Components.SessionControl.Core.ServerBrowsing;

public class ServerParameters
{
	public string GameMode { get; set; }

	public string GameMap { get; set; }

	public string ServerName { get; set; }

	public bool? PasswordProtected { get; set; }

	public int? MaxPlayerCount { get; set; }

	public List<string> Tags { get; set; }

	public Dictionary<string, string> KeyValues { get; set; }

	public List<string> Members { get; set; }

	public ServerParameters()
	{
		GameMode = "default";
		GameMap = "default";
		ServerName = "default";
		MaxPlayerCount = 4;
		PasswordProtected = false;
		Tags = new List<string>();
		KeyValues = new Dictionary<string, string>();
		Members = new List<string>();
	}
}
