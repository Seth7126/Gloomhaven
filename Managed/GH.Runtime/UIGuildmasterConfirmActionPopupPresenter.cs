using System;
using FFSNet;
using GLOOM;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using UnityEngine;

public class UIGuildmasterConfirmActionPopupPresenter : UIGuildmasterConfirmActionPresenter
{
	private const string characterRetiredHeader = "CONSOLES/GUI_MULTIPLAYER_POPUP_CHARACTER_RETIRED_HEADER";

	private const string characterRetiredMessage = "CONSOLES/GUI_MULTIPLAYER_POPUP_CHARACTER_RETIRED_MESSAGE";

	private const string questHeader = "CONSOLES/GUI_MULTIPLAYER_POPUP_QUEST_HEADER";

	private const string questMessage = "CONSOLES/GUI_MULTIPLAYER_POPUP_QUEST_MESSAGE";

	private const string cityEncounterHeader = "CONSOLES/GUI_MULTIPLAYER_POPUP_CITY_ENCOUNTER_HEADER";

	private const string cityEncounterMessage = "CONSOLES/GUI_MULTIPLAYER_POPUP_CITY_ENCOUNTER_MESSAGE";

	[SerializeField]
	private UIGuildmasterConfirmActionPopup _popup;

	public override void ShowQuestSelectedAction(CQuestState quest, Action onConfirmCallback)
	{
		Sprite questSelectedIcon = GetQuestSelectedIcon(quest);
		string translation = LocalizationManager.GetTranslation("CONSOLES/GUI_MULTIPLAYER_POPUP_QUEST_HEADER");
		string translation2 = LocalizationManager.GetTranslation("CONSOLES/GUI_MULTIPLAYER_POPUP_QUEST_MESSAGE");
		_popup.Show(translation, translation2, questSelectedIcon, onConfirmCallback);
	}

	public override void HideQuestSelectedAction()
	{
		_popup.Hide();
	}

	public override void ShowCharacterRetiredAction(CMapCharacter character, NetworkPlayer player, Action onConfirmCallback)
	{
		Sprite characterRetiredIcon = GetCharacterRetiredIcon(character);
		string arg = (character.CharacterName.IsNOTNullOrEmpty() ? character.CharacterName : LocalizationManager.GetTranslation(character.CharacterYMLData.LocKey));
		string arg2 = player.UserNameWithPlatformIcon();
		string header = string.Format(LocalizationManager.GetTranslation("CONSOLES/GUI_MULTIPLAYER_POPUP_CHARACTER_RETIRED_HEADER"), arg);
		string message = string.Format(LocalizationManager.GetTranslation("CONSOLES/GUI_MULTIPLAYER_POPUP_CHARACTER_RETIRED_MESSAGE"), arg2);
		_popup.Show(header, message, characterRetiredIcon, onConfirmCallback);
	}

	public override void HideCharacterRetiredAction()
	{
		_popup.Hide();
	}

	public override void ShowCityEncounterAction(Action onConfirmCallback)
	{
		Sprite cityEncounterIcon = GetCityEncounterIcon();
		string translation = LocalizationManager.GetTranslation("CONSOLES/GUI_MULTIPLAYER_POPUP_CITY_ENCOUNTER_HEADER");
		string translation2 = LocalizationManager.GetTranslation("CONSOLES/GUI_MULTIPLAYER_POPUP_CITY_ENCOUNTER_MESSAGE");
		_popup.Show(translation, translation2, cityEncounterIcon, onConfirmCallback);
	}

	public override void HideCityEncounterAction()
	{
		_popup.Hide();
	}

	public override void ClearAll()
	{
		_popup.Hide();
	}

	public override void ToggleOn()
	{
		base.gameObject.SetActive(value: true);
	}

	public override void ToggleOff()
	{
		base.gameObject.SetActive(value: false);
	}
}
