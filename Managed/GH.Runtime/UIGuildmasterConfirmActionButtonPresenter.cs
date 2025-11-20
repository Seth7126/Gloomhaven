using System;
using FFSNet;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using UnityEngine;

public class UIGuildmasterConfirmActionButtonPresenter : UIGuildmasterConfirmActionPresenter
{
	[SerializeField]
	private UIGuildmasterConfirmActionButton _button;

	public override void ShowQuestSelectedAction(CQuestState quest, Action onConfirmCallback)
	{
		Sprite questSelectedIcon = GetQuestSelectedIcon(quest);
		Sprite questMarkerHighlightSprite = UIInfoTools.Instance.GetQuestMarkerHighlightSprite(quest);
		Action onClickedCallback = delegate
		{
			HideMultiplayerQuestPreview();
			onConfirmCallback?.Invoke();
		};
		Action onHoverCallback = delegate
		{
			ShowMultiplayerQuestPreview(quest);
		};
		Action onUnhoverCallback = delegate
		{
			HideMultiplayerQuestPreview();
		};
		_button.Show(questSelectedIcon, questMarkerHighlightSprite, onClickedCallback, onHoverCallback, onUnhoverCallback);
		PlayShowAudio();
	}

	public override void HideQuestSelectedAction()
	{
		HideMultiplayerQuestPreview();
		_button.Hide();
	}

	public override void ShowCharacterRetiredAction(CMapCharacter character, NetworkPlayer player, Action onConfirmCallback)
	{
		Sprite characterRetiredIcon = GetCharacterRetiredIcon(character);
		Sprite highlightQuestMarker = UIInfoTools.Instance.highlightQuestMarker;
		string characterRetiredTooltipText = GetCharacterRetiredTooltipText();
		_button.Show(characterRetiredIcon, highlightQuestMarker, onConfirmCallback, null, null, showAnimation: true, characterRetiredTooltipText);
		PlayShowAudio();
	}

	public override void HideCharacterRetiredAction()
	{
		_button.Hide();
	}

	public override void ShowCityEncounterAction(Action onConfirmCallback)
	{
	}

	public override void HideCityEncounterAction()
	{
	}

	public override void ClearAll()
	{
		HideQuestSelectedAction();
		HideCharacterRetiredAction();
	}

	private void ShowMultiplayerQuestPreview(CQuestState quest)
	{
		Singleton<UIQuestPopupManager>.Instance.ShowMultiplayerPreview(quest);
	}

	private void HideMultiplayerQuestPreview()
	{
		Singleton<UIQuestPopupManager>.Instance.HideMultiplayerPreview();
	}

	public override void ToggleOn()
	{
	}

	public override void ToggleOff()
	{
	}
}
