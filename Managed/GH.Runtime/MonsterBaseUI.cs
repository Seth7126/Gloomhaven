using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterBaseUI : MonoBehaviour
{
	[SerializeField]
	private string unknownSymbol;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI baseInitiativeText;

	[SerializeField]
	private TextMeshProUGUI baseHealthText;

	[SerializeField]
	private TextMeshProUGUI baseMoveText;

	[SerializeField]
	private TextMeshProUGUI baseAttackText;

	[SerializeField]
	private TextMeshProUGUI baseRangeText;

	[SerializeField]
	private Transform effectsDesriptionHolder;

	[SerializeField]
	private RectTransform contentHolder;

	[SerializeField]
	private Transform monsterCardHolder;

	[SerializeField]
	private Image shuffleIcon;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Image textBackgroundOpacityImage;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private GameObject roleGameObject;

	[SerializeField]
	private TextLocalizedListener roleText;

	private CMonsterAbilityCard m_DeferredMonsterCard;

	private CEnemyActor m_DeferredEnemyActor;

	private Coroutine makeMergingCoroutine;

	private MonsterRoundCardUI monsterRoundCardUI;

	private int monsterRoundCardId;

	private CEnemyActor enemyActor;

	private int baseHealthValue;

	private int baseMoveValue;

	private int baseAttackValue;

	private int baseRangeValue;

	private int baseHealValue;

	private int baseTargetValue;

	private int baseRetaliateValue;

	private Dictionary<CAbility.EAbilityType, int> specialBaseStats = new Dictionary<CAbility.EAbilityType, int>();

	private LTDescr fadeAnimation;

	private LTDescr moveAnimation;

	private LTDescr scaleAnimation;

	private List<LayoutRow> LayoutRows = new List<LayoutRow>();

	private const string DebugCancel = "FinishAnimateAppearance";

	public CEnemyActor Enemy => enemyActor;

	public void TogglePreview(bool active)
	{
		if (active && m_DeferredMonsterCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards && m_DeferredEnemyActor != null)
		{
			GenerateCard(m_DeferredMonsterCard, m_DeferredEnemyActor);
			m_DeferredMonsterCard = null;
			m_DeferredEnemyActor = null;
		}
		base.gameObject.SetActive(active);
		if (active && monsterRoundCardUI != null)
		{
			monsterRoundCardUI.gameObject.SetActive(value: false);
			contentHolder.gameObject.SetActive(value: true);
		}
	}

	private void OnDestroy()
	{
		CancelLeanTweens();
	}

	private void SetBaseStats(CMonsterAbilityCard roundCard, CEnemyActor sourceEnemy, bool clearInstant = true)
	{
		if (roundCard == null)
		{
			return;
		}
		specialBaseStats.Clear();
		enemyActor = sourceEnemy;
		if (monsterRoundCardId != roundCard.ID)
		{
			if (monsterRoundCardUI != null)
			{
				ObjectPool.RecycleCard(monsterRoundCardId, ObjectPool.ECardType.Monster, monsterRoundCardUI.gameObject);
			}
			monsterRoundCardUI = ObjectPool.SpawnCard(roundCard.ID, ObjectPool.ECardType.Monster, monsterCardHolder, resetLocalScale: false, resetToMiddle: false, resetLocalRotation: false, activate: false).GetComponent<MonsterRoundCardUI>();
			monsterRoundCardUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
			monsterRoundCardUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			monsterRoundCardUI.transform.localScale = Vector3.one;
			monsterRoundCardId = roundCard.ID;
		}
		else
		{
			monsterRoundCardUI.Clean(clearInstant);
		}
		GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(sourceEnemy);
		if (gameObject != null)
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
			backgroundImage.sprite = characterManager.CharacterCardBackgroundSprite;
		}
		baseHealValue = 0;
		baseHealthValue = sourceEnemy.MaxHealth;
		baseMoveValue = sourceEnemy.MonsterClass.Move;
		baseAttackValue = sourceEnemy.AttackStrengthForUI;
		baseRangeValue = sourceEnemy.MonsterClass.Range;
		baseTargetValue = sourceEnemy.MonsterClass.Target;
		if (sourceEnemy.Type == CActor.EType.Ally)
		{
			roleText.SetTextKey("GUI_ALLY");
			roleGameObject.SetActive(value: true);
		}
		else if (sourceEnemy.Type == CActor.EType.Enemy2)
		{
			roleText.SetTextKey("GUI_ENEMY2");
			roleGameObject.SetActive(value: true);
		}
		else if (sourceEnemy.Type == CActor.EType.Neutral)
		{
			roleText.SetTextKey("GUI_NEUTRAL");
			roleGameObject.SetActive(value: true);
		}
		else
		{
			roleGameObject.SetActive(value: false);
		}
		foreach (CAbility ability in roundCard.Action.Abilities)
		{
			if (ability.UseSpecialBaseStat && sourceEnemy.MonsterClass.CurrentMonsterStat.SpecialBaseStats.ContainsKey(ability.AbilityType))
			{
				specialBaseStats.Add(ability.AbilityType, sourceEnemy.MonsterClass.CurrentMonsterStat.SpecialBaseStats[ability.AbilityType]);
			}
		}
		foreach (CAbility ability2 in sourceEnemy.MonsterClass.BaseAction.Abilities)
		{
			if (ability2.AbilityType == CAbility.EAbilityType.Heal)
			{
				baseRangeValue = ability2.Range;
				baseHealValue = ability2.Strength;
			}
		}
		titleText.text = LocalizationManager.GetTranslation(sourceEnemy.ActorLocKey());
		baseInitiativeText.text = roundCard.Initiative.ToString();
		baseHealthText.text = baseHealthValue.ToString();
		baseMoveText.text = baseMoveValue.ToString();
		baseAttackText.text = ((baseAttackValue > 0) ? baseAttackValue.ToString() : unknownSymbol);
		baseRangeText.text = ((baseRangeValue == 1) ? unknownSymbol : baseRangeValue.ToString());
	}

	public void GenerateCard(CMonsterAbilityCard monsterCard, CEnemyActor enemyActor, bool generationProgressive = false)
	{
		if (monsterCard == null || PhaseManager.PhaseType < CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			return;
		}
		m_DeferredEnemyActor = null;
		m_DeferredMonsterCard = null;
		if (this.enemyActor == enemyActor && monsterRoundCardId == monsterCard.ID)
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(enemyActor);
			if (gameObject != null)
			{
				CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
				backgroundImage.sprite = characterManager.CharacterCardBackgroundSprite;
			}
		}
		else
		{
			SetBaseStats(monsterCard, enemyActor, !generationProgressive);
			shuffleIcon.enabled = monsterCard.Shuffle;
			contentHolder.gameObject.SetActive(value: true);
			makeMergingCoroutine = CoroutineHelper.RunCoroutine(MakeMerging(monsterCard, enemyActor, generationProgressive));
		}
	}

	public void DeferGenerateCard(CMonsterAbilityCard monsterCard, CEnemyActor enemyActor)
	{
		if (monsterCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			m_DeferredMonsterCard = monsterCard;
			m_DeferredEnemyActor = enemyActor;
		}
	}

	public void AnimateAppearance(CMonsterAbilityCard monsterAbilityCard, CActor actor, float fadeTime, float moveTime, float scaleTime, float delay, Vector3 scaleAnimationOrigin, Vector3 moveDisplacement, LeanTweenType easeType, Action onCompleteCallback)
	{
		container.localScale = scaleAnimationOrigin;
		canvasGroup.alpha = 0f;
		container.anchoredPosition3D = moveDisplacement;
		monsterRoundCardUI.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: true);
		fadeAnimation = LeanTween.alphaCanvas(canvasGroup, 1f, fadeTime).setDelay(delay).setOnComplete((Action)delegate
		{
			fadeAnimation = null;
			if (fadeAnimation == null && scaleAnimation == null && moveAnimation == null)
			{
				onCompleteCallback();
			}
		});
		scaleAnimation = LeanTween.scale(container.gameObject, Vector3.one, scaleTime).setDelay(delay).setOnComplete((Action)delegate
		{
			scaleAnimation = null;
			if (fadeAnimation == null && scaleAnimation == null && moveAnimation == null)
			{
				onCompleteCallback();
			}
		});
		moveAnimation = LeanTween.move(container, Vector3.zero, moveTime).setEase(easeType).setDelay(delay)
			.setOnComplete((Action)delegate
			{
				moveAnimation = null;
				if (fadeAnimation == null && scaleAnimation == null && moveAnimation == null)
				{
					onCompleteCallback();
				}
			});
	}

	private void CleanUpHolder(Transform holder)
	{
		StopMakeMarking();
		foreach (Transform item in holder.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	private void StopMakeMarking()
	{
		if (makeMergingCoroutine != null)
		{
			CoroutineHelper.instance.StopCoroutine(makeMergingCoroutine);
		}
		makeMergingCoroutine = null;
	}

	private IEnumerator MakeMerging(CMonsterAbilityCard roundCard, CActor actor, bool ggenerationProgressive)
	{
		CleanUpHolder(contentHolder);
		if (ggenerationProgressive)
		{
			yield return null;
		}
		CEnemyActor enemyForBaseStats = actor as CEnemyActor;
		GameObject content = monsterRoundCardUI.MakeVersionWithBaseStats(enemyForBaseStats, roundCard.ID);
		if (ggenerationProgressive)
		{
			yield return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(content, contentHolder);
		obj.SetActive(value: true);
		LayoutRows.Clear();
		obj.GetComponentsInChildren(LayoutRows);
		foreach (LayoutRow row in LayoutRows)
		{
			if (row.Value == int.MaxValue || row.IsBuff || row.Entry == null || row.Entry.Name == "")
			{
				continue;
			}
			if (specialBaseStats.Keys.Any((CAbility.EAbilityType a) => a.ToString() == row.Entry.Name))
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				row.Value += specialBaseStats.Single((KeyValuePair<CAbility.EAbilityType, int> s) => s.Key.ToString() == row.Entry.Name).Value;
			}
			else if (row.Entry.Name == CAbility.EAbilityType.Attack.ToString())
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				row.Value += ((!row.FromControlAbility) ? baseAttackValue : 0);
				AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.Strength, roundCard);
			}
			else if (row.Entry.Name == CAbility.EAbilityType.Move.ToString())
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				row.Value += ((!row.FromControlAbility) ? baseMoveValue : 0);
				AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.Strength, roundCard);
			}
			else if (row.Entry.Name == CAbility.EAbilityStatType.Range.ToString())
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				CAbility cAbility = roundCard.Action.Abilities.SingleOrDefault((CAbility x) => x.AbilityType == row.AbilityType && x.Name == row.AbilityName);
				if (cAbility != null && !row.FromBaseStats)
				{
					row.Value += ((!cAbility.RangeIsBase) ? ((!row.FromControlAbility) ? baseRangeValue : 0) : 0);
					AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.Range, roundCard);
				}
				else if (row.Value < 1)
				{
					Debug.LogError("Unable to lookup range for ability " + row.AbilityName);
				}
			}
			else if (row.Entry.Name == CAbility.EAbilityStatType.NumberOfTargets.ToString())
			{
				row.mode = LayoutRow.EMode.GlossaryEntry;
				CAbility cAbility2 = roundCard.Action.Abilities.SingleOrDefault((CAbility x) => x.AbilityType == row.AbilityType && x.Name == row.AbilityName);
				if (cAbility2 != null && !row.FromBaseStats)
				{
					row.Value += ((!cAbility2.TargetIsBase) ? ((!row.FromControlAbility) ? baseTargetValue : 0) : 0);
					AddStatIsXValueIfApplicable(row, CAbility.EAbilityStatType.NumberOfTargets, roundCard);
				}
				else if (row.Value < 1)
				{
					Debug.LogError("Unable to lookup number of targets for ability " + row.AbilityName);
				}
			}
			row.UpdateText(row.Bold, row.PlusX);
		}
		makeMergingCoroutine = null;
	}

	private void AddStatIsXValueIfApplicable(LayoutRow row, CAbility.EAbilityStatType abilityStatType, CMonsterAbilityCard roundCard)
	{
		CAbility cAbility = null;
		cAbility = roundCard.Action.Abilities.SingleOrDefault((CAbility x) => x.Name == row.AbilityName);
		if (cAbility == null)
		{
			foreach (CAbilityControlActor item in roundCard.Action.Abilities.Where((CAbility x) => x.AbilityType == CAbility.EAbilityType.ControlActor))
			{
				cAbility = item.ControlActorData.ControlAbilities.SingleOrDefault((CAbility x) => x.Name == row.AbilityName);
				if (cAbility != null)
				{
					break;
				}
			}
		}
		if (cAbility != null)
		{
			AbilityData.StatIsBasedOnXData statIsBasedOnXData = cAbility.StatIsBasedOnXEntries.FirstOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == abilityStatType);
			if (statIsBasedOnXData != null)
			{
				row.Value += cAbility.GetStatIsBasedOnXValue(cAbility.TargetingActor, statIsBasedOnXData, cAbility.AbilityFilter);
			}
		}
	}

	private Color GetValueColour(int deltaValue)
	{
		if (deltaValue != 0)
		{
			if (deltaValue <= 0)
			{
				return new Color(1f, 0.5f, 0.5f);
			}
			return new Color(0.5f, 1f, 0.5f);
		}
		return Color.white;
	}

	public void FinishAnimateAppearance()
	{
		CancelLeanTweens();
		StopAllCoroutines();
		base.gameObject.SetActive(value: true);
		container.anchoredPosition3D = Vector3.zero;
		container.localScale = Vector3.one;
		canvasGroup.alpha = 1f;
		monsterRoundCardUI.gameObject.SetActive(value: false);
	}

	private void CancelLeanTweens()
	{
		if (fadeAnimation != null)
		{
			LeanTween.cancel(fadeAnimation.id, "FinishAnimateAppearance");
			fadeAnimation = null;
		}
		if (moveAnimation != null)
		{
			LeanTween.cancel(moveAnimation.id, "FinishAnimateAppearance");
			moveAnimation = null;
		}
		if (scaleAnimation != null)
		{
			LeanTween.cancel(scaleAnimation.id, "FinishAnimateAppearance");
			scaleAnimation = null;
		}
	}
}
