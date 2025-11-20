using System;
using MapRuleLibrary.Adventure;

public class WorldMapMode : GuildmasterMode
{
	public override bool IsUnlocked
	{
		get
		{
			if (!AdventureState.MapState.IsCampaign)
			{
				return AdventureState.MapState.HeadquartersState.TrainerUnlocked;
			}
			return true;
		}
	}

	public WorldMapMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.WorldMap, button, onEnter, onExit, EControllerInputAreaType.WorldMap, AWScreenName.destination_selection_map)
	{
	}

	public override void Enter()
	{
		base.Enter();
		Singleton<MapChoreographer>.Instance.OpenWorldMap(transition: false);
	}

	protected override bool CheckNewNotifications()
	{
		return false;
	}

	public override void RefreshUnlocked()
	{
		base.RefreshUnlocked();
		if (MapFTUEManager.IsPlaying && !Singleton<MapFTUEManager>.Instance.HasCompletedStep(EMapFTUEStep.VisitMerchant))
		{
			button.Hide(Singleton<MapFTUEManager>.Instance);
		}
		else
		{
			button.Show(Singleton<MapFTUEManager>.Instance);
		}
	}
}
