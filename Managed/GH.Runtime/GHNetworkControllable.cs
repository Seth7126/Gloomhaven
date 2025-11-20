using System;
using System.Linq;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using ScenarioRuleLibrary;
using UnityEngine;

public class GHNetworkControllable : EntityBehaviour<IGHControllableState>, INetworkedState
{
	private LevelToken prevLevelToken;

	private PerkPointsToken prevPerkPointsToken;

	private PerksToken prevActivePerksToken;

	private CardInventoryToken prevCardInventoryToken;

	private ItemInventoryToken prevItemInventoryToken;

	private TileToken prevStartingTileToken;

	private StartRoundCardsToken prevStartRoundToken;

	private bool initialControllerAssigned;

	private Coroutine roundCardsCoroutine;

	public LevelToken LevelToken
	{
		get
		{
			return (LevelToken)base.state.Level;
		}
		private set
		{
			prevLevelToken = LevelToken;
			value.SetRevision(base.state.ControllerID, prevLevelToken);
			base.state.Level = value;
			value.Print(((ECharacter)base.state.ControllableID/*cast due to .constrained prefix*/).ToString() + " - Level. ");
		}
	}

	public PerkPointsToken PerkPointsToken
	{
		get
		{
			return (PerkPointsToken)base.state.PerkPoints;
		}
		private set
		{
			prevPerkPointsToken = PerkPointsToken;
			value.SetRevision(base.state.ControllerID, prevPerkPointsToken);
			base.state.PerkPoints = value;
			value.Print(((ECharacter)base.state.ControllableID/*cast due to .constrained prefix*/).ToString() + " - PerkPoints. ");
		}
	}

	public PerksToken ActivePerksToken
	{
		get
		{
			return (PerksToken)base.state.ActivePerks;
		}
		private set
		{
			prevActivePerksToken = ActivePerksToken;
			value.SetRevision(base.state.ControllerID, prevActivePerksToken);
			base.state.ActivePerks = value;
			value.Print(((ECharacter)base.state.ControllableID/*cast due to .constrained prefix*/).ToString() + " - ActivePerks. ");
		}
	}

	public CardInventoryToken CardInventoryToken
	{
		get
		{
			return (CardInventoryToken)base.state.CardInventory;
		}
		private set
		{
			prevCardInventoryToken = CardInventoryToken;
			value.SetRevision(base.state.ControllerID, prevCardInventoryToken);
			base.state.CardInventory = value;
			value.Print(((ECharacter)base.state.ControllableID/*cast due to .constrained prefix*/).ToString() + " - CardInventory. ");
		}
	}

	public ItemInventoryToken ItemInventoryToken
	{
		get
		{
			return (ItemInventoryToken)base.state.ItemInventory;
		}
		private set
		{
			prevItemInventoryToken = ItemInventoryToken;
			value.SetRevision(PlayerRegistry.HostPlayerID, prevItemInventoryToken);
			base.state.ItemInventory = value;
			value.Print(((ECharacter)base.state.ControllableID/*cast due to .constrained prefix*/).ToString() + " - ItemInventory. ");
		}
	}

	public TileToken StartingTileToken
	{
		get
		{
			return (TileToken)base.state.StartingTile;
		}
		private set
		{
			prevStartingTileToken = StartingTileToken;
			value.SetRevision(base.state.ControllerID, prevStartingTileToken);
			base.state.StartingTile = value;
			value.Print(((ECharacter)base.state.ControllableID/*cast due to .constrained prefix*/).ToString() + " - StartingTile. ");
		}
	}

	public StartRoundCardsToken StartRoundToken
	{
		get
		{
			return (StartRoundCardsToken)base.state.StartRoundCards;
		}
		private set
		{
			prevStartRoundToken = StartRoundToken;
			value.SetRevision(base.state.ControllerID, prevStartRoundToken);
			base.state.StartRoundCards = value;
			value.Print(((ECharacter)base.state.ControllableID/*cast due to .constrained prefix*/).ToString() + " - StartRoundCards. ");
		}
	}

	private NetworkControllable Controllable => ControllableRegistry.GetControllable(base.state.ControllableID);

	public bool InitialControllerAssigned => initialControllerAssigned;

