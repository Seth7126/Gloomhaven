using System;
using UnityEngine;

namespace Script.GUI.SMNavigation;

[RequireComponent(typeof(UIKeyboard))]
public abstract class KeyboardStateSwitcher<TTag> : MonoBehaviour where TTag : Enum
{
	[SerializeField]
	private TTag _enteringTag;

	[SerializeField]
	private TTag _exitTag;

	[SerializeField]
	private bool _toPreviousStateOnExit;

	private UIKeyboard _uiKeyboard;

	private void Awake()
	{
		_uiKeyboard = GetComponent<UIKeyboard>();
		UIKeyboard uiKeyboard = _uiKeyboard;
		uiKeyboard.OnShow = (Action)Delegate.Combine(uiKeyboard.OnShow, new Action(OnShow));
		UIKeyboard uiKeyboard2 = _uiKeyboard;
		uiKeyboard2.OnHide = (Action)Delegate.Combine(uiKeyboard2.OnHide, new Action(OnHide));
	}

	private void OnDestroy()
	{
		if (!(_uiKeyboard == null))
		{
			UIKeyboard uiKeyboard = _uiKeyboard;
			uiKeyboard.OnShow = (Action)Delegate.Remove(uiKeyboard.OnShow, new Action(OnShow));
			UIKeyboard uiKeyboard2 = _uiKeyboard;
			uiKeyboard2.OnHide = (Action)Delegate.Remove(uiKeyboard2.OnHide, new Action(OnHide));
		}
	}

	private void OnShow()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(_enteringTag);
	}

	private void OnHide()
	{
		if (_toPreviousStateOnExit)
		{
			Singleton<UINavigation>.Instance.StateMachine.ToNonMenuPreviousState();
		}
		else
		{
			Singleton<UINavigation>.Instance.StateMachine.Enter(_exitTag);
		}
	}
}
