using System;
using GLOOM;
using Script.GUI.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIRectangularRaycastFilter))]
public sealed class UITextTooltipTarget : UITooltipTarget
{
	[TextArea(10, 10)]
	[SerializeField]
	public string tooltipText;

	[SerializeField]
	private bool textIsLocalizationKeyInstead;

	[SerializeField]
	private bool textIsLocalizationFormat;

	[SerializeField]
	private bool tempIsPrefabTooltip = true;

	[SerializeField]
	private bool globallyHidable;

	[SerializeField]
	private CanvasGroup _canvas;

	[SerializeField]
	private TMP_SpriteAsset spriteAsset;

	private string shownTooltipText;

	private string shownTooltipSubtext;

	private bool isInfoTooltipActive;

	public string ShownTooltipText => shownTooltipText;

	public bool CanBeShown => CanShowTooltip();

	private void Start()
	{
		if (globallyHidable)
		{
			TooltipsVisibilityHelper instance = TooltipsVisibilityHelper.Instance;
			instance.HideTooltipsEvent = (Action)Delegate.Combine(instance.HideTooltipsEvent, new Action(OnHideTooltips));
			TooltipsVisibilityHelper instance2 = TooltipsVisibilityHelper.Instance;
			instance2.ShowTooltipsEvent = (Action)Delegate.Combine(instance2.ShowTooltipsEvent, new Action(OnShowTooltips));
		}
	}

	private void OnDestroy()
	{
		if (TooltipsVisibilityHelper.Instance != null)
		{
			TooltipsVisibilityHelper instance = TooltipsVisibilityHelper.Instance;
			instance.HideTooltipsEvent = (Action)Delegate.Remove(instance.HideTooltipsEvent, new Action(OnHideTooltips));
			TooltipsVisibilityHelper instance2 = TooltipsVisibilityHelper.Instance;
			instance2.ShowTooltipsEvent = (Action)Delegate.Remove(instance2.ShowTooltipsEvent, new Action(OnShowTooltips));
		}
	}

	public void SetText(string text, bool refreshTooltip = false, string subtext = null)
	{
		shownTooltipText = text;
		shownTooltipSubtext = subtext;
		if (refreshTooltip && TooltipShown)
		{
			HideTooltip(0f);
			PrepareTooltip(null);
			ShowTooltip(0f);
		}
	}

	protected override bool CanShowTooltip()
	{
		bool flag = _canvas == null || _canvas.alpha != 0f;
		return (ShownTooltipText.IsNOTNullOrEmpty() || shownTooltipSubtext.IsNOTNullOrEmpty() || tooltipText.IsNOTNullOrEmpty()) && base.CanShowTooltip() && flag;
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		string text = (ShownTooltipText.IsNOTNullOrEmpty() ? ShownTooltipText : (textIsLocalizationKeyInstead ? LocalizationManager.GetTranslation(tooltipText) : (textIsLocalizationFormat ? CreateLayout.LocaliseText(tooltipText) : tooltipText)));
		if (!globallyHidable || (!TooltipsVisibilityHelper.Instance.TooltipsIsHidden() && !text.IsNullOrEmpty()))
		{
			UITooltip.ResetContent();
			UITooltip.AddTitle(isRemoveSpacesOnStart ? text.RemoveStartSpaces() : text, spriteAsset);
			if (shownTooltipSubtext.IsNOTNullOrEmpty())
			{
				UITooltip.AddDescription(shownTooltipSubtext, spriteAsset);
			}
			base.ShowTooltip(0f);
		}
	}

	protected override void SetControls()
	{
		UITooltip.SetVerticalControls(autoAdjustHeight, tempIsPrefabTooltip);
	}

	public override void HideTooltip(float delay = -1f)
	{
		base.HideTooltip(0f);
	}

	public void ToggleTooltip()
	{
		if (TooltipShown)
		{
			HideTooltip();
			return;
		}
		PrepareTooltip(null);
		ShowTooltip();
	}

	private void OnHideTooltips()
	{
		if (IsSelected && TooltipShown)
		{
			HideTooltip();
		}
	}

	private void OnShowTooltips()
	{
		if (IsSelected && !TooltipShown)
		{
			ToggleTooltip();
		}
	}
}
