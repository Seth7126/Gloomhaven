#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIScenarioMultiplayerController : Singleton<UIScenarioMultiplayerController>
{
	[SerializeField]
	private UIReadyTrackerBar readyTracker;

	[SerializeField]
	private UIScrollText currentPlayerUI;

	private string characterShownID;

	public CPlayerSelectingToAvoidDamageOrNot_MessageData damageMessage { get; private set; }

	public bool IsReady => Singleton<UIReadyToggle>.Instance.ToggledOn;

	public event Action<bool> OnCardSelectionReadyChanged;

	public void HideMultiplayer()
	{
		UIMultiplayerNotifications.HideAllMultiplayerNotification();
		HidePlayerInfo();
		HideReadyTracker();
		InitiativeTrack.Instance.RefreshCharactersController();
		CardsHandManager.Instance.RefreshConnectionState();
		CardsHandManager.Instance.UpdateCharText();
		Singleton<UIScenarioDistributePointsManager>.Instance.Refresh();
		if (damageMessage != null)
		{
			RefreshDamagePhase();
		}
	}

	public void ShowMultiPlayer()
	{
		if (FFSNetwork.IsOnline)
		{
			InitiativeTrack.Instance.RefreshCharactersController();
			CardsHandManager.Instance.UpdateCharText();
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				OnCardSelection();
			}
		}
	}

	public void UpdateCharacterController(NetworkControllable controllable)
	{
		string text = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(controllable.ID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(controllable.ID));
		RefreshWaitingNotifications();
		InitiativeTrack.Instance.RefreshCharacterController(text);
		CardsHandManager.Instance.UpdateCharText();
		readyTracker.RefreshReady(text);
		if (characterShownID == text)
		{
			ShowPlayerInfo(controllable.Controller);
		}
		if (damageMessage != null && damageMessage.m_ActorBeingAttacked is CPlayerActor cPlayerActor && cPlayerActor.CharacterClass.CharacterID == text)
		{
			RefreshDamagePhase();
		}
	}

	public void RefreshConnectionStatus(NetworkPlayer player)
	{
		for (int i = 0; i < player.MyControllables.Count; i++)
		{
			string characterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(player.MyControllables[i].ID) : CharacterClassManager.GetCharacterIDFromModelInstanceID(player.MyControllables[i].ID));
			InitiativeTrack.Instance.RefreshCharacterController(characterID);
		}
	}

	public void ShowReadyTracker(bool bringToFront = false, bool showExhausted = false)
	{
		if (bringToFront)
		{
			UIUtility.BringToFront(readyTracker.gameObject);
		}
		List<string> characterIDs = (showExhausted ? (from s in ScenarioManager.Scenario.PlayerActors.Concat(ScenarioManager.Scenario.ExhaustedPlayers)
			select s.CharacterClass.CharacterID).ToList() : ScenarioManager.Scenario.PlayerActors.Select((CPlayerActor s) => s.CharacterClass.CharacterID).ToList());
		readyTracker.ShowCharactersTrackers(characterIDs, (string character) => showExhausted || ScenarioManager.Scenario == null || ScenarioManager.Scenario.PlayerActors.Exists((CPlayerActor e) => e.CharacterClass.ID == character));
		readyTracker.RefreshReady();
	}

	public void UpdateReadyPlayer(NetworkPlayer player, bool isReady)
	{
		readyTracker.RefreshReady(player, isReady);
		string platformIcon = PlatformTextSpriteProvider.GetPlatformIcon(player.PlatformName);
		string translation = LocalizationManager.GetTranslation(isReady ? "COMBAT_LOG_READY_PLAYER" : "COMBAT_LOG_UNREADY_PLAYER");
		string text = ((!isReady) ? string.Format(translation, platformIcon + " " + player.Username) : string.Format(translation, platformIcon + " <" + translation[1..14] + ">" + player.Username));
		Singleton<CombatLogHandler>.Instance.AddLog(text, CombatLogFilter.MULTIPLAYER);
	}

	public void HideReadyTracker()
	{
		readyTracker.HideTrackers();
	}

	public void ShowPlayerInfo(CPlayerActor actor)
	{
		if (FFSNetwork.IsOnline)
		{
			characterShownID = actor.CharacterClass.CharacterID;
			int controllableID = ((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? actor.CharacterName.GetHashCode() : actor.CharacterClass.ModelInstanceID);
			ShowPlayerInfo(ControllableRegistry.GetController(controllableID));
		}
	}

	private void ShowPlayerInfo(NetworkPlayer player)
	{
		if (player == null || PhaseManager.CurrentPhase == null || PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			currentPlayerUI.Hide();
		}
		else if (player == PlayerRegistry.MyPlayer)
		{
			currentPlayerUI.ShowText(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_MY_TURN"));
		}
		else
		{
			currentPlayerUI.ShowText(string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_PLAYER_TURN"), player.UserNameWithPlatformIcon()));
		}
	}

	public void HidePlayerInfo()
	{
		characterShownID = null;
		currentPlayerUI.Hide();
	}

	public void OnCardSelection()
	{
		if (FFSNetwork.IsOnline)
		{
			CardsHandManager.Instance.RefreshConnectionState();
			RefreshWaitingNotifications();
			if (CardsHandManager.Instance.CurrentHand?.PlayerActor != null)
			{
				ShowPlayerInfo(CardsHandManager.Instance.CurrentHand.PlayerActor);
			}
			ShowReadyTracker();
		}
	}

	public void OnPlayerLeft()
	{
		if (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.AllPlayers.Count == 1)
		{
			UIManager.Instance.ToggleButtonOptions(this, isEnabled: true);
		}
	}

	public void OnStartTurn(CPlayerActor actor)
	{
		ShowPlayerInfo(actor);
		UpdateActorControlButtons(actor);
	}

	public void UpdateActorControlButtons(CActor actor)
	{
		Debug.Log($"[MultiplayerController] Request Toggle {actor} {FFSNetwork.IsOnline} {actor?.IsUnderMyControl ?? false}");
		UIManager.Instance.ToggleButtonOptions(this, !FFSNetwork.IsOnline || (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.AllPlayers.Count == 1) || IsActorUnderMyControl(actor));
	}

	public void ToggleButtonOptions(bool value)
	{
		UIManager.Instance.ToggleButtonOptions(this, value);
	}

	private bool IsActorUnderMyControl(CActor actor)
	{
		if (!(actor is CPlayerActor { IsUnderMyControl: var isUnderMyControl }))
		{
			if (!(actor is CHeroSummonActor cHeroSummonActor))
			{
				if (!(actor is CEnemyActor { IsUnderMyControl: var isUnderMyControl2 }))
				{
					return false;
				}
				return isUnderMyControl2;
			}
			return cHeroSummonActor.Summoner.IsUnderMyControl || cHeroSummonActor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction;
		}
		return isUnderMyControl;
	}

	public void OnEndTurn()
	{
		HidePlayerInfo();
		UIManager.Instance.ToggleButtonOptions(this, isEnabled: true);
	}

	public void OnTakeDamage(CPlayerActor playerActor, CPlayerSelectingToAvoidDamageOrNot_MessageData messageData)
	{
		damageMessage = messageData;
		if (playerActor != null)
		{
			ShowPlayerInfo(playerActor);
		}
		RefreshDamagePhase();
	}

	private void RefreshDamagePhase()
	{
		if (damageMessage == null)
		{
			return;
		}
		CActor actorBeingAttacked = damageMessage.m_ActorBeingAttacked;
		CPlayerActor actorToShowCardsFor = damageMessage.m_ActorToShowCardsFor;
		CActor cActor = ((actorToShowCardsFor != null) ? actorToShowCardsFor : actorBeingAttacked);
		bool flag = false;
		if (!(cActor is CPlayerActor cPlayerActor))
		{
			if (!(cActor is CHeroSummonActor cHeroSummonActor))
			{
				if (cActor is CEnemyActor)
				{
					flag = FFSNetwork.IsHost;
				}
			}
			else
			{
				flag = cHeroSummonActor.Summoner.IsUnderMyControl;
			}
		}
		else
		{
			flag = cPlayerActor.IsUnderMyControl;
		}
		if (FFSNetwork.IsOnline && !flag)
		{
			Singleton<TakeDamagePanel>.Instance.ShowOtherPlayer(actorBeingAttacked, actorToShowCardsFor, damageMessage.m_ModifiedStrength, (damageMessage.m_TargetSummary != null) ? Mathf.Max(0, damageMessage.m_TargetSummary.Pierce - damageMessage.m_TargetSummary.Shield) : 0, damageMessage.m_IsDirectDamage, damageMessage.m_DamagingAbility);
		}
		else if (!Singleton<TakeDamagePanel>.Instance.IsOpen)
		{
			Singleton<TakeDamagePanel>.Instance.Show(actorBeingAttacked, actorToShowCardsFor, damageMessage.m_ModifiedStrength, (damageMessage.m_TargetSummary != null) ? Mathf.Max(0, damageMessage.m_TargetSummary.Pierce - damageMessage.m_TargetSummary.Shield) : 0, damageMessage.m_IsDirectDamage, damageMessage.m_DamagingAbility);
			UIManager.Instance.ToggleButtonOptions(this, flag);
		}
	}

	public void OnFinishTakeDamage(bool hidePlayerInfo = true)
	{
		damageMessage = null;
		if (hidePlayerInfo)
		{
			HidePlayerInfo();
		}
	}

	public void InitializeReadyToggleForCardSelection()
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<UIReadyToggle>.Instance.Initialize(show: true, delegate
			{
				CardsHandManager.Instance.SetHandInteractable(interactable: false);
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
				CardsHandManager.Instance.IsFullCardPreviewAllowed = false;
				CardsHandManager.Instance.IsFullDeckPreviewAllowed = false;
				InvokeCardSelectionReadyChanged(ready: true);
			}, delegate
			{
				UIMultiplayerNotifications.HideCancelReadiedCards();
				CardsHandManager.Instance.SetHandInteractable(interactable: true);
				CardsHandManager.Instance.EnableCancelActiveAbilities = true;
				CardsHandManager.Instance.IsFullCardPreviewAllowed = true;
				CardsHandManager.Instance.IsFullDeckPreviewAllowed = true;
				InvokeCardSelectionReadyChanged(ready: false);
			}, delegate
			{
				HideReadyTracker();
				CardsHandManager.Instance.SetHandInteractable(interactable: true);
				CardsHandManager.Instance.Hide();
				Choreographer.s_Choreographer.Pass();
			}, delegate(NetworkPlayer player, bool isReady)
			{
				UpdateReadyPlayer(player, isReady);
			}, null, null, "GUI_END_SELECTION", "GUI_CANCEL", "PlaySound_UIMultiPlayerReady", bringToFront: false, delegate
			{
				CardsHandManager.Instance.SetHandInteractable(interactable: false);
				CardsHandManager.Instance.EnableCancelActiveAbilities = false;
			}, delegate
			{
				CardsHandManager.Instance.SetHandInteractable(interactable: true);
				CardsHandManager.Instance.EnableCancelActiveAbilities = true;
			});
		}
	}

	private void InvokeCardSelectionReadyChanged(bool ready)
	{
		this.OnCardSelectionReadyChanged?.Invoke(ready);
	}

	public void RefreshWaitingNotifications()
	{
		bool flag;
		if ((FFSNetwork.IsHost && (PlayerRegistry.JoiningPlayers.Count > 0 || PlayerRegistry.AllPlayers.Any((NetworkPlayer w) => w.PlayerID != PlayerRegistry.HostPlayerID && !w.PlayerReadyForAssignment))) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
		{
			flag = true;
			UIMultiplayerNotifications.ShowWaitingPlayersJoining();
		}
		else
		{
			flag = false;
			UIMultiplayerNotifications.HideWaitingPlayersJoining();
		}
		if (!flag && PlayerRegistry.AllPlayers.Count > 1 && PlayerRegistry.AllPlayers.Exists((NetworkPlayer x) => !x.IsParticipant))
		{
			if (PlayerRegistry.ConnectingUsers != null)
			{
				Debug.Log("Connecting: " + PlayerRegistry.ConnectingUsers.Count + " Players:" + string.Join(", ", PlayerRegistry.AllPlayers.Select((NetworkPlayer it) => it.Username + "-" + it.IsParticipant)));
			}
			UIMultiplayerNotifications.ShowWaitingPlayersCharacterAssigned(PlayerRegistry.AllPlayers.Where((NetworkPlayer x) => !x.IsParticipant).ToList());
		}
		else
		{
			UIMultiplayerNotifications.HideWaitingPlayersCharacterAssigned();
		}
		if (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.AllPlayers.Count == 1)
		{
			UIMultiplayerNotifications.ShowWaitingPlayersJoin();
		}
		else
		{
			UIMultiplayerNotifications.HideWaitingPlayersJoin();
		}
	}

	public void PingTile(CClientTile tile)
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<PingManager>.Instance.Ping3DElementMultiPlayer(tile.m_TileBehaviour.gameObject, PlayerRegistry.MyPlayer);
			Synchronizer.SendSideAction(GameActionType.PingHex, new TileToken(tile.m_Tile.m_ArrayIndex));
		}
		else
		{
			Singleton<PingManager>.Instance.Ping3DElementSinglePlayer(tile.m_TileBehaviour.gameObject);
		}
	}
}
