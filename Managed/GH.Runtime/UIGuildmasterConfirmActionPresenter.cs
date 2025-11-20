using System;
using FFSNet;
using GLOOM;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using UnityEngine;

public abstract class UIGuildmasterConfirmActionPresenter : MonoBehaviour
{
	private const string _characterRetiredTooltip = "GUI_OTHER_PLAYER_RETIREMENT_TOOLTIP";

	[SerializeField]
	private string _showAudioItem = "PlaySound_UIMultiQuestSelected";

	public abstract void ShowQuestSelectedAction(CQuestState quest, Action onConfirmCallback);

	public abstract void HideQuestSelectedAction();

	public abstract void ShowCharacterRetiredAction(CMapCharacter character, NetworkPlayer player, Action onConfirmCallback);

	public abstract void HideCharacterRetiredAction();

	public abstract void ShowCityEncounterAction(Action onConfirmCallback);

	public abstract void HideCityEncounterAction();

	public abstract void ClearAll();

	public abstract void ToggleOn();

	public abstract void ToggleOff();

	protected Sprite GetCharacterRetiredIcon(CMapCharacter character)
	{
		return UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData);
	}

	protected Sprite GetQuestSelectedIcon(CQuestState quest)
	{
		return UIInfoTools.Instance.GetQuestMarkerSprite(quest);
	}

	protected Sprite GetCityEncounterIcon()
	{
		return UIInfoTools.Instance.GetCityEncounterMarkerSprite();
	}

	protected string GetCharacterRetiredTooltipText()
	{
		return LocalizationManager.GetTranslation("GUI_OTHER_PLAYER_RETIREMENT_TOOLTIP");
	}

	protected void PlayShowAudio()
	{
		AudioControllerUtils.PlaySound(_showAudioItem);
	}
}
