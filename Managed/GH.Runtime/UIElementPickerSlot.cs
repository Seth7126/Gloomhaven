using System;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.UI;

public class UIElementPickerSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Hotkey hotkey;

	[SerializeField]
	private Image highlightBackground;

	[SerializeField]
	private Sprite highlightedBackground;

	[SerializeField]
	private float highlightedBackgroundAlpha = 1f;

	[SerializeField]
	private Sprite unhighlightedBackground;

	[SerializeField]
	private float unhighlightedBackgroundAlpha = 0.4f;

	[Header("Element")]
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image highlightElement;

	private Action<ElementInfusionBoardManager.EElement> onSelected;

	private bool isHovered;

	private bool isSelected;

	[SerializeField]
	private UINavigationSelectable selectable;

	[SerializeField]
	private ElementInfusionBoardManager.EElement element;

	public bool IsSelected => isSelected;

	public UINavigationSelectable Selectable => selectable;

	private void Awake()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(OnClick);
		}
		button.onMouseEnter.AddListener(delegate
		{
			OnHover(hover: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			OnHover(hover: false);
		});
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, OnClick, ShowHotkey, HideHotkey).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
		Init(element);
	}

	private void OnDestroy()
	{
		if (Singleton<KeyActionHandlerController>.Instance != null)
		{
			Singleton<KeyActionHandlerController>.Instance.RemoveHandler(KeyAction.UI_SUBMIT, OnClick);
		}
		button.onClick.RemoveAllListeners();
		HideHotkey();
	}

	public void Init(ElementInfusionBoardManager.EElement element)
	{
		this.element = element;
		icon.sprite = UIInfoTools.Instance.GetElementPickerSprite(element);
		highlightElement.color = UIInfoTools.Instance.GetElementHighlightColor(element, highlightElement.color.a);
		button.mouseDownAudioItem = UIInfoTools.Instance.GetSelectElementAudioItemIcon(element);
	}

	public void Show(Action<ElementInfusionBoardManager.EElement> onSelected)
	{
		this.onSelected = onSelected;
		isHovered = false;
		SetSelected(selected: false);
		base.gameObject.SetActive(value: true);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	private void OnClick()
	{
		onSelected?.Invoke(element);
	}

	private void ShowHotkey()
	{
		if (hotkey != null)
		{
			hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
	}

	private void HideHotkey()
	{
		if (hotkey != null)
		{
			hotkey.Deinitialize();
			hotkey.DisplayHotkey(active: false);
		}
	}

	private void OnHover(bool hover)
	{
		isHovered = hover;
		RefreshHighlight();
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		bool flag = isHovered || isSelected;
		highlightElement.enabled = flag;
		highlightBackground.sprite = (flag ? highlightedBackground : unhighlightedBackground);
		highlightBackground.SetAlpha(flag ? highlightedBackgroundAlpha : unhighlightedBackgroundAlpha);
	}

	public void Hide()
	{
		isSelected = false;
		isHovered = false;
		base.gameObject.SetActive(value: false);
	}
}
