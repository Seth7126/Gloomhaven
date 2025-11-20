using System;
using MapRuleLibrary.Adventure;

public class CityMapMode : GuildmasterMode
{
	public override bool IsUnlocked
	{
		get
		{
			if (AdventureState.MapState.IsCampaign)
			{
				if (MapFTUEManager.IsPlaying)
				{
					return Singleton<MapFTUEManager>.Instance.HasCompletedStep(EMapFTUEStep.BuyItem);
				}
				return true;
			}
			return false;
		}
	}

	public CityMapMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.City, button, onEnter, onExit, EControllerInputAreaType.WorldMap, AWScreenName.destination_selection_map)
	{
		button.gameObject.SetActive(AdventureState.MapState.IsCampaign);
	}

	public override void Enter()
	{
		base.Enter();
		Singleton<MapChoreographer>.Instance.OpenCityMap(transition: false);
	}

	protected override bool CheckNewNotifications()
	{
		return false;
	}
}
