#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ControllerInputArea : MonoBehaviour, IControllerInputArea
{
	[SerializeField]
	private EControllerInputAreaType m_Id;

	[SerializeField]
	[ConditionalField("m_Id", "None", true)]
	private string m_CustomId;

	[Header("Enable/Disable")]
	public UnityEvent OnEnabledArea;

	public UnityEvent OnDisabledArea;

	[SerializeField]
	[Tooltip("Auto unfocus when group is disabled (switched to keyboard).\nEnable this if this area has to autofocus when a controller is detected (like confirmation boxes).\nDisabled this if focusing to this area is optional (shortcurts to areas)")]
	private bool unfocusOnDisableGroup = true;

	[Header("Focus/Unfocus")]
	[SerializeField]
	[Tooltip("Enable this to stack the current area focused when this area focuses, and when this area is unfocused it will automatically focus the stacked area")]
	private bool stackFocus;

	[SerializeField]
	[Tooltip("Objects enabled when this are is focused")]
	private List<GameObject> m_FocusMasks;

	[SerializeField]
	private UIControllerKeyTip focusTip;

	[SerializeField]
	[Tooltip("Object selected when this are is focused")]
	private GameObject m_OnFocusedObject;

	public UnityEvent OnFocused;

	public UnityEvent OnUnfocused;

	[Tooltip("Canvas groups that will only be interactable while this area is focused")]
	[SerializeField]
	private CanvasGroup[] m_CanvasGroups;

	[Header("Autofocus")]
	[SerializeField]
	private KeyAction keyActionAutofocus = KeyAction.None;

	[SerializeField]
	[ConditionalField("keyActionAutofocus", KeyAction.None, false)]
	[Tooltip("Areas from which this area won't be able to autofocus pressing the keyActionAutofocus")]
	private EControllerInputAreaType[] skipAutofocusFromAreas;

	[Header("Key Actions")]
	[SerializeField]
	private EKeyActionTag m_AvailableKeyActionTagsOnFocused;

	[SerializeField]
	private KeyAction[] m_AvailableKeyActionsOnFocused;

	[SerializeField]
	private EKeyActionTag m_DisabledKeyActionTagsOnFocused;

	[SerializeField]
	private KeyAction[] m_DisabledKeyActionsOnFocused;

	private bool isFocused;

	protected bool isEnabled;

	[field: SerializeField]
	public bool BlockOthersFocusWhileIsFocused { get; private set; }

	public string Id
	{
		get
		{
			if (m_Id != EControllerInputAreaType.None)
			{
				return m_Id.ToString();
			}
			return m_CustomId;
		}
	}

	public bool IsEnabled => isEnabled;

	public bool IsFocused => isFocused;

	protected virtual void OnDestroy()
	{
		OnEnabledArea.RemoveAllListeners();
		OnDisabledArea.RemoveAllListeners();
		OnFocused.RemoveAllListeners();
		OnUnfocused.RemoveAllListeners();
	}

	protected virtual void OnEnable()
	{
		for (int i = 0; i < m_FocusMasks.Count; i++)
		{
			m_FocusMasks[i].SetActive(value: false);
		}
		ControllerInputAreaManager.Instance?.RegisterArea(this);
	}

	protected void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			UnregisterGroup();
		}
	}

	public void UnregisterGroup()
	{
		ControllerInputAreaManager.Instance?.UnregisterArea(this);
		if (isEnabled)
		{
			DisableGroup();
		}
	}

	public virtual void EnableGroup(bool isFocused)
	{
		isEnabled = true;
		for (int i = 0; i < m_CanvasGroups.Length; i++)
		{
			m_CanvasGroups[i].blocksRaycasts = false;
		}
		OnEnabledArea.Invoke();
		focusTip?.Show();
		if (isFocused)
		{
			Focus();
		}
		else
		{
			SetUnfocused();
		}
		if (keyActionAutofocus != KeyAction.None)
		{
			InputManager.RegisterToOnPressed(keyActionAutofocus, Autofocus);
		}
	}

	public void SetKeyActionAutofocus(KeyAction keyAction)
	{
		if (keyActionAutofocus == keyAction)
		{
			return;
		}
		if (isEnabled)
		{
			if (keyActionAutofocus != KeyAction.None)
			{
				InputManager.UnregisterToOnPressed(keyActionAutofocus, Autofocus);
			}
			keyActionAutofocus = keyAction;
			if (keyActionAutofocus != KeyAction.None)
			{
				InputManager.RegisterToOnPressed(keyActionAutofocus, Autofocus);
			}
		}
		else
		{
			keyActionAutofocus = keyAction;
		}
	}

	private void Autofocus()
	{
		if (!skipAutofocusFromAreas.Any(ControllerInputAreaManager.IsFocusedArea))
		{
			Focus();
		}
	}

	public virtual void DisableGroup()
	{
		if (isEnabled)
		{
			if (keyActionAutofocus != KeyAction.None)
			{
				InputManager.UnregisterToOnPressed(keyActionAutofocus, Autofocus);
			}
			InputManager.RequestEnableInput(this, m_AvailableKeyActionTagsOnFocused);
			InputManager.RequestEnableInput(this, m_AvailableKeyActionsOnFocused);
		}
		isEnabled = false;
		if (unfocusOnDisableGroup && !SceneController.Instance.IsLoading)
		{
			Unfocus();
			ControllerInputAreaManager.Instance.RemoveStackedArea(Id);
		}
		else
		{
			SetUnfocused();
		}
		for (int i = 0; i < m_CanvasGroups.Length; i++)
		{
			m_CanvasGroups[i].blocksRaycasts = true;
		}
		focusTip?.Hide();
		OnDisabledArea.Invoke();
	}

	public virtual void Focus()
	{
		if (IsFocused)
		{
			return;
		}
		if (isEnabled)
		{
			Debug.LogController("Focus area " + Id);
			isFocused = true;
			for (int i = 0; i < m_CanvasGroups.Length; i++)
			{
				m_CanvasGroups[i].blocksRaycasts = true;
			}
			for (int j = 0; j < m_FocusMasks.Count; j++)
			{
				m_FocusMasks[j]?.SetActive(value: true);
			}
			InputManager.RequestEnableInput(this, m_AvailableKeyActionTagsOnFocused);
			InputManager.RequestEnableInput(this, m_AvailableKeyActionsOnFocused);
			InputManager.RequestDisableInput(this, m_DisabledKeyActionTagsOnFocused);
			InputManager.RequestDisableInput(this, m_DisabledKeyActionsOnFocused);
			ControllerInputAreaManager.Instance?.FocusArea(this, stackFocus);
			focusTip?.Hide();
			if (m_OnFocusedObject != null)
			{
				EventSystem.current.SetSelectedGameObject(m_OnFocusedObject.gameObject);
			}
			OnFocused.Invoke();
		}
		else
		{
			ControllerInputAreaManager.Instance?.FocusArea(this, stackFocus);
		}
	}

	public virtual void Unfocus()
	{
		if (IsFocused)
		{
			SetUnfocused();
		}
		ControllerInputAreaManager.Instance?.UnfocusArea(this);
	}

	private void SetUnfocused()
	{
		if (isFocused)
		{
			InputManager.RequestEnableInput(this, m_DisabledKeyActionTagsOnFocused);
			InputManager.RequestEnableInput(this, m_DisabledKeyActionsOnFocused);
			isFocused = false;
			if (m_OnFocusedObject != null && m_OnFocusedObject == EventSystem.current.currentSelectedGameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			OnUnfocused.Invoke();
		}
		for (int i = 0; i < m_FocusMasks.Count; i++)
		{
			m_FocusMasks[i].SetActive(value: false);
		}
		for (int j = 0; j < m_CanvasGroups.Length; j++)
		{
			m_CanvasGroups[j].blocksRaycasts = false;
		}
		if (isEnabled)
		{
			InputManager.RequestDisableInput(this, m_AvailableKeyActionTagsOnFocused);
			InputManager.RequestDisableInput(this, m_AvailableKeyActionsOnFocused);
			focusTip?.Show();
		}
	}
}
