using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using FFSNet;
using GLOOM;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using SM.Gamepad;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;
using UnityEngine.UI;

public class UIAdventurePartyAssemblyRoster : MonoBehaviour
{
	[SerializeField]
	private VerticalPointerUI verticalPointer;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private Transform rosterHolder;

	[SerializeField]
	private UIAdventurePartyAssemblyRosterSlot slotPrefab;

	[SerializeField]
	private List<UIAdventurePartyAssemblyRosterSlot> rosterSlots;

	[SerializeField]
	private LocalHotkeys hotkeys;

	[SerializeField]
	private LocalHotkeys charActionsHotkeys;

	[SerializeField]
	private Hotkey scrollbarHotkey;

	[SerializeField]
	private ScrollRect _scrollRect;

	private Dictionary<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlots = new Dictionary<CMapCharacter, UIAdventurePartyAssemblyRosterSlot>();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterHover = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterUnhover = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterSelected = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	public UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent OnCharacterDeselected = new UIAdventurePartyAssemblyRosterSlot.PartyRosterEvent();

	private LTDescr animationDisplay;

	private PlayersChangedEvent OnPlayerJoinedHandler;

	private List<CMapCharacter> characters = new List<CMapCharacter>();

	private CMapCharacter selectedCharacter;

	private CMapCharacter lastSelelectedCharacter;

	private bool isNavigationEnabled;

	private Coroutine autoselectElementCoroutine;

	private UiNavigationManager navManager;

	private IHotkeySession _hotkeysSession;

	private IHotkeySession _charActionsHotkeysSession;

	private SessionHotkey _mercenaryInfoSessionHotkey;

	private SessionHotkey _concealPersonalQuestSessionHotkey;

	private SessionHotkey _changeSkinSessionHotkey;

	private SessionHotkey _deleteCharacterSessionHotkey;

	private bool characterHotkeysVisible = true;

	public bool CharacterHotkeysVisible
	{
		get
		{
			return characterHotkeysVisible;
		}
		set
		{
			characterHotkeysVisible = value;
			RefreshCharacterHotkeys();
		}
	}

	[UsedImplicitly]
	private void Awake()
	{
		navManager = Singleton<UINavigation>.Instance.NavigationManager;
		if (InputManager.GamePadInUse)
		{
			scrollbarHotkey.Initialize(Singleton<UINavigation>.Instance.Input);
			scrollbarHotkey.DisplayHotkey(active: false);
		}
	}

	[UsedImplicitly]
	protected void OnDestroy()
	{
		OnPlayerJoinedHandler = null;
		if (InputManager.GamePadInUse)
		{
			scrollbarHotkey.Deinitialize();
		}
	}