	public override void Attached()
	{
		ControllableToken controllableToken = (ControllableToken)base.entity.AttachToken;
		base.state.ControllableID = controllableToken.ControllableID;
		FFSNet.Console.LogInfo("NetworkControllable ATTACHED (ControllableID: " + base.state.ControllableID + ").");
		if (Controllable != null)
		{
			Controllable.NetworkEntity = base.entity;
		}
		base.state.AddCallback("ControllerID", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnControllerIDChanged();
			}
		});
		base.state.AddCallback("Level", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnLevelChanged();
			}
		});
		base.state.AddCallback("PerkPoints", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnPerkPointsChanged();
			}
		});
		base.state.AddCallback("ActivePerks", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnActivePerksChanged();
			}
		});
		base.state.AddCallback("CardInventory", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnCardInventoryChanged();
			}
		});
		base.state.AddCallback("ItemInventory", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnItemInventoryChanged();
			}
		});
		base.state.AddCallback("StartingTile", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnStartingTileChanged();
			}
		});
		base.state.AddCallback("StartRoundCards", (PropertyCallbackSimple)delegate
		{
			if (Controllable != null)
			{
				OnStartRoundCardsChanged();
			}
		});
	}

	public override void Detached()
	{
		FFSNet.Console.LogInfo("NetworkControllable DETACHED (ControllableID: " + base.state.ControllableID + ").");
	}

	public override void ControlGained()
	{
		if (PlayerRegistry.MyPlayer != null)
		{
			FFSNet.Console.LogCoreInfo(PlayerRegistry.MyPlayer.Username + " (ME, PlayerID: " + PlayerRegistry.MyPlayer.PlayerID + " ControllerID: " + base.state.ControllerID + ") gained control over " + Controllable?.ControllableObject?.GetName() + " (ControllableID: " + base.state.ControllableID + ").", customFlag: true);
		}
	}

	public void UpdateControllerID(int controllerID)
	{
		if (base.entity != null && base.entity.IsAttached)
		{
			base.state.ControllerID = controllerID;
			FFSNet.Console.LogCoreInfo(PlayerRegistry.GetPlayer(base.state.ControllerID).Username + " (PlayerID: " + base.state.ControllerID + ") gained control over " + Controllable?.ControllableObject?.GetName() + " (ControllableID: " + base.state.ControllableID + ").", customFlag: true);
		}
	}

	public void UpdateState(GameAction action)
	{
		if (base.entity != null && base.entity.IsAttached)
		{
			switch ((GameActionType)action.ActionTypeID)
			{
			case GameActionType.ResetCharacter:
				CardInventoryToken = (CardInventoryToken)action.SupplementaryDataToken;
				LevelToken = (LevelToken)action.SupplementaryDataToken2;
				PerkPointsToken = (PerkPointsToken)action.SupplementaryDataToken3;
				ItemInventoryToken = (ItemInventoryToken)action.SupplementaryDataToken4;
				break;
			case GameActionType.LevelUp:
				LevelToken = (LevelToken)action.SupplementaryDataToken2;
				PerkPointsToken = (PerkPointsToken)action.SupplementaryDataToken3;
				CardInventoryToken = (CardInventoryToken)action.SupplementaryDataToken;
				break;
			case GameActionType.ModifyPerks:
				PerkPointsToken = (PerkPointsToken)action.SupplementaryDataToken;
				ActivePerksToken = (PerksToken)action.SupplementaryDataToken2;
				break;
			case GameActionType.ModifyCardInventory:
				CardInventoryToken = (CardInventoryToken)action.SupplementaryDataToken;
				break;
			case GameActionType.ModifyItemInventory:
				ItemInventoryToken = (ItemInventoryToken)action.SupplementaryDataToken;
				break;
			case GameActionType.PlaceCharacter:
				if (FFSNetwork.IsHost)
				{
					StartingTileToken = (TileToken)action.SupplementaryDataToken;
				}
				break;
			case GameActionType.SelectRoundCards:
			case GameActionType.ShortRest:
			case GameActionType.CancelActiveBonus:
				StartRoundToken = (StartRoundCardsToken)action.SupplementaryDataToken;
				break;
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error trying to Update Controllable State. Entity does not exist.");
		}
	}

	public void ApplyState()
	{
		if (base.entity != null && base.entity.IsAttached)
		{
			OnControllerIDChanged();
			if (ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.MapLoadoutScreen))
			{
				OnLevelChanged(saveToFile: false);
				OnPerkPointsChanged(saveToFile: false);
				OnActivePerksChanged(saveToFile: false);
				OnCardInventoryChanged(saveToFile: false);
				OnItemInventoryChanged(saveToFile: false);
			}
			if (ActionProcessor.CurrentPhase == ActionPhaseType.StartOfRound && !(ControllableRegistry.GetControllable(base.state.ControllableID)?.ControllableObject is BenchedCharacter))
			{
				CharacterManager characterManager = (CharacterManager)(ControllableRegistry.GetControllable(base.state.ControllableID)?.ControllableObject);
				if (!Choreographer.s_Choreographer.m_ClientDeadActors.Contains(characterManager.gameObject))
				{
					OnStartingTileChanged();
					OnStartRoundCardsChanged();
				}
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error trying to Apply Controllable State. Entity does not exist.");
		}
	}

	public void ResetState()
	{
		if (base.entity != null && base.entity.IsAttached)
		{
			if (FFSNetwork.IsHost)
			{
				base.state.StartingTile = null;
			}
			if (FFSNetwork.IsHost || PlayerRegistry.MyPlayer.PlayerID == base.state.ControllerID)
			{
				base.state.StartRoundCards = null;
				prevStartRoundToken = null;
				FFSNet.Console.LogInfo("Controllable state reset (ControllableID: " + base.state.ControllableID + ", ControllerID: " + base.state.ControllerID + ").");
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error trying to Apply Controllable State. Entity does not exist.");
		}
	}

	public void ClearScenarioState()
	{
		if (base.entity != null && base.entity.IsAttached)
		{
			if (FFSNetwork.IsHost)
			{
				base.state.StartingTile = null;
				prevStartingTileToken = null;
			}
			if (FFSNetwork.IsHost || PlayerRegistry.MyPlayer.PlayerID == base.state.ControllerID)
			{
				base.state.StartRoundCards = null;
				prevStartRoundToken = null;
			}
		}
		else
		{
			FFSNet.Console.LogWarning("Error trying to ClearScenarioState. Entity does not exist.");
		}
	}

	public ulong GetRevisionHash()
	{
		long num = 17L;
		ulong num2 = 19uL;
		return (ulong)(((((((num * (long)num2 + ((LevelToken != null) ? LevelToken.Revision : 0)) * (long)num2 + ((PerkPointsToken != null) ? PerkPointsToken.Revision : 0)) * (long)num2 + ((ActivePerksToken != null) ? ActivePerksToken.Revision : 0)) * (long)num2 + ((CardInventoryToken != null) ? CardInventoryToken.Revision : 0)) * (long)num2 + ((ItemInventoryToken != null) ? ItemInventoryToken.Revision : 0)) * (long)num2 + ((StartingTileToken != null) ? StartingTileToken.Revision : 0)) * (long)num2 + ((StartRoundToken != null) ? StartRoundToken.Revision : 0));
	}

	private void OnControllerIDChanged()
	{
		try
		{
			if (!FFSNetwork.IsClient || FFSNetwork.HasDesynchronized || initialControllerAssigned || !ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.StartOfRound))
			{
				return;
			}
			NetworkPlayer player = PlayerRegistry.GetPlayer(base.state.ControllerID);
			if (!(player != null) || player.MyControllables.ToList().Exists((NetworkControllable e) => e.ID == base.state.ControllableID))
			{
				return;
			}
			bool releaseFirst = PlayerRegistry.AllPlayers.Where((NetworkPlayer w) => w.PlayerID != player.PlayerID).ToList().Exists((NetworkPlayer e) => e.MyControllables.Select((NetworkControllable s) => s.ID).Contains(base.state.ControllableID));
			initialControllerAssigned = player.AssignControllable(base.state.ControllableID, releaseFirst, syncAssignmentToClientsIfServer: false);
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}

	public void SetInitialControllerAssigned()
	{
		initialControllerAssigned = true;
	}

	private void OnLevelChanged(bool saveToFile = true)
	{
		try
		{
			if (!FFSNetwork.IsClient || FFSNetwork.HasDesynchronized || (!(PlayerRegistry.MyPlayer == null) && PlayerRegistry.MyPlayer.HasControlOver(base.state.ControllableID)))
			{
				return;
			}
			FFSNet.Console.LogInfo("OnLevelChanged (ControllableID: " + base.state.ControllableID + ", ControllerID: " + base.state.ControllerID + "). LevelToken: " + LevelToken?.ToString() + ", prev token: " + prevLevelToken);
			if (LevelToken == null || !LevelToken.IsNewerRevision(prevLevelToken))
			{
				return;
			}
			if (ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.MapLoadoutScreen))
			{
				CMapCharacter cMapCharacter = null;
				cMapCharacter = ((!AdventureState.MapState.IsCampaign) ? AdventureState.MapState.MapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => CharacterClassManager.GetModelInstanceIDFromCharacterID(x.CharacterID) == base.state.ControllableID) : AdventureState.MapState.MapParty.CheckCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterName.GetHashCode() == base.state.ControllableID));
				if (cMapCharacter == null)
				{
					throw new Exception("Error changing level. Character returns null (ControllableID: " + base.state.ControllableID + ").");
				}
				if (cMapCharacter.Level != LevelToken.Level)
				{
					if (cMapCharacter.Level > LevelToken.Level && LevelToken.Level == 1)
					{
						Singleton<UIResetLevelUpWindow>.Instance.MPResetCharacter(base.state.ControllableID);
					}
					else
					{
						NewPartyDisplayUI.PartyDisplay.ProxyIncreaseLevelNumber(base.state, saveToFile);
					}
				}
				prevLevelToken = LevelToken;
			}
			else
			{
				FFSNet.Console.LogWarning("Trying to modify level at phase: " + ActionProcessor.CurrentPhase);
			}
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}

	private void OnPerkPointsChanged(bool saveToFile = true)
	{
		try
		{
			if (!FFSNetwork.IsClient || FFSNetwork.HasDesynchronized || (!(PlayerRegistry.MyPlayer == null) && PlayerRegistry.MyPlayer.HasControlOver(base.state.ControllableID)))
			{
				return;
			}
			FFSNet.Console.LogInfo("OnPerkPointsChanged (ControllableID: " + base.state.ControllableID + ", ControllerID: " + base.state.ControllerID + "). PerkPointsToken: " + PerkPointsToken?.ToString() + ", prev token: " + prevPerkPointsToken);
			if (PerkPointsToken != null && PerkPointsToken.IsNewerRevision(prevPerkPointsToken))
			{
				if (ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.MapLoadoutScreen))
				{
					prevPerkPointsToken = PerkPointsToken;
					NewPartyDisplayUI.PartyDisplay.ProxyUpdatePerkPoints(base.state, saveToFile);
				}
				else
				{
					FFSNet.Console.LogWarning("Trying to modify perk points at phase: " + ActionProcessor.CurrentPhase);
				}
			}
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}

	private void OnActivePerksChanged(bool saveToFile = true)
	{
		try
		{
			if (!FFSNetwork.IsClient || FFSNetwork.HasDesynchronized || (!(PlayerRegistry.MyPlayer == null) && PlayerRegistry.MyPlayer.HasControlOver(base.state.ControllableID)))
			{
				return;
			}
			FFSNet.Console.LogInfo("OnActivePerksChanged (ControllableID: " + base.state.ControllableID + ", ControllerID: " + base.state.ControllerID + "). ActivePerksToken: " + ActivePerksToken?.ToString() + ", prev token: " + prevActivePerksToken);
			if (ActivePerksToken != null && ActivePerksToken.IsNewerRevision(prevActivePerksToken))
			{
				if (ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.MapLoadoutScreen))
				{
					prevActivePerksToken = ActivePerksToken;
					NewPartyDisplayUI.PartyDisplay.ProxyModifyPerks(base.state, saveToFile);
				}
				else
				{
					FFSNet.Console.LogWarning("Trying to modify active perks at phase: " + ActionProcessor.CurrentPhase);
				}
			}
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}

	private void OnCardInventoryChanged(bool saveToFile = true)
	{
		try
		{
			if (!FFSNetwork.IsClient || FFSNetwork.HasDesynchronized || (!(PlayerRegistry.MyPlayer == null) && PlayerRegistry.MyPlayer.HasControlOver(base.state.ControllableID)))
			{
				return;
			}
			FFSNet.Console.LogInfo("OnCardInventoryChanged (ControllableID: " + base.state.ControllableID + ", ControllerID: " + base.state.ControllerID + "). CardInventoryToken: " + CardInventoryToken?.ToString() + ", prev token: " + prevCardInventoryToken);
			if (CardInventoryToken != null && CardInventoryToken.IsNewerRevision(prevCardInventoryToken))
			{
				if (ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.MapLoadoutScreen))
				{
					prevCardInventoryToken = CardInventoryToken;
					NewPartyDisplayUI.PartyDisplay.ProxyModifyCardInventory(base.state, saveToFile);
				}
				else
				{
					FFSNet.Console.LogWarning("Trying to modify card inventory at phase: " + ActionProcessor.CurrentPhase);
				}
			}
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}

	private void OnItemInventoryChanged(bool saveToFile = true)
	{
		try
		{
			if (FFSNetwork.IsClient && !FFSNetwork.HasDesynchronized)
			{
				if (ItemInventoryToken != null && ItemInventoryToken.IsNewerRevision(prevItemInventoryToken))
				{
					FFSNet.Console.LogInfo("OnItemInventoryChanged (ControllableID: " + base.state.ControllableID + ", ControllerID: " + base.state.ControllerID + "). ItemInventoryToken: " + ItemInventoryToken.ToString() + ((prevItemInventoryToken != null) ? (", prev token: " + prevItemInventoryToken.ToString()) : ""));
					if (ActionProcessor.CurrentPhase.In(ActionPhaseType.MapHQ, ActionPhaseType.MapLoadoutScreen))
					{
						prevItemInventoryToken = ItemInventoryToken;
						NewPartyDisplayUI.PartyDisplay.ProxyModifyItemInventory(base.state, saveToFile);
					}
					else
					{
						FFSNet.Console.LogWarning("Trying to modify item inventory at phase: " + ActionProcessor.CurrentPhase);
					}
				}
			}
			else
			{
				NewPartyDisplayUI.PartyDisplay.LogProxyModifyItemInventoryOnHost(base.state.ControllableID, (ItemInventoryToken)base.state.ItemInventory);
			}
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}

	private void OnStartingTileChanged()
	{
		try
		{
			if (FFSNetwork.IsClient && !FFSNetwork.HasDesynchronized && StartingTileToken != null)
			{
				if (ActionProcessor.CurrentPhase == ActionPhaseType.StartOfRound)
				{
					Choreographer.s_Choreographer.ProxyUpdateCharacterStartingTile(base.state);
				}
				else
				{
					FFSNet.Console.LogWarning("Trying to modify starting tile at phase: " + ActionProcessor.CurrentPhase);
				}
			}
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}

	public void OnStartRoundCardsChanged()
	{
		try
		{
			if (!FFSNetwork.IsClient || FFSNetwork.HasDesynchronized || (!(PlayerRegistry.MyPlayer == null) && PlayerRegistry.MyPlayer.HasControlOver(base.state.ControllableID)))
			{
				return;
			}
			FFSNet.Console.LogInfo("Controllable (ID: " + base.state.ControllableID + ") - OnStartRoundCardsChanged (callback). Token: " + StartRoundToken?.ToString() + ", Prev Token: " + prevStartRoundToken);
			if (StartRoundToken != null && StartRoundToken.IsNewerRevision(prevStartRoundToken))
			{
				if (ActionProcessor.CurrentPhase == ActionPhaseType.StartOfRound)
				{
					if (((CharacterManager)(ControllableRegistry.GetControllable(base.state.ControllableID)?.ControllableObject))?.CharacterActor is CPlayerActor cPlayerActor)
					{
						CardsHandManager.Instance.GetHand(cPlayerActor).ProxyShortRest(StartRoundToken, fromStateUpdate: true);
						CardsHandManager.Instance.ProxySelectRoundCards(base.state.ControllableID, StartRoundToken, isHandUnderMyControl: false);
						StartRoundCardState startRoundCardState = StartRoundToken.GetStartRoundCardState();
						ScenarioRuleClient.ProxySetStartRoundDeckState(cPlayerActor, startRoundCardState);
					}
				}
				else
				{
					FFSNet.Console.LogWarning("Trying to modify start round cards at phase: " + ActionProcessor.CurrentPhase);
				}
			}
			else if (StartRoundToken == null)
			{
				prevStartRoundToken = null;
				FFSNet.Console.LogInfo("PrevStartRoundToken nullified.");
			}
		}
		catch (Exception ex)
		{
			FFSNetwork.HandleDesync(ex);
		}
	}
}
