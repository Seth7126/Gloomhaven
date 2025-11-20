using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAttackModifierCalculator : MonoBehaviour
{
	[SerializeField]
	private TMP_Text attackModText;

	[SerializeField]
	private Image attackModImage;

	[SerializeField]
	private Color defaultTextColor;

	[SerializeField]
	private Transform container;

	[SerializeField]
	private GameObject unfocusedMask;

	private List<Image> attackModCounters = new List<Image>();

	public void SetupCounters(string counterID, CActor actor = null, CCharacterClass charClass = null, bool isFocused = true)
	{
		int num = 0;
		int num2 = 0;
		if (charClass == null)
		{
			if (actor == null)
			{
				num = MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
				num2 = MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			}
			else if (actor is CEnemyActor cEnemyActor)
			{
				if (cEnemyActor.MonsterClass.Boss)
				{
					num = MonsterClassManager.BossMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
					num2 = MonsterClassManager.BossMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
				}
				else
				{
					switch (cEnemyActor.OriginalType)
					{
					case CActor.EType.Enemy:
						num = MonsterClassManager.EnemyMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						num2 = MonsterClassManager.EnemyMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						break;
					case CActor.EType.Ally:
						num = MonsterClassManager.AlliedMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						num2 = MonsterClassManager.AlliedMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						break;
					case CActor.EType.Enemy2:
						num = MonsterClassManager.Enemy2MonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						num2 = MonsterClassManager.Enemy2MonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						break;
					case CActor.EType.Neutral:
						num = MonsterClassManager.NeutralMonsterAttackModifierDeck.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						num2 = MonsterClassManager.NeutralMonsterAttackModifierDeck.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
						break;
					}
				}
			}
			else
			{
				Debug.LogError("Tried to setup counters for non-enemy actor without a character class");
			}
		}
		else
		{
			num2 = charClass.DiscardedAttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
			num = charClass.AttackModifierCards.Count((AttackModifierYMLData x) => IsBasicModifier(x, counterID));
		}
		for (int num3 = 0; num3 < num + num2; num3++)
		{
			if (attackModCounters.Count <= num3)
			{
				attackModCounters.Add(Object.Instantiate(Singleton<ActorStatPanel>.Instance.CounterPrefab, (container == null) ? base.transform : container, worldPositionStays: false).GetComponent<Image>());
			}
			else
			{
				attackModCounters[num3].gameObject.SetActive(value: true);
			}
		}
		for (int num4 = num + num2; num4 < attackModCounters.Count; num4++)
		{
			attackModCounters[num4].gameObject.SetActive(value: false);
		}
		int num5 = num - 1;
		if (num == 0)
		{
			if (attackModText != null)
			{
				attackModText.color = UIInfoTools.Instance.greyedOutTextColor;
			}
			if (attackModImage != null)
			{
				attackModImage.color = UIInfoTools.Instance.greyedOutTextColor;
			}
			for (int num6 = 0; num6 < attackModCounters.Count; num6++)
			{
				attackModCounters[num6].sprite = Singleton<ActorStatPanel>.Instance.UsedCounter;
			}
		}
		else
		{
			if (attackModText != null)
			{
				attackModText.color = defaultTextColor;
			}
			if (attackModImage != null)
			{
				attackModImage.color = defaultTextColor;
			}
			for (int num7 = 0; num7 < attackModCounters.Count; num7++)
			{
				if (num7 <= num5)
				{
					attackModCounters[num7].sprite = Singleton<ActorStatPanel>.Instance.AvailableCounter;
				}
				else
				{
					attackModCounters[num7].sprite = Singleton<ActorStatPanel>.Instance.UsedCounter;
				}
			}
		}
		SetUnfocused(!isFocused);
	}

	public void SetUnfocused(bool unfocused)
	{
		unfocusedMask.SetActive(unfocused);
	}

	private bool IsBasicModifier(AttackModifierYMLData modif, string counterId)
	{
		if (modif.MathModifier.Equals(counterId) && !modif.IsConditionalModifier && !modif.IsBless)
		{
			return !modif.IsCurse;
		}
		return false;
	}
}
