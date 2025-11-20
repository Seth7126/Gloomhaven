using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.State;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class ActorStatPanel : Singleton<ActorStatPanel>
{
	[Header("Character Stats")]
	[SerializeField]
	private GameObject characterStatsObject;

	[SerializeField]
	private Image characterPortrait;

	[SerializeField]
	private TMP_Text characterLifeAmount;

	[SerializeField]
	private UICharacterStats characterStats;

	[SerializeField]
	private UIPersonalQuestProgress personalQuestProgress;

	[SerializeField]
	private GameObject personalQuestContainer;

	[SerializeField]
	private UIScenarioBattleGoalProgress battleGoalProgress;

	[SerializeField]
	private GameObject battleGoalContainer;

	[SerializeField]
	private GoldCounter goldCounter;

	[Header("Attack Modifiers")]
	[SerializeField]
	private UIAttackModifierCalculator characterMissMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterMinusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterZeroMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator characterPlusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator characterTimesTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyMissMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyMinusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyMinusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyZeroMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyPlusOneMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyPlusTwoMod;

	[SerializeField]
	private UIAttackModifierCalculator enemyTimesTwoMod;

	[SerializeField]
	private Sprite availableCounter;

	[SerializeField]
	private Sprite usedCounter;

	[SerializeField]
	private GameObject counterPrefab;

	[SerializeField]
	private GameObject ModifierObject;

	[SerializeField]
	private RectTransform modifierContainer;

	[SerializeField]
	private UIScenarioAttackModifier modifierPrefab;

	[Header("Monster Card")]
	[SerializeField]
	private GameObject enemyStatsObject;

	[SerializeField]
	private TMP_Text enemyCardTitle;

	[SerializeField]
	private TMP_Text enemyName;

	[SerializeField]
	private RectTransform enemyLevelRect;

	[SerializeField]
	private float enemyLevelAligment;

	[SerializeField]
	private TMP_Text enemyLevel;

	[SerializeField]
	private HorizontalLayoutGroupExtended enemyLevelLayoutAligment;

	[SerializeField]
	private Image enemyHealthIcon;

	[SerializeField]
	private TMP_Text enemyHealthName;

	[SerializeField]
	private TMP_Text enemyHealthAmount;

	[SerializeField]
	private Image enemyMovementIcon;

	[SerializeField]
	private TMP_Text enemyMovementName;

	[SerializeField]
	private TMP_Text enemyMovementAmount;

	[SerializeField]
	private Image enemyAttackIcon;

	[SerializeField]
	private TMP_Text enemyAttackName;

	[SerializeField]
	private TMP_Text enemyAttackAmount;

	[SerializeField]
	private Image enemyRangeIcon;

	[SerializeField]
	private TMP_Text enemyRangeName;

	[SerializeField]
	private TMP_Text enemyRangeAmount;

	[SerializeField]
	private GameObject enemyImmunitiesObject;

	[SerializeField]
	private Transform enemyImmunitiesContentHolder;

	[SerializeField]
	private AutoScrollRect enemyImmunitiesScroller;

	[SerializeField]
	private GameObject uiMonsterImmunityPrefab;

	[SerializeField]
	private Image enemyPortrait;

	[SerializeField]
	private GameObject onAttackConditionsObject;

	[SerializeField]
	private TMP_Text onAttackConditionsTitle;

	[SerializeField]
	private Transform onAttackConditionsContentHolder;

	[SerializeField]
	private AutoScrollRect onAttackConditionsScroller;

	[SerializeField]
	private GameObject onAttackConditionsPrefab;

	[SerializeField]
	private Transform enemyModifierContainer;

	[Header("Current Turn Enemy Stats")]
	[SerializeField]
	private GameObject currentTurnObject;

	[SerializeField]
	private TMP_Text currentTurnTitle;

	[SerializeField]
	private Transform currentTurnCardHolder;

	[SerializeField]
	private MonsterBaseUI currentTurnCard;

	[Header("Conditions & Effects")]
	[SerializeField]
	private TMP_Text statusEffectsTitle;

	[SerializeField]
	private GameObject statusEffectsContent;

	[SerializeField]
	private GameObject statusEffectsFrame;

	[SerializeField]
	private GameObject statusEffectPrefab;

	[SerializeField]
	private AutoScrollRect statusEffectsScroller;

	[SerializeField]
	private Transform permanentEffectsContainer;

	[SerializeField]
	private Transform permanentEffectsHolder;

	[SerializeField]
	private Transform positiveEffectsContainer;

	[SerializeField]
	private Transform positiveEffectsHolder;

	[SerializeField]
	private Transform negativeEffectsContainer;

	[SerializeField]
	private Transform negativeEffectsHolder;

	[Header("Active Abilities")]
	[SerializeField]
	private GameObject activeAbilitiesObject;

	[SerializeField]
	private TMP_Text activeAbilitiesTitle;

	[SerializeField]
	private GameObject activeAbilitiesContent;

	[SerializeField]
	private GameObject activeAbilitiesFrame;

	[SerializeField]
	private Transform activeAbilitiesHolder;

	[SerializeField]
	private GameObject activeAbilityPrefab;

	[SerializeField]
	private AutoScrollRect activeAbilityScroll;

	[SerializeField]
	private Transform persistentAbilitiesContainer;

	[SerializeField]
	private Transform persistentAbilitiesHolder;

	[SerializeField]
	private Transform roundAbilitiesContainer;

	[SerializeField]
	private Transform roundAbilitiesHolder;

	[SerializeField]
	private Transform summonedAbilitiesContainer;

	[SerializeField]
	private Transform summonedAbilitiesHolder;

	[SerializeField]
	private Transform attributesContainer;

	[SerializeField]
	private Transform attributesHolder;

	[SerializeField]
	private UIStatusEffect passivePerkPrefab;

	[Header("Spawner")]
	[SerializeField]
	private GameObject spawnerPanel;

	[SerializeField]
	private ActorStatPanelEffects spawnerEffects;

	[Header("Utilites")]
	[SerializeField]
	private ImageAddressableLoader _imageSpriteLoader;

	[SerializeField]
	private GameObject _middleContainer;

	private UIWindow myWindow;

	private List<UIMonsterImmunity> monsterImmunities = new List<UIMonsterImmunity>();

	private List<UIOnAttackCondition> onAttackConditions = new List<UIOnAttackCondition>();

	private List<GameObject> permanentEffects = new List<GameObject>();

	private List<UIStatusEffect> positiveEffects = new List<UIStatusEffect>();

	private List<UIStatusEffect> negativeEffects = new List<UIStatusEffect>();

	private List<GameObject> activeAbilities = new List<GameObject>();

	private List<UIScenarioAttackModifier> conditionalAttackModifiersPool = new List<UIScenarioAttackModifier>();

	private bool isGhosted;

	private CSpawner m_SpawnerShown;

	private CActor m_ActorShown;

	private CSpawner lastSelectedSpawner;

	private CActor lastSelectedActor;

	private bool _shouldBeHidden;

	private bool _isHidden;

	private bool _doShow = true;

	public Sprite AvailableCounter => availableCounter;

	public Sprite UsedCounter => usedCounter;

	public GameObject CounterPrefab => counterPrefab;

	public bool DoShow
	{
		get
		{
			return _doShow;
		}
		set
		{
			_doShow = value;
			HideTemporary(!_doShow);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		myWindow = GetComponent<UIWindow>();
	}

	private static void OnHideTooltipsStatic()
	{
		Singleton<ActorStatPanel>.Instance.OnHideTooltips();
	}

	private static void OnShowTooltipsStatic()
	{
		Singleton<ActorStatPanel>.Instance.OnShowTooltips();
	}

	private bool CanShow()
	{
		if (TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone)
		{
			return false;
		}
		if (m_ActorShown != null || m_SpawnerShown != null)
		{
			return false;
		}
		if (Singleton<UIResultsManager>.Instance.IsShown)
		{
			return false;
		}
		if (!DoShow)
		{
			return false;
		}
		return true;
	}

	private void CheckMiddleContainerActivity()
	{
		if (!(_middleContainer == null))
		{
			bool active = activeAbilitiesObject.activeSelf || enemyImmunitiesObject.activeSelf;
			_middleContainer.SetActive(active);
		}
	}

	private void SetAbilitiesContainerActivity(bool activity)
	{
		activeAbilitiesObject.SetActive(activity);
		CheckMiddleContainerActivity();
	}

	public void Show(CSpawner spawner)
	{
		if (!CanShow())
		{
			return;
		}
		CMonsterClass nextMonsterClassToSpawn = spawner.GetNextMonsterClassToSpawn(ScenarioManager.CurrentScenarioState.Players.Count, ScenarioManager.CurrentScenarioState.RoundNumber);
		if (nextMonsterClassToSpawn != null)
		{
			m_SpawnerShown = spawner;
			lastSelectedSpawner = m_SpawnerShown;
			InitializeMonsterCard(nextMonsterClassToSpawn);
			InitializeConditionsAndEffectsForSpawner(nextMonsterClassToSpawn);
			currentTurnObject.SetActive(value: false);
			characterStatsObject.SetActive(value: false);
			SetAbilitiesContainerActivity(activity: false);
			if (!isGhosted)
			{
				spawnerEffects.ToggleEffect(active: false);
			}
			isGhosted = true;
			spawnerPanel.gameObject.SetActive(value: true);
			if (!_shouldBeHidden)
			{
				myWindow.Show();
			}
		}
	}

	public void Show(CActor actor)
	{
		if (!CanShow())
		{
			return;
		}
		if (actor == null)
		{
			Debug.LogError("Trying to show the stat panel but actor provided is null.");
			return;
		}
		m_ActorShown = actor;
		lastSelectedActor = m_ActorShown;
		spawnerPanel.gameObject.SetActive(value: false);
		if (isGhosted)
		{
			spawnerEffects.RestoreCard();
		}
		isGhosted = false;
		if (actor is CEnemyActor enemyActor)
		{
			InitializeMonsterCard(enemyActor);
			SpawnCurrentTurnCard(enemyActor);
			characterStatsObject.SetActive(value: false);
			SetAbilitiesContainerActivity(activity: false);
			if (actor is CObjectActor { AttachedProp: not null })
			{
				ModifierObject.SetActive(value: false);
				currentTurnObject.SetActive(value: false);
			}
			else
			{
				ModifierObject.SetActive(value: true);
				currentTurnObject.SetActive(value: true);
			}
		}
		else
		{
			if (actor is CHeroSummonActor heroSummonActor)
			{
				InitializeHeroSummonCard(heroSummonActor);
				characterStatsObject.SetActive(value: false);
			}
			else
			{
				InitializeCharacterCard(actor);
				enemyStatsObject.SetActive(value: false);
			}
			currentTurnObject.SetActive(value: false);
			ModifierObject.SetActive(value: true);
		}
		InitializeActiveAbilities(actor);
		InitializeConditionsAndEffects(actor);
		if (!_shouldBeHidden)
		{
			myWindow.Show();
		}
	}

	public void HideTemporary(bool hide)
	{
		if (hide)
		{
			if (myWindow.IsVisible)
			{
				_isHidden = true;
				myWindow.Hide(instant: true);
			}
		}
		else if (_isHidden)
		{
			myWindow.Show();
			_isHidden = false;
		}
	}

	private void OnHideTooltips()
	{
		_shouldBeHidden = true;
		if (m_ActorShown != null)
		{
			CActor actorShown = m_ActorShown;
			HideForActor(m_ActorShown);
			lastSelectedActor = actorShown;
		}
		if (m_SpawnerShown != null)
		{
			CSpawner spawnerShown = m_SpawnerShown;
			HideForSpawner(m_SpawnerShown);
			lastSelectedSpawner = spawnerShown;
		}
	}

	private void OnShowTooltips()
	{
		_shouldBeHidden = false;
		if (lastSelectedActor != null)
		{
			Show(lastSelectedActor);
			lastSelectedActor = null;
		}
		if (lastSelectedSpawner != null)
		{
			Show(lastSelectedSpawner);
			lastSelectedSpawner = null;
		}
	}

	private void InitializeCharacterCard(CActor actor)
	{
		characterStatsObject.SetActive(value: true);
		CPlayerActor cPlayerActor = (CPlayerActor)actor;
		characterStats.Init(cPlayerActor);
		Sprite scenarioPreviewInfoPortrait = UIInfoTools.Instance.GetCharacterConfigUI(actor.Class.DefaultModel, useDefault: true, cPlayerActor.CharacterClass.CharacterYML.CustomCharacterConfig).scenarioPreviewInfoPortrait;
		characterPortrait.sprite = ((scenarioPreviewInfoPortrait == null) ? UIInfoTools.Instance.GetCharacterHeroPortrait(actor.Class.DefaultModel, cPlayerActor.CharacterClass.CharacterYML.CustomCharacterConfig) : scenarioPreviewInfoPortrait);
		characterLifeAmount.text = $"<sprite name=\"AA_Add_Health\">{actor.Health}/{actor.MaxHealth}";
		CPersonalQuestState cPersonalQuestState = ((AdventureState.MapState?.MapParty == null || !AdventureState.MapState.IsCampaign) ? null : AdventureState.MapState.MapParty.SelectedCharacters.ToList().FirstOrDefault((CMapCharacter x) => x != null && x.CharacterID == actor.Class.ID)?.PersonalQuest);
		if (cPersonalQuestState == null || (cPersonalQuestState.IsConcealed && FFSNetwork.IsOnline && !actor.IsUnderMyControl))
		{
			personalQuestContainer.SetActive(value: false);
		}
		else
		{
			personalQuestContainer.SetActive(value: true);
			personalQuestProgress.SetPersonalQuest(cPersonalQuestState);
		}
		if (AdventureState.MapState?.InProgressQuestState != null && (!FFSNetwork.IsOnline || actor.IsUnderMyControl))
		{
			CBattleGoalState chosenBattleGoal = AdventureState.MapState.InProgressQuestState.GetChosenBattleGoal(actor.Class.ID);
			if (chosenBattleGoal != null)
			{
				battleGoalProgress.SetBattleGoal(chosenBattleGoal);
				battleGoalContainer.SetActive(value: true);
			}
			else
			{
				battleGoalContainer.SetActive(value: false);
			}
		}
		else
		{
			battleGoalContainer.SetActive(value: false);
		}
		CCharacterClass charClass = (CCharacterClass)actor.Class;
		if (AdventureState.MapState != null && AdventureState.MapState.GoldMode == EGoldMode.CharacterGold)
		{
			goldCounter.gameObject.SetActive(value: true);
			goldCounter.SetCount(actor.Gold);
		}
		else
		{
			goldCounter.gameObject.SetActive(value: false);
		}
		ShowConditionalModifiers(charClass);
		characterMissMod.SetupCounters("*0", actor, charClass);
		characterMinusTwoMod.SetupCounters("-2", actor, charClass);
		characterMinusOneMod.SetupCounters("-1", actor, charClass);
		characterZeroMod.SetupCounters("+0", actor, charClass);
		characterPlusOneMod.SetupCounters("+1", actor, charClass);
		characterPlusTwoMod.SetupCounters("+2", actor, charClass);
		characterTimesTwoMod.SetupCounters("*2", actor, charClass);
	}

	private void ShowConditionalModifiers(CCharacterClass charClass)
	{
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in charClass.AttackModifierCards.Concat(charClass.DiscardedAttackModifierCards)
			where it.IsConditionalModifier || it.IsBless || it.IsCurse
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalAttackModifiersPool, modifierPrefab.gameObject, modifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			conditionalAttackModifiersPool[num].transform.SetParent(modifierContainer);
			AttackModifierYMLData modif = list[num].First();
			conditionalAttackModifiersPool[num].Init(modif, list[num].Count());
			conditionalAttackModifiersPool[num].UpdateCounters(list[num].Count(), charClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
	}

	private void InitializeMonsterCard(CMonsterClass enemyClass)
	{
		InitializeMonsterClass(enemyClass, enemyClass.StatLevel, enemyClass.Health(), enemyClass.Health());
	}

	private void InitializeMonsterCard(CEnemyActor enemyActor)
	{
		string titleKey = enemyActor.Type switch
		{
			CActor.EType.Ally => "GUI_ALLY", 
			CActor.EType.Neutral => "GUI_NEUTRAL", 
			CActor.EType.Enemy2 => "GUI_ENEMY2", 
			_ => "GUI_MONSTER_CARD", 
		};
		string overrideLocKey = string.Empty;
		string overrideCharacterPortrait = string.Empty;
		if (enemyActor is CObjectActor cObjectActor)
		{
			titleKey = "GUI_OBJECT_WITH_HEALTH";
			if (cObjectActor.IsAttachedToProp)
			{
				overrideLocKey = (string.IsNullOrEmpty(cObjectActor.AttachedProp.PropHealthDetails.CustomLocKey) ? cObjectActor.AttachedProp.PrefabName : cObjectActor.AttachedProp.PropHealthDetails.CustomLocKey);
				overrideCharacterPortrait = (string.IsNullOrEmpty(cObjectActor.AttachedProp.PropHealthDetails.ActorSpriteName) ? string.Empty : cObjectActor.AttachedProp.PropHealthDetails.ActorSpriteName);
			}
		}
		string suffixName = "<color=#" + ColorUtility.ToHtmlStringRGB(UIInfoTools.Instance.greyedOutTextColor) + ">" + Choreographer.GetActorIDForCombatLogIfNeeded(enemyActor) + "</color>";
		InitializeMonsterClass(enemyActor.MonsterClass, enemyActor.Level, enemyActor.Health, enemyActor.MaxHealth, titleKey, suffixName, enemyActor, overrideLocKey, overrideCharacterPortrait);
	}

	private void InitializeMonsterClass(CMonsterClass monsterClass, int level, int health, int maxHealth, string titleKey = "GUI_MONSTER_CARD", string suffixName = "", CEnemyActor enemyActor = null, string overrideLocKey = null, string overrideCharacterPortrait = null)
	{
		string term = (string.IsNullOrEmpty(overrideLocKey) ? monsterClass.LocKey : overrideLocKey);
		string actorModel = (string.IsNullOrEmpty(overrideCharacterPortrait) ? monsterClass.DefaultModel : overrideCharacterPortrait);
		enemyStatsObject.SetActive(value: true);
		onAttackConditionsObject.SetActive(value: false);
		enemyCardTitle.text = LocalizationManager.GetTranslation(titleKey);
		enemyLevel.text = level.ToString();
		StartCoroutine(SetEnemyName(LocalizationManager.GetTranslation(term) + suffixName));
		string customPortrait = ScenarioRuleClient.SRLYML.MonsterConfigs.SingleOrDefault((MonsterConfigYMLData s) => monsterClass.MonsterYML.CustomConfig == s.ID)?.Portrait;
		_imageSpriteLoader.LoadAsync(this, enemyPortrait, UIInfoTools.Instance.GetActorPortraitRef(actorModel, customPortrait).SpriteReference);
		InitStatValues(health, maxHealth, monsterClass.Move, monsterClass.Attack, monsterClass.Range, enemyActor, null, monsterClass.StatIsBasedOnXEntries);
		CMonsterAttackModifierDeck cMonsterAttackModifierDeck = MonsterClassManager.EnemyMonsterAttackModifierDeck;
		if (enemyActor != null)
		{
			if (enemyActor.MonsterClass.Boss)
			{
				cMonsterAttackModifierDeck = MonsterClassManager.BossMonsterAttackModifierDeck;
			}
			else
			{
				switch (enemyActor.OriginalType)
				{
				case CActor.EType.Enemy:
					cMonsterAttackModifierDeck = MonsterClassManager.EnemyMonsterAttackModifierDeck;
					break;
				case CActor.EType.Ally:
					cMonsterAttackModifierDeck = MonsterClassManager.AlliedMonsterAttackModifierDeck;
					break;
				case CActor.EType.Enemy2:
					cMonsterAttackModifierDeck = MonsterClassManager.Enemy2MonsterAttackModifierDeck;
					break;
				case CActor.EType.Neutral:
					cMonsterAttackModifierDeck = MonsterClassManager.NeutralMonsterAttackModifierDeck;
					break;
				}
			}
		}
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in cMonsterAttackModifierDeck.AttackModifierCards.Concat(cMonsterAttackModifierDeck.DiscardedAttackModifierCards)
			where it.IsConditionalModifier || it.IsBless || it.IsCurse
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalAttackModifiersPool, modifierPrefab.gameObject, enemyModifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			conditionalAttackModifiersPool[num].transform.SetParent(enemyModifierContainer);
			AttackModifierYMLData modif = list[num].First();
			conditionalAttackModifiersPool[num].Init(modif, list[num].Count());
			conditionalAttackModifiersPool[num].UpdateCounters(list[num].Count(), cMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
		enemyMissMod.SetupCounters("*0", enemyActor);
		enemyMinusTwoMod.SetupCounters("-2", enemyActor);
		enemyMinusOneMod.SetupCounters("-1", enemyActor);
		enemyZeroMod.SetupCounters("+0", enemyActor);
		enemyPlusOneMod.SetupCounters("+1", enemyActor);
		enemyPlusTwoMod.SetupCounters("+2", enemyActor);
		enemyTimesTwoMod.SetupCounters("*2", enemyActor);
		List<CAbility.EAbilityType> list2 = ((enemyActor != null) ? enemyActor.Immunities.ToList() : monsterClass.Immunities.ToList());
		if (list2.Count > 0)
		{
			SetEnemiesImmunityContainerActivity(activity: true);
			monsterImmunities.ForEach(delegate(UIMonsterImmunity x)
			{
				UnityEngine.Object.Destroy(x.gameObject);
			});
			monsterImmunities.Clear();
			if (CActor.ImmuneToAllConditionsCheck(list2))
			{
				list2 = list2.Except(CActor.ImmunityConditionAbilityTypes).ToList();
				UIMonsterImmunity component = UnityEngine.Object.Instantiate(uiMonsterImmunityPrefab, enemyImmunitiesContentHolder, worldPositionStays: false).GetComponent<UIMonsterImmunity>();
				component.Initialize("AllConditions");
				monsterImmunities.Add(component);
			}
			foreach (CAbility.EAbilityType item in list2)
			{
				if (item != CAbility.EAbilityType.ControlActor)
				{
					UIMonsterImmunity component2 = UnityEngine.Object.Instantiate(uiMonsterImmunityPrefab, enemyImmunitiesContentHolder, worldPositionStays: false).GetComponent<UIMonsterImmunity>();
					component2.Initialize(item.ToString());
					monsterImmunities.Add(component2);
				}
			}
			enemyImmunitiesScroller?.StartAutoscroll();
		}
		else
		{
			enemyImmunitiesScroller?.StopAutoscroll();
			SetEnemiesImmunityContainerActivity(activity: false);
		}
	}

	private void SetEnemiesImmunityContainerActivity(bool activity)
	{
		enemyImmunitiesObject.SetActive(activity);
		CheckMiddleContainerActivity();
	}

	private void InitializeHeroSummonCard(CHeroSummonActor heroSummonActor)
	{
		enemyStatsObject.SetActive(value: true);
		SetEnemiesImmunityContainerActivity(activity: false);
		enemyCardTitle.text = LocalizationManager.GetTranslation("GUI_SUMMON_CARD");
		string text = "<color=#" + ColorUtility.ToHtmlStringRGB(UIInfoTools.Instance.greyedOutTextColor) + ">" + Choreographer.GetActorIDForCombatLogIfNeeded(heroSummonActor) + "</color>";
		StartCoroutine(SetEnemyName(LocalizationManager.GetTranslation(heroSummonActor.ActorLocKey()) + text));
		enemyLevel.text = heroSummonActor.Level.ToString();
		string customPortrait = ScenarioRuleClient.SRLYML.MonsterConfigs.SingleOrDefault((MonsterConfigYMLData s) => heroSummonActor.HeroSummonClass.SummonYML.CustomConfig == s.ID)?.Portrait;
		_imageSpriteLoader.LoadAsync(this, enemyPortrait, UIInfoTools.Instance.GetActorPortraitRef(heroSummonActor.GetPrefabName(), customPortrait).SpriteReference);
		HeroSummonYMLData summonData = heroSummonActor.SummonData;
		InitStatValues(heroSummonActor.Health, heroSummonActor.MaxHealth, summonData.Move, summonData.Attack, summonData.Range, null, heroSummonActor, summonData.StatIsBasedOnXEntries);
		CCharacterClass characterClass = heroSummonActor.Summoner.CharacterClass;
		List<IGrouping<string, AttackModifierYMLData>> list = (from it in characterClass.AttackModifierCards.Concat(characterClass.DiscardedAttackModifierCards)
			where it.IsConditionalModifier
			group it by it.Name).ToList();
		HelperTools.NormalizePool(ref conditionalAttackModifiersPool, modifierPrefab.gameObject, enemyModifierContainer, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			conditionalAttackModifiersPool[num].transform.SetParent(enemyModifierContainer);
			AttackModifierYMLData modif = list[num].First();
			conditionalAttackModifiersPool[num].Init(modif, list[num].Count());
			conditionalAttackModifiersPool[num].UpdateCounters(list[num].Count(), characterClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => x.Name == modif.Name));
		}
		enemyMissMod.SetupCounters("*0", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyMinusTwoMod.SetupCounters("-2", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyMinusOneMod.SetupCounters("-1", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyZeroMod.SetupCounters("+0", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyPlusOneMod.SetupCounters("+1", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyPlusTwoMod.SetupCounters("+2", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		enemyTimesTwoMod.SetupCounters("*2", heroSummonActor, heroSummonActor.Summoner.CharacterClass);
		if (heroSummonActor?.SummonData?.OnAttackConditions != null)
		{
			CHeroSummonActor cHeroSummonActor = heroSummonActor;
			if (cHeroSummonActor != null && cHeroSummonActor.SummonData?.OnAttackConditions.Count > 0)
			{
				goto IL_0460;
			}
		}
		CHeroSummonActor cHeroSummonActor2 = heroSummonActor;
		if (cHeroSummonActor2 != null)
		{
			HeroSummonYMLData summonData2 = cHeroSummonActor2.SummonData;
			if (summonData2 != null && summonData2.AttackInfuse.HasValue)
			{
				goto IL_0460;
			}
		}
		onAttackConditionsScroller.StopAutoscroll();
		onAttackConditionsObject.SetActive(value: false);
		return;
		IL_0460:
		onAttackConditionsObject.SetActive(value: true);
		onAttackConditionsTitle.text = LocalizationManager.GetTranslation("GUI_ON_ATTACK_CONDITIONS_TITLE");
		onAttackConditions.ForEach(delegate(UIOnAttackCondition x)
		{
			UnityEngine.Object.Destroy(x.gameObject);
		});
		onAttackConditions.Clear();
		if (heroSummonActor?.SummonData?.OnAttackConditions != null)
		{
			foreach (CCondition.ENegativeCondition item in heroSummonActor?.SummonData?.OnAttackConditions)
			{
				UIOnAttackCondition component = UnityEngine.Object.Instantiate(onAttackConditionsPrefab, onAttackConditionsContentHolder, worldPositionStays: false).GetComponent<UIOnAttackCondition>();
				component.Initialize(item);
				onAttackConditions.Add(component);
			}
		}
		CHeroSummonActor cHeroSummonActor3 = heroSummonActor;
		if (cHeroSummonActor3 != null)
		{
			HeroSummonYMLData summonData3 = cHeroSummonActor3.SummonData;
			if (summonData3 != null && summonData3.AttackInfuse.HasValue)
			{
				UIOnAttackCondition component2 = UnityEngine.Object.Instantiate(onAttackConditionsPrefab, onAttackConditionsContentHolder, worldPositionStays: false).GetComponent<UIOnAttackCondition>();
				component2.Initialize(CCondition.ENegativeCondition.NA, (heroSummonActor?.SummonData?.AttackInfuse).Value);
				onAttackConditions.Add(component2);
			}
		}
		CHeroSummonActor cHeroSummonActor4 = heroSummonActor;
		if (cHeroSummonActor4 != null && cHeroSummonActor4.SummonData?.AdjacentAttackOnDeath > 0)
		{
			string onDeath = string.Format(LocalizationManager.GetTranslation("GUI_ON_DEATH_ADJACENT"), heroSummonActor.SummonData.AdjacentAttackOnDeath);
			UIOnAttackCondition component3 = UnityEngine.Object.Instantiate(onAttackConditionsPrefab, onAttackConditionsContentHolder, worldPositionStays: false).GetComponent<UIOnAttackCondition>();
			component3.Initialize(CCondition.ENegativeCondition.NA, null, onDeath);
			onAttackConditions.Add(component3);
		}
		onAttackConditionsScroller.StartAutoscroll();
	}

	private void InitStatValues(int health, int maxHealth, int move, int attack, int range, CEnemyActor enemyActor = null, CHeroSummonActor heroSummonActor = null, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries = null)
	{
		enemyHealthAmount.text = health + "/" + maxHealth;
		if (move > 0 || (statIsBasedOnXEntries != null && statIsBasedOnXEntries.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.BaseStatType == EMonsterBaseStats.Move)))
		{
			enemyMovementAmount.text = move.ToString();
			Image image = enemyMovementIcon;
			TMP_Text tMP_Text = enemyMovementName;
			Color color = (enemyMovementAmount.color = UIInfoTools.Instance.neutralActionTextColor);
			Color color2 = (tMP_Text.color = color);
			image.color = color2;
		}
		else
		{
			enemyMovementAmount.text = "-";
			Image image2 = enemyMovementIcon;
			TMP_Text tMP_Text2 = enemyMovementName;
			Color color = (enemyMovementAmount.color = UIInfoTools.Instance.greyedOutTextColor);
			Color color2 = (tMP_Text2.color = color);
			image2.color = color2;
		}
		bool flag = statIsBasedOnXEntries?.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.BaseStatType == EMonsterBaseStats.Attack) ?? false;
		if (attack > 0 || flag)
		{
			string empty = string.Empty;
			empty = ((enemyActor != null) ? enemyActor.AttackStrengthForUI.ToString() : ((heroSummonActor == null) ? attack.ToString() : heroSummonActor.AttackStrength.ToString()));
			enemyAttackAmount.text = empty;
			Image image3 = enemyAttackIcon;
			TMP_Text tMP_Text3 = enemyAttackName;
			Color color = (enemyAttackAmount.color = UIInfoTools.Instance.neutralActionTextColor);
			Color color2 = (tMP_Text3.color = color);
			image3.color = color2;
		}
		else
		{
			enemyAttackAmount.text = "-";
			Image image4 = enemyAttackIcon;
			TMP_Text tMP_Text4 = enemyAttackName;
			Color color = (enemyAttackAmount.color = UIInfoTools.Instance.greyedOutTextColor);
			Color color2 = (tMP_Text4.color = color);
			image4.color = color2;
		}
		if (range > 1)
		{
			enemyRangeAmount.text = range.ToString();
			Image image5 = enemyRangeIcon;
			TMP_Text tMP_Text5 = enemyRangeName;
			Color color = (enemyRangeAmount.color = UIInfoTools.Instance.neutralActionTextColor);
			Color color2 = (tMP_Text5.color = color);
			image5.color = color2;
		}
		else
		{
			enemyRangeAmount.text = "-";
			Image image6 = enemyRangeIcon;
			TMP_Text tMP_Text6 = enemyRangeName;
			Color color = (enemyRangeAmount.color = UIInfoTools.Instance.greyedOutTextColor);
			Color color2 = (tMP_Text6.color = color);
			image6.color = color2;
		}
	}

	private IEnumerator SetEnemyName(string name)
	{
		enemyName.text = name;
		yield return null;
	}

	private void SpawnCurrentTurnCard(CEnemyActor enemyActor)
	{
		currentTurnObject.SetActive(value: true);
		CMonsterAbilityCard roundAbilityCard = enemyActor.MonsterClass.RoundAbilityCard;
		if (roundAbilityCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			currentTurnCardHolder.gameObject.SetActive(value: true);
			currentTurnTitle.color = UIInfoTools.Instance.basicTextColor;
			currentTurnCard.GenerateCard(roundAbilityCard, enemyActor);
		}
		else
		{
			currentTurnCardHolder.gameObject.SetActive(value: false);
			currentTurnTitle.color = UIInfoTools.Instance.greyedOutTextColor;
		}
	}

	private void InitializeConditionsAndEffects(CActor actor)
	{
		List<CCondition.ENegativeCondition> scenarioModifierAddConditionToAttacks = GetScenarioModifierAddConditionToAttacks(actor);
		Dictionary<string, string> monsterOrSummonBaseStats = PersistentAbilitiesUI.GetMonsterOrSummonBaseStats(actor, scenarioModifierAddConditionToAttacks);
		bool flag = actor is CEnemyActor && actor.MindControlDuration != CAbilityControlActor.EControlDurationType.None;
		bool flag2 = monsterOrSummonBaseStats.Count > 0 || flag;
		bool flag3 = actor.Tokens.CheckPositiveTokens.Count > 0;
		bool flag4 = actor.Tokens.CheckNegativeTokens.Count > 0;
		if (flag2 || flag3 || flag4)
		{
			statusEffectsContent.SetActive(value: true);
			statusEffectsFrame.SetActive(value: true);
			statusEffectsTitle.color = UIInfoTools.Instance.basicTextColor;
			if (flag2)
			{
				permanentEffectsContainer.gameObject.SetActive(value: true);
				permanentEffects.ForEach(delegate(GameObject x)
				{
					UnityEngine.Object.Destroy(x);
				});
				permanentEffects.Clear();
				foreach (KeyValuePair<string, string> item in monsterOrSummonBaseStats)
				{
					UIStatusEffect component = UnityEngine.Object.Instantiate(statusEffectPrefab, permanentEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component.Initialize("$Innate$", UIInfoTools.Instance.GetActiveAbilityIcon(item.Value), UIInfoTools.Instance.basicTextColor, CreateLayout.LocaliseText(item.Key));
					component.GetComponent<ContentSizeFitter>().enabled = false;
					permanentEffects.Add(component.gameObject);
				}
				if (flag)
				{
					string customEffectDescription = string.Empty;
					if (actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction)
					{
						customEffectDescription = "$GUI_MIND_CONTROL_ACTION$";
					}
					else if (actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneTurn)
					{
						customEffectDescription = "$GUI_MIND_CONTROL_TURN$";
					}
					UIStatusEffect component2 = UnityEngine.Object.Instantiate(statusEffectPrefab, permanentEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component2.Initialize("$GUI_MIND_CONTROL$", UIInfoTools.Instance.GetActiveAbilityIcon("MindControl"), UIInfoTools.Instance.basicTextColor, customEffectDescription);
					component2.GetComponent<ContentSizeFitter>().enabled = false;
					permanentEffects.Add(component2.gameObject);
				}
			}
			else
			{
				permanentEffectsContainer.gameObject.SetActive(value: false);
			}
			if (flag3)
			{
				positiveEffectsContainer.gameObject.SetActive(value: true);
				positiveEffects.ForEach(delegate(UIStatusEffect x)
				{
					UnityEngine.Object.Destroy(x.gameObject);
				});
				positiveEffects.Clear();
				foreach (CCondition.EPositiveCondition allPositiveCondition in actor.Tokens.GetAllPositiveConditions())
				{
					UIStatusEffect component3 = UnityEngine.Object.Instantiate(statusEffectPrefab, positiveEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component3.Initialize(allPositiveCondition.ToString(), UIInfoTools.Instance.positiveStatusEffectColor);
					component3.GetComponent<ContentSizeFitter>().enabled = false;
					positiveEffects.Add(component3);
				}
			}
			else
			{
				positiveEffectsContainer.gameObject.SetActive(value: false);
			}
			if (flag4)
			{
				negativeEffectsContainer.gameObject.SetActive(value: true);
				negativeEffects.ForEach(delegate(UIStatusEffect x)
				{
					UnityEngine.Object.Destroy(x.gameObject);
				});
				negativeEffects.Clear();
				foreach (CCondition.ENegativeCondition allNegativeCondition in actor.Tokens.GetAllNegativeConditions())
				{
					UIStatusEffect component4 = UnityEngine.Object.Instantiate(statusEffectPrefab, negativeEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
					component4.Initialize(allNegativeCondition.ToString(), UIInfoTools.Instance.negativeStatusEffectColor);
					component4.GetComponent<ContentSizeFitter>().enabled = false;
					negativeEffects.Add(component4);
				}
			}
			else
			{
				negativeEffectsContainer.gameObject.SetActive(value: false);
			}
			statusEffectsScroller.StartAutoscroll();
		}
		else
		{
			statusEffectsScroller.StopAutoscroll();
			statusEffectsContent.SetActive(value: false);
			statusEffectsFrame.SetActive(value: false);
			statusEffectsTitle.color = UIInfoTools.Instance.greyedOutTextColor;
		}
	}

	private void InitializeConditionsAndEffectsForSpawner(CMonsterClass monsterClass)
	{
		Dictionary<string, string> dictionary = PersistentAbilitiesUI.BuildMonsterBaseStatsDict(monsterClass);
		positiveEffectsContainer.gameObject.SetActive(value: false);
		negativeEffectsContainer.gameObject.SetActive(value: false);
		if (dictionary.Count > 0)
		{
			statusEffectsContent.SetActive(value: true);
			statusEffectsFrame.SetActive(value: true);
			statusEffectsTitle.color = UIInfoTools.Instance.basicTextColor;
			permanentEffectsContainer.gameObject.SetActive(value: true);
			permanentEffects.ForEach(delegate(GameObject x)
			{
				UnityEngine.Object.Destroy(x);
			});
			permanentEffects.Clear();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				UIStatusEffect component = UnityEngine.Object.Instantiate(statusEffectPrefab, permanentEffectsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
				component.Initialize("$Innate$", UIInfoTools.Instance.GetActiveAbilityIcon(item.Value), UIInfoTools.Instance.basicTextColor, CreateLayout.LocaliseText(item.Key));
				component.GetComponent<ContentSizeFitter>().enabled = false;
				permanentEffects.Add(component.gameObject);
			}
			statusEffectsScroller.StartAutoscroll();
		}
		else
		{
			permanentEffectsContainer.gameObject.SetActive(value: false);
			statusEffectsScroller.StopAutoscroll();
			statusEffectsContent.SetActive(value: false);
			statusEffectsFrame.SetActive(value: false);
			statusEffectsTitle.color = UIInfoTools.Instance.greyedOutTextColor;
		}
	}

	private void InitializeActiveAbilities(CActor actor)
	{
		List<CActiveBonus> list = (from x in GetAllActiveBonuses(actor)
			where !x.Ability.ActiveBonusData.Hidden
			select x).ToList();
		List<CharacterPerk> passivePerks = GetPassivePerks(actor);
		List<Tuple<string, string, Sprite>> passiveAttributes = GetPassiveAttributes(actor);
		if (list.Count > 0 || (passivePerks != null && passivePerks.Count > 0) || !passiveAttributes.IsNullOrEmpty())
		{
			SetAbilitiesContainerActivity(activity: true);
			activeAbilitiesContent.SetActive(value: true);
			activeAbilitiesFrame.SetActive(value: true);
			activeAbilitiesTitle.color = UIInfoTools.Instance.basicTextColor;
			activeAbilities.ForEach(delegate(GameObject x)
			{
				UnityEngine.Object.Destroy(x.gameObject);
			});
			activeAbilities.Clear();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int num4 = 0; num4 < passivePerks.Count; num4++)
			{
				UIStatusEffect component = UnityEngine.Object.Instantiate(passivePerkPrefab, persistentAbilitiesHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
				component.Initialize(LocalizationManager.GetTranslation(passivePerks[num4].Perk.Name), passivePerks[num4].Perk.IgnoreNegativeItemEffects ? UIInfoTools.Instance.IgnoreNegativeItemEffectsIcon : UIInfoTools.Instance.IgnoreNegativeScenarioEffectsIcon, UIInfoTools.Instance.basicTextColor, LocalizationManager.GetTranslation(passivePerks[num4].Perk.Description));
				activeAbilities.Add(component.gameObject);
				num++;
			}
			for (int num5 = 0; num5 < passiveAttributes.Count; num5++)
			{
				UIStatusEffect component2 = UnityEngine.Object.Instantiate(statusEffectPrefab, attributesHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
				component2.Initialize(LocalizationManager.GetTranslation(passiveAttributes[num5].Item1), passiveAttributes[num5].Item3, UIInfoTools.Instance.basicTextColor, LocalizationManager.GetTranslation(passiveAttributes[num5].Item2));
				activeAbilities.Add(component2.gameObject);
			}
			foreach (IGrouping<string, CActiveBonus> item in from x in list
				group x by x.BaseCard.Name)
			{
				foreach (IGrouping<Guid, CActiveBonus> item2 in from x in item
					group x by x.Ability.ID)
				{
					CActiveBonus cActiveBonus = item2.FirstOrDefault();
					if (cActiveBonus != null)
					{
						Transform parent = activeAbilitiesHolder;
						if (cActiveBonus.Duration == CActiveBonus.EActiveBonusDurationType.Persistent)
						{
							parent = persistentAbilitiesHolder;
							num++;
						}
						else if (cActiveBonus.Duration == CActiveBonus.EActiveBonusDurationType.Round)
						{
							parent = roundAbilitiesHolder;
							num2++;
						}
						else if (cActiveBonus.Duration == CActiveBonus.EActiveBonusDurationType.Summon)
						{
							parent = summonedAbilitiesHolder;
							num3++;
						}
						UIActiveAbility component3 = UnityEngine.Object.Instantiate(activeAbilityPrefab, parent, worldPositionStays: false).GetComponent<UIActiveAbility>();
						int instances = ((cActiveBonus.Type() == CAbility.EAbilityType.Summon || cActiveBonus.IsDoom) ? 1 : item2.Count());
						component3.Initialize(cActiveBonus, instances);
						activeAbilities.Add(component3.gameObject);
						if (cActiveBonus.IsDoom)
						{
							break;
						}
					}
				}
			}
			persistentAbilitiesContainer.gameObject.SetActive(num > 0);
			roundAbilitiesContainer.gameObject.SetActive(num2 > 0);
			summonedAbilitiesContainer.gameObject.SetActive(num3 > 0);
			attributesContainer.gameObject.SetActive(!passiveAttributes.IsNullOrEmpty());
			activeAbilityScroll.StartAutoscroll();
		}
		else
		{
			activeAbilityScroll.StopAutoscroll();
			SetAbilitiesContainerActivity(activity: false);
			activeAbilitiesContent.SetActive(value: false);
			activeAbilitiesFrame.SetActive(value: false);
			activeAbilitiesTitle.color = UIInfoTools.Instance.greyedOutTextColor;
		}
	}

	private List<CCondition.ENegativeCondition> GetScenarioModifierAddConditionToAttacks(CActor actor)
	{
		List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
		foreach (CScenarioModifierAddConditionsToAbilities item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.AddConditionsToAbilities))
		{
			if (item.ShouldAddConditions(actor, CAbility.EAbilityType.Attack))
			{
				list.AddRange(item.NegativeConditions);
			}
		}
		return list.Distinct().ToList();
	}

	private List<CharacterPerk> GetPassivePerks(CActor actor)
	{
		if (!(actor is CPlayerActor) || AdventureState.MapState?.MapParty == null)
		{
			return new List<CharacterPerk>();
		}
		CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter it) => it.CharacterID == actor.Class.ID);
		if (cMapCharacter != null)
		{
			return cMapCharacter.GetPassivePerks();
		}
		return new List<CharacterPerk>();
	}

	private List<Tuple<string, string, Sprite>> GetPassiveAttributes(CActor actor)
	{
		List<Tuple<string, string, Sprite>> list = new List<Tuple<string, string, Sprite>>();
		if (!(actor is CPlayerActor cPlayerActor) || cPlayerActor.CarriedQuestItems.Count == 0)
		{
			return list;
		}
		for (int i = 0; i < cPlayerActor.CarriedQuestItems.Count; i++)
		{
			CObjectProp cObjectProp = cPlayerActor.CarriedQuestItems[i];
			if (cObjectProp.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem)
			{
				list.Add(new Tuple<string, string, Sprite>(cObjectProp.PrefabName + "_TOOLTIP", cObjectProp.PrefabName + "_DESCR_TOOLTIP", UIInfoTools.Instance.CarryableQuestItemIconSprite));
			}
		}
		return list;
	}

	private List<CActiveBonus> GetAllActiveBonuses(CActor actor)
	{
		List<CActiveBonus> allActives = new List<CActiveBonus>();
		allActives.AddRange(CActiveBonus.FindApplicableActiveBonuses(actor));
		if (!actor.IsEnemyMonsterType)
		{
			allActives.AddRange(from w in CharacterClassManager.FindAllSongActiveBonuses(actor)
				where !allActives.Contains(w)
				select w);
		}
		if (actor.IsDoomed || actor is CPlayerActor { DoomTarget: not null })
		{
			CActor actor2 = (actor as CPlayerActor)?.DoomTarget ?? actor;
			allActives.AddRange(from x in CharacterClassManager.FindAllActiveBonuses(actor2)
				where x.IsDoom
				select x);
		}
		allActives.AddRange(from x in CActiveBonus.FindAllActiveBonuses()
			where x.BaseCard != null && x.BaseCard.CardType == CBaseCard.ECardType.Item && x.Caster == actor && x.Actor != actor
			select x);
		return allActives;
	}

	public void HideForActor(CActor actor = null)
	{
		lastSelectedActor = null;
		if (m_SpawnerShown == null && (actor == null || actor == m_ActorShown))
		{
			Hide();
		}
	}

	public void HideForSpawner(CSpawner spawner = null)
	{
		lastSelectedSpawner = null;
		if (m_ActorShown == null && (spawner == null || spawner == m_SpawnerShown))
		{
			Hide();
		}
	}

	private void Hide()
	{
		m_ActorShown = null;
		m_SpawnerShown = null;
		activeAbilityScroll.StopAutoscroll();
		statusEffectsScroller.StopAutoscroll();
		enemyImmunitiesScroller?.StopAutoscroll();
		onAttackConditionsScroller.StopAutoscroll();
		SetAbilitiesContainerActivity(activity: false);
		myWindow.Hide(instant: true);
		SetAbilitiesContainerActivity(activity: false);
		SetEnemiesImmunityContainerActivity(activity: false);
		spawnerPanel.gameObject.SetActive(value: false);
		_isHidden = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_imageSpriteLoader.Unload(enemyPortrait);
	}
}
