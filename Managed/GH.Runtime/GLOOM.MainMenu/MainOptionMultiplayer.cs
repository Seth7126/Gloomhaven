using System.Collections.Generic;
using Platforms.Social;
using UnityEngine;

namespace GLOOM.MainMenu;

public class MainOptionMultiplayer : MainOption
{
	[SerializeField]
	private MenuOptionIcon joinIcon;

	[SerializeField]
	private MenuOptionIcon hostIcon;

	[SerializeField]
	private UIMultiplayerJoinSessionWindow joinSessionWindow;

	[SerializeField]
	private UIMultiplayerHowToHostWindow hostToHostWindow;

	[SerializeField]
	private UIMainMenuSubptionsPanel suboptionsPanel;

	[SerializeField]
	private UIMainMenuOption _mainMenuOption;

	private MenuSuboption joinSession;

	private MenuSuboption howToHost;

	private bool _skipCurrentMultiplayerValidation;

	protected override void Awake()
	{
		base.Awake();
		joinSessionWindow.OnStartCancel.AddListener(DisableInteraction);
		joinSessionWindow.OnFinishCancel.AddListener(EnableInteraction);
		joinSessionWindow.OnStartJoin.AddListener(DisableInteraction);
		joinSessionWindow.OnFailedJoin.AddListener(EnableInteraction);
		joinSession = new MenuSuboption("GUI_MULTIPLAYER_JOIN", joinIcon, OnJoinSessionSelected, joinSessionWindow.Hide);
		howToHost = new MenuSuboption("GUI_MAIN_MENU_MULTIPLAYER_HOW_TO_HOST", hostIcon, hostToHostWindow.Show, hostToHostWindow.Hide);
		if (!PlatformLayer.Instance.IsDelayedInit)
		{
			RefreshJoinSessionButton();
		}
		hostToHostWindow.OnHidden.AddListener(howToHost.Deselect);
		joinSessionWindow.OnHidden.AddListener(joinSession.Deselect);
	}

	private void OnJoinSessionSelected()
	{
		PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.Multiplayer, delegate(bool isMultiplayerValid)
		{
			if (isMultiplayerValid)
			{
				joinSessionWindow.Show();
			}
			else
			{
				suboptionsPanel.Hide();
				ShowMultiplayerSubOption();
			}
		}, PrivilegePlatform.Switch);
	}

	public void RefreshJoinSessionButton()
	{
		joinSession.IsInteractable = PlatformLayer.UserData.IsSignedIn;
	}

	private void DisableInteraction()
	{
		suboptionsPanel.SetInteractable(interactable: false);
	}

	private void EnableInteraction()
	{
		suboptionsPanel.SetInteractable(interactable: true);
	}

	public override void Select()
	{
		base.Select();
		base.Button.SetSelected(isSelected: true);
		if (_skipCurrentMultiplayerValidation)
		{
			ShowMultiplayerSubOption();
			_skipCurrentMultiplayerValidation = false;
			return;
		}
		PlatformLayer.Networking.CheckNetworkAvailabilityAsync(delegate(bool isConnected)
		{
			if (isConnected)
			{
				PlatformLayer.Networking.CheckForPrivilegeValidityAsync(Privilege.Multiplayer, delegate(bool isMultiplayerValid)
				{
					if (isMultiplayerValid)
					{
						ShowMultiplayerSubOption();
					}
					else
					{
						_mainMenuOption.Deselect();
					}
				}, PrivilegePlatform.AllExceptSwitch);
			}
			else
			{
				_mainMenuOption.Deselect();
			}
		});
	}

	private void ShowMultiplayerSubOption()
	{
		joinSession.Reset();
		howToHost.Reset();
		suboptionsPanel.Show(new List<MenuSuboption> { joinSession, howToHost }, base.Button.transform as RectTransform, Deselect);
	}

	public override void Deselect()
	{
		EnableInteraction();
		base.Deselect();
		suboptionsPanel.Hide();
	}

	public void SelectJoinMultiplayer()
	{
		_skipCurrentMultiplayerValidation = true;
		Select();
		joinSession.Select();
	}
}
