using UnityEngine;

namespace SM.Gamepad;

public abstract class NavigationAction : ScriptableObject
{
	public struct NavigationActionArgs
	{
		public UiNavigationBase NavigationElement;

		public MonoBehaviour[] Components;
	}

	public abstract void HandleAction(NavigationActionArgs args);
}
