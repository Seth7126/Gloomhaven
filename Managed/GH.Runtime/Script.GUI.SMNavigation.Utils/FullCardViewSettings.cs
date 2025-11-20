using UnityEngine;

namespace Script.GUI.SMNavigation.Utils;

public class FullCardViewSettings
{
	public Vector3 DefaultScale;

	public Vector3 AnotherCardHoveredScale;

	public Vector3 HoverScale;

	public bool OverridePosition;

	public Vector3 Position;

	public Vector3 AnotherCardHoveredPosition;

	public FullCardViewSettings(Vector3 defaultScale, Vector3 hoverScale)
	{
		DefaultScale = defaultScale;
		HoverScale = hoverScale;
		OverridePosition = false;
	}

	public FullCardViewSettings(Vector3 defaultScale, Vector3 hoverScale, Vector3 position)
	{
		DefaultScale = defaultScale;
		HoverScale = hoverScale;
		OverridePosition = true;
		Position = position;
	}

	public FullCardViewSettings(Vector3 defaultScale, Vector3 hoverScale, Vector3 position, Vector3 anotherCardHoveredScale, Vector3 anotherCardHoveredPosition)
	{
		DefaultScale = defaultScale;
		HoverScale = hoverScale;
		OverridePosition = true;
		Position = position;
		AnotherCardHoveredScale = anotherCardHoveredScale;
		AnotherCardHoveredPosition = anotherCardHoveredPosition;
	}
}
