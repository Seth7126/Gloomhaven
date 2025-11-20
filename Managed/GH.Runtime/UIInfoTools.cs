#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AsmodeeNet.Utils.Extensions;
using FFSNet;
using GLOO.Introduction;
using JetBrains.Annotations;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.MapState;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.Quest;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

public class UIInfoTools : MonoBehaviour
{
	[Serializable]
	public struct EffectInfo
	{
		public Sprite Icon;

		public string Name;

		public string Description;

		public Sprite BigIcon;

		public Sprite TempleIcon;
	}

	public enum BackgroundColour
	{
		None,
		Blue,
		Red,
		AdvantageBlue,
		DisadvantageRed
	}

	public enum AdvantageType
	{
		None,
		PositiveUsed,
		PositiveDiscarded,
		NegativeUsed,
		NegativeDiscarded
	}

	public enum TooltipColours
	{
		NoChange,
		White,
		Orange,
		Red,
		Purple
	}

	[Serializable]
	public class CharacterOutlinableColourSetting
	{
		public CActor.EType ActorType;

		public Color OutlineColor;
	}

	[Serializable]
	public class PropOutlinableColourSetting
	{
		public ScenarioManager.ObjectImportType PropType;

		public Color OutlineColor;
	}

	[Serializable]
	private class AchievementIconSetting
	{
		public EAchievementType type;

		public AchievementIconLevelSetting[] levels;
	}

	[Serializable]
	private struct AchievementIconLevelSetting
	{
		public Sprite iconCompleted;

		public Sprite icon;
	}

	[Serializable]
	private class GuildmasterModeSetting
	{
		public EGuildmasterMode mode;

		public Sprite icon;
	}

	[Header("Ability Card Layout Sprites")]
	public Sprite AugmentIcon;

	public Sprite CommandIcon;

	public Sprite DoomIcon;

	public Sprite SongIcon;

	public Sprite XPArrowIcon;

	public SpriteAtlas AreaEffectSpriteAtlas;

	public EffectInfo Muddled;

	public EffectInfo Strengthened;

	public EffectInfo Wounded;

	public EffectInfo Immobilized;

	public EffectInfo Poisoned;

	public EffectInfo Disarmed;

	public EffectInfo Shield;

	public EffectInfo Retaliate;

	public EffectInfo Stun;

	public EffectInfo Invisible;

	public EffectInfo Blessed;

	public EffectInfo Cursed;

	public EffectInfo Advantage;

	public EffectInfo Disadvantage;

	public EffectInfo Sleeping;

	public Sprite DamageSprite;

	public ElementConfigUI airConfig;

	public ElementConfigUI fireConfig;

	public ElementConfigUI iceConfig;

	public ElementConfigUI earthConfig;

	public ElementConfigUI lightConfig;

	public ElementConfigUI darkConfig;

	public ElementConfigUI anyElementConfig;

	public ElementConfigUI firelightConfig;

	[SerializeField]
	private Sprite[] xpSprites;

	[SerializeField]
	private Sprite[] modifiersSprites;

	[SerializeField]
	private Sprite[] immunityIcons;

	[SerializeField]
	private EnhancementEffectsData[] enhancementEffects;

	public Color White;

	public Color Orange;

	public Color Red;

	public Color Purple;

	public Color Green;

	[Header("Attack modifiers")]
	public Sprite NullModifierIcon;

	public Sprite Pull;

	public Sprite Push;

	public Sprite Push2;

	public Sprite Heal;

	public Sprite Target;

	public Sprite Pierce;

	public Sprite Generic;

	public Sprite Damage;

	public Sprite HealAlly0;

	public Sprite HealAlly1;

	public Sprite IgnoreNegativeItemEffectsIcon;

	public Sprite IgnoreNegativeScenarioEffectsIcon;

	public AttackModifierConfig[] customModifierConfigs;

	public Color lowModifierColor = Color.magenta;

	public Color regularModifierColor = Color.white;

	public Color highModifierColor = Color.red;

	[Header("Characters")]
	[SerializeField]
	public List<CharacterConfigUI> characterConfigsUI;

	public Sprite highlightQuestMarker;

	[SerializeField]
	private Sprite highlightSoloQuestMarker;

	[SerializeField]
	public ReferenceToSprite[] CharacterIconsAds;

	[SerializeField]
	public ReferenceToSprite[] actorPortraitsAds;

	[Header("UI Text Colors")]
	public Color mainColor = new Color(236f, 208f, 141f, 255f);

	public Color basicTextColor = new Color(243f, 221f, 171f);

	public Color descriptionTextColor;

	public Color dialogueTextColor;

	public Color neutralActionTextColor;

	public Color positiveTextColor;

	public Color negativeTextColor;

	public Color greyedOutTextColor;

	public Color positiveStatusEffectColor = new Color(255f, 164f, 20f);

	public Color negativeStatusEffectColor = new Color(160f, 80f, 224f);

	public Color greenDifficultyColor;

	public Color yellowDifficultyColor;

	public Color redDifficultyColor;

	public Material greyedOutMaterial;

	public Material disabledGrayscaleMaterial;

	[Header("New adventure Text Colors")]
	public Color warningColor;

	public Color goldColor;

	public Color goldTextColor = new Color(253f, 199f, 74f);

	public Color buyEnhancementColor;

	public Color sellEnhancementColor;

	[Header("MP Text Colors")]
	public Color playerOnlineColor = Color.green;

	public Color playerConnectingColor = Color.red;

	public Color playerOfflineColor = new Color(236f, 126f, 0f);

	public Color playerAwayColor = new Color(160f, 160f, 160f);

	[Header("Active abilities")]
	public Color currentRoundColor = Color.white;

	public Color pastRoundColor;

	public Color leftRoundColor;

	[SerializeField]
	private Sprite[] activeAbilityIcons;

	[SerializeField]
	public Sprite[] activeAbilityDurationIcons;

	[SerializeField]
	public Sprite[] durationIcons;

	[Header("Character Outline Colors")]
	public List<CharacterOutlinableColourSetting> m_CharacterOutlinableColourSettings = new List<CharacterOutlinableColourSetting>();

	private Dictionary<CActor.EType, Color> m_CharacterOutlineColourDictionary = new Dictionary<CActor.EType, Color>();

	[Header("Prop Outline Colors")]
	public List<PropOutlinableColourSetting> m_PropOutlinableColourSettings = new List<PropOutlinableColourSetting>();

	private Dictionary<ScenarioManager.ObjectImportType, Color> m_PropOutlineColourDictionary = new Dictionary<ScenarioManager.ObjectImportType, Color>();

	[Header("Items")]
	[SerializeField]
	private Sprite[] itemSlotIcons;

	[SerializeField]
	private ItemRarityConfigUI[] itemRarityConfigs;

	[SerializeField]
	private List<ItemConfigUI> itemConfigs;

	private Dictionary<string, ItemConfigUI> tempItemConfigsDict;

	[Header("Enhancements")]
	[SerializeField]
	private EnhancementConfigUI[] enhancementConfigs;

	[Header("Abilities")]
	[SerializeField]
	private AbilityTypePreviewConfigUI[] abilityTypePreviewConfigs;

	private Dictionary<CAbility.EAbilityType, AbilityTypePreviewConfigUI> tempAbilityTypePreviewConfigsDict;

	[SerializeField]
	private PreviewEffectConfigUI[] previewEffectConfigs;

