using System.Collections;
using System.Linq;
using Chronos;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary.YML;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.UI;

public class UIScenarioBattleGoalResult : MonoBehaviour
{
	[SerializeField]
	private Image characterMaker;

	[SerializeField]
	protected TextLocalizedListener title;

	[SerializeField]
	protected TextLocalizedListener description;

	[SerializeField]
	protected UIProgressBar progressBar;

	[SerializeField]
	protected GameObject resultHolder;

	[Header("Complete")]
	[SerializeField]
	protected UIReward rewardUI;

	[SerializeField]
	protected float delayToShowUnlockedPerk;

	[Header("Fail")]
	[SerializeField]
	private string failedAudioItem;

	[SerializeField]
	private GUIAnimator failedAnimator;

	private bool hasAchieved;

	private CBattleGoalState battleGoal;

	private CMapCharacter mapCharacter;

	private void Awake()
	{
		progressBar.OnFinishedProgress.AddListener(ShowReward);
	}

	public void SetBattleGoal(CBattleGoalState battleGoal, CMapCharacter character, bool hasAchieved)
	{
		this.hasAchieved = hasAchieved;
		this.battleGoal = battleGoal;
		mapCharacter = character;
		characterMaker.sprite = UIInfoTools.Instance.GetCharacterMarker(character.CharacterYMLData);
		title.SetTextKey(battleGoal.BattleGoal.LocalisedName);
		description.SetTextKey(battleGoal.BattleGoal.LocalisedDescription);
		progressBar.SetAmount(battleGoal.BattleGoalConditionState.TotalConditionsAndTargets, battleGoal.BattleGoalConditionState.CurrentProgress);
		resultHolder.SetActive(value: false);
		rewardUI.gameObject.SetActive(value: false);
		failedAnimator.Stop();
		failedAnimator.GoInitState();
		if (base.gameObject.activeInHierarchy)
		{
			progressBar.PlayFromProgress(0);
		}
	}

	private void OnEnable()
	{
		if (battleGoal != null)
		{
			progressBar.PlayFromProgress(0);
		}
	}

	private void ShowReward()
	{
		bool flag = battleGoal.BattleGoalConditionState.TotalConditionsAndTargets <= battleGoal.BattleGoalConditionState.CurrentProgress && !battleGoal.BattleGoalConditionState.Failed;
		if (battleGoal.BattleGoalConditionState.NegativeCondition)
		{
			flag = battleGoal.BattleGoalConditionState.TotalConditionsAndTargets >= battleGoal.BattleGoalConditionState.CurrentProgress && !battleGoal.BattleGoalConditionState.Failed;
			progressBar.ShowCompleted(flag);
		}
		if (hasAchieved && flag)
		{
			SimpleLog.AddToSimpleLog("Battle Goal " + battleGoal.ID + " succeeded. Target=" + battleGoal.BattleGoalConditionState.TotalConditionsAndTargets + ", progress=" + battleGoal.BattleGoalConditionState.CurrentProgress);
			int num = (from it in battleGoal.Rewards.SelectMany((RewardGroup it) => it.Rewards)
				where it.Type == ETreasureType.PerkCheck
				select it).Sum((Reward it) => it.Amount);
			if (num > 0)
			{
				rewardUI.ShowReward(new Reward(ETreasureType.PerkCheck, num, ETreasureDistributionType.None, null));
				rewardUI.gameObject.SetActive(value: true);
				resultHolder.SetActive(value: true);
				if (mapCharacter.UnlockedPerkPoints > 0)
				{
					StartCoroutine(ShowUnlockedPerks(mapCharacter.UnlockedPerkPoints));
				}
			}
		}
		else
		{
			SimpleLog.AddToSimpleLog("Battle Goal " + battleGoal.ID + " failed. Target=" + battleGoal.BattleGoalConditionState.TotalConditionsAndTargets + ", progress=" + battleGoal.BattleGoalConditionState.CurrentProgress);
			AudioControllerUtils.PlaySound(failedAudioItem);
			resultHolder.SetActive(!hasAchieved);
			failedAnimator.Play();
		}
	}

	private IEnumerator ShowUnlockedPerks(int amount)
	{
		yield return Timekeeper.instance.WaitForSeconds(delayToShowUnlockedPerk);
		rewardUI.ShowReward(new Reward(ETreasureType.PerkPoint, amount, ETreasureDistributionType.None, null));
	}

	private void OnDisable()
	{
		failedAnimator.Stop();
	}
}
