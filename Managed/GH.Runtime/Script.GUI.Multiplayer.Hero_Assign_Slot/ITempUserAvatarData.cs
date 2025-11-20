using System;
using UnityEngine;

namespace Script.GUI.Multiplayer.Hero_Assign_Slot;

public interface ITempUserAvatarData
{
	string AvatarUrl { get; set; }

	event Action<Sprite> OnAvatarDownloadedEvent;
}
