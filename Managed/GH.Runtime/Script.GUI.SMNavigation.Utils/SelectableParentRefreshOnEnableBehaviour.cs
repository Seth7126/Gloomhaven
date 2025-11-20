using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation.Utils;

[RequireComponent(typeof(UINavigationSelectable))]
public class SelectableParentRefreshOnEnableBehaviour : MonoBehaviour
{
	private UINavigationSelectable _selectable;

	private UINavigationSelectable Selectable => _selectable ?? (_selectable = GetComponent<UINavigationSelectable>());

	private void OnEnable()
	{
		Selectable.RefreshParent();
	}
}
