using System;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI;

public class MonoHotkeySessionOnShow : MonoHotkeySession
{
	[SerializeField]
	private UIWindow _controllerArea;

	private void Awake()
	{
		UIWindow controllerArea = _controllerArea;
		controllerArea.OnShow = (Action)Delegate.Combine(controllerArea.OnShow, new Action(base.Show));
		UIWindow controllerArea2 = _controllerArea;
		controllerArea2.OnHide = (Action)Delegate.Combine(controllerArea2.OnHide, new Action(base.Hide));
	}
}
