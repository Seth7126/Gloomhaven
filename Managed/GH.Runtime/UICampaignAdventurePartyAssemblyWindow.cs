using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using SM.Gamepad;
using ScenarioRuleLibrary;
using Script.GUI.SMNavigation;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow), typeof(ControllerInputAreaLocal))]
public class UICampaignAdventurePartyAssemblyWindow : UIAdventurePartyAssemblyWindow
{
	[Header("Delete")]
	[SerializeField]
	private ExtendedButton deleteButton;

	[SerializeField]
	private UIAdventureCharacterConfirmationBox characterConfirmationBox;

	[Header("Create")]
	[SerializeField]
	private ExtendedButton createButton;

	[SerializeField]
	private UINavigationSelectable createButtonNavigationSelectable;

	[SerializeField]
	private GameObject createHighlight;

	[SerializeField]
	private AdventureCharacterCreator characterCreator;

	[SerializeField]
	private UINewNotificationTip newClassNotification;

	[SerializeField]
	private UIMapFTUEStep ftueStep;

	private ControllerInputAreaLocal controllerArea;

	private Color createButtonTextColor;

	private bool interactableDeleteCondition;

	private bool activeDeleteCondition;

	private IUiNavigationSelectable _createButtonSelectable;

	public AdventureCharacterCreator CharacterCreator => characterCreator;

