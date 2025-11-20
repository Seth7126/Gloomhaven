using UnityEngine;
using UnityEngine.Events;

public class ControllerInputNavigableBar : ControllerInputElement
{
	[SerializeField]
	private KeyAction m_NavigatePreviousKeyAction;

	[SerializeField]
	private KeyAction m_NavigateNextKeyAction;

	[SerializeField]
	private UIControllerKeyTip m_PreviousControllerTip;

	[SerializeField]
	private UIControllerKeyTip m_NextControllerTip;

	public UnityEvent OnNavigatePrevious;

	public UnityEvent OnNavigateNext;

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		InitializePrevious();
		InitializeNext();
	}

	private void InitializePrevious()
	{
		InputManager.RegisterToOnPressed(m_NavigatePreviousKeyAction, OnNavigatePrevious.Invoke);
		SetBinding(m_NavigatePreviousKeyAction, m_PreviousControllerTip);
	}

	private void InitializeNext()
	{
		InputManager.RegisterToOnPressed(m_NavigateNextKeyAction, OnNavigateNext.Invoke);
		SetBinding(m_NavigateNextKeyAction, m_NextControllerTip);
	}

	private void SetBinding(KeyAction keyAction, UIControllerKeyTip controllerTip)
	{
		if (controllerTip != null)
		{
			controllerTip.Show(keyAction);
			InputManager.RegisterToOnBindingChanged(keyAction, controllerTip.Show);
		}
	}

	private void ClearBinding(KeyAction keyAction, UIControllerKeyTip controllerTip)
	{
		if (controllerTip != null)
		{
			controllerTip.Hide();
			InputManager.UnregisterToOnBindingChanged(keyAction, controllerTip.Show);
		}
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		InputManager.UnregisterToOnPressed(m_NavigatePreviousKeyAction, OnNavigatePrevious.Invoke);
		InputManager.UnregisterToOnPressed(m_NavigateNextKeyAction, OnNavigateNext.Invoke);
		ClearBinding(m_NavigatePreviousKeyAction, m_PreviousControllerTip);
		ClearBinding(m_NavigateNextKeyAction, m_NextControllerTip);
	}
}
