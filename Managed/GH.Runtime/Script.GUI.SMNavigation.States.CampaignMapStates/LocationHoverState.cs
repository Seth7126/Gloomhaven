using System;
using System.Collections.Generic;
using Code.State;
using SM.Gamepad;
using Script.GUI.Controller.Area;
using Script.GUI.SMNavigation.States.ScenarioStates.Hotkey;

namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class LocationHoverState : CampaignMapState
{
	private MapLocation _mapLocation;

	public override CampaignMapStateTag StateTag => CampaignMapStateTag.LocationHover;

	protected override bool SelectedFirst => false;

	protected override string RootName => "WorldMap";

	public LocationHoverState(UiNavigationManager navigationManager)
		: base(navigationManager)
	{
	}

	public override void Enter<TPayload>(IStateProvider stateProvider, TPayload payload)
	{
		UpdateHotkeys();
		Singleton<AdventureMapUIManager>.Instance.LockChanged += OnLockChanged;
		if (payload is MapLocationStateData mapLocationStateData)
		{
			_mapLocation = mapLocationStateData.MapLocation;
		}
		InputManager.RegisterToOnPressed(KeyAction.UI_SUBMIT, MapLocationClick);
		if (InputManager.GamePadInUse && Singleton<UINavigation>.Instance.StateMachine.PreviousState == Singleton<UINavigation>.Instance.StateMachine.GetState(CampaignMapStateTag.WorldMap))
		{
			FixCursorVisibility();
		}
		if (Singleton<UIGuildmasterHUD>.Instance.CurrentMode == EGuildmasterMode.City)
		{
			SetActiveReturnToMap(value: true);
		}
	}

	private void FixCursorVisibility()
	{
		MapCursorProxy.EnableCursor();
	}

	private static void UpdateHotkeys()
	{
		Dictionary<string, Action> dictionary = new Dictionary<string, Action>
		{
			{ "MoveCameraMap", null },
			{ "ZoomCamera", null }
		};
		if (!Singleton<AdventureMapUIManager>.Instance.IsLocked)
		{
			dictionary.Add("Select", null);
		}
		Hotkeys.Instance.AddOrReplaceHotkeysForObject("LocationHoverState", dictionary);
	}

	public override void Exit<TPayload>(IStateProvider stateProvider, TPayload payload = null)
	{
		SetActiveReturnToMap(value: false);
		Hotkeys.Instance.RemoveHotkeysForObject("LocationHoverState");
		_mapLocation = null;
		InputManager.UnregisterToOnPressed(KeyAction.UI_SUBMIT, MapLocationClick);
		Singleton<AdventureMapUIManager>.Instance.LockChanged -= OnLockChanged;
	}

	private void OnLockChanged()
	{
		UpdateHotkeys();
	}

	private void MapLocationClick()
	{
		if (!Singleton<AdventureMapUIManager>.Instance.IsLocked && _mapLocation != null)
		{
			_mapLocation.OnGamepadClick();
		}
	}
}
