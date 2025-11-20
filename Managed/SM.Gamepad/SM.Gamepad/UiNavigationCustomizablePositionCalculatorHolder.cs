using UnityEngine;

namespace SM.Gamepad;

public abstract class UiNavigationCustomizablePositionCalculatorHolder : MonoBehaviour
{
	public abstract IUiNavigationPositionCalculator GetCalculator();
}
