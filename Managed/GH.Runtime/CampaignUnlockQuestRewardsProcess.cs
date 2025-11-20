using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary.YML;

public class CampaignUnlockQuestRewardsProcess : CampaignExtraRewardsProcess
{
	public override ICallbackPromise Process(List<Reward> rewards)
	{
		if (rewards.Count == 0 || Singleton<MapChoreographer>.Instance.MovingToLocation != null)
		{
			return CallbackPromise.Resolved();
		}
		List<MapLocation> list = (from it in (from it in rewards
				where it.Type == ETreasureType.UnlockQuest
				select it.UnlockName).Concat(AdventureState.MapState.QueuedUnlockedQuestIDs).Distinct()
			select Singleton<MapChoreographer>.Instance.GetMapLocationByQuest(it)).ToList();
		if (list.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		CallbackPromise callbackPromise = new CallbackPromise();
		Singleton<UIUnlockLocationFlowManager>.Instance.ShowUnlockedLocations(list, callbackPromise.Resolve);
		return callbackPromise;
	}
}
