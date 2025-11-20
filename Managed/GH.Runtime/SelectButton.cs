using System;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using TMPro;
using UnityEngine;

public class SelectButton : MonoBehaviour
{
	[SerializeField]
	private Hotkey _hotkey;

	[SerializeField]
	private TextMeshProUGUI _buttonText;

	[SerializeField]
	private LeanTweenGUIAnimator _guiAnimator;

	private CanvasGroup _canvasGroup;

	private Action _clickCallback;

	private string _originalText;

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		if (_hotkey != null)
		{
			_hotkey.Initialize(Singleton<UINavigation>.Instance.Input);
		}
		if (_guiAnimator != null)
		{
			_guiAnimator.OnAnimationFinished.AddListener(ReleaseClick);
		}
		base.gameObject.SetActive(value: false);
		_originalText = _buttonText.text;
	}

	private void OnDestroy()
	{
		if (_hotkey != null)
		{
			_hotkey.Deinitialize();
		}
		if (_guiAnimator != null)
		{
			_guiAnimator.OnAnimationFinished.RemoveListener(ReleaseClick);
		}
	}

	private void ReleaseClick()
	{
		_clickCallback?.Invoke();
	}

	public void SetDisableVisualState(bool value)
	{
		_canvasGroup.interactable = !value;
		_canvasGroup.alpha = (value ? 0f : 1f);
	}

	public void PlayAnimation(Action callback)
	{
		_clickCallback = callback;
		if (_guiAnimator != null)
		{
			_guiAnimator.Play();
		}
	}

	public void SetActive(bool activate, string text = null)
	{
		base.gameObject.SetActive(activate);
		UpdateText(text ?? _originalText);
	}

	public void UpdateText(string text)
	{
		_buttonText.text = text;
	}
}
