using System;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MenuSuboption : IMenuSuboption
{
	private string nameLocKey;

	private MenuOptionIcon icon;

	private Action onSelected;

	private Action onDeselected;

	private bool interactable;

	private bool selected;

	private string tooltip;

	private BasicEventHandler onUpdatedInteractable;

	private BasicEventHandler onToggledSelect;

	private BasicEventHandler onClearFrame;

	public string NameLocKey => nameLocKey;

	public Sprite Icon => icon.Icon;

	public Sprite IconHighlight => icon.IconHighlight;

	public bool IsInteractable
	{
		get
		{
			return interactable;
		}
		set
		{
			if (interactable != value)
			{
				interactable = value;
				onUpdatedInteractable?.Invoke();
			}
		}
	}

	public string Tooltip => tooltip;

	public MenuSuboption(string nameLocKey, MenuOptionIcon icon, Action onSelected, Action onDeselected = null, bool interactable = true, string tooltip = null)
	{
		this.nameLocKey = nameLocKey;
		this.icon = icon;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		this.interactable = interactable;
		this.tooltip = tooltip;
		Reset();
	}

	public void Reset()
	{
		onUpdatedInteractable = null;
		onToggledSelect = null;
	}

	public void RegisterOnUpdatedInteraction(Action<bool> callback)
	{
		onUpdatedInteractable = (BasicEventHandler)Delegate.Combine(onUpdatedInteractable, (BasicEventHandler)delegate
		{
			callback(IsInteractable);
		});
	}

	public void RegisterOnToggledSelect(Action<bool> callback)
	{
		onToggledSelect = (BasicEventHandler)Delegate.Combine(onToggledSelect, (BasicEventHandler)delegate
		{
			callback(selected);
		});
	}

	public void Deselect()
	{
		if (selected)
		{
			selected = false;
			onToggledSelect();
		}
	}

	public void Select()
	{
		if (!selected)
		{
			selected = true;
			onToggledSelect();
		}
	}

	public void OnSelected()
	{
		if (!PlatformLayer.Instance.IsConsole)
		{
			Singleton<PromotionDLCManager>.Instance?.Hide();
		}
		selected = true;
		onSelected?.Invoke();
	}

	public void OnDeselected()
	{
		if (!PlatformLayer.Instance.IsConsole)
		{
			Singleton<PromotionDLCManager>.Instance?.Show();
		}
		selected = false;
		onDeselected?.Invoke();
	}

	public void ClearFrame()
	{
		onClearFrame?.Invoke();
	}

	public void RegisterOnClearFrame(Action callback)
	{
		onClearFrame = (BasicEventHandler)Delegate.Combine(onClearFrame, (BasicEventHandler)delegate
		{
			callback();
		});
	}
}
