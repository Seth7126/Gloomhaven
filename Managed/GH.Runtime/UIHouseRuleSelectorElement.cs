using SM.Gamepad;
using UnityEngine;
using UnityEngine.UI;

public class UIHouseRuleSelectorElement : MonoBehaviour
{
	private const float DefaultColorAlpha = 0.5f;

	[SerializeField]
	private UINavigationSelectable _uiNavigationSelectable;

	[SerializeField]
	private CanvasGroup _frameCanvasGroup;

	[SerializeField]
	private Color _defaultColor;

	[SerializeField]
	private Color _selectedColor;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private CanvasGroup _mainFrame;

	[SerializeField]
	private CanvasGroup _tooltipCanvasGroup;

	private bool _isSelected;

	private bool _isTooltipShown;

	public bool ShouldDisplayTooltips { get; set; }

	private void OnEnable()
	{
		SubscribeOnEvents();
		SetDefaultColor();
	}

	private void OnDisable()
	{
		UnsubscribeOnEvents();
		OnDeselected(null);
	}

	public void ToggleTooltip()
	{
		if (_isTooltipShown)
		{
			HideTooltip();
		}
		else
		{
			ShowTooltip();
		}
	}

	public void OnHideTooltips()
	{
		if (_isSelected && _isTooltipShown)
		{
			HideTooltip();
		}
	}

	public void OnShowTooltips()
	{
		if (_isSelected && !_isTooltipShown)
		{
			ToggleTooltip();
		}
	}

	private void ShowTooltip()
	{
		if (ShouldDisplayTooltips)
		{
			_isTooltipShown = true;
			_tooltipCanvasGroup.alpha = 1f;
		}
	}

	private void HideTooltip()
	{
		_isTooltipShown = false;
		_tooltipCanvasGroup.alpha = 0f;
	}

	private void SubscribeOnEvents()
	{
		_uiNavigationSelectable.OnNavigationSelectedEvent += OnSelected;
		_uiNavigationSelectable.OnNavigationDeselectedEvent += OnDeselected;
	}

	private void OnSelected(IUiNavigationSelectable obj)
	{
		_isSelected = true;
		if (_frameCanvasGroup != null)
		{
			_frameCanvasGroup.alpha = 1f;
		}
		SetSelectedColor();
		ShowTooltip();
	}

	private void OnDeselected(IUiNavigationSelectable obj)
	{
		_isSelected = false;
		if (_frameCanvasGroup != null)
		{
			_frameCanvasGroup.alpha = 0f;
		}
		SetDefaultColor();
		HideTooltip();
	}

	private void UnsubscribeOnEvents()
	{
		_uiNavigationSelectable.OnNavigationSelectedEvent -= OnSelected;
		_uiNavigationSelectable.OnNavigationDeselectedEvent -= OnDeselected;
	}

	private void SetSelectedColor()
	{
		if (_background != null)
		{
			_background.color = new Color(_selectedColor.r, _selectedColor.g, _selectedColor.b, 0.5f);
		}
		if (_mainFrame != null)
		{
			_mainFrame.alpha = 1f;
		}
	}

	private void SetDefaultColor()
	{
		if (_background != null)
		{
			_background.color = new Color(_defaultColor.r, _defaultColor.g, _defaultColor.b, 0.5f);
		}
		if (_mainFrame != null)
		{
			_mainFrame.alpha = 0f;
		}
	}
}
