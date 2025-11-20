using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Foundation;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPersonalQuestState : UIPersonalQuestProgress
{
	[SerializeField]
	protected RectTransform rewardContent;

	[SerializeField]
	protected List<UIQuestReward> rewardPool;

	[SerializeField]
	protected GameObject concealMask;

	[SerializeField]
	private List<Image> focusMasks;

	[SerializeField]
	private List<TextMeshProUGUI> focusTexts;

	[SerializeField]
	private List<GameObject> unhighlightMasks;

	[SerializeField]
	private Color unfocusedColorText;

	[SerializeField]
	private List<UIPersonalQuestStateInfo> extraInfo;

	[SerializeField]
	private UnityEvent OnHighlightEvent;

	[SerializeField]
	private UnityEvent OnUnhighlightEvent;

	private UIQuestReward focusedRewardSlot;

	private List<Color> defaultTextColors;

	private void Awake()
	{
		defaultTextColors = focusTexts.Select((TextMeshProUGUI it) => it.color).ToList();
	}

	public void OnHighlight()
	{
		OnHighlightEvent?.Invoke();
	}

	public void OnUnhighlight()
	{
		OnUnhighlightEvent?.Invoke();
	}

	public override void SetPersonalQuest(CPersonalQuestState personalQuest)
	{
		base.SetPersonalQuest(personalQuest);
		List<Reward> list = personalQuest.FinalRewards.SelectMany((RewardGroup it) => it.Rewards).ToList();
		HelperTools.NormalizePool(ref rewardPool, rewardPool[0].gameObject, rewardContent, list.Count);
		for (int num = 0; num < list.Count; num++)
		{
			UIQuestReward rewardSlot = rewardPool[num];
			rewardSlot.ShowReward(list[num], delegate(Reward _, bool hovered)
			{
				OnHoveredRewardSlot(rewardSlot, hovered);
			});
		}
		for (int num2 = 0; num2 < extraInfo.Count; num2++)
		{
			extraInfo[num2].SetPersonalQuest(personalQuest);
		}
		RefreshConceal(personalQuest.IsConcealed);
		SetHighlighted(isHilighted: false);
	}

	private void OnHoveredRewardSlot(UIQuestReward reward, bool hovered)
	{
	}

	public void RefreshConceal(bool isConcealed)
	{
		if (concealMask != null)
		{
			concealMask.SetActive(isConcealed);
		}
		for (int i = 0; i < rewardPool.Count && rewardPool[i].gameObject.activeSelf; i++)
		{
			rewardPool[i].ShowUnlocked(!isConcealed);
		}
		SetFocused(!isConcealed);
	}

	public void SetHighlighted(bool isHilighted)
	{
		foreach (GameObject unhighlightMask in unhighlightMasks)
		{
			unhighlightMask.SetActive(!isHilighted);
		}
	}

	public void SetFocused(bool isFocused)
	{
		foreach (Image focusMask in focusMasks)
		{
			focusMask.material = (isFocused ? null : UIInfoTools.Instance.greyedOutMaterial);
		}
		for (int i = 0; i < focusTexts.Count; i++)
		{
			focusTexts[i].color = (isFocused ? defaultTextColors[i] : unfocusedColorText);
		}
	}

	public void EnableNavigation()
	{
		List<UIQuestReward> list = (from it in rewardPool.TakeWhile((UIQuestReward it) => it.gameObject.activeSelf)
			where it.IsInteractable
			select it).ToList();
		StopAllCoroutines();
		if (list.Count > 0)
		{
			SetElementFocused(list[0]);
		}
	}

	public void DisableNavigation()
	{
		StopAllCoroutines();
		SetElementFocused(null);
	}

	private void SetElementFocused(UIQuestReward element)
	{
		if (!(focusedRewardSlot == element))
		{
			if (focusedRewardSlot != null)
			{
				ExecuteEvents.Execute(focusedRewardSlot.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
				focusedRewardSlot = null;
			}
			focusedRewardSlot = element;
			if (element != null)
			{
				ExecuteEvents.Execute(focusedRewardSlot.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
			}
		}
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
		}
	}
}
