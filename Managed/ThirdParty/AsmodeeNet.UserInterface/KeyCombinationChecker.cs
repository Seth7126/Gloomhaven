using UnityEngine.InputSystem;
using Utilities;

namespace AsmodeeNet.UserInterface;

public static class KeyCombinationChecker
{
	public static bool IsDebugKeyCombination()
	{
		if (InputSystemUtilities.GetKey(Key.LeftAlt) || InputSystemUtilities.GetKey(Key.RightAlt))
		{
			if (!InputSystemUtilities.GetKey(Key.LeftCtrl))
			{
				return InputSystemUtilities.GetKey(Key.RightCtrl);
			}
			return true;
		}
		return false;
	}
}
