using System;
using SM.Gamepad;
using UnityEngine;

namespace Script.GUI.SMNavigation;

public class UINavigation : Singleton<UINavigation>
{
	public SmInput Input { get; private set; }

	public UiNavigationManager NavigationManager { get; private set; }

	public NavigationStateMachine StateMachine { get; private set; }

	public static event Action<UINavigation> OnInitialize;

	public void Setup()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		Input = new SmInput();
		NavigationManager = new UiNavigationManager(Input);
		StateMachine = new NavigationStateMachine(NavigationManager);
		UINavigation.OnInitialize?.Invoke(this);
	}

	public void Reset()
	{
		StateMachine.Reset();
	}
}
