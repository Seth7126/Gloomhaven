using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.Ability_Card;

public static class FullCardEventPusherExtension
{
	public static void AddFullCardEventPusher(this GameObject go, FullCardEventPusher.EventType eventType = FullCardEventPusher.EventType.None)
	{
		if (InputManager.GamePadInUse)
		{
			return;
		}
		MaskableGraphic[] componentsInChildren = go.GetComponentsInChildren<MaskableGraphic>();
		foreach (MaskableGraphic maskableGraphic in componentsInChildren)
		{
			if (maskableGraphic.gameObject.GetComponent<FullCardEventPusher>() == null)
			{
				maskableGraphic.gameObject.AddComponent<FullCardEventPusher>().Type = eventType | FullCardEventPusher.EventType.Highlight;
			}
		}
	}
}
