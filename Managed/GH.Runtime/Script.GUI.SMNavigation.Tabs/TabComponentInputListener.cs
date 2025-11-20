using System;
using SM.Gamepad;
using Script.GUI.SMNavigation.Input;
using UnityEngine;

namespace Script.GUI.SMNavigation.Tabs;

[RequireComponent(typeof(UINavigationTabComponent))]
public class TabComponentInputListener : KeyActionInputListener
{
	[SerializeField]
	private UINavigationTabComponent _tab;

	private Func<bool> _nextCondition;

	private Func<bool> _prevCondition;

	protected override void Awake()
	{
		if (_tab == null)
		{
			_tab = GetComponent<UINavigationTabComponent>();
		}
		base.Awake();
	}

	public void SetTabChangeConditions(Func<bool> nextCondition, Func<bool> prevCondition)
	{
		_nextCondition = nextCondition;
		_prevCondition = prevCondition;
	}

	protected override void Next()
	{
		if (_nextCondition != null)
		{
			if (_nextCondition())
			{
				_tab.Next();
				base.Next();
			}
		}
		else
		{
			_tab.Next();
			base.Next();
		}
	}

	protected override void Previous()
	{
		if (_prevCondition != null)
		{
			if (_prevCondition())
			{
				_tab.Previous();
				base.Previous();
			}
		}
		else
		{
			_tab.Previous();
			base.Previous();
		}
	}
}
