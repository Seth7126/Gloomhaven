using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ControllerInputEscapableArea : ControllerInputArea, IEscapable
{
	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	public UnityEvent OnEscape;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public override void DisableGroup()
	{
		base.DisableGroup();
		UIWindowManager.UnregisterEscapable(this);
	}

	public override void Focus()
	{
		if (isEnabled)
		{
			UIWindowManager.RegisterEscapable(this);
		}
		base.Focus();
	}

	public override void Unfocus()
	{
		UIWindowManager.UnregisterEscapable(this);
		base.Unfocus();
	}

	public virtual bool Escape()
	{
		if (base.IsFocused)
		{
			Unfocus();
			OnEscape?.Invoke();
			return true;
		}
		return false;
	}

	protected new virtual void OnDestroy()
	{
		if (!CoreApplication.IsQuitting)
		{
			UIWindowManager.UnregisterEscapable(this);
		}
	}

	public int Order()
	{
		return 0;
	}
}
