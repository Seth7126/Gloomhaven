using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class SelectableExtensions
{
	public static void SetNavigation(this Selectable selectable, Navigation.Mode mode, bool wrapAround = false)
	{
		Navigation navigation = selectable.navigation;
		navigation.mode = mode;
		navigation.wrapAround = wrapAround;
		selectable.navigation = navigation;
	}

	public static void ClearNavigation(this Selectable selectable)
	{
		Navigation navigation = selectable.navigation;
		navigation.selectOnDown = null;
		navigation.selectOnRight = null;
		navigation.selectOnLeft = null;
		navigation.selectOnUp = null;
		navigation.wrapAround = false;
		selectable.navigation = navigation;
	}

	public static void SetNavigation(this Selectable selectable, Selectable right = null, Selectable left = null, Selectable up = null, Selectable down = null, bool wrapAround = false)
	{
		Navigation navigation = selectable.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		if (down != null)
		{
			navigation.selectOnDown = down;
		}
		if (right != null)
		{
			navigation.selectOnRight = right;
		}
		if (left != null)
		{
			navigation.selectOnLeft = left;
		}
		if (up != null)
		{
			navigation.selectOnUp = up;
		}
		navigation.wrapAround = wrapAround;
		selectable.navigation = navigation;
	}

	public static void DisableNavigation(this Selectable selectable, bool clear = false, bool deselect = true)
	{
		selectable.SetNavigation(Navigation.Mode.None);
		if (clear)
		{
			selectable.ClearNavigation();
		}
		if (deselect && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == selectable.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}
}
