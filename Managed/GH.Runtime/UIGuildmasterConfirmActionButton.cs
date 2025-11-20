using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildmasterConfirmActionButton : MonoBehaviour
{
	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private ExtendedButton _button;

	[SerializeField]
	private UITextTooltipTarget _tooltip;

	[SerializeField]
	private GUIAnimator _showAnimation;

	[SerializeField]
	private GUIAnimator _highlightAnimation;

	protected Action _onConfirmCallback;

	protected Action _onHoverCallback;

	protected Action _onUnhoverCallback;

	private void OnEnable()
	{
		_button.onMouseEnter.AddListener(OnHovered);
		_button.onMouseExit.AddListener(OnUnhovered);
		_button.onClick.AddListener(OnClicked);
		if (!_showAnimation.IsPlaying)
		{
			_highlightAnimation.Play(fromStart: true);
		}
	}

	private void OnDisable()
	{
		_button.onClick.RemoveListener(OnClicked);
		_button.onMouseEnter.RemoveListener(OnHovered);
		_button.onMouseExit.RemoveListener(OnUnhovered);
		_showAnimation?.Stop();
		_highlightAnimation.Stop();
	}

	public void Show(Sprite icon, Sprite highlight, Action onClickedCallback, Action onHoverCallback = null, Action onUnhoverCallback = null, bool showAnimation = false, string tooltip = null)
	{
		_onConfirmCallback = onClickedCallback;
		_onHoverCallback = onHoverCallback;
		_onUnhoverCallback = onUnhoverCallback;
		if (showAnimation)
		{
			_showAnimation.Play(fromStart: true);
		}
		base.gameObject.SetActive(value: true);
		_icon.sprite = icon;
		_highlight.sprite = highlight;
		if (!tooltip.IsNullOrEmpty())
		{
			_tooltip.SetText(tooltip);
			_tooltip.enabled = true;
		}
		else
		{
			_tooltip.enabled = false;
		}
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnHovered()
	{
		_onHoverCallback?.Invoke();
	}

	private void OnUnhovered()
	{
		_onUnhoverCallback?.Invoke();
	}

	private void OnClicked()
	{
		_onConfirmCallback?.Invoke();
	}
}
