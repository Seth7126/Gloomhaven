using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using Utilities.MemoryManager;

public class GlobalSettings : MonoBehaviour
{
	[Serializable]
	public class DefaultHitEffects
	{
		public GameObject DefaultStandardHitEffect;

		public GameObject DefaultCriticalHitEffect;

		public GameObject DefaultShieldHitEffect;

		public int CriticalHitThreshold;
	}

	[Serializable]
	public class GlobalParticleEffects
	{
		public GameObject DefaultHealEffect;

		public GameObject DefaultDeathDissolve;

		public GameObject DefaultPositiveCondition;

		public GameObject DefaultNegativeCondition;

		public GameObject DefaultCharacterReveal;

		public GameObject DefaultCharacterSwap;
	}

	[Serializable]
	public class JumpMoveParticleEffects
	{
		public GameObject PhaseStart;

		public GameObject PhaseBits;

		public GameObject PhaseTrailBrown;

		public GameObject PhaseTrailPink;

		public GameObject PhaseTrailRed;

		public GameObject PhaseTrailDistort;

		public GameObject PhaseArrive;

		public GameObject InvisibilityIdle;

		public GameObject InvisibilitySmoke;
	}

	[Serializable]
	public class ActiveBonusBuffTargetEffects
	{
		public GameObject AttackBuffTargetEffect;

		public GameObject ShieldActiveBonusTargetEffect;

		public GameObject RetaliateActiveBonusTargetEffect;

		public GameObject GainShield;

		public GameObject GainRetaliate;

		public GameObject GainDisarm;

		public GameObject GainImmobilize;

		public GameObject GainPoison;

		public GameObject GainStun;

		public GameObject GainWound;

		public GameObject GainBless;

		public GameObject GainCurse;

		public GameObject GainSleep;

		public GameObject GainStrengthen;

		public GameObject GainMuddle;

		public GameObject GainInvisibility;

		public GameObject GainAddTarget;

		public GameObject GainAddHeal;

		public GameObject GainAddRange;

		public GameObject GainAttackersGainDisadvantage;

		public GameObject GainAttackActiveBonus;

		public GameObject GainDefault;
	}

	[Serializable]
	public class MagicEffects
	{
		public GameObject RetaliateHit;

		public GameObject RetaliateTarget;

		public GameObject WoundDamage;
	}

	[Serializable]
	public class AbilityCardSettings
	{
		[Header("Font")]
		public TMP_FontAsset CardFont;

		public float PreviewFontSize;

		public float StandardFontSize;

		public float SmallFontSize;

		public float SmallerFontSize;

		public float MinFontSize;

		public float SummonStatFontSize;

		public float ItemCardFontSize;

		[Header("Spacing")]
		public float RowSpacing;

		public float ColumnSpacing;

		[Header("Hex Size")]
		public float AreaHexWidth;

		[Header("Enhancement Icons")]
		public float EnhancementIconSize;

		public float EnhancementIconSpacing;

		public float EnhancementIconInitialSpace;

		[Header("Element Icons")]
		public float ElementIconSize;

		[Header("XP Icons")]
		public float XPIconSize;

		public float XPArrowSize;

		[Header("Misc Icons")]
		public float DurationIconSize;
	}

	[Serializable]
	public class ApparanceProps
	{
		public GameObject Trap;

		public GameObject GoldPile;

		public GameObject Chest;

		public GameObject GoalChest;

		public GameObject OneHexObstacles;

		public GameObject TwoHexObstacles;

		public GameObject ThreeHexObstacles;

		public GameObject ThreeHexCurvedObstacles;

		public GameObject ThreeHexStraightObstacles;

		public GameObject Spawner;

		public GameObject PressurePlate;

		public GameObject TerrainHotCoals;

		public GameObject TerrainWater;

		public GameObject TerrainRubble;

		public GameObject TerrainThorns;

		public GameObject DarkPitObstacle;

		public GameObject Portal;

		public GameObject TerrainVisualEffect;

		public GameObject MonsterGrave;
	}

	[Serializable]
	public class SpecificProps
	{
		public GameObject RockSingle;

		public GameObject RockTriple;

		public GameObject PlinthSingle;

