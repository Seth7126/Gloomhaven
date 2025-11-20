using System;
using UnityEngine.UI;

namespace SM.Gamepad;

public interface IUiNavigationSelectable : IUiNavigationElement
{
	Selectable ControlledSelectable { get; }

	event Action<IUiNavigationSelectable> OnPointerEnterEvent;

	event Action<IUiNavigationSelectable> OnPointerExitEvent;

	void OnNavigationSelected();

	void OnNavigationDeselected();
}
