using System;
using UnityEngine;

namespace Script.GUI.SMNavigation;

public abstract class StateSwitcher<TTag> : MonoBehaviour where TTag : Enum
{
	[SerializeField]
	private TTag _defaultState;

	public void Switch()
	{
		Switch(_defaultState);
	}

	public void Switch(TTag to)
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(to);
	}
}
