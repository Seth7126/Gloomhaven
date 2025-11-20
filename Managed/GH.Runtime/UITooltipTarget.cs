using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UITooltipTarget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private UITooltip.Corner corner = UITooltip.Corner.Auto;

	[SerializeField]
	private Vector2 anchoredOffset = Vector2.zero;

	[SerializeField]
	private Vector2 anchoredPositionOffset = Vector2.zero;

	[SerializeField]
	private bool anchorToExactMouseTargetInstead;

	[SerializeField]
	private float width = 100f;

	[SerializeField]
	protected bool autoAdjustHeight = true;

	[ConditionalField("autoAdjustHeight", false, true)]
	[SerializeField]
	private float height = 50f;

	[SerializeField]
	protected bool hideBackground;

	[SerializeField]
	private bool showOnSelect = true;

	[SerializeField]
	protected bool screenBound;

	[ConditionalField("screenBound", true, true)]
	[SerializeField]
	private float screenBoundOffset = 20f;

	[SerializeField]
	private bool isUseTooltipPosition;

	[SerializeField]
	protected bool isRemoveSpacesOnStart;

	protected bool IsSelected;

	public bool TooltipShown;

	public bool TooltipEnabled { get; set; } = true;

	public void Initialize(UITooltip.Corner corner, Vector2 anchoredOffset, bool anchorToExactMouseTargetInstead, float width, float height, bool autoAdjustHeight, bool hideBackground)
	{
		this.corner = corner;
		this.anchoredOffset = anchoredOffset;
		this.anchorToExactMouseTargetInstead = anchorToExactMouseTargetInstead;
		this.width = width;
		this.height = height;
		this.autoAdjustHeight = autoAdjustHeight;
		this.hideBackground = hideBackground;
	}

	public void SetCorner(UITooltip.Corner corner)
	{
		this.corner = corner;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (TooltipEnabled && !TooltipShown && CanShowTooltip())
		{
			PrepareTooltip(eventData);
			ShowTooltip();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (TooltipEnabled && TooltipShown)
		{
			HideTooltip();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		IsSelected = true;
		if (showOnSelect)
		{
			OnPointerEnter(new PointerEventData(EventSystem.current));
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		IsSelected = false;
		if (showOnSelect)
		{
			OnPointerExit(null);
		}
	}

	public void OnNavigationSelect()
	{
		IsSelected = true;
		if (showOnSelect)
		{
			OnPointerEnter(new PointerEventData(EventSystem.current));
		}
	}

	public void OnNavigationDeselect()
	{
		IsSelected = false;
		if (showOnSelect)
		{
			OnPointerExit(null);
		}
	}

	private void OnDisable()
	{
		if (TooltipEnabled && TooltipShown)
		{
			HideTooltip();
		}
	}

	protected virtual bool CanShowTooltip()
	{
		return true;
	}

	protected void PrepareTooltip(PointerEventData eventData)
	{
		UITooltip.AnchorToRect(((anchorToExactMouseTargetInstead && eventData != null) ? eventData.pointerEnter.transform : base.transform) as RectTransform, corner);
		UITooltip.SetAnchoredOffset(anchoredOffset);
		UITooltip.SetAnchoredPositionOffset(anchoredPositionOffset);
		UITooltip.ShowBackgroundImage(!hideBackground);
		UITooltip.SetScreenBound(screenBound, screenBoundOffset);
		SetControls();
		if (autoAdjustHeight)
		{
			UITooltip.SetWidth(width);
		}
		else
		{
			UITooltip.SetSize(width, height);
		}
	}

	protected virtual void SetControls()
	{
		UITooltip.SetVerticalControls(autoAdjustHeight);
	}

	protected virtual void ShowTooltip(float delay = -1f)
	{
		if (isUseTooltipPosition)
		{
			UITooltip.Show(delay, base.transform as RectTransform);
		}
		else
		{
			UITooltip.Show(delay);
		}
		TooltipShown = true;
	}

	public virtual void HideTooltip(float delay = -1f)
	{
		UITooltip.Hide(delay);
		TooltipShown = false;
	}
}
