using System;
using System.Collections.Generic;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterSkinSelector : MonoBehaviour
{
	[Header("Skin settings")]
	[SerializeField]
	private Sprite[] skinToggleSprites;

	[SerializeField]
	private Image skinToggleImage;

	[SerializeField]
	private UITextTooltipTarget skinTooltip;

	[SerializeField]
	private TextMeshProUGUI skinName;

	private Action<string> onSelectedSkin;

	private bool defaultSkin;

	private List<string> skins;

	private ECharacter character;

	private CharacterConfigUI _characterConfigUi;

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Display(CMapCharacter characterData, Action<string> onSelectedSkin)
	{
		this.onSelectedSkin = onSelectedSkin;
		string characterName = characterData.CharacterYMLData.Model.ToString();
		string customCharacterConfig = characterData.CharacterYMLData.CustomCharacterConfig;
		skins = UIInfoTools.Instance.GetCharacterAppearanceSkin(characterName, customCharacterConfig);
		bool flag = UIInfoTools.Instance.CanUseAdditionalSkins(characterName, customCharacterConfig);
		character = characterData.CharacterYMLData.Model;
		_characterConfigUi = UIInfoTools.Instance.GetCharacterConfigUI(characterData.CharacterYMLData.Model.ToString(), useDefault: false, characterData.CharacterYMLData.CustomCharacterConfig);
		if (skins != null && skins.Count > 0 && flag)
		{
			defaultSkin = string.IsNullOrEmpty(characterData.SkinId);
			skinToggleImage.sprite = skinToggleSprites[(!defaultSkin) ? 1u : 0u];
			skinTooltip.SetText(LocalizationManager.GetTranslation(defaultSkin ? "GUI_SKIN_OFF_TOOLTIP" : "GUI_SKIN_ON_TOOLTIP"));
			skinName.text = LocalizationManager.GetTranslation(defaultSkin ? "GUI_SKIN_OFF_TOOLTIP" : $"{_characterConfigUi._skinPrefix}{character}_SKIN_{skins[0]}");
			base.gameObject.SetActive(value: true);
		}
		else
		{
			Hide();
		}
	}

	public void ToggleSkin()
	{
		if (skins != null && skins.Count != 0)
		{
			defaultSkin = !defaultSkin;
			skinToggleImage.sprite = skinToggleSprites[(!defaultSkin) ? 1u : 0u];
			skinTooltip.SetText(LocalizationManager.GetTranslation(defaultSkin ? "GUI_SKIN_OFF_TOOLTIP" : "GUI_SKIN_ON_TOOLTIP"), refreshTooltip: true);
			skinName.text = LocalizationManager.GetTranslation(defaultSkin ? "GUI_SKIN_OFF_TOOLTIP" : $"{_characterConfigUi._skinPrefix}{character}_SKIN_{skins[0]}");
			onSelectedSkin?.Invoke(defaultSkin ? null : skins[0]);
		}
	}
}
