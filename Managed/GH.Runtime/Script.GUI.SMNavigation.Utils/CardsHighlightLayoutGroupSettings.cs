using UnityEngine;

namespace Script.GUI.SMNavigation.Utils;

public class CardsHighlightLayoutGroupSettings
{
	public readonly Vector2 AnyCardHoveredSpacing;

	public readonly Vector2 NoneCardHoveredSpacing;

	public CardsHighlightLayoutGroupSettings(Vector2 anyCardHoveredSpacing, Vector2 noneCardHoveredSpacing)
	{
		AnyCardHoveredSpacing = anyCardHoveredSpacing;
		NoneCardHoveredSpacing = noneCardHoveredSpacing;
	}
}
