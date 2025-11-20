#define ENABLE_LOGS
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerInputClickeable : ControllerInputElement
{
	private enum EClickMode
	{
		None,
		Selected
	}

	[SerializeField]
	private Selectable m_Selectable;

	[SerializeField]
	private KeyAction m_KeyAction;

	[SerializeField]
	private EClickMode m_Mode;

	[SerializeField]
	private EControllerInputAreaType m_AreaAssociated;

	[SerializeField]
	private UIControllerKeyTip controllerTip;

	private bool listenClick = true;

	private void Awake()
	{
		if (m_Selectable == null)
		{
			m_Selectable = GetComponent<Selectable>();
		}
	}

	private void OnDestroy()
	{
		OnDisabledControllerControl();
	}

	private void OnValidate()
	{
		if (m_Selectable == null)
		{
			m_Selectable = GetComponent<Selectable>();
		}
	}

	protected override void OnEnabledControllerControl()
	{
		base.OnEnabledControllerControl();
		if (m_KeyAction != KeyAction.None)
		{
			if (listenClick)
			{
				InputManager.RegisterToOnPressed(m_KeyAction, Click);
			}
			SetBinding(m_KeyAction);
		}
	}

	private void SetBinding(KeyAction keyAction)
	{
		if (controllerTip != null)
		{
			controllerTip.Show(keyAction);
			InputManager.RegisterToOnBindingChanged(keyAction, controllerTip.Show);
		}
	}

	protected override void OnDisabledControllerControl()
	{
		base.OnDisabledControllerControl();
		if (m_KeyAction != KeyAction.None)
		{
			InputManager.UnregisterToOnPressed(m_KeyAction, Click);
			if (controllerTip != null)
			{
				controllerTip.Hide();
				InputManager.UnregisterToOnBindingChanged(m_KeyAction, controllerTip.Show);
			}
		}
	}

	private void Click()
	{
		if (!base.isActiveAndEnabled)
		{
			Debug.LogWarningController($"Clicked disabled {base.name} {m_KeyAction}");
		}
		else if ((m_AreaAssociated == EControllerInputAreaType.None || ControllerInputAreaManager.IsFocusedArea(m_AreaAssociated)) && m_Selectable.IsInteractable() && (m_Mode != EClickMode.Selected || EventSystem.current.currentSelectedGameObject == m_Selectable.gameObject))
		{
			ExecuteEvents.Execute(m_Selectable.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
		}
	}

	public void SetKeyAction(KeyAction keyAction, bool listenClick = true)
	{
		this.listenClick = listenClick;
		if (m_KeyAction == keyAction)
		{
			InputManager.UnregisterToOnPressed(m_KeyAction, Click);
			if (listenClick && isEnabled)
			{
				InputManager.RegisterToOnPressed(keyAction, Click);
			}
			return;
		}
		if (isEnabled)
		{
			if (m_KeyAction != KeyAction.None)
			{
				InputManager.UnregisterToOnPressed(m_KeyAction, Click);
				InputManager.UnregisterToOnBindingChanged(m_KeyAction, controllerTip.Show);
			}
			SetBinding(keyAction);
			if (listenClick && keyAction != KeyAction.None)
			{
				InputManager.RegisterToOnPressed(keyAction, Click);
			}
		}
		m_KeyAction = keyAction;
	}

	public void RefreshInteractable()
	{
		controllerTip?.ShowInteractable(m_Selectable.IsInteractable());
	}
}
