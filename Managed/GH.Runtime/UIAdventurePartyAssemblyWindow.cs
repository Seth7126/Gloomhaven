#define ENABLE_LOGS
using System;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using ScenarioRuleLibrary;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIAdventurePartyAssemblyWindow : MonoBehaviour
{
	[SerializeField]
	protected UIAdventurePartyAssemblyRoster roster;

	[SerializeField]
	protected UIAdventurePartyAssemblyCharacterDisplay characterDisplay;

	[SerializeField]
	private CanvasGroup warningPanel;

	[SerializeField]
	private TextMeshProUGUI warningText;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	protected UIWindow window;

	protected CMapCharacter selectedCharacter;

	private Action<CMapCharacter, NewPartyCharacterUI> onSelected;

	private Action<CMapCharacter> onDeselected;

	protected CMapParty party;

	private Action onHidden;

	public static bool IsChangingCharacter;

	public bool IsVisible => window.IsVisible;

	public CMapCharacter SelectedCharacter
	{
		set
		{
			selectedCharacter = value;
		}
	}

	protected virtual void Awake()
	{
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(OnHidden);
		roster.OnCharacterHover.AddListener(PreviewCharacterInfo);
		roster.OnCharacterUnhover.AddListener(FinishPreviewCharacterInfo);
		roster.OnCharacterSelected.AddListener(delegate(CMapCharacter character)
		{
			if (selectedCharacter == null || selectedCharacter != character)
			{
				bool executionFinished = false;
				SelectCharacter(NewPartyDisplayUI.PartyDisplay.SelectedUISlot, character, ref executionFinished);
			}
		});
		roster.OnCharacterDeselected.AddListener(delegate(CMapCharacter character)
		{
			FFSNet.Console.Log("SelectedCharacter: " + selectedCharacter);
			if (selectedCharacter != null)
			{
				FFSNet.Console.Log("Selected Char ID: " + selectedCharacter.CharacterID);
			}
			if (selectedCharacter == character)
			{
				bool executionFinished = false;
				RemoveCharacterFromParty(selectedCharacter, ref executionFinished);
			}
		});
		controllerArea.OnFocusedArea.AddListener(OnControllerFocused);
		controllerArea.OnUnfocusedArea.AddListener(OnControllerUnfocused);
	}

	private void OnControllerFocused()
	{
		window.escapeKeyAction = ((!MapFTUEManager.IsPlaying) ? UIWindow.EscapeKeyAction.Hide : UIWindow.EscapeKeyAction.Skip);
		roster.OnFocus();
		characterDisplay.EnableNavigation();
	}

	private void OnControllerUnfocused()
	{
		roster.OnUnfocus();
		characterDisplay.DisableNavigation();
	}

	public virtual void Initialize(CMapParty party, Action<CMapCharacter, NewPartyCharacterUI> onSelected, Action<CMapCharacter> onDeselected)
	{
		this.party = party;
		this.onSelected = onSelected;
		this.onDeselected = onDeselected;
		roster.Refresh(AdventureState.MapState.IsCampaign ? party.CheckCharacters.FindAll((CMapCharacter it) => it.PersonalQuest != null) : party.CheckCharacters, null);
	}

	public virtual void Show(RectTransform point, CMapCharacter defaultSelected = null, Action onHidden = null, bool canEdit = true, bool instant = false)
	{
		selectedCharacter = defaultSelected;
		this.onHidden = onHidden;
		HideInformation();
		AnalyticsWrapper.LogScreenDisplay(AWScreenName.character_select);
		roster.Show(AdventureState.MapState.IsCampaign ? party.CheckCharacters.FindAll((CMapCharacter it) => it.PersonalQuest != null) : party.CheckCharacters, selectedCharacter, point, instant);
		if (defaultSelected != null)
		{
			ShowSelectedCharacter(defaultSelected);
		}
		else
		{
			ShowNoSelectedCharacter();
		}
		if (!canEdit)
		{
			roster.Lock(party.SelectedCharactersArray);
		}
		window.Show();
		window.escapeKeyAction = ((!MapFTUEManager.IsPlaying) ? UIWindow.EscapeKeyAction.Hide : UIWindow.EscapeKeyAction.Skip);
		ValidatePartyCharacter();
		controllerArea.Enable();
	}

	public bool SelectCharacter(NewPartyCharacterUI targetUISlot, CMapCharacter characterToSelect, ref bool executionFinished, NetworkPlayer initialController = null, bool networkActionIfOnline = true, bool isLocalExecution = true, bool isProxyCall = false, int switchingPlayerID = 0, bool notifySwitch = true)
	{
		if (FFSNetwork.IsHost && PlayerRegistry.IsSwitchingCharacter)
		{
			Debug.Log("Select Character action discarded as another player is already switching");
			executionFinished = true;
			return false;
		}
		if (FFSNetwork.IsOnline && networkActionIfOnline)
		{
			int actorID = (AdventureState.MapState.IsCampaign ? characterToSelect.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(characterToSelect.CharacterID));
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.SwitchCharacter, ActionProcessor.CurrentPhase, disableAutoReplication: false, actorID, 0, NewPartyDisplayUI.PartyDisplay.SelectedUISlotIndex, PlayerRegistry.MyPlayer.PlayerID);
			executionFinished = true;
			IsChangingCharacter = true;
			DetermineRosterSlotInteractability();
			return true;
		}
		CMapCharacter data = targetUISlot.Data;
		IsChangingCharacter = true;
		if (party.SelectedCharacters.Contains(characterToSelect) && data != null && data.CharacterID != null && party.SelectedCharacters.Contains(data))
		{
			try
			{
				if (FFSNetwork.IsOnline && switchingPlayerID != 0 && PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID != switchingPlayerID && (from se in s.MyParticipatingControllables
					select se.ControllableObject into w
					where w is NewPartyCharacterUI newPartyCharacterUI && newPartyCharacterUI.Data?.CharacterID == characterToSelect.CharacterID
					select w).Count() > 0) != null)
				{
					executionFinished = true;
					Debug.Log("Player not switched as the controllable is owned by another player");
					IsChangingCharacter = false;
					DetermineRosterSlotInteractability();
					PlayerRegistry.IsSwitchingCharacter = false;
					return false;
				}
			}
			catch (InvalidOperationException ex)
			{
				Debug.LogError("More than one matching element was found in PlayerRegistery.AllPlayers for PlayerID: " + switchingPlayerID + "\n" + ex.Message + "\n" + ex.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00049", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
				return false;
			}
			int num = party.SelectedCharactersArray.IndexOf(data);
			int num2 = party.SelectedCharactersArray.IndexOf(characterToSelect);
			party.SelectedCharactersArray[num] = characterToSelect;
			party.SelectedCharactersArray[num2] = data;
		}
		else
		{
			party.RemoveNewUnlockedCharacter(characterToSelect.CharacterYMLData.ID);
			if (data != null && data.CharacterID != null)
			{
				int num3 = party.SelectedCharactersArray.IndexOf(data);
				party.SelectedCharactersArray[num3] = characterToSelect;
				party.AddSelectedCharacter(characterToSelect, num3);
				SaveDataShared.ApplyEnhancementIcons(characterToSelect.Enhancements, characterToSelect.CharacterID);
				if (!NewPartyDisplayUI.PartyDisplay.CharacterAlreadyBenched(data))
				{
					NewPartyDisplayUI.PartyDisplay.AddBenchedCharacter(data);
				}
				if (Singleton<UIReadyTrackerBar>.Instance != null)
				{
					Singleton<UIReadyTrackerBar>.Instance.RemoveTracker(data.CharacterID);
				}
			}
			else
			{
				int characterSlot = NewPartyDisplayUI.PartyDisplay.GetCharacterSlot(targetUISlot);
				if (characterSlot >= 0 && characterSlot <= 3)
				{
					party.SelectedCharactersArray[characterSlot] = characterToSelect;
					party.AddSelectedCharacter(characterToSelect, characterSlot);
					SaveDataShared.ApplyEnhancementIcons(characterToSelect.Enhancements, characterToSelect.CharacterID);
				}
				else
				{
					Debug.LogError("Invalid slot index `" + characterSlot + "' sent for character assignment.\n" + Environment.StackTrace);
				}
			}
		}
		if (FFSNetwork.IsHost && notifySwitch)
		{
			Debug.Log("[IsSwitchingCharacter] True - Host SelectCharacter about to send ToggleIsSwitchingCharacter side action");
			PlayerRegistry.IsSwitchingCharacter = true;
			PlayerRegistry.SwitchingPlayerID = switchingPlayerID;
			Synchronizer.SendSideAction(dataInt2: PlayerRegistry.SwitchingCharacterSlot = NewPartyDisplayUI.PartyDisplay.GetCharacterSlot(targetUISlot), actionType: GameActionType.ToggleIsSwitchingCharacter, supplementaryDataToken: null, canBeUnreliable: false, sendToHostOnly: false, targetPlayerID: 0, dataInt: switchingPlayerID, dataBool: true);
		}
		if (isLocalExecution)
		{
			if (data != null && data.CharacterID != null)
			{
				roster.ClearIfCharacterSelected(data);
			}
			selectedCharacter = characterToSelect;
			onSelected(characterToSelect, targetUISlot);
			BenchedCharacter benchedCharacter = null;
			try
			{
				benchedCharacter = NewPartyDisplayUI.PartyDisplay.GetBenchedCharacter(selectedCharacter);
			}
			catch (InvalidOperationException ex2)
			{
				Debug.LogError("More than one matching element was found in NewPartyDisplayUI.PartyDisplay.BenchedCharacters for CharacterID: " + selectedCharacter.CharacterID + " and CharacterName: " + selectedCharacter.CharacterName + "\n" + ex2.Message + "\n" + ex2.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00049", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
				return false;
			}
			if (benchedCharacter != null)
			{
				(Singleton<APartyDisplayUI>.Instance as NewPartyDisplayUI).SelectedUISlot.CreateControllable(initialController, overrideBenched: true);
				NewPartyDisplayUI.PartyDisplay.RemoveBenchedCharacter(selectedCharacter);
			}
			ShowSelectedCharacter(characterToSelect);
		}
		else
		{
			NewPartyDisplayUI.PartyDisplay.AssignNewCharacterToUISlot(characterToSelect, targetUISlot, initialController);
			if (IsVisible)
			{
				if (targetUISlot == NewPartyDisplayUI.PartyDisplay.SelectedUISlot)
				{
					DeselectAllCharacters();
					selectedCharacter = characterToSelect;
					ShowSelectedCharacter(characterToSelect);
				}
				else
				{
					DetermineRosterSlotInteractability();
				}
			}
		}
		Singleton<APartyDisplayUI>.Instance?.UpdatePartyLevel();
		int characterSlot2 = NewPartyDisplayUI.PartyDisplay.GetCharacterSlot(targetUISlot);
		string text = "Selected Characters:";
		try
		{
			CMapCharacter[] selectedCharactersArray = party.SelectedCharactersArray;
			foreach (CMapCharacter cMapCharacter in selectedCharactersArray)
			{
				if (cMapCharacter != null)
				{
					text = text + " Character ID: " + cMapCharacter.CharacterID;
					text = text + " Character Name: " + cMapCharacter.CharacterName;
				}
			}
		}
		catch
		{
		}
		if (data == null)
		{
			SimpleLog.AddToSimpleLog("Party Slot: " + characterSlot2 + " Selected: " + characterToSelect.CharacterID + " Character Name: " + characterToSelect.CharacterID);
		}
		else
		{
			SimpleLog.AddToSimpleLog("Party Slot: " + characterSlot2 + " Switched Character: " + data.CharacterID + " Character Name: " + data.CharacterID + " for " + characterToSelect.CharacterID + " Character Name: " + characterToSelect.CharacterID);
		}
		SimpleLog.AddToSimpleLog(text);
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.ProxyRequestRegenerateAllMapScenarios();
		}
		else if (PlayerRegistry.IsSwitchingCharacter)
		{
			PlayerRegistry.IsSwitchingCharacter = false;
		}
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Map)
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
		executionFinished = true;
		IsChangingCharacter = false;
		DetermineRosterSlotInteractability();
		return true;
	}

	public virtual void ShowSelectedCharacter(CMapCharacter character)
	{
		roster.ShowCharacterSelected(character);
		PreviewCharacterInfo(character);
		ValidatePartyCharacter();
	}

	public void DeselectAllCharacters()
	{
		roster.DeselectAllCharacters();
	}

	public void DetermineRosterSlotInteractability()
	{
		roster.DetermineSlotInteractability();
	}

	protected virtual void PreviewCharacterInfo(CMapCharacter character)
	{
		characterDisplay.Display(character);
		if (controllerArea.IsFocused)
		{
			characterDisplay.EnableNavigation();
		}
	}

	protected virtual void FinishPreviewCharacterInfo(CMapCharacter previeweCharacter)
	{
		if (window.IsOpen && !InputManager.GamePadInUse)
		{
			if (selectedCharacter != null && selectedCharacter.CharacterID != null)
			{
				PreviewCharacterInfo(selectedCharacter);
			}
			else
			{
				ShowNoSelectedCharacter();
			}
		}
	}

	public virtual bool RemoveCharacterFromParty(CMapCharacter character, ref bool executionFinished, bool networkIfOnline = true, bool save = true, bool isProxyCall = false, int switchingPlayerID = 0)
	{
		if (character == null)
		{
			FFSNet.Console.Log("Trying to remove a character but the character returns null.");
			executionFinished = true;
			return false;
		}
		if (!isProxyCall && !SceneController.Instance.RetiringCharacter && ((FFSNetwork.IsHost && ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && ActionProcessor.CurrentPhase == ActionPhaseType.MapHQ && PlayerRegistry.OtherClientsAreJoining)))
		{
			Debug.Log("Remove Character action discarded as another player is joining the game");
			executionFinished = true;
			return false;
		}
		if (FFSNetwork.IsHost && PlayerRegistry.IsSwitchingCharacter)
		{
			Debug.Log("Remove Character action discarded as another player is already switching");
			executionFinished = true;
			return false;
		}
		if (FFSNetwork.IsOnline && networkIfOnline)
		{
			int actorID = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
			ActionPhaseType currentPhase = ActionProcessor.CurrentPhase;
			int playerID = PlayerRegistry.MyPlayer.PlayerID;
			IProtocolToken supplementaryDataToken = new CampaignCharacterData(character.CharacterID, character.CharacterName);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.RemoveCharacter, currentPhase, disableAutoReplication: false, actorID, 0, 0, playerID, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			executionFinished = true;
			IsChangingCharacter = true;
			DetermineRosterSlotInteractability();
			return true;
		}
		IsChangingCharacter = true;
		if (FFSNetwork.IsHost)
		{
			Debug.Log("[IsSwitchingCharacter] True - Host RemoveCharacterFromParty about to send ToggleIsSwitchingCharacter side action");
			PlayerRegistry.IsSwitchingCharacter = true;
			PlayerRegistry.SwitchingPlayerID = switchingPlayerID;
			Synchronizer.SendSideAction(GameActionType.ToggleIsSwitchingCharacter, null, canBeUnreliable: false, sendToHostOnly: false, 0, switchingPlayerID, 0, dataBool: true);
		}
		if (party.CheckCharacters.Contains(character) && !NewPartyDisplayUI.PartyDisplay.CharacterAlreadyBenched(character))
		{
			NewPartyDisplayUI.PartyDisplay.AddBenchedCharacter(character);
		}
		if (Singleton<UIReadyTrackerBar>.Instance != null)
		{
			Singleton<UIReadyTrackerBar>.Instance.RemoveTracker(character.CharacterID);
		}
		if (character == selectedCharacter)
		{
			selectedCharacter = null;
			roster.ClearIfCharacterSelected(character);
			ShowNoSelectedCharacter();
		}
		party.RemoveSelectedCharacter(character);
		onDeselected(character);
		if (Singleton<APartyDisplayUI>.Instance != null)
		{
			Singleton<APartyDisplayUI>.Instance.UpdatePartyLevel();
		}
		if (save)
		{
			SaveData.Instance.SaveCurrentAdventureData();
		}
		if (Singleton<MapChoreographer>.Instance != null)
		{
			Singleton<MapChoreographer>.Instance.ProxyRequestRegenerateAllMapScenarios();
		}
		else if (PlayerRegistry.IsSwitchingCharacter)
		{
			Debug.Log("[IsSwitchingCharacter] False - RemoveCharacterFromParty finished locally");
			PlayerRegistry.IsSwitchingCharacter = false;
		}
		string text = "Selected Characters:";
		try
		{
			CMapCharacter[] selectedCharactersArray = party.SelectedCharactersArray;
			foreach (CMapCharacter cMapCharacter in selectedCharactersArray)
			{
				if (cMapCharacter != null)
				{
					text = text + " Character ID: " + cMapCharacter.CharacterID;
					text = text + " Character Name: " + cMapCharacter.CharacterName;
				}
			}
		}
		catch
		{
		}
		SimpleLog.AddToSimpleLog("Removed Character: " + character.CharacterID + " Character Name: " + character.CharacterName);
		SimpleLog.AddToSimpleLog(text);
		executionFinished = true;
		IsChangingCharacter = false;
		DetermineRosterSlotInteractability();
		return true;
	}

	protected virtual void ShowNoSelectedCharacter()
	{
		characterDisplay.Hide();
		ValidatePartyCharacter();
	}

	private void ValidatePartyCharacter()
	{
		if (party.SelectedCharacters.Count() >= AdventureState.MapState.MinRequiredCharacters)
		{
			HideInformation();
			return;
		}
		int count = AdventureState.MapState.MapParty.CheckCharacters.Count;
		if (count < AdventureState.MapState.MinRequiredCharacters)
		{
			if (party.SelectedCharacters.Count() < count)
			{
				warningText.text = LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROE");
				warningPanel.alpha = 1f;
			}
			else
			{
				HideInformation();
			}
		}
		else
		{
			warningText.text = string.Format(LocalizationManager.GetTranslation("GUI_ASSEMBLY_ERROR_QUEST_MIN_HEROES"), AdventureState.MapState.MinRequiredCharacters);
			warningPanel.alpha = 1f;
		}
	}

	protected void HideInformation()
	{
		warningPanel.alpha = 0f;
	}

	public void Hide(bool instant = false, bool playHideAudio = true)
	{
		string audioItemHide = window.AudioItemHide;
		if (!playHideAudio)
		{
			window.AudioItemHide = null;
		}
		window.Hide(instant);
		window.AudioItemHide = audioItemHide;
	}

	protected virtual void OnHidden()
	{
		characterDisplay.Hide(instant: true);
		roster.Hide();
		controllerArea.Destroy();
		onHidden?.Invoke();
	}

	public void FocusController()
	{
		if (window.IsOpen)
		{
			controllerArea.Focus();
		}
	}

	public void ToggeMercenaryInfoTooltip()
	{
		characterDisplay.ToggleMercenaryInfoTooltip();
	}
}
