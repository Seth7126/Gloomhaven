using System;
using UnityEngine;
using UnityEngine.UI;

public class ControllerInputAreaEscapableCustom : ControllerInputAreaCustom, IEscapable
{
	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public override void Focus()
	{
		if (isEnabled)
		{
			UIWindowManager.RegisterEscapable(this);
		}
		base.Focus();
	}

	protected override void SetUnfocused()
	{
		base.SetUnfocused();
		UIWindowManager.UnregisterEscapable(this);
	}

	public ControllerInputAreaEscapableCustom(string id, Action onFocused = null, Action onUnfocused = null, bool stackArea = false, EKeyActionTag disabledKeyActionTagOnFocused = EKeyActionTag.None)
		: base(id, onFocused, onUnfocused, stackArea, disabledKeyActionTagOnFocused)
	{
	}

	public bool Escape()
	{
		if (base.IsFocused && isEnabled)
		{
			Unfocus();
			return true;
		}
		return false;
	}

	public int Order()
	{
		return 0;
	}
}
