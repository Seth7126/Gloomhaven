using System;
using System.Collections.Generic;
using System.Linq;
using Code.State;
using MapRuleLibrary.Adventure;
using SM.Gamepad;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class WorldMapState : CampaignMapState
{
	private bool _oldMercenariesHotkeysState;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.WorldMap;

	protected override bool SelectedFirst => false;

	protected override string RootName => "WorldMap";

	public WorldMapState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		Hotkeys.Instance.SetState(Hotkeys.HotkeyPositionState.WorldMap);
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>
		{
			{ "MoveCameraMap", null },
			{ "ZoomCamera", null }
		};
		bool flag = Singleton<MapFTUEManager>.Instance == null || !Singleton<MapFTUEManager>.Instance.HasToShowFTUE || Singleton<MapFTUEManager>.Instance.CurrentStep == EMapFTUEStep.InteractWithMap;
		if ((AdventureState.MapState.MapParty.SelectedCharacters.Count() >= AdventureState.MapState.MinRequiredCharacters || SaveData.Instance.Global.GameMode == EGameMode.Guildmaster) && flag)
		{
			dictionary.Add("QuestList", null);
		}
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("WorldMapState", dictionary);
		NewPartyDisplayUI.PartyDisplay.SetTooltipHotkeyPanelActive(value: true);
		_navigationManager.DeselectAll();
		if (!NewPartyDisplayUI.PartyDisplay.TabInputLocked)
		{
			NewPartyDisplayUI.PartyDisplay.TabInput.Register();
		}
		_oldMercenariesHotkeysState = NewPartyDisplayUI.PartyDisplay.MercenariesHotkeysActiveState;
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(value: true);
		if (flag)
		{
			InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, Singleton<QuestManager>.Instance.FocusQuestLog);
		}
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City)
		{
			SetActiveReturnToMap(value: true);
		}
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		SetActiveReturnToMap(value: false);
		Hotkeys.Instance.RemoveHotkeysForObject("WorldMapState");
		NewPartyDisplayUI.PartyDisplay.SetTooltipHotkeyPanelActive(value: false);
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, Singleton<QuestManager>.Instance.FocusQuestLog);
		NewPartyDisplayUI.PartyDisplay.SetHotkeysActive(_oldMercenariesHotkeysState);
	}
}
