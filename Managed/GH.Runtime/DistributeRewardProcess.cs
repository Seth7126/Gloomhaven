using ScenarioRuleLibrary.YML;

public abstract class DistributeRewardProcess : CampaignExtraRewardsProcess
{
	public enum EDistributeRewardProcessType
	{
		NotSet,
		DistributeGold,
		DistributeItems,
		DistributeModifiers,
		DistributeConditions,
		DistributeGoldBag
	}

	public abstract bool IsRewardToProcess(Reward reward);

	public abstract EDistributeRewardProcessType GetRewardType();

	public abstract void ProxyConfirmClick();

	public abstract void ProxyAddPoint(IDistributePointsActor actor);

	public abstract void ProxyRemovePoint(IDistributePointsActor actor);

	public abstract IDistributePointsActor GetActor(int modelInstanceID);
}
