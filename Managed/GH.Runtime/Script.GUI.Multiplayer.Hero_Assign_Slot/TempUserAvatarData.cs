using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Script.GUI.Multiplayer.Hero_Assign_Slot;

public class TempUserAvatarData : ITempUserAvatarData
{
	public Texture2D Texture2D;

	public Sprite Sprite;

	public UnityWebRequest UnityWebRequest;

	public Coroutine Coroutine;

	public string AvatarUrl { get; set; }

	public event Action<Sprite> OnAvatarDownloadedEvent;

	internal void OnAvatarDownloaded()
	{
		this.OnAvatarDownloadedEvent?.Invoke(Sprite);
	}
}
