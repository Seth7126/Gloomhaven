using System;
using SM.Gamepad;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.IngameMenu;

[RequireComponent(typeof(CanvasGroup))]
public class FrameView : MonoBehaviour
{
	[SerializeField]
	private Image _frameImage;

	[SerializeField]
	private UINavigationSelectable _uiNavigationSelectable;

	[SerializeField]
	private Sprite _customFrame;

	[SerializeField]
	private Sprite _defaultFrame;

	[SerializeField]
	private bool _showGlow;

	[SerializeField]
	private GameObject _glow;

	[SerializeField]
	private bool _showHighlight;

	[SerializeField]
	private Image _highlightImage;

	[SerializeField]
	private bool _showForKeyboardMouseInput = true;

	private bool _showForInput = true;

	private CanvasGroup _canvasGroup;

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		Deactivate();
		if (!global::PlatformLayer.Instance.IsConsole)
		{
			OnDeviceChanged();
			InputManager.DeviceChangedEvent = (Action)Delegate.Combine(InputManager.DeviceChangedEvent, new Action(OnDeviceChanged));
		}
	}

	private void OnDestroy()
	{
		if (!global::PlatformLayer.Instance.IsConsole)
		{
			InputManager.DeviceChangedEvent = (Action)Delegate.Remove(InputManager.DeviceChangedEvent, new Action(OnDeviceChanged));
		}
	}

	private void OnEnable()
	{
		SubscribeOnEvents();
	}

	private void OnDisable()
	{
		UnsubscribeOnEvents();
		Deactivate();
	}

	public void Activate()
	{
		_canvasGroup.alpha = 1f;
		SetActiveElements(active: true);
	}

	public void Deactivate()
	{
		_canvasGroup.alpha = 0f;
		SetActiveElements(active: false);
	}

	private void SubscribeOnEvents()
	{
		if (_uiNavigationSelectable != null && _showForInput)
		{
			_uiNavigationSelectable.OnNavigationSelectedEvent += OnSelected;
			_uiNavigationSelectable.OnNavigationDeselectedEvent += OnDeselected;
		}
	}

	private void UnsubscribeOnEvents()
	{
		if (_uiNavigationSelectable != null && _showForInput)
		{
			_uiNavigationSelectable.OnNavigationSelectedEvent -= OnSelected;
			_uiNavigationSelectable.OnNavigationDeselectedEvent -= OnDeselected;
		}
	}

	private void OnDeselected(IUiNavigationSelectable obj)
	{
		Deactivate();
	}

	private void OnSelected(IUiNavigationSelectable obj)
	{
		Activate();
	}

	private void OnDeviceChanged()
	{
		bool gamePadInUse = InputManager.GamePadInUse;
		_showForInput = gamePadInUse || (!gamePadInUse && _showForKeyboardMouseInput);
		Deactivate();
	}

	private void SetActiveElements(bool active)
	{
		SetActiveGlow(active);
		SetActiveHighlight(active);
	}

	private void SetActiveGlow(bool active)
	{
		if (_glow != null)
		{
			_glow.SetActive(_showGlow && active);
		}
	}

	private void SetActiveHighlight(bool active)
	{
		if (!(_highlightImage == null))
		{
			_highlightImage.enabled = _showHighlight && active;
		}
	}
}
