#define ENABLE_LOGS
using System;
using System.Linq;
using UnityEngine;

public class ControllerInputAreaCustom : IControllerInputArea
{
	private Action onFocused;

	private Action onUnfocused;

	private bool stackArea;

	private KeyAction[] disabledKeyActionsOnFocused;

	private KeyAction[] disabledKeyActionsOnEnabled;

	protected bool isEnabled;

	[field: SerializeField]
	public bool BlockOthersFocusWhileIsFocused { get; set; }

	public string Id { get; private set; }

	public bool IsFocused { get; private set; }

	public event Action OnFocused;

	public event Action OnUnfocused;

	public ControllerInputAreaCustom(string id, Action onFocused = null, Action onUnfocused = null, bool stackArea = false, EKeyActionTag disabledKeyActionTagOnFocused = EKeyActionTag.None)
		: this(id, onFocused, onUnfocused, stackArea, InputManager.GetKeyActionsAssociated(disabledKeyActionTagOnFocused).ToArray())
	{
	}

	public ControllerInputAreaCustom(string id, Action onFocused = null, Action onUnfocused = null, bool stackArea = false, KeyAction[] disabledKeyActionsOnFocused = null, params KeyAction[] disabledKeyActionsOnEnabled)
	{
		this.onFocused = onFocused;
		this.onUnfocused = onUnfocused;
		Id = id;
		this.stackArea = stackArea;
		this.disabledKeyActionsOnFocused = disabledKeyActionsOnFocused;
		this.disabledKeyActionsOnEnabled = disabledKeyActionsOnEnabled;
		ControllerInputAreaManager.Instance?.RegisterArea(this);
	}

	public void EnableGroup(bool isFocused)
	{
		isEnabled = true;
		InputManager.RequestDisableInput(this, disabledKeyActionsOnEnabled);
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
			InputManager.RequestEnableInput(this, disabledKeyActionsOnEnabled);
		}
		if (IsFocused)
		{
			SetUnfocused();
		}
	}

	public void Destroy()
	{
		Unfocus();
		DisableGroup();
		ControllerInputAreaManager.Instance?.UnregisterArea(this);
	}

	public virtual void Focus()
	{
		if (isEnabled)
		{
			if (!IsFocused)
			{
				Debug.LogController("Focus area " + Id);
				IsFocused = true;
				InputManager.RequestDisableInput(this, disabledKeyActionsOnFocused);
				ControllerInputAreaManager.Instance.FocusArea(Id, stackArea);
				onFocused?.Invoke();
				this.OnFocused?.Invoke();
			}
		}
		else
		{
			ControllerInputAreaManager.Instance.FocusArea(Id, stackArea);
		}
	}

	public virtual void Unfocus()
	{
		SetUnfocused();
		ControllerInputAreaManager.Instance.UnfocusArea(Id);
	}

	protected virtual void SetUnfocused()
	{
		if (IsFocused)
		{
			Debug.LogController("Unfocus area " + Id);
			InputManager.RequestEnableInput(this, disabledKeyActionsOnFocused);
			IsFocused = false;
			onUnfocused?.Invoke();
			this.OnUnfocused?.Invoke();
		}
	}
}