	private Dictionary<string, PreviewEffectConfigUI> tempPreviewEffectConfigsDict;

	[Header("Story")]
	public List<StoryCharacterConfigUI> StoryCharacters;

	[Header("Quest")]
	public QuestTypeConfigUI bossQuest;

	[SerializeField]
	private QuestTypeConfigUI travelQuest;

	public QuestTypeConfigUI relicQuest;

	[SerializeField]
	private QuestTypeConfigUI jobQuest;

	[SerializeField]
	private QuestTypeConfigUI storyQuest;

	public QuestTypeConfigUI casualModeQuest;

	[SerializeField]
	private List<QuestAreaConfigUI> questAreaConfigs;

	[Header("Narrative")]
	[FormerlySerializedAs("questLoadouts")]
	public NarrativeConfigUI[] narrativeImages;

	[Header("Rewards")]
	[SerializeField]
	[FormerlySerializedAs("questRewards")]
	private RewardConfigUI[] guildmasterQuestRewards;

	[SerializeField]
	private RewardConfigUI[] campaingQuestRewards;

	[SerializeField]
	private RewardVisibilityConfigUI[] rewardsVisiblity;

	private Dictionary<string, RewardVisibilityConfigUI> tempRewardVisibilityDict;

	[Header("Locations")]
	[SerializeField]
	private List<LocationConfigUI> locationsConfig;

	[Header("Achievements")]
	public Color achievementCompletedColor;

	[SerializeField]
	private AchievementIconSetting[] achievementLevelIcons;

	[Header("Guildmaster")]
	[SerializeField]
	private GuildmasterModeSetting[] guildmasterIcons;

	[Space]
	public Texture2D defaultModThumbnail;

	[Header("Carryable Item")]
	public Sprite CarryableQuestItemWorldSprite;

	public Sprite CarryableQuestItemIconSprite;

	[Header("Map")]
	public Sprite defaultEventImage;

	public Sprite defaultCityEventImage;

	public float eventZoomDuration = 0.5f;

	public LeanTweenType focusAnimationEase;

	[ConditionalField("focusAnimationEase", "animationCurve", true)]
	public AnimationCurve focusAnimationCurve;

	[Header("UI Components")]
	public ScrollRectSettings scrollRectConfig;

	public float typingRevealDelay = 0.1f;

	[SerializeField]
	private TMP_SpriteAsset AbilityCardSpriteAsset;

	private List<TMP_SpriteAsset> tmpAbilityCardSpriteAssets = new List<TMP_SpriteAsset>();

	[Header("Audio")]
	public AudioButtonProfile generalAudioButtonProfile;

	public string toggleUseCharacterSlotAudioItem = "PlaySound_UIMercAbility";

	public string tileClickAudioItem = "PlaySound_ScenarioUI_TileConfirm";

	public string tileCancelClickAudioItem = "PlaySound_UIUndoHex";

	[Header("MP Notifications")]
	public float disconnectedPlayerNotificationDuration = 10f;

	public float selectedOtherPlayerCardNotificationDuration = 10f;

	public float defaultNotificationDuration = 5f;

	private static readonly Color[] defaultPlayerColors = new Color[4]
	{
		Color.red,
		Color.blue,
		Color.yellow,
		Color.green
	};

	[Header("Introductions")]
	[SerializeField]
	private List<IntroductionConceptConfigUI> introductionConfigs;

	[SerializeField]
	private List<MapFTUEStepConfigUI> campaignFTUEConfigs;

	[Header("DLC")]
	[SerializeField]
	private List<DLCConfig> dlcConfigs;

	[Header("Controller")]
	[SerializeField]
	private List<GamepadDeviceMappingConfig> devicesMapping;

	public static UIInfoTools Instance { get; private set; }

	public string InvalidOptionAudioItem => generalAudioButtonProfile.nonInteractableMouseDownAudioItem;

	public Sprite GetIconPositiveCondition(CCondition.EPositiveCondition condition, bool big = false)
	{
		EffectInfo effectInfoCondition = GetEffectInfoCondition(condition);
		if (!big)
		{
			return effectInfoCondition.Icon;
		}
		return effectInfoCondition.BigIcon;
	}

	public Sprite GetIconNegativeCondition(CCondition.ENegativeCondition condition, bool big = false)
	{
		EffectInfo effectInfoCondition = GetEffectInfoCondition(condition);
		if (!big)
		{
			return effectInfoCondition.Icon;
		}
		return effectInfoCondition.BigIcon;
	}

	public EffectInfo GetEffectInfoCondition(CCondition.ENegativeCondition condition)
	{
		return condition switch
		{
			CCondition.ENegativeCondition.Poison => Poisoned, 
			CCondition.ENegativeCondition.Curse => Cursed, 
			CCondition.ENegativeCondition.Disadvantage => Disadvantage, 
			CCondition.ENegativeCondition.Disarm => Disarmed, 
			CCondition.ENegativeCondition.Immobilize => Immobilized, 
			CCondition.ENegativeCondition.Stun => Stun, 
			CCondition.ENegativeCondition.Muddle => Muddled, 
			CCondition.ENegativeCondition.Wound => Wounded, 
			CCondition.ENegativeCondition.Sleep => Sleeping, 
			_ => default(EffectInfo), 
		};
	}

	public EffectInfo GetEffectInfoCondition(CCondition.EPositiveCondition condition)
	{
		return condition switch
		{
			CCondition.EPositiveCondition.Strengthen => Strengthened, 
			CCondition.EPositiveCondition.Bless => Blessed, 
			CCondition.EPositiveCondition.Invisible => Invisible, 
			CCondition.EPositiveCondition.Advantage => Advantage, 
			_ => default(EffectInfo), 
		};
	}

	public ElementConfigUI GetElementConfig(ElementInfusionBoardManager.EElement element, ElementInfusionBoardManager.EElement? secondElement = null)
	{
		switch (element)
		{
		case ElementInfusionBoardManager.EElement.Air:
			return airConfig;
		case ElementInfusionBoardManager.EElement.Earth:
			return earthConfig;
		case ElementInfusionBoardManager.EElement.Fire:
			if (secondElement == ElementInfusionBoardManager.EElement.Light)
			{
				return firelightConfig;
			}
			return fireConfig;
		case ElementInfusionBoardManager.EElement.Ice:
			return iceConfig;
		case ElementInfusionBoardManager.EElement.Light:
			return lightConfig;
		case ElementInfusionBoardManager.EElement.Dark:
			return darkConfig;
		default:
			return anyElementConfig;
		}
	}

	public string GetSelectElementAudioItemIcon(ElementInfusionBoardManager.EElement element)
	{
		return GetElementConfig(element)?.selectAudioItem;
	}

	public Sprite GetRewardElementIcon(ElementInfusionBoardManager.EElement element)
	{
		ElementConfigUI elementConfig = GetElementConfig(element);
		if (!(elementConfig.pickerIcon == null))
		{
			return elementConfig.rewardIcon;
		}
		return elementConfig.icon;
	}

	public Sprite GetElementIcon(ElementInfusionBoardManager.EElement element, ElementInfusionBoardManager.EElement? secondElement = null)
	{
		return GetElementConfig(element, secondElement).icon;
	}

