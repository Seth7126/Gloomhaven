using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using SM.Gamepad;
using Script.GUI;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using UnityEngine;

public class UICharacterCreatorClassRoster : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener information;

	[SerializeField]
	private UiNavigationRoot uiNavigationRoot;

	[SerializeField]
	private UITextTooltipTarget perksTooltip;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private Transform rosterHolder;

	[SerializeField]
	private UICharacterCreatorClassRosterSlot slotPrefab;

	[SerializeField]
	private List<UICharacterCreatorClassRosterSlot> rosterSlots;

	[SerializeField]
	private UIMapFTUEClassHighlight ftueHighlight;

	[SerializeField]
	private PanelHotkeyContainer panelHotkeyContainer;

	[SerializeField]
	private Hotkey[] _tooltipHotkeys;

	private Dictionary<ICharacterCreatorClass, UICharacterCreatorClassRosterSlot> assignedSlots = new Dictionary<ICharacterCreatorClass, UICharacterCreatorClassRosterSlot>();

	public UICharacterCreatorClassRosterSlot.CharacterClassRosterEvent OnCharacterHover = new UICharacterCreatorClassRosterSlot.CharacterClassRosterEvent();

	public UICharacterCreatorClassRosterSlot.CharacterClassRosterEvent OnCharacterUnhover = new UICharacterCreatorClassRosterSlot.CharacterClassRosterEvent();

	public UICharacterCreatorClassRosterSlot.CharacterClassRosterEvent OnCharacterSelected = new UICharacterCreatorClassRosterSlot.CharacterClassRosterEvent();

	private LTDescr animationDisplay;

	private ICharacterCreatorClass selectedCharacter;

	public ICharacterCreatorClass SelectedCharacter => selectedCharacter;

	private void OnDestroy()
	{
		OnCharacterHover.RemoveAllListeners();
		OnCharacterUnhover.RemoveAllListeners();
		OnCharacterSelected.RemoveAllListeners();
		CancelAnimations();
	}

	public void Setup(List<ICharacterCreatorClass> characters)
	{
		if (assignedSlots.Keys.Except(characters).Any() || characters.Except(assignedSlots.Keys).Any())
		{
			selectedCharacter = null;
			assignedSlots.Clear();
			HelperTools.NormalizePool(ref rosterSlots, slotPrefab.gameObject, rosterHolder, characters.Count);
			for (int i = 0; i < characters.Count; i++)
			{
				ICharacterCreatorClass characterCreatorClass = characters[i];
				rosterSlots[i].Init(characterCreatorClass, selected: false, characterCreatorClass.IsNew);
				rosterSlots[i].OnCharacterHover.AddListener(OnHoveredCharacter);
				rosterSlots[i].OnCharacterUnhover.AddListener(OnUnhoveredCharacter);
				rosterSlots[i].OnCharacterSelected.AddListener(OnSelectedCharacter);
				assignedSlots[characterCreatorClass] = rosterSlots[i];
			}
			ftueHighlight.SetClass(assignedSlots);
		}
	}

	public void Show(bool instant = false)
	{
		if (uiNavigationRoot != null)
		{
			uiNavigationRoot.enabled = true;
		}
		information.SetFormat($"<color=#d0d0d0>{AdventureState.MapState.HeadquartersState.CurrentStartingPerksAmount}</color> {{0}}");
		CancelAnimations();
		if (instant)
		{
			showAnimation.GoToFinishState();
		}
		else
		{
			showAnimation.Play();
		}
	}

	public void Hide()
	{
		if (uiNavigationRoot != null)
		{
			uiNavigationRoot.enabled = false;
		}
		CancelAnimations();
	}

	private void OnDisable()
	{
		Hide();
	}

	private void CancelAnimations()
	{
		if (animationDisplay != null)
		{
			LeanTween.cancel(animationDisplay.id);
		}
		animationDisplay = null;
	}

	public void ShowCharacterSelected(ICharacterCreatorClass character)
	{
		ClearCharacterSelected();
		selectedCharacter = character;
		assignedSlots[character].SetSelected(isSelected: true);
	}

	public void ClearCharacterSelected()
	{
		if (selectedCharacter != null)
		{
			assignedSlots[selectedCharacter].SetSelected(isSelected: false);
		}
		selectedCharacter = null;
	}

	private void OnSelectedCharacter(ICharacterCreatorClass character)
	{
		if (!object.Equals(selectedCharacter, character))
		{
			ShowCharacterSelected(character);
			OnCharacterSelected?.Invoke(character);
		}
	}

	private void OnHoveredCharacter(ICharacterCreatorClass character)
	{
		OnCharacterHover?.Invoke(character);
		if (InputManager.GamePadInUse)
		{
			OnSelectedCharacter(character);
		}
	}

	private void OnUnhoveredCharacter(ICharacterCreatorClass character)
	{
		if (perksTooltip != null)
		{
			perksTooltip.HideTooltip();
		}
		OnCharacterUnhover?.Invoke(character);
	}

	public void TogglePerksTooltip()
	{
		if (perksTooltip != null)
		{
			perksTooltip.ToggleTooltip();
		}
	}

	public void OnFocused()
	{
		if (panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActive(value: true);
			panelHotkeyContainer.transform.GetChild(0).gameObject.SetActive(!MapFTUEManager.IsPlaying);
		}
		Hotkey[] tooltipHotkeys = _tooltipHotkeys;
		for (int i = 0; i < tooltipHotkeys.Length; i++)
		{
			tooltipHotkeys[i].Initialize(Singleton<UINavigation>.Instance.Input);
		}
		for (int j = 0; j < assignedSlots.Count; j++)
		{
			rosterSlots[j].OnParentFocused();
		}
		Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.CharacterCreatorClassRoster);
	}

	public void OnUnfocused()
	{
		if (panelHotkeyContainer != null)
		{
			panelHotkeyContainer.SetActive(value: false);
		}
		for (int i = 0; i < assignedSlots.Count; i++)
		{
			rosterSlots[i].OnParentUnfocused();
		}
		if (perksTooltip != null)
		{
			perksTooltip.HideTooltip();
		}
		Hotkey[] tooltipHotkeys = _tooltipHotkeys;
		for (int j = 0; j < tooltipHotkeys.Length; j++)
		{
			tooltipHotkeys[j].Deinitialize();
		}
	}
}
