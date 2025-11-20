using System;
using System.Collections.Generic;
using ScenarioRuleLibrary;
using SpriteMemoryManagement;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Character", fileName = "Character Config")]
public class CharacterConfigUI : ScriptableObject
{
	[Serializable]
	public class Object3DPlacement
	{
		public Vector3 position;

		public Vector3 rotation;
	}

	[Serializable]
	public class Summon3DPlacement
	{
		public CClass.ENPCModel summon;

		[SerializeField]
		public Object3DPlacement guildmasterPlacement;

		[SerializeField]
		public Object3DPlacement campaignPlacement;

		public Object3DPlacement GetObject3DPlacement(bool isGuildmaster)
		{
			if (isGuildmaster)
			{
				return guildmasterPlacement;
			}
			return campaignPlacement;
		}
	}

	public enum ECharacterGender
	{
		Male,
		Female
	}

	public ECharacter character;

	[Header("General")]
	public ReferenceToSprite IconClass;

	public ReferenceToSprite IconClassHighlight;

	public ReferenceToSprite IconClassGold;

	public ECharacterGender gender;

	[SerializeField]
	private bool _requireSkinsDlc;

	public AbilityCardUISkin cardSkin;

	public List<string> alternativeSkins;

	[Header("Adventure")]
	[SerializeField]
	public Sprite newAdventurePortrait;

	[SerializeField]
	public Sprite newHighlightAdventurePortrait;

	public QuestTypeConfigUI questConfig;

	[Header("New Adventure Assembly")]
	public bool customAssemblyCharacter3DPosition = true;

	[Tooltip("Position to place the 3D character in the adventure selection panel")]
	[ConditionalField("customAssemblyCharacter3DPosition", "true", true)]
	public Object3DPlacement guildmasterAssemblyCharacterPlacement;

	[ConditionalField("customAssemblyCharacter3DPosition", "true", true)]
	public Object3DPlacement campaignAssemblyCharacterPlacement;

	public List<Summon3DPlacement> assemblyCharacter3DCompanions;

	[Tooltip("Animation played when a new merc is created")]
	public string createAnimation = "Attack";

	[Header("Scenario")]
	public Sprite scenarioPortrait;

	[Tooltip("Portrait shown when the character is hovered in the scenario to sell its information")]
	public Sprite scenarioPreviewInfoPortrait;

	[Tooltip("Background for the right panel that appear when the character has active bonuses")]
	public Sprite initativeExtensionBackground;

	[Header("Battle Portrait")]
	public Sprite _tacticalBattlePortrait;

	public CharacterTabSkin tabIconConfig;

	public Sprite activeAbility;

	[Header("Assembly party")]
	public Sprite assemblyPortrait;

	public Vector2 rosterPortraitOffset;

	public bool hasToReveal;

	[Header("Distribution panel")]
	public Sprite distributionPortrait;

	[HideInInspector]
	public string ID;

	[Header("Solo Quest")]
	public Sprite soloMarker;

	public Sprite soloCompletedMarker;

	public string _skinPrefix = string.Empty;

	public bool RequireSkinsDlc => _requireSkinsDlc;

	public Sprite GetAdventurePortrait(bool highlighted = false)
	{
		if (!highlighted)
		{
			return newAdventurePortrait;
		}
		return newHighlightAdventurePortrait;
	}

	public ReferenceToSprite GetQuestRewardIcon(bool isGuildmaster)
	{
		ReferenceToSprite referenceToSprite = new ReferenceToSprite(questConfig.GetRewardIcon(isGuildmaster));
		if (referenceToSprite != null)
		{
			return referenceToSprite;
		}
		return IconClass;
	}

	public Object3DPlacement GetAssemblyCharacter3DPlacement(bool isGuildmaster)
	{
		if (isGuildmaster)
		{
			return guildmasterAssemblyCharacterPlacement;
		}
		return campaignAssemblyCharacterPlacement;
	}
}