		public GameObject BearTrap;

		public GameObject BombTrap;

		public GameObject DamageMine;

		public GameObject PoisonMine;

		public GameObject StunMine;

		public GameObject SewerPipe;

		public GameObject GraveSingle;

		public GameObject GraveDouble;

		public GameObject QuestClaw;

		public GameObject QuestDoll;

		public GameObject TheFavorite;

		public GameObject TheNewFavorite;
	}

	[Serializable]
	public class AttackModifierSettings
	{
		[Tooltip("Consider TimeScale and FadeIn time as 0")]
		public bool InstantSlowMo = true;

		[Tooltip("Time we should wait until we begin the attack modifier flow in the case of not receiving the signal to do so")]
		public float AttackModifierFlowBeginTimeout = 5f;

		[Tooltip("Time we should wait until reveal attack modifier in case ProgressModifier is not firing")]
		public float DefaultAttackModifierTimeout = 5f;

		[Tooltip("Time offset for reveal time. Gives ability to microtune mod revealing during attack")]
		public float AttackRevealTimeOffset;

		[Tooltip("During in sec of one Rolling Modifier")]
		public float RollingModDuration = 2f;

		[Tooltip("SlowMo duration")]
		public float SlowMoDuration = 2f;

		[Tooltip("SlowMo fade-in time. How fast we go to minimum TimeScale value")]
		public float SlowMoFadeInTime = 1f;

		[Tooltip("SlowMo minimal TimeScale. To completely stop action set to 0")]
		public float SlowMoMinimalTimeScale = 0.1f;
	}

	[Serializable]
	public class AdventureLocationMeshes
	{
	}

	[Serializable]
	public class MapLocationEffects
	{
		public GameObject VisitableNodeEffectPrefab;

		public GameObject CurrentBossIndicatorPrefab;

		public EffectAlphaFadeParticles NodeHoverIndicatorPrefab;

		public GameObject NodeSelectIndicatorPrefab;
	}

	[Serializable]
	public class VisualEffectPrefabs
	{
		public GameObject HexSelectControlParticles;

		public GameObject CardSmokeMini;

		public GameObject CardSmoke;
	}

	[Serializable]
	public class AdventureLocationMaterialSettings
	{
		public ReferenceToObject<Material> ReferenceMaterial;

		public bool ShouldOverrideLocationScale;

		[ConditionalField("ShouldOverrideLocationScale", null, true)]
		public Vector3 OverrideLocationScale;

		public Vector3 CityLocationCenterPositionOffset;
	}

	public DefaultHitEffects m_DefaultHitEffects;

	public GlobalParticleEffects m_GlobalParticles;

	public JumpMoveParticleEffects m_JumpMoveParticleEffects;

	public ActiveBonusBuffTargetEffects m_ActiveBonusBuffTargetEffects;

	public MagicEffects m_MagicEffects;

	public GameObject m_WaypointHolder;

	public GameObject m_GenericHexStar;

	public AbilityCardSettings m_AbilityCardSettingsPC;

	public AbilityCardSettings m_AbilityCardSettingsGamepad;

	public AbilityCardSettings m_GlossaryCardSettings;

	public ApparanceProps m_ApparanceProps;

	public SpecificProps m_SpecificProps;

	public AttackModifierSettings m_AttackModifierSettings;

	public AdventureLocationMeshes AdventureMapLocationMeshes;

	public MapLocationEffects MapLocationEffectsPrefabs;

	public GameObject AdventureLocationDecalPrefab;

	public List<AdventureLocationMaterialSettings> m_AdventureLocationMaterialSettings;

	public int LoadingTipsCount;

	public int LoadingTipsConsoleCount;

	public GameObject m_WaypointPrefab;

	[AudioPlaylistName]
	public string m_MainMenuMusicPlaylist;

	public ApparanceEnumFilterSettings ApparanceEnumFilter;

	public VisualEffectPrefabs VisualEffects;

	private static GlobalSettings s_instance;

	private static AbilityCardSettings s_defaultAbilityCardSettings;

	private static bool s_isGamePadOptionsInUse;

