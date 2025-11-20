using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenuOption : UIMenuOptionToggle
{
	[Header("UIMainMenuOption")]
	[SerializeField]
	protected TextMeshProUGUI text;

	[SerializeField]
	private Color defaultTextColor;

	[SerializeField]
	private Color higlightedTextColor;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	[SerializeField]
	protected Image focusMask;

	[SerializeField]
	protected List<Graphic> interactionMasks;

	[Header("Locked")]
	[SerializeField]
	private GameObject lockedMask;

	[SerializeField]
	private Color lockedTextColor;

	private bool isFocused;

	public bool IsInteractable
	{
		get
		{
			return toggle.interactable;
		}
		set
		{
			if (value != toggle.interactable)
			{
				toggle.interactable = value;
				lockedMask.SetActive(!value);
				RefreshHighlight();
				for (int i = 0; i < interactionMasks.Count; i++)
				{
					interactionMasks[i].material = (toggle.interactable ? null : UIInfoTools.Instance.greyedOutMaterial);
				}
				if (!toggle.interactable && isSelected)
				{
					Deselect();
				}
			}
		}
	}

	public override void Init(Action onSelected, Action onDeselected = null, Action<bool> onHovered = null, bool isSelected = false)
	{
		base.Init(onSelected, onDeselected, onHovered, isSelected);
		IsInteractable = true;
		SetTooltip(enable: false);
		SetFocused(isFocused: true);
	}

	public void Highlight(bool isHighlighted)
	{
		OnHovered(isHighlighted);
		RefreshHighlight();
	}

	protected override void RefreshHighlight(bool isHighlighted)
	{
		base.RefreshHighlight(isHighlighted && toggle.interactable);
		text.enableVertexGradient = !isHighlighted;
		text.color = ((!toggle.interactable) ? lockedTextColor : (isHighlighted ? higlightedTextColor : defaultTextColor));
		focusMask.enabled = !isFocused && !isHighlighted;
	}

	public void ResetInteractionMasks()
	{
		for (int i = 0; i < interactionMasks.Count; i++)
		{
			interactionMasks[i].material = null;
		}
	}

	public void SetTooltip(bool enable, string tooltipText = null)
	{
		if (!(tooltip == null))
		{
			if (tooltipText.IsNOTNullOrEmpty())
			{
				tooltip.SetText(tooltipText);
			}
			tooltip.enabled = enable;
		}
	}

	public void SetFocused(bool isFocused)
	{
		this.isFocused = isFocused;
		focusMask.enabled = !isFocused && !isSelected && !isHovered;
	}
}