	public bool CanDeleteCharacter
	{
		get
		{
			if (interactableDeleteCondition)
			{
				return activeDeleteCondition;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		controllerArea = GetComponent<ControllerInputAreaLocal>();
		base.Awake();
		deleteButton.onClick.AddListener(ConfirmDeleteCharacter);
		if (!InputManager.GamePadInUse)
		{
			createButton.onClick.AddListener(BuildNewCharacter);
		}
		if (InputManager.GamePadInUse)
		{
			_createButtonSelectable = createButton.GetComponent<IUiNavigationSelectable>();
		}
		createButtonTextColor = createButton.buttonText.color;
		ControllerAreaLocalFocusKeyActionHandlerBlocker keyActionHandlerBlocker = new ControllerAreaLocalFocusKeyActionHandlerBlocker(controllerArea);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.DELETE_CHARACTER, ConfirmDeleteCharacter).AddBlocker(keyActionHandlerBlocker));
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, BuildNewCharacter).AddBlocker(keyActionHandlerBlocker).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(createButton)).AddBlocker(new ExtendedButtonInteractableKeyActionHandlerBlocker(createButton)));
	}

	private void OnDestroy()
	{
		Singleton<KeyActionHandlerController>.Instance?.RemoveHandler(KeyAction.UI_SUBMIT, BuildNewCharacter);
		deleteButton.onClick.RemoveAllListeners();
		if (!InputManager.GamePadInUse)
		{
			createButton.onClick.RemoveAllListeners();
		}
	}

	public override void Initialize(CMapParty party, Action<CMapCharacter, NewPartyCharacterUI> onSelected, Action<CMapCharacter> onDeselected)
	{
		base.Initialize(party, delegate(CMapCharacter character, NewPartyCharacterUI slot)
		{
			onSelected(character, slot);
			CheckFTUE();
		}, onDeselected);
	}

	public override void Show(RectTransform point, CMapCharacter defaultSelected = null, Action onHidden = null, bool canEdit = true, bool instant = false)
	{
		base.Show(point, defaultSelected, onHidden, canEdit, instant);
		createHighlight.SetActive(party.CheckCharacters.Count < 2);
		createButton.interactable = CharacterClassManager.Classes.Count((CCharacterClass it) => party.UnlockedCharacterIDs.Contains(it.CharacterID)) > 0 && canEdit;
		createButton.buttonText.color = (createButton.interactable ? createButtonTextColor : UIInfoTools.Instance.greyedOutTextColor);
		if (createButton.interactable && InputManager.GamePadInUse && _createButtonSelectable != null)
		{
			Singleton<UINavigation>.Instance.NavigationManager.TrySelect(_createButtonSelectable);
		}
		interactableDeleteCondition = canEdit && (!FFSNetwork.IsOnline || selectedCharacter == null || selectedCharacter.IsUnderMyControl);
		deleteButton.interactable = interactableDeleteCondition;
		deleteButton.buttonText.color = (deleteButton.interactable ? UIInfoTools.Instance.warningColor : UIInfoTools.Instance.greyedOutTextColor);
		if (InputManager.GamePadInUse)
		{
			deleteButton.gameObject.SetActive(value: false);
		}
		RefreshNotifications();
		characterCreator.Close();
		RefreshDisabledCharacterOptions();
	}

	private void ConfirmDeleteCharacter()
	{
		if ((!FFSNetwork.IsHost || ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || PlayerRegistry.JoiningPlayers.Count <= 0) && (!FFSNetwork.IsClient || ActionProcessor.CurrentPhase != ActionPhaseType.MapHQ || !PlayerRegistry.OtherClientsAreJoining) && CanDeleteCharacter)
		{
			characterConfirmationBox.ShowConfirmationBox(selectedCharacter, delegate
			{
				DeleteCharacter(selectedCharacter);
			});
		}
	}

	private void RefreshNotifications()
	{
		createHighlight.SetActive(party.CheckCharacters.Count < 2 || party.NewUnlockedCharacterIDs.Count > 0);
		if (party.NewUnlockedCharacterIDs.Count > 0)
		{
			newClassNotification.Show();
		}
		else
		{
			newClassNotification.Hide();
		}
	}

	private void DeleteCharacter(CMapCharacter character, bool networkIfOnline = true)
	{
		if (FFSNetwork.IsOnline && networkIfOnline)
		{
			int supplementaryDataIDMax = (AdventureState.MapState.IsCampaign ? character.CharacterName.GetHashCode() : CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID));
			IProtocolToken supplementaryDataToken = new CampaignCharacterData(character.CharacterID, character.CharacterName);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.DeleteCharacter, ActionPhaseType.NONE, disableAutoReplication: false, 0, 0, 0, supplementaryDataIDMax, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			return;
		}
		bool executionFinished = false;
		if (RemoveCharacterFromParty(character, ref executionFinished, networkIfOnline: false))
		{
			if (!FFSNetwork.IsOnline && party.CheckCharacters.Count < 2)
			{
				Singleton<UIGuildmasterHUD>.Instance.Hide(EGuildmasterOptionsLock.Enough_Characters);
			}
			else
			{
				Singleton<UIGuildmasterHUD>.Instance.Show(EGuildmasterOptionsLock.Enough_Characters);
			}
			int num = 0;
			num = ((!AdventureState.MapState.IsCampaign) ? CharacterClassManager.GetModelInstanceIDFromCharacterID(character.CharacterID) : character.CharacterName.GetHashCode());
			ControllableRegistry.DestroyControllable(num);
			roster.RemoveCharacterOption(character);
			party.DeleteCharacter(character);
			NewPartyDisplayUI.PartyDisplay.RemoveBenchedCharacter(character);
			SaveData.Instance.SaveCurrentAdventureData();
			if (createButtonNavigationSelectable != null)
			{
				Singleton<UINavigation>.Instance.NavigationManager.TrySelect(createButtonNavigationSelectable);
			}
		}
	}

	public override bool RemoveCharacterFromParty(CMapCharacter character, ref bool executionFinished, bool networkIfOnline = true, bool save = true, bool isProxyCall = false, int switchingPlayerID = 0)
	{
		bool result = base.RemoveCharacterFromParty(character, ref executionFinished, networkIfOnline, save, isProxyCall, switchingPlayerID);
		if (base.IsVisible && !party.CheckCharacters.Contains(character))
		{
			roster.RemoveCharacterOption(character);
		}
		if (base.IsVisible)
		{
			RefreshDisabledCharacterOptions();
		}
		return result;
	}

	private void BuildNewCharacter()
	{
		HideInformation();
		characterDisplay.Hide();
		roster.CharacterHotkeysVisible = false;
		characterCreator.Create(party).Done(OnCreatedNewCharacter, delegate
		{
			if (FFSNetwork.IsOnline)
			{
				PlayerRegistry.MyPlayer.ToggleCharacterCreationScreen(value: false);
				Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
			}
			if (window.IsOpen)
			{
				RefreshNotifications();
				if (selectedCharacter != null)
				{
					ShowSelectedCharacter(selectedCharacter);
				}
				roster.CharacterHotkeysVisible = true;
			}
		});
	}

	protected override void FinishPreviewCharacterInfo(CMapCharacter previeweCharacter)
	{
		if (!characterCreator.IsCreating)
		{
			base.FinishPreviewCharacterInfo(previeweCharacter);
		}
	}

	protected override void PreviewCharacterInfo(CMapCharacter character)
	{
		base.PreviewCharacterInfo(character);
		activeDeleteCondition = selectedCharacter == character;
		if (!InputManager.GamePadInUse)
		{
			deleteButton.gameObject.SetActive(activeDeleteCondition);
		}
	}

	private void OnCreatedNewCharacter(CMapCharacter character)
	{
		OnCreatedNewCharacter(character, networkIfOnline: true);
		if (MapFTUEManager.IsPlaying)
		{
			NewPartyDisplayUI.PartyDisplay.EscapeCurrentCharacter();
		}
		roster.CharacterHotkeysVisible = true;
	}

	private void CheckFTUE()
	{
		if (MapFTUEManager.IsPlaying)
		{
			if (party.SelectedCharacters.Count() == 2)
			{
				ftueStep.SetStepConfig(UIInfoTools.Instance.GetMapFTUEConfig(EMapFTUEStep.CreatedSecondCharacter));
				Singleton<MapFTUEManager>.Instance.StartStep(ftueStep);
			}
			else if (party.SelectedCharacters.Count() == 1)
			{
				ftueStep.SetStepConfig(UIInfoTools.Instance.GetMapFTUEConfig(EMapFTUEStep.CreatedFirstCharacter));
				Singleton<MapFTUEManager>.Instance.StartStep(ftueStep);
			}
		}
	}

	public void RefreshDisabledCharacterOptions()
	{
		if (window.IsOpen)
		{
			List<string> usedCharacterModels = party.SelectedCharacters.Select((CMapCharacter it) => it.CharacterYMLData.ID).ToList();
			roster.DisableOptions(party.CheckCharacters.FindAll((CMapCharacter it) => (selectedCharacter == null || it.CharacterYMLData.ID != selectedCharacter.CharacterYMLData.ID) && usedCharacterModels.Contains(it.CharacterYMLData.ID)));
		}
	}

	private void OnCreatedNewCharacter(CMapCharacter character, bool networkIfOnline, int slotID = int.MaxValue, int playerID = int.MaxValue, bool selectCharacter = false)
	{
		RefreshNotifications();
		roster.RefreshSlots(AdventureState.MapState.IsCampaign ? party.CheckCharacters.FindAll((CMapCharacter it) => it.PersonalQuest != null) : party.CheckCharacters);
		RefreshDisabledCharacterOptions();
		bool flag = slotID == int.MaxValue && NewPartyDisplayUI.PartyDisplay.SelectedUISlot != null && NewPartyDisplayUI.PartyDisplay.SelectedUISlot.State == PartySlotState.Available && party.SelectedCharacters.All((CMapCharacter it) => it.CharacterYMLData.ID != character.CharacterYMLData.ID);
		if (networkIfOnline && FFSNetwork.IsOnline)
		{
			IProtocolToken supplementaryDataToken = new CampaignPersonalQuestData(character.CharacterID, character.PersonalQuest.ID, PlayerRegistry.MyPlayer.PlayerID, NewPartyDisplayUI.PartyDisplay.SelectedUISlot.SlotIndex, flag, character.CharacterName);
			Synchronizer.AutoExecuteServerAuthGameAction(GameActionType.CampaignAssignPersonalQuest, ActionPhaseType.NONE, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, default(Guid), supplementaryDataToken);
			return;
		}
		bool executionFinished = false;
		if (selectCharacter && slotID < NewPartyDisplayUI.PartyDisplay.CharacterSlots.Count)
		{
			if (!AdventureState.MapState.MapParty.SelectedCharactersArray.Any((CMapCharacter x) => x != null && x.CharacterID == character.CharacterID))
			{
				NetworkPlayer initialController = PlayerRegistry.AllPlayers.SingleOrDefault((NetworkPlayer s) => s.PlayerID == playerID);
				SelectCharacter(NewPartyDisplayUI.PartyDisplay.CharacterSlots[slotID], character, ref executionFinished, initialController, networkActionIfOnline: false, isLocalExecution: false, isProxyCall: true, 0, notifySwitch: false);
			}
		}
		else if (flag)
		{
			SelectCharacter(NewPartyDisplayUI.PartyDisplay.SelectedUISlot, character, ref executionFinished, null, networkActionIfOnline: false, isLocalExecution: true, isProxyCall: false, 0, notifySwitch: false);
		}
		else if (selectedCharacter == null)
		{
			if (!NewPartyDisplayUI.PartyDisplay.CharacterAlreadyBenched(character))
			{
				NewPartyDisplayUI.PartyDisplay.AddBenchedCharacter(character);
			}
			ShowNoSelectedCharacter();
			CheckFTUE();
		}
		else
		{
			if (!NewPartyDisplayUI.PartyDisplay.CharacterAlreadyBenched(character))
			{
				NewPartyDisplayUI.PartyDisplay.AddBenchedCharacter(character);
			}
			if (!FFSNetwork.IsOnline || slotID == int.MaxValue || (NewPartyDisplayUI.PartyDisplay.SelectedUISlot != null && NewPartyDisplayUI.PartyDisplay.SelectedUISlot.SlotIndex == slotID))
			{
				ShowSelectedCharacter(selectedCharacter);
			}
			CheckFTUE();
		}
		if (!FFSNetwork.IsOnline && party.CheckCharacters.Count < 2)
		{
			Singleton<UIGuildmasterHUD>.Instance.Hide(EGuildmasterOptionsLock.Enough_Characters);
		}
		else
		{
			Singleton<UIGuildmasterHUD>.Instance.Show(EGuildmasterOptionsLock.Enough_Characters);
		}
		SaveData.Instance.SaveCurrentAdventureData();
		SceneController.Instance.SelectingPersonalQuest = false;
		Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
	}

	protected override void ShowNoSelectedCharacter()
	{
		base.ShowNoSelectedCharacter();
		RefreshDisabledCharacterOptions();
	}

	public override void ShowSelectedCharacter(CMapCharacter character)
	{
		base.ShowSelectedCharacter(character);
		RefreshDisabledCharacterOptions();
	}

	public void ProxyCreateCampaignCharacter(GameAction action)
	{
		CampaignPersonalQuestData data = action.SupplementaryDataToken as CampaignPersonalQuestData;
		StartCoroutine(ProxyCreateCampaignCharacterCoroutine(data));
	}

	private IEnumerator ProxyCreateCampaignCharacterCoroutine(CampaignPersonalQuestData data)
	{
		CMapCharacter character;
		try
		{
			character = AdventureState.MapState.MapParty.CheckCharacters.Single((CMapCharacter s) => s.CharacterID == data.CharacterID && s.CharacterName == data.CharacterName);
			character.AssignPersonalQuest(character.PossiblePersonalQuests.First((CPersonalQuestState s) => s.ID == data.PersonalQuestID));
			character.DisplayCharacterName = (FFSNetwork.IsUGCEnabled ? character.CharacterName : LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey));
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while processing ProxyCreateCampaignCharacterCoroutine\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MULTIPLAYER_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			yield break;
		}
		yield return null;
		try
		{
			OnCreatedNewCharacter(character, networkIfOnline: false, data.SlotIndex, data.PlayerID, data.SelectCharacter);
		}
		catch (Exception ex2)
		{
			Debug.LogError("An exception occurred while processing ProxyCreateCampaignCharacterCoroutine\n" + ex2.Message + "\n" + ex2.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_MULTIPLAYER_00022", "GUI_ERROR_MAIN_MENU_BUTTON", ex2.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex2.Message);
		}
		if (FFSNetwork.IsOnline)
		{
			if (PlayerRegistry.MyPlayer.IsCreatingCharacter && PlayerRegistry.MyPlayer.CreatingCharacter.CharacterName == data.CharacterName)
			{
				PlayerRegistry.MyPlayer.ToggleCharacterCreationScreen(value: false);
			}
			Singleton<UIMapMultiplayerController>.Instance.RefreshWaitingNotifications();
		}
	}

	public void ProxyDeleteCharacter(GameAction action)
	{
		string characterID = (AdventureState.MapState.IsCampaign ? AdventureState.MapState.GetMapCharacterIDWithCharacterNameHash(action.SupplementaryDataIDMax, suppressErrors: true) : CharacterClassManager.GetCharacterIDFromModelInstanceID(action.SupplementaryDataIDMax));
		CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter s) => s.CharacterID == characterID);
		if (cMapCharacter != null)
		{
			DeleteCharacter(cMapCharacter, networkIfOnline: false);
		}
	}

	protected override void OnHidden()
	{
		characterCreator.Close();
		characterConfirmationBox.Hide();
		base.OnHidden();
		if (MapFTUEManager.IsPlaying && party.SelectedCharacters.Count() == 2)
		{
			Singleton<MapFTUEManager>.Instance.CompleteStep(EMapFTUEStep.CreatedSecondCharacter);
		}
	}
}