	public AbilityCardSettings m_AbilityCardSettings
	{
		get
		{
			if (InputManager.GamePadInUse)
			{
				return m_AbilityCardSettingsGamepad;
			}
			return m_AbilityCardSettingsPC;
		}
	}

	public static GlobalSettings Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = (Resources.Load("Settings/GlobalSettings", typeof(GameObject)) as GameObject).GetComponent<GlobalSettings>();
			}
			return s_instance;
		}
	}

	public static GameObject GetPropPrefab(string propType)
	{
		GameObject gameObject = GetApparancePropPrefab(propType);
		if (gameObject == null)
		{
			gameObject = GetSpecificPropPrefab(propType);
		}
		return gameObject;
	}

	public static GameObject GetApparancePropPrefab(string propTypeString)
	{
		return GetApparancePropPrefab(CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == propTypeString));
	}

	public static GameObject GetApparancePropPrefab(EPropType propType)
	{
		return propType switch
		{
			EPropType.Chest => Instance.m_ApparanceProps.Chest, 
			EPropType.GoalChest => Instance.m_ApparanceProps.GoalChest, 
			EPropType.GoldPile => Instance.m_ApparanceProps.GoldPile, 
			EPropType.OneHexObstacle => Instance.m_ApparanceProps.OneHexObstacles, 
			EPropType.TwoHexObstacle => Instance.m_ApparanceProps.TwoHexObstacles, 
			EPropType.ThreeHexObstacle => Instance.m_ApparanceProps.ThreeHexObstacles, 
			EPropType.Trap => Instance.m_ApparanceProps.Trap, 
			EPropType.Spawner => Instance.m_ApparanceProps.Spawner, 
			EPropType.PressurePlate => Instance.m_ApparanceProps.PressurePlate, 
			EPropType.TerrainHotCoals => Instance.m_ApparanceProps.TerrainHotCoals, 
			EPropType.TerrainWater => Instance.m_ApparanceProps.TerrainWater, 
			EPropType.TerrainRubble => Instance.m_ApparanceProps.TerrainRubble, 
			EPropType.TerrainThorns => Instance.m_ApparanceProps.TerrainThorns, 
			EPropType.DarkPitObstacle => Instance.m_ApparanceProps.DarkPitObstacle, 
			EPropType.Portal => Instance.m_ApparanceProps.Portal, 
			EPropType.TerrainVisualEffect => Instance.m_ApparanceProps.TerrainVisualEffect, 
			EPropType.ThreeHexCurvedObstacle => Instance.m_ApparanceProps.ThreeHexCurvedObstacles, 
			EPropType.ThreeHexStraightObstacle => Instance.m_ApparanceProps.ThreeHexStraightObstacles, 
			EPropType.MonsterGrave => Instance.m_ApparanceProps.MonsterGrave, 
			_ => null, 
		};
	}

	public static ScenarioManager.ObjectImportType GetObjectImportType(string propTypeString)
	{
		EPropType ePropType = CObjectProp.PropTypes.SingleOrDefault((EPropType x) => x.ToString() == propTypeString);
		if (ePropType != EPropType.None)
		{
			return GetApparancePropType(ePropType);
		}
		ESpecificPropType eSpecificPropType = CObjectProp.SpecificPropTypes.SingleOrDefault((ESpecificPropType x) => x.ToString() == propTypeString);
		if (eSpecificPropType != ESpecificPropType.None)
		{
			return GetSpecificPropType(eSpecificPropType);
		}
		return ScenarioManager.ObjectImportType.None;
	}

	public static ScenarioManager.ObjectImportType GetApparancePropType(EPropType apparancePropType)
	{
		switch (apparancePropType)
		{
		case EPropType.Chest:
			return ScenarioManager.ObjectImportType.Chest;
		case EPropType.GoalChest:
			return ScenarioManager.ObjectImportType.GoalChest;
		case EPropType.GoldPile:
			return ScenarioManager.ObjectImportType.MoneyToken;
		case EPropType.OneHexObstacle:
		case EPropType.TwoHexObstacle:
		case EPropType.ThreeHexObstacle:
		case EPropType.DarkPitObstacle:
		case EPropType.ThreeHexCurvedObstacle:
		case EPropType.ThreeHexStraightObstacle:
			return ScenarioManager.ObjectImportType.Obstacle;
		case EPropType.Trap:
			return ScenarioManager.ObjectImportType.Trap;
		case EPropType.PressurePlate:
			return ScenarioManager.ObjectImportType.PressurePlate;
		case EPropType.TerrainHotCoals:
			return ScenarioManager.ObjectImportType.TerrainHotCoals;
		case EPropType.TerrainWater:
			return ScenarioManager.ObjectImportType.TerrainWater;
		case EPropType.TerrainRubble:
			return ScenarioManager.ObjectImportType.TerrainRubble;
		case EPropType.TerrainThorns:
			return ScenarioManager.ObjectImportType.TerrainThorns;
		case EPropType.Spawner:
			return ScenarioManager.ObjectImportType.Spawner;
		case EPropType.Portal:
			return ScenarioManager.ObjectImportType.Portal;
		case EPropType.TerrainVisualEffect:
			return ScenarioManager.ObjectImportType.TerrainVisualEffect;
		case EPropType.MonsterGrave:
			return ScenarioManager.ObjectImportType.MonsterGrave;
		default:
			return ScenarioManager.ObjectImportType.None;
		}
	}

	public static ScenarioManager.ObjectImportType GetSpecificPropType(ESpecificPropType specificPropType)
	{
		switch (specificPropType)
		{
		case ESpecificPropType.RockSingle:
		case ESpecificPropType.RockTriple:
		case ESpecificPropType.PlinthSingle:
			return ScenarioManager.ObjectImportType.Obstacle;
		case ESpecificPropType.BearTrap:
		case ESpecificPropType.BombTrap:
		case ESpecificPropType.DamageMine:
		case ESpecificPropType.PoisonMine:
		case ESpecificPropType.StunMine:
			return ScenarioManager.ObjectImportType.Trap;
		case ESpecificPropType.SewerPipeSpawner:
		case ESpecificPropType.GraveSingleSpawner:
		case ESpecificPropType.GraveDoubleSpawner:
			return ScenarioManager.ObjectImportType.Spawner;
		case ESpecificPropType.QuestDoll:
		case ESpecificPropType.QuestClaw:
			return ScenarioManager.ObjectImportType.CarryableQuestItem;
		case ESpecificPropType.TheFavorite:
		case ESpecificPropType.TheNewFavorite:
			return ScenarioManager.ObjectImportType.Resource;
		default:
			return ScenarioManager.ObjectImportType.None;
		}
	}

	public static GameObject GetSpecificPropPrefab(string propTypeString)
	{
		return CObjectProp.SpecificPropTypes.SingleOrDefault((ESpecificPropType x) => x.ToString() == propTypeString) switch
		{
			ESpecificPropType.RockSingle => Instance.m_SpecificProps.RockSingle, 
			ESpecificPropType.RockTriple => Instance.m_SpecificProps.RockTriple, 
			ESpecificPropType.PlinthSingle => Instance.m_SpecificProps.PlinthSingle, 
			ESpecificPropType.BearTrap => Instance.m_SpecificProps.BearTrap, 
			ESpecificPropType.BombTrap => Instance.m_SpecificProps.BombTrap, 
			ESpecificPropType.DamageMine => Instance.m_SpecificProps.DamageMine, 
			ESpecificPropType.PoisonMine => Instance.m_SpecificProps.PoisonMine, 
			ESpecificPropType.StunMine => Instance.m_SpecificProps.StunMine, 
			ESpecificPropType.SewerPipeSpawner => Instance.m_SpecificProps.SewerPipe, 
			ESpecificPropType.GraveSingleSpawner => Instance.m_SpecificProps.GraveSingle, 
			ESpecificPropType.GraveDoubleSpawner => Instance.m_SpecificProps.GraveDouble, 
			ESpecificPropType.QuestClaw => Instance.m_SpecificProps.QuestClaw, 
			ESpecificPropType.QuestDoll => Instance.m_SpecificProps.QuestDoll, 
			ESpecificPropType.TheFavorite => Instance.m_SpecificProps.TheFavorite, 
			ESpecificPropType.TheNewFavorite => Instance.m_SpecificProps.TheNewFavorite, 
			_ => null, 
		};
	}
}
