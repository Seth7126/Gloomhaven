using System;
using UnityEngine;

public abstract class PickController : IEscapable
{
	[SerializeField]
	protected bool m_IsAllowedToEscapeDuringSave;

	protected Action onOpenPicker;

	protected Action onClosePicker;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	public PickController SetOnOpenPicker(Action callback)
	{
		onOpenPicker = callback;
		return this;
	}

	public PickController SetOnClosePicker(Action callback)
	{
		onClosePicker = callback;
		return this;
	}

	public abstract bool Pick();

	public abstract bool IsSelecting();

	public abstract void ClosePicker();

	public virtual void InitializePicker()
	{
	}

	public bool Escape()
	{
		if (!IsSelecting())
		{
			return false;
		}
		ClosePicker();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
