using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using GLOO.Introduction;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class UIIntroductionRewardsProcess : MonoBehaviour
{
	[Serializable]
	private class RewardIntroPair
	{
		public ETreasureType rewardType;

		public EIntroductionConcept introConcept;
	}

	[SerializeField]
	private UIIntroduceProcess process;

	[SerializeField]
	private List<RewardIntroPair> rewardsIntroductions;

	public ICallbackPromise Process(List<Reward> rewards)
	{
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		if (rewards.Exists((Reward it) => it.Item != null && !it.Item.YMLData.ValidEquipCharacterClassIDs.IsNullOrEmpty()))
		{
			callbackPromise = callbackPromise.Then(() => Process(EIntroductionConcept.PersonalItem));
		}
		foreach (ETreasureType rewardType in rewards.Select((Reward it) => it.Type).Distinct())
		{
			callbackPromise = callbackPromise.Then(() => Process(rewardType));
		}
		return callbackPromise;
	}

	public ICallbackPromise Process(Reward reward)
	{
		return Process(reward.Type);
	}

	private ICallbackPromise Process(ETreasureType rewardType)
	{
		RewardIntroPair rewardIntroPair = rewardsIntroductions.FirstOrDefault((RewardIntroPair it) => rewardType == it.rewardType);
		if (rewardIntroPair != null)
		{
			return Process(rewardIntroPair.introConcept);
		}
		return CallbackPromise.Resolved();
	}

	private ICallbackPromise Process(EIntroductionConcept concept)
	{
		if (!AdventureState.MapState.MapParty.HasIntroduced(concept.ToString()))
		{
			AdventureState.MapState.MapParty.MarkIntroDone(concept.ToString());
			return process.Process(UIInfoTools.Instance.GetIntroductionConfig(concept));
		}
		return CallbackPromise.Resolved();
	}
}