	public Color GetElementHighlightColor(ElementInfusionBoardManager.EElement element, float? alpha = null)
	{
		Color result = GetElementConfig(element).highlightColor;
		if (alpha.HasValue)
		{
			result = new Color(result.r, result.g, result.b, alpha.Value);
		}
		return result;
	}

	public Sprite GetElementPickerSprite(ElementInfusionBoardManager.EElement element)
	{
		ElementConfigUI elementConfig = GetElementConfig(element);
		if (!(elementConfig.pickerIcon == null))
		{
			return elementConfig.pickerIcon;
		}
		return elementConfig.icon;
	}

	public Sprite GetElementUseSprite(ElementInfusionBoardManager.EElement element)
	{
		ElementConfigUI elementConfig = GetElementConfig(element);
		if (!(elementConfig.useIcon == null))
		{
			return elementConfig.useIcon;
		}
		return elementConfig.icon;
	}

	public Sprite GetXPSprite(int xp)
	{
		if (xp < 0 || xp >= xpSprites.Count())
		{
			Debug.LogWarning("Can't find 'XP" + xp + "' sprite, using 0 instead");
			return xpSprites[0];
		}
		return xpSprites[xp];
	}

	public CharacterTabSkin GetCharacterTabSkin(string characterName, string custom = null)
	{
		return GetCharacterConfigUI(characterName, useDefault: true, custom).tabIconConfig;
	}

