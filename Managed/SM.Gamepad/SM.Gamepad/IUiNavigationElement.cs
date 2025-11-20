using System;
using UnityEngine;

namespace SM.Gamepad;

public interface IUiNavigationElement
{
	string NavigationName { get; }

	int NavigationPriority { get; }

	string[] NavigationTags { get; }

	GameObject GameObject { get; }

	string ElementID { get; }

	Vector2 NavigationPosition { get; }

	UiNavigationGroup Parent { get; }

	IUiNavigationRoot Root { get; }

	IUiNavigationManager UiNavigationManager { get; }

	RectTransform RectTransform { get; }

	event Action<IUiNavigationElement> DestroyedEvent;

	void RefreshParent();

	void ClearParent();

	void ClearRoot();
}
