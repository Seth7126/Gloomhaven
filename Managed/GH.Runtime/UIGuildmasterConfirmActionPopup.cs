using System;
using System.Linq;
using Code.State;
using SM.Gamepad;
using Script.GUI.Popups;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.HotkeysBehaviour;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.MainMenuStates;
using Script.GUI.SMNavigation.States.PopupStates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildmasterConfirmActionPopup : MonoBehaviour
{
	[SerializeField]
	private KeyAction _keyAction;

	[SerializeField]
	private Hotkey _hotkey;

	[SerializeField]
	private LongPressHandler _longPressHandler;

	[SerializeField]
	private TMP_Text _header;

	[SerializeField]
	private TMP_Text _message;

	[SerializeField]
	private Image _icon;

	private SimpleKeyActionHandlerBlocker _partyPanelBlocker = new SimpleKeyActionHandlerBlocker();

	protected Action _onConfirmCallback;

	private void OnEnable()
	{
		_hotkey.TryEnableHotkey();
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(_keyAction, OnConfirmPressed).AddBlocker(_partyPanelBlocker));
		Singleton<UINavigation>.Instance.StateMachine.EventStateChanged += StateMachineOnEventStateChanged;
		StateMachineOnEventStateChanged(Singleton<UINavigation>.Instance.StateMachine.CurrentState);
	}

	private void OnDisable()
	{
		_hotkey.DisableHotkey();
		Singleton<KeyActionHandlerController>.Instance.RemoveHandler(_keyAction, OnConfirmPressed);
		Singleton<UINavigation>.Instance.StateMachine.EventStateChanged -= StateMachineOnEventStateChanged;
	}

	private void StateMachineOnEventStateChanged(IState state)
	{
		CampaignMapStateTag[] source = new CampaignMapStateTag[9]
		{
			CampaignMapStateTag.WorldMap,
			CampaignMapStateTag.Temple,
			CampaignMapStateTag.Merchant,
			CampaignMapStateTag.ShopItemTooltip,
			CampaignMapStateTag.GuildmasterTrainer,
			CampaignMapStateTag.LocationHover,
			CampaignMapStateTag.QuestLog,
			CampaignMapStateTag.MapEvent,
			CampaignMapStateTag.TravelQuestState
		};
		bool block = ((!(state is CampaignMapState campaignMapState)) ? (state is PopupState || state is MainMenuState) : (!source.Contains(campaignMapState.StateTag)));
		_partyPanelBlocker.SetBlock(block);
	}

	public void Show(string header, string message, Sprite icon, Action onConfirmCallback)
	{
		_onConfirmCallback = onConfirmCallback;
		base.gameObject.SetActive(value: true);
		_header.text = header;
		_message.text = message;
		_icon.sprite = icon;
	}

	public void Hide()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Confirm()
	{
		_onConfirmCallback?.Invoke();
	}

	private void OnConfirmPressed()
	{
		_longPressHandler.Pressed(Confirm);
	}
}
