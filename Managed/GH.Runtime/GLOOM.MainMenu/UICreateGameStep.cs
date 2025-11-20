using System;
using Assets.Script.Misc;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UICreateGameStep : MonoBehaviour
{
	[SerializeField]
	protected ControllerInputAreaLocal controllerArea;

	[SerializeField]
	protected UIControllerKeyTip controllerConfirmTip;

	[SerializeField]
	protected ExtendedButton confirmButton;

	[SerializeField]
	protected Button cancelButton;

	protected Action m_OnConfirmed;

	protected Action m_OnCancelled;

	protected UIWindow m_Window;

	protected GameData m_Data;

	private Color? m_ConfirmTextColor;

	protected bool m_IsOpen;

	protected bool _isConfirmedCallbackSetted;

	protected virtual void Awake()
	{
		_isConfirmedCallbackSetted = false;
		m_Window = GetComponent<UIWindow>();
		if (cancelButton != null)
		{
			cancelButton.onClick.AddListener(Cancel);
		}
		m_Window.onHidden.AddListener(OnUIWindowHidden);
		m_Window.onShown.AddListener(OnUIWindowShown);
		if (!InputManager.GamePadInUse)
		{
			_isConfirmedCallbackSetted = true;
			confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		}
		controllerArea?.OnFocusedArea.AddListener(OnControllerFocused);
		controllerArea?.OnUnfocusedArea.AddListener(OnControllerUnfocused);
	}

	[UsedImplicitly]
	protected virtual void OnDestroy()
	{
		if (cancelButton != null)
		{
			cancelButton.onClick.RemoveListener(Cancel);
		}
		if (!InputManager.GamePadInUse)
		{
			confirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
		}
	}

	protected virtual void OnControllerUnfocused()
	{
	}

	protected virtual void OnControllerFocused()
	{
	}

	public virtual ICallbackPromise Show(IGameModeService service, GameData data, bool instant = false)
	{
		CallbackPromise callbackPromise = new CallbackPromise();
		Setup(service, data, callbackPromise.Resolve, callbackPromise.Cancel);
		Show(instant);
		return callbackPromise;
	}

	protected virtual void Show(bool instant = false)
	{
		m_IsOpen = true;
		m_Window.ShowOrUpdateStartingState(instant);
		controllerArea?.Enable();
	}

	protected virtual void Setup(IGameModeService service, GameData data, Action onConfirmed, Action onCancelled = null)
	{
		m_IsOpen = true;
		m_Data = data;
		m_OnCancelled = onCancelled;
		m_OnConfirmed = onConfirmed;
		EnableConfirmationButton(enable: false);
	}

	public virtual void Hide(bool instant = false)
	{
		if (m_IsOpen)
		{
			m_IsOpen = false;
			m_Window.Hide(instant);
		}
	}

	public void Confirm()
	{
		Validate(delegate(bool isValid)
		{
			if (isValid)
			{
				OnConfirmedStep();
			}
		});
	}

	protected virtual void OnConfirmedStep()
	{
		m_OnConfirmed?.Invoke();
	}

	protected virtual void Validate(Action<bool> callback)
	{
		callback(obj: true);
	}

	protected virtual void Cancel()
	{
		Hide();
		m_OnCancelled?.Invoke();
	}

	protected virtual void OnUIWindowHidden()
	{
		controllerArea?.Destroy();
		if (m_IsOpen)
		{
			Cancel();
		}
	}

	protected virtual void OnUIWindowShown()
	{
	}

	protected virtual void EnableConfirmationButton(bool enable)
	{
		if (!InputManager.GamePadInUse)
		{
			Color valueOrDefault = m_ConfirmTextColor.GetValueOrDefault();
			if (!m_ConfirmTextColor.HasValue)
			{
				valueOrDefault = confirmButton.buttonText.color;
				m_ConfirmTextColor = valueOrDefault;
			}
			confirmButton.targetGraphic.material = (enable ? null : UIInfoTools.Instance.disabledGrayscaleMaterial);
			confirmButton.buttonText.color = (enable ? m_ConfirmTextColor.Value : UIInfoTools.Instance.greyedOutTextColor);
			if (controllerConfirmTip != null)
			{
				controllerConfirmTip.ShowInteractable(enable);
			}
		}
	}

	protected void OnConfirmButtonClicked()
	{
		Confirm();
	}
}
