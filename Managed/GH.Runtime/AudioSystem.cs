using System;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
	[Serializable]
	public class EnvironmentSounds
	{
		[AudioEventName]
		public string DoorOpeningAudioEvent;
	}

	public static AudioSystem instance;

	[AudioPlaylistName]
	public string m_MusicPlaylist;

	[AudioEventName]
	public string m_AmbientEvent;

	public EnvironmentSounds EnvironmentAudio;

	private void Awake()
	{
		instance = this;
		if (!string.IsNullOrEmpty(m_MusicPlaylist))
		{
			AudioController.PlayMusicPlaylist(m_MusicPlaylist);
		}
		if (!string.IsNullOrEmpty(m_AmbientEvent))
		{
			AudioController.PlayAmbienceSound(m_AmbientEvent);
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}
}
