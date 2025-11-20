using UnityEngine;

namespace Script.GUI.SMNavigation.Utils;

public static class UISettings
{
	private static readonly Vector3 _pcHoverScale = new Vector3(1.1f, 1.1f, 1.1f);

	private static readonly Vector3 _hoverScale = new Vector3(1.04f, 1.04f, 1.04f);

	private static readonly Vector3 _gamepadFullCardViewDefaultScale = new Vector3(0.75f, 0.75f, 0.75f);

	private static readonly Vector3 _gamepadFullCardViewAnotherCardHoveredScale = new Vector3(0.85f, 0.85f, 0.85f);

	private static readonly Vector3 _gamepadFullCardViewPadding = new Vector3(40f, -70f, 0f);

	private static readonly Vector3 _gamepadFullCardViewPaddingAnotherCardHovered = new Vector3(40f, 0f, 0f);

	private static readonly Vector2 _anyCardHoveredSpacing = new Vector2(30f, 0f);

	private static readonly Vector2 _noneCardHoveredSpacing = new Vector2(30f, -35f);

	private static readonly FullCardViewSettings _defaultFullCardViewOnActionSelectSettings = new FullCardViewSettings(Vector3.one, _pcHoverScale, Vector3.zero, Vector3.one, Vector3.zero);

	private static readonly FullCardViewSettings _gamePadFullCardViewOnActionSelectSettings = new FullCardViewSettings(_gamepadFullCardViewDefaultScale, _hoverScale, _gamepadFullCardViewPadding, _gamepadFullCardViewAnotherCardHoveredScale, _gamepadFullCardViewPaddingAnotherCardHovered);

	public static readonly FullCardViewSettings DefaultFullCardViewSettings = new FullCardViewSettings(Vector3.one, Vector3.one);

	public static FullCardViewSettings FullCardViewInActionSelect
	{
		get
		{
			if (!InputManager.GamePadInUse)
			{
				return _defaultFullCardViewOnActionSelectSettings;
			}
			return _gamePadFullCardViewOnActionSelectSettings;
		}
	}

	public static CardsHighlightLayoutGroupSettings CardsHighlightLayoutGroupSettings => new CardsHighlightLayoutGroupSettings(_anyCardHoveredSpacing, _noneCardHoveredSpacing);
}
