using SM.Gamepad;
using UnityEngine;

namespace Script.GUI;

public abstract class BaseNavigationFilter : MonoBehaviour
{
	public abstract bool IsTrue(IUiNavigationElement navigationElement);
}
