using AsmodeeNet.Foundation;
using UnityEngine;

public class UIControllerKeyTracker : MonoBehaviour
{
	[SerializeField]
	private KeyAction m_KeyAction = KeyAction.None;

	[SerializeField]
	private UIControllerKeyTip m_Tip;

	protected void OnEnable()
	{
		InputManager.RegisterToOnBindingChanged(m_KeyAction, RefreshBinding);
		RefreshBinding();
	}

	protected void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			InputManager.UnregisterToOnBindingChanged(m_KeyAction, RefreshBinding);
		}
	}

	private void RefreshBinding()
	{
		if (InputManager.GamePadInUse)
		{
			m_Tip.SetText(m_KeyAction);
		}
	}

	public void SetKeyAction(KeyAction keyAction)
	{
		if (m_KeyAction == keyAction || keyAction == KeyAction.None)
		{
			return;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			m_KeyAction = keyAction;
			return;
		}
		if (m_KeyAction != KeyAction.None)
		{
			InputManager.UnregisterToOnBindingChanged(m_KeyAction, RefreshBinding);
		}
		m_KeyAction = keyAction;
		InputManager.RegisterToOnBindingChanged(m_KeyAction, RefreshBinding);
		RefreshBinding();
	}
}
