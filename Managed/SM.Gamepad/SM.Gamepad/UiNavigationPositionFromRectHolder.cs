using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationPositionFromRectHolder : UiNavigationCustomizablePositionCalculatorHolder
{
	[SerializeField]
	private RectTransform _rect;

	private IUiNavigationPositionCalculator _calculator;

	public override IUiNavigationPositionCalculator GetCalculator()
	{
		if (_rect == null)
		{
			_rect = GetComponent<RectTransform>();
		}
		return _calculator ?? new UiNavigationPositionFromRect(_rect);
	}
}
