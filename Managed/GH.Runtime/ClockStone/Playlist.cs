using System;

namespace ClockStone;

[Serializable]
public class Playlist
{
	public string name;

	public string[] playlistItems;

	public Playlist()
	{
		name = "Default";
		playlistItems = null;
	}

	public Playlist(string name, string[] playlistItems)
	{
		this.name = name;
		this.playlistItems = playlistItems;
	}
}
