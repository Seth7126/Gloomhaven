using System;
using FFSNet;
using GLOOM;
using Script.GUI;
using UnityEngine;

public class UIScenarioDistributePointsManager : Singleton<UIScenarioDistributePointsManager>
{
	[SerializeField]
	private UIDistributePointsPopup distributePointsSelectPopup;

	[SerializeField]
	private UIDistributePointsPopup distributePointsAssignPopup;

	[SerializeField]
	private PanelHotkeyContainer _hotkeyContainer;

	[SerializeField]
	private ReadyButton _readyButton;

	[SerializeField]
	private LongConfirmHandler _longConfirmHandler;

	private UIDistributePointsPopup _currentPopup;

	public PanelHotkeyContainer HotkeyContainer => _hotkeyContainer;

	public event Action<bool> DistributePointsChanged;

	public void Refresh()
	{
		distributePointsAssignPopup.Refresh();
		distributePointsSelectPopup.Refresh();
	}

	public void Hide()
	{
		distributePointsAssignPopup.Hide();
		distributePointsSelectPopup.Hide();
		SetHotkeysVisibility(isActive: false);
		DisableShortPressListening();
		_currentPopup = null;
	}

	public void ShowAssign(IDistributePointsService service, Action<IDistributePointsActor, int> onUpdatedPoints, Action<IDistributePointsActor, bool> onHovered)
	{
		distributePointsAssignPopup.Show(service, onUpdatedPoints, onHovered);
		_currentPopup = distributePointsAssignPopup;
		SetHotkeysVisibility(isActive: true);
		EnableShortPressListening();
	}

	public void ShowSelect(IDistributePointsService service, Action<IDistributePointsActor, int> onUpdatedPoints, Action<IDistributePointsActor, bool> onHovered = null)
	{
		distributePointsSelectPopup.Show(service, onUpdatedPoints, onHovered, LocalizationManager.GetTranslation("GUI_SELECT"), isRewards: false, OnSelectedSlot);
		_currentPopup = distributePointsSelectPopup;
		SetHotkeysVisibility(isActive: true);
		EnableShortPressListening();
	}

	private void EnableShortPressListening()
	{
		InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, OnShortPress);
	}

	private void DisableShortPressListening()
	{
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, OnShortPress);
	}

	private void OnSelectedSlot(bool hover)
	{
		this.DistributePointsChanged?.Invoke(hover);
	}

	private void OnShortPress()
	{
		if (!(_currentPopup != null))
		{
			return;
		}
		if (_readyButton.gameObject.activeSelf)
		{
			_longConfirmHandler.ResetShortPressedCallback();
			_longConfirmHandler.Pressed(delegate
			{
				_readyButton.OnClickInternal();
			}, _currentPopup.CurrentHoveredSlot.OnClick);
		}
		else
		{
			_currentPopup.CurrentHoveredSlot.OnClick();
		}
	}

	private void SetHotkeysVisibility(bool isActive)
	{
		if (!(_currentPopup == distributePointsSelectPopup) && InputManager.GamePadInUse && !(_hotkeyContainer == null))
		{
			_hotkeyContainer.gameObject.SetActive(isActive);
		}
	}

	public void ProxyDistributeAssignPopup(GameAction action)
	{
		distributePointsAssignPopup.ProxyRedistributeHealth(action);
	}

	public void ProxyDistributeSelectPopup(GameAction action)
	{
		distributePointsSelectPopup.ProxyRedistributeHealth(action);
	}
}
