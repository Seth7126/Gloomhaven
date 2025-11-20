using System;
using System.Collections.Generic;
using System.Linq;
using DynamicScroll;
using Platforms.Social;
using SM.Gamepad;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.HotkeysBehaviour;
using Script.GUI.SMNavigation.States.MainMenuStates;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.Multiplayer;

[RequireComponent(typeof(UIWindow))]
public class MultiplayerFriendList : Singleton<MultiplayerFriendList>
{
	[SerializeField]
	private DynamicScrollView _dynamicScroll;

	[SerializeField]
	private Hotkey[] _hotkeys;

	private IPlatformSocial _platformSocial;

	private UIWindow _window;

	private List<User> _currentFriends;

	private UiNavigationBlocker _navigationBlocker;

	private bool _isTaskRunning;

	public Action OnHiddenCallback;

	public List<User> CurrentFriends => _currentFriends;

	private void Start()
	{
		_window = GetComponent<UIWindow>();
		_window.onShown.AddListener(OnShown);
		_window.onHidden.AddListener(OnHidden);
		_platformSocial = global::PlatformLayer.Platform.PlatformSocial;
	}

	protected override void OnDestroy()
	{
		_window.onShown.RemoveListener(OnShown);
		_window.onHidden.RemoveListener(OnHidden);
		_dynamicScroll.OnRefreshEvent -= EnterFriendListState;
		base.OnDestroy();
	}

	public void Show()
	{
		_hotkeys.TryEnableHotkeys();
		_hotkeys.RebuildHotkeysLayout();
		_window.Show();
	}

	public void Hide()
	{
		_hotkeys.DisableHotkeys();
		_window.Hide();
	}

	private void OnShown()
	{
		if (!_isTaskRunning)
		{
			_isTaskRunning = true;
			_platformSocial.GetFriendsListAsync(OnGetFriendsList, isUserStatusRequired: true);
		}
		_navigationBlocker = new UiNavigationBlocker("multiplayerSubMenuBlocker");
		Singleton<UINavigation>.Instance.NavigationManager.BlockNavigation(_navigationBlocker);
		InputManager.RequestDisableInput(this, KeyAction.UI_SUBMIT);
		_dynamicScroll.OnRefreshEvent += EnterFriendListState;
	}

	private void OnHidden()
	{
		OnHiddenCallback?.Invoke();
		_dynamicScroll.Deactivate();
		Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
		InputManager.RequestEnableInput(this, KeyAction.UI_SUBMIT);
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerOnlineContainer);
	}

	private void EnterFriendListState()
	{
		Singleton<UINavigation>.Instance.StateMachine.Enter(MainStateTag.MultiplayerFriendList);
		_dynamicScroll.OnRefreshEvent -= EnterFriendListState;
	}

	private void OnGetFriendsList(OperationResult result, List<User> users)
	{
		_isTaskRunning = false;
		if (result == OperationResult.UnspecifiedError || !_window.IsOpen || users.Count == 0)
		{
			_dynamicScroll.OnRefreshEvent -= EnterFriendListState;
			return;
		}
		_currentFriends = users.OrderByDescending((User user) => user.UserStatus).ToList();
		_dynamicScroll.Activate(_currentFriends.Count);
		Singleton<UINavigation>.Instance.NavigationManager.UnblockNavigation(_navigationBlocker);
		InputManager.RequestEnableInput(this, KeyAction.UI_SUBMIT);
	}
}
