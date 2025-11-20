using System;
using System.Collections.Generic;
using Assets.Script.GUI.Quest;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestPreviewPopup : MonoBehaviour
{
	[SerializeField]
	private UIQuestDescription information;

	[SerializeField]
	private UIFollowMapLocationInsideArea followTarget;

	[SerializeField]
	private CanvasGroup popupCanvasGroup;

	[SerializeField]
	private float unfocusedOpacity = 0.3f;

	[SerializeField]
	private Image questIcon;

	[Header("Enemies")]
	[SerializeField]
	private UIQuestEnemy enemyPrefab;

	[SerializeField]
	private List<UIQuestEnemy> enemiesUI;

	[SerializeField]
	private List<GameObject> enemyMasksToCompleteRow;

	[SerializeField]
	private RectTransform enemiesContainer;

	[Header("Rewards")]
	[SerializeField]
	private RectTransform rewardsSection;

	[SerializeField]
	private UIQuestRewardPreview rewardPrefab;

	[SerializeField]
	private List<UIQuestRewardPreview> rewardsUI;

	[SerializeField]
	private List<GameObject> rewardMasksToCompleteRow;

	[SerializeField]
	private GameObject treasureCheck;

	private UIWindow window;

	private void Awake()
	{
		if (followTarget == null)
		{
			followTarget = base.gameObject.AddComponent<UIFollowMapLocationInsideArea>();
		}
		followTarget.enabled = false;
		window = GetComponent<UIWindow>();
		window.onHidden.AddListener(delegate
		{
			followTarget.enabled = false;
		});
	}

	public void ShowQuest(IQuest questState, Transform location, Vector3 offset, bool unfocused = false)
	{
		SetQuest(questState);
		Show(location, offset, unfocused);
	}

	public void SetQuest(IQuest questState)
	{
		information.Setup(questState);
		questIcon.sprite = questState.Icon;
		SetupEnemies(questState.Enemies);
		SetupRewards(questState.Rewards, questState.Treasures > 0);
	}

	private void SetupRewards(List<Reward> rewards, bool hasTreasures)
	{
		if ((rewards == null || rewards.Count == 0) && !hasTreasures)
		{
			rewardsSection.gameObject.SetActive(value: false);
			return;
		}
		int num = rewards?.Count ?? 0;
		HelperTools.NormalizePool(ref rewardsUI, rewardPrefab.gameObject, rewardsSection, num);
		for (int i = 0; i < num; i++)
		{
			rewardsUI[i].ShowReward(rewards[i]);
		}
		if (hasTreasures)
		{
			num++;
			treasureCheck.SetActive(value: true);
		}
		else
		{
			treasureCheck.SetActive(value: false);
		}
		int num2 = Math.Max(0, rewardMasksToCompleteRow.Count - num);
		for (int j = 0; j < num2; j++)
		{
			rewardMasksToCompleteRow[j].transform.SetAsLastSibling();
			rewardMasksToCompleteRow[j].SetActive(value: true);
		}
		for (int k = num2; k < rewardMasksToCompleteRow.Count; k++)
		{
			rewardMasksToCompleteRow[k].SetActive(value: false);
		}
		rewardsSection.gameObject.SetActive(value: true);
	}

	public void SetupEnemies(List<string> enemyClasses)
	{
		if (enemyClasses == null || enemyClasses.Count == 0)
		{
			enemiesContainer.gameObject.SetActive(value: false);
			return;
		}
		HelperTools.NormalizePool(ref enemiesUI, enemyPrefab.gameObject, enemiesContainer, enemyClasses.Count);
		for (int i = 0; i < enemyClasses.Count; i++)
		{
			enemiesUI[i].ShowEnemy(enemyClasses[i], AdventureState.MapState.MapParty.ScenarioLevel);
		}
		int num = Math.Max(enemyMasksToCompleteRow.Count - enemyClasses.Count, 0);
		for (int j = 0; j < num; j++)
		{
			enemyMasksToCompleteRow[j].transform.SetAsLastSibling();
			enemyMasksToCompleteRow[j].SetActive(value: true);
		}
		for (int k = num; k < enemyMasksToCompleteRow.Count; k++)
		{
			enemyMasksToCompleteRow[k].SetActive(value: false);
		}
		enemiesContainer.gameObject.SetActive(value: true);
	}

	public void Show(bool unfocused = false)
	{
		Show(null, Vector3.zero, unfocused);
	}

	public void Show(Transform location, Vector3 offset, bool unfocused = false)
	{
		window.Show();
		Focus(unfocused);
		if (followTarget != null)
		{
			if (location != null)
			{
				followTarget.Track(location, offset);
				followTarget.enabled = true;
			}
			else
			{
				followTarget.enabled = false;
			}
		}
	}

	public void RefreshLocked()
	{
		information.RefreshLocked();
	}

	public void Hide(bool instant = false)
	{
		if (followTarget != null)
		{
			followTarget.enabled = false;
		}
		if (window != null)
		{
			window.Hide(instant);
		}
	}

	public void Focus(bool focus)
	{
		if (window.IsOpen)
		{
			popupCanvasGroup.alpha = (focus ? 1f : unfocusedOpacity);
		}
	}
}
