#define ENABLE_LOGS
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ControllerInputAreaLocal : MonoBehaviour, IControllerInputArea
{
	[SerializeField]
	private EControllerInputAreaType m_Id;

	[SerializeField]
	[ConditionalField("m_Id", "None", true)]
	private string m_CustomId;

	public UnityEvent OnFocusedArea;

	public UnityEvent OnUnfocusedArea;

	public UnityEvent OnEnabledArea;

	public UnityEvent OnDisabledArea;

	[SerializeField]
	private EKeyActionTag m_DisabledKeyActionTagsOnFocused;

	[SerializeField]
	private KeyAction[] m_DisabledKeyActionsOnFocused;

	[SerializeField]
	private KeyAction[] m_DisabledExcludedKeyActions;

	[SerializeField]
	[Tooltip("Object selected when this are is focused")]
	private GameObject m_OnFocusedObject;

	[SerializeField]
	private List<ControllerInputElement> m_EnabledElementsOnFocused;

	[SerializeField]
	[Tooltip("Enable this to stack the current area focused when this area focuses, and when this area is unfocused it will automatically focus the stacked area")]
	private bool stackPreviousArea = true;

	[SerializeField]
	[Tooltip("Enable this to stack this area when it's unfocused")]
	private bool stackUnfocus;

	private bool isEnabled;

	[field: SerializeField]
	public bool BlockOthersFocusWhileIsFocused { get; private set; }

	public bool IsEnabled => isEnabled;

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

	public bool IsFocused { get; private set; }

	private void OnDestroy()
	{
		OnFocusedArea.RemoveAllListeners();
		OnUnfocusedArea.RemoveAllListeners();
		OnEnabledArea.RemoveAllListeners();
		OnDisabledArea.RemoveAllListeners();
		m_OnFocusedObject = null;
		m_EnabledElementsOnFocused = null;
	}

	public void EnableGroup(bool isFocused)
	{
		if (!isEnabled)
		{
			isEnabled = true;
			OnEnabledArea?.Invoke();
		}
		if (isFocused)
		{
			Focus();
		}
	}

	public void DisableGroup()
	{
		if (isEnabled)
		{
			isEnabled = false;
			SetUnfocused();
			OnDisabledArea.Invoke();
		}
		else
		{
			SetUnfocused();
		}
	}

	public void Enable()
	{
		ControllerInputAreaManager.Instance?.RegisterArea(this);
		Focus();
	}

	public void Destroy()
	{
		Unfocus();
		DisableGroup();
		ControllerInputAreaManager.Instance?.UnregisterArea(this);
	}

	public void Focus()
	{
		if (IsFocused)
		{
			return;
		}
		if (isEnabled)
		{
			Debug.LogController("Focus area " + Id);
			IsFocused = true;
			InputManager.RequestDisableInput(this, m_DisabledKeyActionTagsOnFocused, m_DisabledExcludedKeyActions);
			InputManager.RequestDisableInput(this, m_DisabledKeyActionsOnFocused);
			for (int i = 0; i < m_EnabledElementsOnFocused.Count; i++)
			{
				m_EnabledElementsOnFocused[i].enabled = true;
			}
			ControllerInputAreaManager.Instance?.FocusArea(Id, stackPreviousArea);
			if (m_OnFocusedObject != null)
			{
				EventSystem.current.SetSelectedGameObject(m_OnFocusedObject);
			}
			OnFocusedArea.Invoke();
		}
		else
		{
			ControllerInputAreaManager.Instance?.FocusArea(Id, stackPreviousArea);
		}
	}

	public void Unfocus()
	{
		SetUnfocused();
		ControllerInputAreaManager.Instance?.UnfocusArea(Id, stackUnfocus && isEnabled);
	}

	private void SetUnfocused()
	{
		if (IsFocused)
		{
			IsFocused = false;
			InputManager.RequestEnableInput(this, m_DisabledKeyActionTagsOnFocused, m_DisabledExcludedKeyActions);
			InputManager.RequestEnableInput(this, m_DisabledKeyActionsOnFocused);
			for (int i = 0; i < m_EnabledElementsOnFocused.Count; i++)
			{
				m_EnabledElementsOnFocused[i].enabled = false;
			}
			if (m_OnFocusedObject != null && m_OnFocusedObject == EventSystem.current.currentSelectedGameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			OnUnfocusedArea.Invoke();
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			Destroy();
		}
	}
}