	public void Show(List<CMapCharacter> characters, CMapCharacter selectedCharacter, RectTransform sourceUI, bool instant = false)
	{
		if (sourceUI != null)
		{
			verticalPointer.PointAt(sourceUI);
		}
		CancelAnimations();
		Refresh(characters, selectedCharacter);
		if (instant)
		{
			showAnimation.GoToFinishState();
		}
		else
		{
			showAnimation.Play();
		}
		OnPlayerJoinedHandler = delegate
		{
			OnPlayerConnected();
		};
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, OnPlayerJoinedHandler);
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Combine(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserDisconnected));
		PlayerRegistry.OnDetermineRosterSlotInteractability = (DetermineRosterSlotInteractability)Delegate.Combine(PlayerRegistry.OnDetermineRosterSlotInteractability, new DetermineRosterSlotInteractability(DetermineSlotInteractability));
	}

	public void Refresh(List<CMapCharacter> characters, CMapCharacter selectedCharacter)
	{
		this.selectedCharacter = selectedCharacter;
		RefreshSlots(characters);
	}

	public void RefreshSlots(List<CMapCharacter> characters)
	{
		this.characters = characters;
		assignedSlots.Clear();
		HelperTools.NormalizePool(ref rosterSlots, slotPrefab.gameObject, rosterHolder, characters.Count);
		for (int i = 0; i < characters.Count; i++)
		{
			CMapCharacter cMapCharacter = characters[i];
			rosterSlots[i].Init(cMapCharacter, cMapCharacter == selectedCharacter, locked: false, _scrollRect);
			rosterSlots[i].OnCharacterHover.AddListener(OnHoveredCharacter);
			rosterSlots[i].OnCharacterUnhover.AddListener(OnUnhoveredCharacter);
			rosterSlots[i].OnCharacterSelected.AddListener(OnSelectedCharacter);
			rosterSlots[i].OnCharacterDeselected.AddListener(OnDeselectedCharacter);
			rosterSlots[i].DetermineInteractability(selectedCharacter);
			assignedSlots[cMapCharacter] = rosterSlots[i];
		}
		if (isNavigationEnabled)
		{
			for (int j = 0; j < assignedSlots.Count; j++)
			{
				rosterSlots[j].OnFocus();
			}
		}
	}

	public void Hide()
	{
		OnUnfocus();
		CancelAnimations();
		PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, OnPlayerJoinedHandler);
		PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserDisconnected));
		PlayerRegistry.OnDetermineRosterSlotInteractability = (DetermineRosterSlotInteractability)Delegate.Remove(PlayerRegistry.OnDetermineRosterSlotInteractability, new DetermineRosterSlotInteractability(DetermineSlotInteractability));
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			OnUnfocus(toPrevState: false);
			CancelAnimations();
			PlayerRegistry.OnUserConnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnUserConnected, new ConnectionEstablishedEvent(OnPlayerConnected));
			PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, OnPlayerJoinedHandler);
			PlayerRegistry.OnJoiningUserDisconnected = (ConnectionEstablishedEvent)Delegate.Remove(PlayerRegistry.OnJoiningUserDisconnected, new ConnectionEstablishedEvent(OnJoiningUserDisconnected));
			PlayerRegistry.OnDetermineRosterSlotInteractability = (DetermineRosterSlotInteractability)Delegate.Remove(PlayerRegistry.OnDetermineRosterSlotInteractability, new DetermineRosterSlotInteractability(DetermineSlotInteractability));
		}
	}

	private void CancelAnimations()
	{
		if (animationDisplay != null)
		{
			LeanTween.cancel(animationDisplay.id);
		}
		animationDisplay = null;
	}

	public void ShowCharacterSelected(CMapCharacter character)
	{
		ClearCharacterSelected();
		selectedCharacter = character;
		assignedSlots[character].SetSelected(isSelected: true);
		if (FFSNetwork.IsOnline)
		{
			DetermineSlotInteractability();
		}
	}

	public void ClearCharacterSelected()
	{
		if (selectedCharacter != null)
		{
			assignedSlots[selectedCharacter].SetSelected(isSelected: false);
			lastSelelectedCharacter = selectedCharacter;
			selectedCharacter = null;
			if (FFSNetwork.IsOnline)
			{
				DetermineSlotInteractability();
			}
		}
	}

	public void ClearIfCharacterSelected(CMapCharacter character)
	{
		if (selectedCharacter == character)
		{
			ClearCharacterSelected();
		}
	}

	public void RemoveCharacterOption(CMapCharacter character)
	{
		ClearIfCharacterSelected(character);
		UIAdventurePartyAssemblyRosterSlot uIAdventurePartyAssemblyRosterSlot = assignedSlots[character];
		uIAdventurePartyAssemblyRosterSlot.gameObject.SetActive(value: false);
		assignedSlots.Remove(character);
		if (isNavigationEnabled && navManager.CurrentlySelectedElement == uIAdventurePartyAssemblyRosterSlot.Selectable)
		{
			ControllerSelectSlot();
		}
	}

	public void DeselectAllCharacters()
	{
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.SetSelected(isSelected: false);
		}
	}

	private void OnSelectedCharacter(CMapCharacter character)
	{
		if (assignedSlots[character].IsLocked)
		{
			Singleton<UIConfirmationBoxManager>.Instance.ShowGenericCancelConfirmation(LocalizationManager.GetTranslation("GUI_CONFIRMATION_PARTY_LOCKED_TITLE"), LocalizationManager.GetTranslation("GUI_CONFIRMATION_PARTY_LOCKED"), "GUI_CLOSE");
			return;
		}
		OnCharacterSelected?.Invoke(character);
		if (InputManager.GamePadInUse)
		{
			UpdateSelectHotkey(assignedSlots[character].IsSelected);
			SetActiveDeleteCharacterHotkey();
		}
	}

	private void OnDeselectedCharacter(CMapCharacter character)
	{
		navManager.DeselectCurrentSelectable();
		OnCharacterDeselected?.Invoke(character);
	}

	private void OnHoveredCharacter(CMapCharacter character)
	{
		OnCharacterHover?.Invoke(character);
		if (InputManager.GamePadInUse)
		{
			SetActiveChangeSkinHotkey(character);
			SetActiveCampaignHotkeys();
			SetActiveDeleteCharacterHotkey();
			UpdateSelectHotkey(assignedSlots[character].IsSelected);
		}
	}

	private void UpdateSelectHotkey(bool isSelected)
	{
		if (InputManager.GamePadInUse)
		{
			if (isSelected)
			{
				_hotkeysSession.RemoveHotkey("Select");
				_hotkeysSession.SetHotkeyAdded("Unselect", added: true);
			}
			else
			{
				_hotkeysSession.RemoveHotkey("Unselect");
				_hotkeysSession.SetHotkeyAdded("Select", added: true);
			}
		}
	}

	private void OnUnhoveredCharacter(CMapCharacter character)
	{
		if (selectedCharacter == null)
		{
			_charActionsHotkeysSession?.RemoveAllHotkeys();
		}
		OnCharacterUnhover?.Invoke(character);
	}

	private void RefreshCharacterHotkeys()
	{
		if (_hotkeysSession != null && selectedCharacter != null)
		{
			SetActiveCampaignHotkeys();
			SetActiveChangeSkinHotkey(selectedCharacter);
			SetActiveDeleteCharacterHotkey();
		}
	}

	private void SetActiveChangeSkinHotkey(CMapCharacter character)
	{
		string characterName = character.CharacterYMLData.Model.ToString();
		string customCharacterConfig = character.CharacterYMLData.CustomCharacterConfig;
		List<string> characterAppearanceSkin = UIInfoTools.Instance.GetCharacterAppearanceSkin(characterName, customCharacterConfig);
		bool flag = UIInfoTools.Instance.CanUseAdditionalSkins(characterName, customCharacterConfig);
		bool shown = characterHotkeysVisible && characterAppearanceSkin != null && characterAppearanceSkin.Count > 0 && flag;
		_changeSkinSessionHotkey.SetShown(shown);
	}

	private void SetActiveDeleteCharacterHotkey()
	{
		bool shown = characterHotkeysVisible && AdventureState.MapState.IsCampaign && NewPartyDisplayUI.PartyDisplay.CampaignCharacterSelector.CanDeleteCharacter;
		_deleteCharacterSessionHotkey.SetShown(shown);
	}

	private void SetActiveCampaignHotkeys()
	{
		bool shown = AdventureState.MapState.IsCampaign && characterHotkeysVisible;
		_mercenaryInfoSessionHotkey.SetShown(shown);
		_concealPersonalQuestSessionHotkey.SetShown(shown);
	}

	public void Lock(CMapCharacter[] showLocks)
	{
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> slot in assignedSlots)
		{
			slot.Value.SetLocked(isLocked: true, Array.Exists(showLocks, (CMapCharacter e) => e == slot.Key));
		}
	}

	public void DisableOptions(List<CMapCharacter> options)
	{
		CMapCharacter cMapCharacter = null;
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlot in assignedSlots)
		{
			if (options.Contains(assignedSlot.Key))
			{
				assignedSlot.Value.DisableOption(disable: true, this);
				continue;
			}
			if (navManager.CurrentlySelectedElement == null && lastSelelectedCharacter != null && lastSelelectedCharacter == assignedSlot.Key)
			{
				cMapCharacter = assignedSlot.Key;
			}
			assignedSlot.Value.DisableOption(disable: false, this);
		}
		if (isNavigationEnabled && cMapCharacter != null)
		{
			StopAutoselectElementCoroutine();
			autoselectElementCoroutine = StartCoroutine(WaitControllerSelectSlot(cMapCharacter));
		}
	}

	public void DetermineSlotInteractability()
	{
		foreach (KeyValuePair<CMapCharacter, UIAdventurePartyAssemblyRosterSlot> assignedSlot in assignedSlots)
		{
			assignedSlot.Value.DetermineInteractability(selectedCharacter);
		}
	}

	public void OnFocus()
	{
		if (InputManager.GamePadInUse)
		{
			StartHotkeySession();
			scrollbarHotkey.DisplayHotkey(active: true);
		}
		isNavigationEnabled = true;
		for (int i = 0; i < assignedSlots.Count; i++)
		{
			rosterSlots[i].OnFocus();
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.AdventurePartyAssemblyRoster);
		StopAutoselectElementCoroutine();
		autoselectElementCoroutine = StartCoroutine(WaitControllerSelectSlot(null));
	}

	public void OnUnfocus(bool toPrevState = true)
	{
		for (int i = 0; i < rosterSlots.Count; i++)
		{
			rosterSlots[i].OnUnfocus();
		}
		StopAutoselectElementCoroutine();
		isNavigationEnabled = false;
		if (InputManager.GamePadInUse)
		{
			scrollbarHotkey.DisplayHotkey(active: false);
			FinishHotkeySession();
		}
	}

	private void StartHotkeySession()
	{
		_hotkeysSession = hotkeys.GetSessionOrEmpty().SetHotkeyAdded("Select", added: true).SetHotkeyAdded("Back", !MapFTUEManager.IsPlaying);
		_charActionsHotkeysSession = charActionsHotkeys.GetSessionOrEmpty().GetHotkey(out _mercenaryInfoSessionHotkey, "MercenaryInfo", NewPartyDisplayUI.PartyDisplay.CharacterSelector.ToggeMercenaryInfoTooltip).GetHotkey(out _concealPersonalQuestSessionHotkey, "ConcealPersonalQuest")
			.GetHotkey(out _changeSkinSessionHotkey, "ChangeSkin")
			.GetHotkey(out _deleteCharacterSessionHotkey, "DeleteCharacter");
	}

	private void FinishHotkeySession()
	{
		_hotkeysSession?.Dispose();
		_hotkeysSession = null;
		if (_charActionsHotkeysSession != null)
		{
			_charActionsHotkeysSession.Dispose();
			_charActionsHotkeysSession = null;
		}
	}

	private IEnumerator WaitControllerSelectSlot(CMapCharacter preferredSlot)
	{
		yield return null;
		autoselectElementCoroutine = null;
		ControllerSelectSlot(preferredSlot);
	}

	private void StopAutoselectElementCoroutine()
	{
		if (autoselectElementCoroutine != null)
		{
			StopCoroutine(autoselectElementCoroutine);
			autoselectElementCoroutine = null;
		}
	}

	private void ControllerSelectSlot(CMapCharacter preferredSlot = null)
	{
		StopAutoselectElementCoroutine();
		if (!isNavigationEnabled)
		{
			return;
		}
		IUiNavigationSelectable uiNavigationSelectable = null;
		if (preferredSlot != null && assignedSlots.ContainsKey(preferredSlot) && assignedSlots[preferredSlot].IsInteractable)
		{
			uiNavigationSelectable = assignedSlots[preferredSlot].Selectable;
		}
		else if (selectedCharacter != null)
		{
			uiNavigationSelectable = assignedSlots[selectedCharacter].Selectable;
		}
		else
		{
			UIAdventurePartyAssemblyRosterSlot uIAdventurePartyAssemblyRosterSlot = assignedSlots.Values.FirstOrDefault((UIAdventurePartyAssemblyRosterSlot it) => it.IsInteractable);
			if (uIAdventurePartyAssemblyRosterSlot != null)
			{
				uiNavigationSelectable = uIAdventurePartyAssemblyRosterSlot.Selectable;
			}
		}
		if (uiNavigationSelectable != null && navManager.CurrentlySelectedElement != uiNavigationSelectable)
		{
			navManager.TrySelect(uiNavigationSelectable);
		}
	}

	public void OnPlayerConnected(BoltConnection connection = null)
	{
		DetermineSlotInteractability();
	}

	private void OnJoiningUserDisconnected(BoltConnection connection = null)
	{
		DetermineSlotInteractability();
	}
}
