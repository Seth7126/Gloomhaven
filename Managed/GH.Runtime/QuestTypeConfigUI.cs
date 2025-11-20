using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class QuestTypeConfigUI
{
	[FormerlySerializedAs("icon")]
	[SerializeField]
	public Sprite guildmasterRewardIcon;

	[SerializeField]
	public Sprite campaingRewardIcon;

	[Tooltip("Color of  icon")]
	public Color color = Color.white;

	public Sprite marker;

	public Sprite GetRewardIcon(bool isGuildmaster)
	{
		if (!isGuildmaster && !(campaingRewardIcon == null))
		{
			return campaingRewardIcon;
		}
		return guildmasterRewardIcon;
	}
}