	public Sprite GetCharacterAssemblySprite(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).assemblyPortrait;
	}

	public ReferenceToSprite GetCharacterAssemblyIcon(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).IconClassGold;
	}

	public ReferenceToSprite GetCharacterAssemblyIcon(string character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).IconClassGold;
	}

	public List<string> GetCharacterAppearanceSkin(string characterName, string custom = null)
	{
		return GetCharacterConfigUI(characterName, useDefault: false, custom)?.alternativeSkins;
	}

	public bool CanUseAdditionalSkins(string characterName, string custom = null)
	{
		CharacterConfigUI characterConfigUI = Instance.GetCharacterConfigUI(characterName, useDefault: false, custom);
		bool num = characterConfigUI != null && characterConfigUI.RequireSkinsDlc;
		bool result = true;
		if (num)
		{
			result = PlatformLayer.DLC.UserInstalledDLC(DLCRegistry.EDLCKey.DLC3);
		}
		return result;
	}

	public Sprite GetCharacterActiveAbilityIcon(string characterName, string custom = null)
	{
		return GetCharacterConfigUI(characterName, useDefault: true, custom)?.activeAbility;
	}

	public Sprite GetActiveAbilityIcon(string iconName, string fallbackIconName = null, bool displayError = true)
	{
		Sprite sprite = activeAbilityIcons.GetSprite(iconName, toLower: false, useDefaultWhenMissing: false);
		if (sprite != null)
		{
			return sprite;
		}
		sprite = GetActiveBonusIconFromDLC(iconName, suppressError: true);
		if (sprite != null)
		{
			return sprite;
		}
		if (!fallbackIconName.IsNullOrEmpty())
		{
			return GetActiveAbilityIcon(fallbackIconName, null, displayError);
		}
		if (sprite == null && !iconName.StartsWith("AA_"))
		{
			sprite = GetActiveAbilityIcon("AA_" + iconName, null, displayError);
		}
		if (sprite == null && displayError)
		{
			Debug.LogErrorGUI("Missing active ability icon " + iconName);
		}
		return sprite;
	}

	public Sprite GetActiveBonusIconFromDLC(string iconName, bool suppressError = false)
	{
		return GetIconFromDLC(iconName, "Content/GUI/AbilityCardIcons/", suppressError);
	}

	private Sprite GetIconFromDLC(string iconName, string folder = "Content/GUI", bool suppressError = false)
	{
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey eDLCKey in dLCKeys)
		{
			if (eDLCKey != DLCRegistry.EDLCKey.None && PlatformLayer.DLC.CanPlayDLC(eDLCKey))
			{
				Sprite dLCGUIAssetFromBundle = PlatformLayer.DLC.GetDLCGUIAssetFromBundle<Sprite>(eDLCKey, iconName, folder, "png", suppressError);
				if (dLCGUIAssetFromBundle != null)
				{
					return dLCGUIAssetFromBundle;
				}
			}
		}
		return null;
	}

	public Sprite GetActiveAbilityIcon(CActiveBonus activeBonus)
	{
		string text = null;
		if (activeBonus?.Layout?.IconNames != null && activeBonus.Layout.IconNames.Count > 0)
		{
			text = activeBonus.Layout.IconNames[0];
		}
		else if (activeBonus?.Ability?.Augment != null)
		{
			text = "Mindthief";
		}
		else if (activeBonus != null && activeBonus.IsSong)
		{
			text = "Soothsinger";
		}
		if (text.IsNullOrEmpty())
		{
			text = activeBonus?.Caster?.GetPrefabName();
		}
		if (activeBonus != null && activeBonus.Ability?.AbilityType == CAbility.EAbilityType.Shield && GetActiveAbilityIcon(text, null, displayError: false) == null)
		{
			text = "Voidwarden";
		}
		return GetActiveAbilityIcon(text);
	}

	public EffectDataBase GetElementEffect(ElementInfusionBoardManager.EElement element)
	{
		return GetElementConfig(element).effectData;
	}

	public EnhancementEffectsData GetEnhancementEffect(EEnhancement enhancement)
	{
		EnhancementEffectsData[] array = enhancementEffects;
		foreach (EnhancementEffectsData enhancementEffectsData in array)
		{
			if (enhancementEffectsData.enhancement == enhancement)
			{
				return enhancementEffectsData;
			}
		}
		return null;
	}

	public string GetActiveAbilityIconName(CActiveBonus activeBonus)
	{
		return activeAbilityIcons.GetObjectName((activeBonus == null || activeBonus.Layout == null || activeBonus.Layout.IconNames == null || activeBonus.Layout.IconNames.Count == 0) ? null : activeBonus.Layout.IconNames[0]);
	}

	public Sprite GetActiveAbilityIconBackground(CActor actor)
	{
		if (actor is CPlayerActor)
		{
			return GetCharacterConfigUI(actor.GetPrefabName(), useDefault: true, (actor as CPlayerActor)?.CharacterClass?.CharacterYML.CustomCharacterConfig).initativeExtensionBackground;
		}
		return GetCharacterConfigUI(actor.GetPrefabName()).initativeExtensionBackground;
	}

	public Sprite GetAttackModifierSprite(string mathModifier)
	{
		return modifiersSprites.GetSprite(mathModifier.Replace('*', 'x'));
	}

	public Sprite GetImmunityIcon(string immunity)
	{
		return immunityIcons.GetSprite(immunity);
	}

	public Sprite GetCharacterHeroPortrait(string character, string custom = null)
	{
		if (custom == null)
		{
			string charName = Regex.Replace(character, "ID$", "");
			CharacterConfigUI characterConfigUI = characterConfigsUI.SingleOrDefault((CharacterConfigUI x) => charName == x.character.ToString());
			return (characterConfigUI ? characterConfigUI : ((characterConfigsUI.Count > 0) ? characterConfigsUI[0] : null))._tacticalBattlePortrait;
		}
		return (SceneController.Instance.YML.ModdedCharacterConfigs?.SingleOrDefault((CharacterConfigUI s) => custom == s.ID) ?? ((characterConfigsUI.Count > 0) ? characterConfigsUI[0] : null)).scenarioPortrait;
	}

	public Sprite GetNewAdventureCharacterPortrait(ECharacter character, bool highlighted = false, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).GetAdventurePortrait(highlighted);
	}

	public Sprite GetAttackModifierIcon(AttackModifierYMLData modifier, bool useOriginal = false)
	{
		AttackModifierConfig attackModifierConfig = customModifierConfigs.FirstOrDefault((AttackModifierConfig it) => it.name == modifier.Name);
		if (attackModifierConfig != null)
		{
			return attackModifierConfig.icon;
		}
		if (modifier.UseGenericIcon)
		{
			return Generic;
		}
		if (modifier.Card.Infuse)
		{
			if (modifier.Card.InfuseElements.Count > 1)
			{
				return GetElementIcon(modifier.Card.InfuseElements[0], modifier.Card.InfuseElements[1]);
			}
			return GetElementIcon(modifier.Card.InfuseElements[0]);
		}
		if (modifier.Card.NegativeConditions.Count > 0)
		{
			return GetIconNegativeCondition(modifier.Card.NegativeConditions[0]);
		}
		if (modifier.AddTarget)
		{
			return Target;
		}
		if (modifier.IsBless)
		{
			return Blessed.Icon;
		}
		if (modifier.IsCurse)
		{
			return Cursed.Icon;
		}
		if (modifier.Card.Pierce)
		{
			return Pierce;
		}
		if (modifier.Card.Damage)
		{
			return Damage;
		}
		if (modifier.Card.Heal)
		{
			return Heal;
		}
		if (modifier.Card.HealAlly)
		{
			if (modifier.MathModifier == "+0")
			{
				return HealAlly0;
			}
			return HealAlly1;
		}
		if (modifier.Card.Pull)
		{
			return Pull;
		}
		if (modifier.Card.Push)
		{
			if (modifier.Card.Push2)
			{
				return Push2;
			}
			return Push;
		}
		if (modifier.Card.Shield)
		{
			return Shield.Icon;
		}
		if (modifier.Card.PositiveConditions.Count > 0)
		{
			return GetIconPositiveCondition(modifier.Card.PositiveConditions[0]);
		}
		string text = ((useOriginal && modifier.OriginalModifier.IsNOTNullOrEmpty()) ? modifier.OriginalModifier : modifier.MathModifier);
		if (text == "*0" || text == "x0")
		{
			return NullModifierIcon;
		}
		return null;
	}

	public string GetAttackModifierText(AttackModifierYMLData modifier, bool useOriginal = false)
	{
		AttackModifierConfig attackModifierConfig = customModifierConfigs.FirstOrDefault((AttackModifierConfig it) => it.name == modifier.Name);
		string text = ((useOriginal && modifier.OriginalModifier.IsNOTNullOrEmpty()) ? modifier.OriginalModifier : modifier.MathModifier);
		if (attackModifierConfig != null)
		{
			if (!attackModifierConfig.showMathModifier)
			{
				return string.Empty;
			}
			return text;
		}
		if (modifier.Card.Damage)
		{
			return string.Empty;
		}
		if (modifier.Card.HealAlly)
		{
			return string.Empty;
		}
		if (modifier.Card.Heal)
		{
			return $"+{modifier.Card.HealAmount}";
		}
		if (modifier.AddTarget && text == "+0")
		{
			return "+";
		}
		if (modifier.Rolling && text == "+0")
		{
			return string.Empty;
		}
		if (text == "*0")
		{
			return "x0";
		}
		if (text == "*2")
		{
			return "x2";
		}
		return text;
	}

	public Color GetAttackModifierColor(string modifierMath)
	{
		switch (modifierMath)
		{
		case "x2":
		case "*2":
			return highModifierColor;
		case "x0":
		case "*0":
		case "-2":
		case "-1":
			return lowModifierColor;
		case "+1":
		case "+2":
		case "+3":
		case "+4":
		case "+0":
		case "-0":
			return regularModifierColor;
		default:
			return regularModifierColor;
		}
	}

	public CharacterConfigUI GetCharacterConfigUIFromString(string character, bool useDefault = true)
	{
		ECharacter eCharacter = CCharacterClass.CharacterNames.SingleOrDefault((ECharacter s) => s.ToString() == character);
		if (eCharacter != ECharacter.None)
		{
			return GetCharacterConfigUI(eCharacter, useDefault);
		}
		return null;
	}

	public CharacterConfigUI GetCharacterConfigUI(ECharacter character, bool useDefault = true, string customConfigID = null)
	{
		return GetCharacterConfigUI(character.ToString(), useDefault, customConfigID);
	}

	public Color GetCharacterColor(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character.ToString(), useDefault: true, custom).questConfig.color;
	}

	public Color GetCharacterColor(CPlayerActor character)
	{
		return GetCharacterColor(character.CharacterClass.CharacterModel, character.CharacterClass.CharacterYML.CustomCharacterConfig);
	}

	public string GetCharacterHexColor(CPlayerActor character)
	{
		return GetCharacterColor(character).ToHex();
	}

	public string GetCharacterHexColor(ECharacter character)
	{
		return GetCharacterColor(character).ToHex();
	}

	public Sprite GetCharacterMarker(CharacterYMLData characterYmlData)
	{
		return GetCharacterMarker(characterYmlData.Model, characterYmlData.CustomCharacterConfig);
	}

	public Sprite GetCharacterMarker(ECharacter character, string custom = null)
	{
		return GetCharacterMarker(character.ToString(), custom);
	}

	public Sprite GetCharacterMarker(string character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom).questConfig.marker;
	}

	public CharacterConfigUI GetCharacterConfigUI(string character, bool useDefault = true, string customConfigID = null)
	{
		if (customConfigID == null)
		{
			string charName = Regex.Replace(character, "ID$", "");
			CharacterConfigUI characterConfigUI = characterConfigsUI.SingleOrDefault((CharacterConfigUI x) => charName == x.character.ToString());
			if (!characterConfigUI)
			{
				if (!useDefault || characterConfigsUI.Count <= 0)
				{
					return null;
				}
				return characterConfigsUI[0];
			}
			return characterConfigUI;
		}
		object obj = SceneController.Instance.YML.ModdedCharacterConfigs?.SingleOrDefault((CharacterConfigUI s) => customConfigID == s.ID);
		if (obj == null)
		{
			if (!useDefault || characterConfigsUI.Count <= 0)
			{
				return null;
			}
			obj = characterConfigsUI[0];
		}
		return (CharacterConfigUI)obj;
	}

	public ReferenceToSprite GetCharacterSpriteRef(ECharacter character, bool highlight = false, string custom = null)
	{
		CharacterConfigUI characterConfigUI = GetCharacterConfigUI(character, useDefault: false, custom);
		if (characterConfigUI != null)
		{
			if (!highlight)
			{
				return characterConfigUI.IconClass;
			}
			return characterConfigUI.IconClassHighlight;
		}
		return CharacterIconsAds.GetReferenceToSprite(character.ToString());
	}

	public ReferenceToSprite GetActorPortraitRef(string actorModel, string customPortrait = null)
	{
		if (customPortrait != null)
		{
			ReferenceToSprite referenceToSprite = new ReferenceToSprite();
			referenceToSprite.SetSpriteInsteadAddressable(SceneController.Instance.YML?.ModdedMonsterPortraits?.ToArray().GetSprite(customPortrait));
			return referenceToSprite;
		}
		return actorPortraitsAds.GetReferenceToSprite(actorModel);
	}

	public Sprite GetCharacterDistributionPortrait(ECharacter character, string custom = null)
	{
		return GetCharacterConfigUI(character, useDefault: true, custom)?.distributionPortrait;
	}

	public Sprite GetActiveAbilityDurationIcon(CActiveBonus.EActiveBonusDurationType duration)
	{
		return activeAbilityDurationIcons.GetSprite(duration.ToString());
	}

	public Sprite GetDurationIcon(CActiveBonus.EActiveBonusDurationType duration)
	{
		return durationIcons.GetSprite(duration.ToString());
	}

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
		m_CharacterOutlineColourDictionary = new Dictionary<CActor.EType, Color>();
		foreach (CharacterOutlinableColourSetting characterOutlinableColourSetting in m_CharacterOutlinableColourSettings)
		{
			m_CharacterOutlineColourDictionary.Add(characterOutlinableColourSetting.ActorType, characterOutlinableColourSetting.OutlineColor);
		}
		m_PropOutlineColourDictionary = new Dictionary<ScenarioManager.ObjectImportType, Color>();
		foreach (PropOutlinableColourSetting propOutlinableColourSetting in m_PropOutlinableColourSettings)
		{
			m_PropOutlineColourDictionary.Add(propOutlinableColourSetting.PropType, propOutlinableColourSetting.OutlineColor);
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		for (int i = 0; i < tmpAbilityCardSpriteAssets.Count; i++)
		{
			AbilityCardSpriteAsset.fallbackSpriteAssets.Remove(tmpAbilityCardSpriteAssets[i]);
		}
		Instance = null;
	}

	public static Color GetCharacterOutlineColor(CActor.EType actorType)
	{
		Instance.m_CharacterOutlineColourDictionary.TryGetValue(actorType, out var value);
		return value;
	}

	public static Color GetPropOutlineColor(ScenarioManager.ObjectImportType objectImportType)
	{
		if (Instance.m_PropOutlineColourDictionary.TryGetValue(objectImportType, out var value))
		{
			return value;
		}
		Instance.m_PropOutlineColourDictionary.TryGetValue(ScenarioManager.ObjectImportType.None, out value);
		return value;
	}

	public Color ReturnHealthColour(int maxHealth, int currentHealth)
	{
		if ((float)currentHealth <= (float)maxHealth * 0.25f)
		{
			return Red;
		}
		if (currentHealth < maxHealth)
		{
			return Orange;
		}
		return Green;
	}

	public int CalculateModifierTotal(int baseValue, ModifierDisplayStruct mod, bool noNegativeResults = false)
	{
		int num = baseValue + mod.Value;
		int num2 = baseValue * mod.Value;
		if (noNegativeResults)
		{
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
		}
		return mod.MathType switch
		{
			ModifierDisplayStruct.MathOperation.Add => num, 
			ModifierDisplayStruct.MathOperation.Subtract => num, 
			ModifierDisplayStruct.MathOperation.Multiply => num2, 
			_ => num, 
		};
	}

	public string ConstructModifierValueForDisplay(ModifierDisplayStruct mod)
	{
		return mod.MathType switch
		{
			ModifierDisplayStruct.MathOperation.Add => "+" + mod.Value, 
			ModifierDisplayStruct.MathOperation.Subtract => mod.Value.ToString(), 
			ModifierDisplayStruct.MathOperation.Multiply => "X" + mod.Value, 
			_ => "+" + mod.Value, 
		};
	}

	public BackgroundColour ReturnModifierColour(ModifierDisplayStruct mod, bool advantage = false)
	{
		if (mod.ModifierType == ModifierDisplayStruct.ModType.Critical)
		{
			return BackgroundColour.Blue;
		}
		switch (mod.BenfitType)
		{
		case ModifierDisplayStruct.Benefit.Positive:
			if (!advantage)
			{
				return BackgroundColour.Blue;
			}
			return BackgroundColour.AdvantageBlue;
		case ModifierDisplayStruct.Benefit.Negative:
			if (!advantage)
			{
				return BackgroundColour.Red;
			}
			return BackgroundColour.DisadvantageRed;
		default:
			return BackgroundColour.None;
		}
	}

	public Color ReturnTooltipColour(TooltipColours col)
	{
		return col switch
		{
			TooltipColours.Orange => Orange, 
			TooltipColours.White => White, 
			TooltipColours.Red => Red, 
			TooltipColours.Purple => Purple, 
			_ => White, 
		};
	}

	public AbilityCardUISkin GetCardSkin(string characterName, string custom = null)
	{
		return GetCharacterConfigUI(characterName, useDefault: true, custom).cardSkin;
	}

	public Color GetDifficultyColor(EAdventureDifficulty eDifficulty)
	{
		switch (eDifficulty)
		{
		case EAdventureDifficulty.Easy:
		case EAdventureDifficulty.Normal:
		case EAdventureDifficulty.Friendly:
			return greenDifficultyColor;
		case EAdventureDifficulty.Hard:
			return yellowDifficultyColor;
		case EAdventureDifficulty.Insane:
		case EAdventureDifficulty.Deadly:
			return redDifficultyColor;
		default:
			return greenDifficultyColor;
		}
	}

	public ItemConfigUI GetItemConfig(string name)
	{
		if (tempItemConfigsDict == null)
		{
			tempItemConfigsDict = itemConfigs.Where((ItemConfigUI it) => it != null).ToDictionary((ItemConfigUI it) => it.itemName.ToLower(), (ItemConfigUI it) => it);
		}
		if (tempItemConfigsDict.ContainsKey(name.ToLower()))
		{
			return tempItemConfigsDict[name.ToLower()];
		}
		List<ItemConfigUI> moddedItemConfigs = SceneController.Instance.YML.ModdedItemConfigs;
		if (!moddedItemConfigs.IsNullOrEmpty())
		{
			ItemConfigUI itemConfigUI = moddedItemConfigs.FirstOrDefault((ItemConfigUI it) => it.itemName.ToLower() == name.ToLower());
			if ((bool)itemConfigUI)
			{
				return itemConfigUI;
			}
		}
		ItemConfigUI itemConfigUI2 = itemConfigs.FirstOrDefault((ItemConfigUI it) => it.itemName.ToLower() == name.ToLower());
		if ((bool)itemConfigUI2)
		{
			tempItemConfigsDict[itemConfigUI2.itemName.ToLower()] = itemConfigUI2;
			return itemConfigUI2;
		}
		Debug.LogErrorFormat("Not found item configuration for {0}", name);
		return Enumerable.First(tempItemConfigsDict).Value;
	}

	public void AddItemConfig(ItemConfigUI config)
	{
		itemConfigs.Add(config);
		if (tempItemConfigsDict != null)
		{
			tempItemConfigsDict[config.itemName.ToLower()] = config;
		}
	}

	public ReferenceToSprite GetItemBackgroundSprite(string name)
	{
		return GetItemConfig(name).BackgroundImage;
	}

	public Sprite GetItemSlotIcon(string slotName)
	{
		return itemSlotIcons.GetSprite(slotName);
	}

	public Sprite GetEnhancementIcon(EEnhancement enhancement)
	{
		EnhancementConfigUI enhancementConfigUI = enhancementConfigs.FirstOrDefault((EnhancementConfigUI it) => it.enhancement == enhancement);
		if (enhancementConfigUI == null)
		{
			Debug.LogErrorFormat("Not found enhancement configuration ui for {0}", enhancement);
			return enhancementConfigs[0].icon;
		}
		return enhancementConfigUI.icon;
	}

	public string GetAbilityPreviewEffectIconName(CAbility.EAbilityType abilityType)
	{
		return GetAbilityPreviewConfig(abilityType)?.iconName;
	}

	public AbilityTypePreviewConfigUI GetAbilityPreviewConfig(CAbility.EAbilityType abilityType)
	{
		if (tempAbilityTypePreviewConfigsDict == null)
		{
			tempAbilityTypePreviewConfigsDict = abilityTypePreviewConfigs.ToDictionary((AbilityTypePreviewConfigUI it) => it.type, (AbilityTypePreviewConfigUI it) => it);
		}
		if (tempAbilityTypePreviewConfigsDict.ContainsKey(abilityType))
		{
			return tempAbilityTypePreviewConfigsDict[abilityType];
		}
		AbilityTypePreviewConfigUI abilityTypePreviewConfigUI = abilityTypePreviewConfigs.FirstOrDefault((AbilityTypePreviewConfigUI it) => it.type == abilityType);
		if (abilityTypePreviewConfigUI != null)
		{
			tempAbilityTypePreviewConfigsDict[abilityTypePreviewConfigUI.type] = abilityTypePreviewConfigUI;
			return abilityTypePreviewConfigUI;
		}
		return null;
	}

	public PreviewEffectInfo GetPreviewEffectConfig(string id)
	{
		if (id.IsNullOrEmpty())
		{
			return null;
		}
		if (tempPreviewEffectConfigsDict == null)
		{
			tempPreviewEffectConfigsDict = previewEffectConfigs.ToDictionary((PreviewEffectConfigUI it) => it.id, (PreviewEffectConfigUI it) => it);
		}
		if (tempPreviewEffectConfigsDict.ContainsKey(id))
		{
			return tempPreviewEffectConfigsDict[id].previewData;
		}
		PreviewEffectConfigUI previewEffectConfigUI = previewEffectConfigs.FirstOrDefault((PreviewEffectConfigUI it) => it.id == id);
		if (previewEffectConfigUI != null)
		{
			tempPreviewEffectConfigsDict[previewEffectConfigUI.id] = previewEffectConfigUI;
			return previewEffectConfigUI.previewData;
		}
		return null;
	}

	public StoryCharacterConfigUI GetStoryCharacter(string characterGUID)
	{
		StoryCharacterConfigUI storyCharacterConfigUI = StoryCharacters.FirstOrDefault((StoryCharacterConfigUI c) => c.CharacterGUID == characterGUID);
		if (storyCharacterConfigUI == null)
		{
			Debug.LogWarning("Story Character config Warning - Unable to find Character with GUID:" + characterGUID);
		}
		return storyCharacterConfigUI;
	}

	public string GetStoryCharacterDisplayNameKey(string characterGUID)
	{
		return GetStoryCharacter(characterGUID)?.CharacterDisplayNameKey;
	}

	public Sprite GetStoryCharacterExpression(string characterGUID, EExpression expression)
	{
		return GetStoryCharacter(characterGUID)?.GetExpression(expression);
	}

	public Sprite GetStoryCharacterShield(string characterGUID)
	{
		return GetStoryCharacter(characterGUID)?.shieldIcon;
	}

	public Sprite GetQuestAreaMarker(EQuestAreaType locationArea, EQuestIconType iconType)
	{
		return questAreaConfigs.FirstOrDefault((QuestAreaConfigUI it) => it.locationArea == locationArea)?.GetConfig(iconType, completed: false)?.marker;
	}

	public Color GetQuestAreaColor(EQuestAreaType area, EQuestIconType iconType)
	{
		if (area != EQuestAreaType.None)
		{
			QuestAreaConfigUI questAreaConfigUI = questAreaConfigs.FirstOrDefault((QuestAreaConfigUI it) => it.locationArea == area);
			if (questAreaConfigUI != null)
			{
				return questAreaConfigUI.GetConfig(iconType, completed: false)?.color ?? questAreaConfigUI.GetConfig(EQuestIconType.None, completed: false).color;
			}
		}
		return White;
	}

	public QuestTypeConfigUI GetQuestTypeConfig(CQuest quest)
	{
		if (quest.LocationArea != EQuestAreaType.None)
		{
			QuestTypeConfigUI questTypeConfigUI = questAreaConfigs.FirstOrDefault((QuestAreaConfigUI it) => it.locationArea == quest.LocationArea)?.GetConfig(quest.IconType, completed: false);
			if (questTypeConfigUI != null)
			{
				return questTypeConfigUI;
			}
		}
		string customConfigID = ((quest.QuestCharacterRequirements.Count > 0) ? CharacterClassManager.Classes.SingleOrDefault((CCharacterClass s) => s.ID == quest.QuestCharacterRequirements[0].RequiredCharacterID).CharacterYML.CustomCharacterConfig : null);
		switch (quest.IconType)
		{
		case EQuestIconType.Boss:
			return bossQuest;
		case EQuestIconType.RequiredCharacter:
		{
			ECharacter eCharacter = ((quest.CharacterIcon != ECharacter.None) ? quest.CharacterIcon : ((quest.QuestCharacterRequirements != null && quest.QuestCharacterRequirements.Count > 0) ? CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == quest.QuestCharacterRequirements[0].RequiredCharacterID).CharacterModel : ECharacter.None));
			if (eCharacter != ECharacter.None)
			{
				return GetCharacterConfigUI(eCharacter, useDefault: true, customConfigID).questConfig;
			}
			Debug.LogError("No CharacterIcon or RequiredCharacter found for Quest with IconType: RequiredCharacter: " + quest.ID);
			return null;
		}
		default:
			switch (quest.Type)
			{
			case EQuestType.Travel:
				return travelQuest;
			case EQuestType.Job:
				return jobQuest;
			case EQuestType.Relic:
				return relicQuest;
			case EQuestType.Story:
			case EQuestType.City:
			case EQuestType.CityAdjacent:
				if (quest.CharacterIcon != ECharacter.None)
				{
					return GetCharacterConfigUI(quest.CharacterIcon, useDefault: true, customConfigID)?.questConfig;
				}
				return jobQuest;
			default:
				Debug.LogError("No QuestTypeConfigUI found for QuestType: " + quest.Type);
				return null;
			}
		}
	}

	public Sprite GetQuestGroupIcon(Enum typeObj)
	{
		if (typeObj is GuildmasterQuestLogService.EGuildmasterLogGroup eGuildmasterLogGroup)
		{
			switch (eGuildmasterLogGroup)
			{
			case GuildmasterQuestLogService.EGuildmasterLogGroup.Travel:
				return travelQuest.marker;
			case GuildmasterQuestLogService.EGuildmasterLogGroup.Job:
				return jobQuest.marker;
			case GuildmasterQuestLogService.EGuildmasterLogGroup.Relic:
				return relicQuest.marker;
			case GuildmasterQuestLogService.EGuildmasterLogGroup.Story:
				return storyQuest.marker;
			case GuildmasterQuestLogService.EGuildmasterLogGroup.Completed:
				return casualModeQuest.marker;
			default:
				Debug.LogError("No QuestTypeConfigUI found for QuestType: " + eGuildmasterLogGroup);
				return null;
			}
		}
		if (typeObj is CampaignQuestLogService.ECampaignLogGroup eCampaignLogGroup)
		{
			switch (eCampaignLogGroup)
			{
			case CampaignQuestLogService.ECampaignLogGroup.Completed:
				return casualModeQuest.marker;
			case CampaignQuestLogService.ECampaignLogGroup.City:
				return GetGuildmasterModeSprite(EGuildmasterMode.City);
			case CampaignQuestLogService.ECampaignLogGroup.World:
				return GetGuildmasterModeSprite(EGuildmasterMode.WorldMap);
			default:
				Debug.LogError("No QuestTypeConfigUI found for QuestType: " + eCampaignLogGroup);
				return null;
			}
		}
		return null;
	}

	public Sprite GetQuestMarkerHighlightSprite()
	{
		return highlightQuestMarker;
	}

	public Sprite GetQuestMarkerHighlightSprite(CQuestState questState)
	{
		if (questState.Quest.IconType != EQuestIconType.RequiredCharacter || !questState.IsSoloScenario)
		{
			return highlightQuestMarker;
		}
		return highlightSoloQuestMarker;
	}

	public Sprite GetCityEncounterMarkerSprite()
	{
		return storyQuest.marker;
	}

	public Sprite GetQuestMarkerSprite(CQuestState questState)
	{
		if (questState.QuestState >= CQuestState.EQuestState.Completed && questState.QuestState != CQuestState.EQuestState.Blocked)
		{
			if (questState.Quest.IconType == EQuestIconType.RequiredCharacter && questState.IsSoloScenario && questState.CountTreasures() > 0 && !questState.Quest.HideTreasureWhenCompleted)
			{
				ECharacter character = ((questState.Quest.CharacterIcon != ECharacter.None) ? questState.Quest.CharacterIcon : ((questState.Quest.QuestCharacterRequirements != null && questState.Quest.QuestCharacterRequirements.Count > 0) ? CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == questState.Quest.QuestCharacterRequirements[0].RequiredCharacterID).CharacterModel : ECharacter.None));
				return GetCharacterConfigUI(character).soloCompletedMarker;
			}
			QuestAreaConfigUI questAreaConfigUI = ((questState.Quest.LocationArea == EQuestAreaType.None) ? null : questAreaConfigs.FirstOrDefault((QuestAreaConfigUI it) => it.locationArea == questState.Quest.LocationArea));
			if (questAreaConfigUI != null && questState.ScenarioState.NonSerializedInitialState != null && questState.CountTreasures() > 0 && !questState.Quest.HideTreasureWhenCompleted)
			{
				return questAreaConfigUI.GetConfig(questState.Quest.IconType, completed: true)?.marker;
			}
			return casualModeQuest.marker;
		}
		if (questState.Quest.IconType == EQuestIconType.RequiredCharacter && questState.IsSoloScenario)
		{
			ECharacter character2 = ((questState.Quest.CharacterIcon != ECharacter.None) ? questState.Quest.CharacterIcon : ((questState.Quest.QuestCharacterRequirements != null && questState.Quest.QuestCharacterRequirements.Count > 0) ? CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == questState.Quest.QuestCharacterRequirements[0].RequiredCharacterID).CharacterModel : ECharacter.None));
			return GetCharacterConfigUI(character2).soloMarker;
		}
		return GetQuestTypeConfig(questState.Quest).marker;
	}

	public NarrativeConfigUI GetNarrativeImageConfig(string id)
	{
		NarrativeConfigUI narrativeConfigUI = narrativeImages.FirstOrDefault((NarrativeConfigUI it) => it.id.Equals(id, StringComparison.OrdinalIgnoreCase));
		if (narrativeConfigUI == null)
		{
			Debug.LogError("No image found for id:" + id);
			return narrativeImages[0];
		}
		return narrativeConfigUI;
	}

	public bool? GetRewardVisibility(Reward reward)
	{
		if (tempRewardVisibilityDict == null)
		{
			tempRewardVisibilityDict = rewardsVisiblity.ToDictionary((RewardVisibilityConfigUI it) => it.type.ToString() + it.unlockId, (RewardVisibilityConfigUI it) => it);
		}
		if (reward.UnlockName.IsNOTNullOrEmpty() && tempRewardVisibilityDict.ContainsKey(reward.Type.ToString() + reward.UnlockName))
		{
			return tempRewardVisibilityDict[reward.Type.ToString() + reward.UnlockName].isVisible;
		}
		if (tempRewardVisibilityDict.ContainsKey(reward.Type.ToString()))
		{
			return tempRewardVisibilityDict[reward.Type.ToString()].isVisible;
		}
		return null;
	}

	public ReferenceToSprite GetRewardIcon(Reward reward)
	{
		if (reward.Type == ETreasureType.UnlockCharacter)
		{
			string defaultModel = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).DefaultModel;
			string customCharacterConfig = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).CharacterYML.CustomCharacterConfig;
			CharacterConfigUI characterConfigUI = GetCharacterConfigUI(defaultModel, useDefault: true, customCharacterConfig);
			if (characterConfigUI == null)
			{
				return null;
			}
			return characterConfigUI.GetQuestRewardIcon(!AdventureState.MapState.IsCampaign);
		}
		if (reward.Type == ETreasureType.Item || reward.Type == ETreasureType.ItemStock || reward.Type == ETreasureType.LoseItem || reward.Type == ETreasureType.UnlockProsperityItem || reward.Type == ETreasureType.UnlockProsperityItemStock || reward.Type == ETreasureType.LoseItem)
		{
			return new ReferenceToSprite(GetItemConfig(reward.Item.YMLData.Art)?.miniIcon);
		}
		if (reward.Type == ETreasureType.Enhancement)
		{
			return new ReferenceToSprite(GetEnhancementIcon(reward.Enhancement));
		}
		if (reward.Type == ETreasureType.UnlockQuest)
		{
			CQuestState cQuestState = AdventureState.MapState.AllLockedQuests.FirstOrDefault((CQuestState it) => it.ID == reward.UnlockName);
			if (cQuestState != null)
			{
				return new ReferenceToSprite(GetQuestTypeConfig(cQuestState.Quest).GetRewardIcon(!AdventureState.MapState.IsCampaign));
			}
		}
		if (reward.Type == ETreasureType.Infuse)
		{
			return new ReferenceToSprite(GetRewardElementIcon(reward.Infusions.SingleOrDefault()));
		}
		if (reward.Type == ETreasureType.ConsumeItem)
		{
			return new ReferenceToSprite((AdventureState.MapState.IsCampaign ? campaingQuestRewards : guildmasterQuestRewards).FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && it.extraId == reward.ConsumeSlot && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon);
		}
		if (reward.Type == ETreasureType.Condition)
		{
			RewardCondition condition = reward.Conditions.SingleOrDefault();
			if (condition.PositiveCondition != CCondition.EPositiveCondition.NA)
			{
				return new ReferenceToSprite((AdventureState.MapState.IsCampaign ? campaingQuestRewards : guildmasterQuestRewards).FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && it.extraId == condition.PositiveCondition.ToString() && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon ?? GetIconPositiveCondition(condition.PositiveCondition));
			}
			return new ReferenceToSprite((AdventureState.MapState.IsCampaign ? campaingQuestRewards : guildmasterQuestRewards).FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && it.extraId == condition.NegativeCondition.ToString() && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon ?? GetIconNegativeCondition(condition.NegativeCondition));
		}
		return new ReferenceToSprite((AdventureState.MapState.IsCampaign ? campaingQuestRewards : guildmasterQuestRewards).FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.icon);
	}

	public Color GetRewardColor(Reward reward)
	{
		if (reward.Type == ETreasureType.UnlockCharacter)
		{
			if (AdventureState.MapState.IsCampaign)
			{
				return Color.white;
			}
			string defaultModel = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).DefaultModel;
			string customCharacterConfig = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == reward.CharacterID).CharacterYML.CustomCharacterConfig;
			CharacterConfigUI characterConfigUI = GetCharacterConfigUI(defaultModel, useDefault: true, customCharacterConfig);
			if (!(characterConfigUI == null))
			{
				return characterConfigUI.questConfig.color;
			}
			return Color.white;
		}
		if (reward.Type == ETreasureType.UnlockQuest)
		{
			CQuestState cQuestState = AdventureState.MapState.AllLockedQuests.FirstOrDefault((CQuestState it) => it.ID == reward.UnlockName);
			if (cQuestState != null)
			{
				return GetQuestTypeConfig(cQuestState.Quest).color;
			}
		}
		return (AdventureState.MapState.IsCampaign ? campaingQuestRewards : guildmasterQuestRewards).FirstOrDefault((RewardConfigUI it) => it.type == reward.Type && (it.distribution == ETreasureDistributionType.None || it.distribution == reward.TreasureDistributionType))?.color ?? Color.white;
	}

	public Sprite GetItemMiniSprite(string itemName)
	{
		return GetItemConfig(itemName).miniIcon;
	}

	public Color GetItemColor(CItem.EItemRarity itemRarity)
	{
		return itemRarityConfigs.First((ItemRarityConfigUI it) => it.rarity == itemRarity).itemColor;
	}

	public Vector3 GetLocationMarkerOffset(string locationId, Vector3 defaultOffset)
	{
		return GetLocationConfig(locationId)?.locationMarkerOffset ?? defaultOffset;
	}

	public LocationConfigUI GetLocationConfig(string locationId)
	{
		return locationsConfig.FirstOrDefault((LocationConfigUI it) => it.location == locationId);
	}

	public Sprite GetAchievementIcon(EAchievementType type, int level, bool completed)
	{
		AchievementIconSetting achievementIconSetting = achievementLevelIcons.FirstOrDefault((AchievementIconSetting it) => it.type == type);
		if (achievementIconSetting == null)
		{
			Debug.LogErrorFormat("Not found achievement icon for {0}", type);
			achievementIconSetting = achievementLevelIcons[0];
		}
		if (achievementIconSetting.levels.Length < level)
		{
			Debug.LogErrorFormat("Not found achievement icon for {0} level {1}", type, level);
			if (!completed)
			{
				return achievementIconSetting.levels[0].icon;
			}
			return achievementIconSetting.levels[0].iconCompleted;
		}
		if (!completed)
		{
			return achievementIconSetting.levels[level].icon;
		}
		return achievementIconSetting.levels[level].iconCompleted;
	}

	public Sprite GetGuildmasterModeSprite(EGuildmasterMode mode)
	{
		GuildmasterModeSetting guildmasterModeSetting = guildmasterIcons.FirstOrDefault((GuildmasterModeSetting it) => it.mode == mode);
		if (guildmasterModeSetting == null)
		{
			Debug.LogErrorFormat("Not found icon for {0}", mode);
			return guildmasterIcons[0].icon;
		}
		return guildmasterModeSetting.icon;
	}

	public void AddFallbackAbilityCardSpriteAsset(TMP_SpriteAsset spriteAsset)
	{
		tmpAbilityCardSpriteAssets.Add(spriteAsset);
		AbilityCardSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
	}

	public static Color GetPlaceholderPlayerColor(NetworkPlayer player)
	{
		if (player.Avatar == null && player.PlayerID > 0 && player.PlayerID <= defaultPlayerColors.Length)
		{
			return defaultPlayerColors[player.PlayerID - 1];
		}
		return Color.white;
	}

	public CharacterConfigUI.ECharacterGender GetCharacterGender(ECharacter character)
	{
		return GetCharacterConfigUI(character).gender;
	}

	public IntroductionConceptConfigUI GetIntroductionConfig(EIntroductionConcept concept)
	{
		IntroductionConceptConfigUI introductionConceptConfigUI = introductionConfigs.FirstOrDefault((IntroductionConceptConfigUI it) => it.Concept == concept);
		if (introductionConceptConfigUI == null)
		{
			Debug.LogErrorFormat("Not found introduction config for concept {0}", concept);
		}
		return introductionConceptConfigUI;
	}

	public MapFTUEStepConfigUI GetMapFTUEConfig(EMapFTUEStep phase)
	{
		MapFTUEStepConfigUI mapFTUEStepConfigUI = campaignFTUEConfigs.FirstOrDefault((MapFTUEStepConfigUI it) => it.Phase == phase);
		if (mapFTUEStepConfigUI == null)
		{
			Debug.LogErrorFormat("Not found ftue config for phase {0}", phase);
		}
		return mapFTUEStepConfigUI;
	}

	public DLCConfig GetDLCConfig(DLCRegistry.EDLCKey dlc)
	{
		return dlcConfigs.FirstOrDefault((DLCConfig it) => it.DLCKey == dlc);
	}

	public Sprite GetMissingCharacterDLC(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.MissingCharacterIcon;
	}

	public Sprite[] GetDLCPromotionSprites(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.PromotionalImages;
	}

	public DLCConfig.EDLCState GetDLCState(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.State ?? DLCConfig.EDLCState.Unavailable;
	}

	public bool IsDlcHideForPromotion(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.HideForPromotion ?? false;
	}

	public Sprite GetDLCShieldIcon(DLCRegistry.EDLCKey dlc)
	{
		return GetDLCConfig(dlc)?.ShieldIcon;
	}

	public GamepadDeviceMappingConfig GetDeviceMapping(ControllerType controllerType)
	{
		if (Singleton<InputManager>.Instance.IsPCAndGamepadVersion())
		{
			return devicesMapping.FirstOrDefault((GamepadDeviceMappingConfig it) => it.DeviceTypeCompatible.HasFlag(ControllerType.Generic));
		}
		return devicesMapping.FirstOrDefault((GamepadDeviceMappingConfig it) => it.DeviceTypeCompatible.HasFlag(controllerType));
	}

	public string GetCharacterCreateAnimation(ECharacter character)
	{
		string createAnimation = GetCharacterConfigUI(character).createAnimation;
		if (!createAnimation.IsNullOrEmpty())
		{
			return createAnimation;
		}
		return "Attack";
	}

	public string GetSkinForCharacter(CMapCharacter characterData)
	{
		if (characterData == null)
		{
			return null;
		}
		string result = characterData.SkinId;
		string characterName = characterData.CharacterYMLData.Model.ToString();
		string customCharacterConfig = characterData.CharacterYMLData.CustomCharacterConfig;
		if (!Instance.CanUseAdditionalSkins(characterName, customCharacterConfig))
		{
			result = null;
		}
		return result;
	}
}
