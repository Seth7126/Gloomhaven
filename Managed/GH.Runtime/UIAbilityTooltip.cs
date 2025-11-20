using ScenarioRuleLibrary;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityTooltip : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Image titleIcon;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Transform descriptionContainer;

	[SerializeField]
	private UIItemTooltip itemTooltip;

	[SerializeField]
	private Transform itemContainer;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private ReferenceToSprite _referenceForTitleIcon;

	public void Initialize(IAbility ability, CPlayerActor player)
	{
		Clear();
		if (ability.DescriptionItem != null)
		{
			itemTooltip.Init(ability.DescriptionItem);
			itemTooltip.Show();
			descriptionContainer.gameObject.SetActive(value: false);
			itemContainer.gameObject.SetActive(value: true);
		}
		else
		{
			titleText.text = ability.DescriptionTitle;
			_referenceForTitleIcon = UIInfoTools.Instance.GetCharacterConfigUI(player.CharacterClass.CharacterModel, useDefault: true, player.CharacterClass.CharacterYML.CustomCharacterConfig).IconClass;
			descriptionText.text = ability.DescriptionText;
			descriptionContainer.gameObject.SetActive(value: true);
			itemContainer.gameObject.SetActive(value: false);
			itemTooltip.Hide();
		}
	}

	public void Clear()
	{
		itemTooltip.Clear();
	}

	protected void OnEnable()
	{
		if (_referenceForTitleIcon != null)
		{
			_imageSpriteLoader.AddReferenceToSpriteForImage(titleIcon, _referenceForTitleIcon);
		}
	}

	protected void OnDisable()
	{
		_imageSpriteLoader.Release();
	}
}
