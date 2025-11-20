using System;
using MapRuleLibrary.Adventure;

public class EnchantressMode : GuildmasterMode
{
	public override bool IsUnlocked
	{
		get
		{
			if (AdventureState.MapState.HeadquartersState.EnhancerUnlocked)
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

	public EnchantressMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.Enchantress, button, onEnter, onExit, EControllerInputAreaType.Enchantress, AWScreenName.guildmaster_enchanter)
	{
	}

	protected override bool CheckNewNotifications()
	{
		return AdventureState.MapState.HeadquartersState.EnhancerHasNewStock;
	}

	public override void Enter()
	{
		base.Enter();
		RefreshNotifications();
	}

	public override void Exit()
	{
		base.Exit();
		if (!FFSNetwork.IsOnline || !Singleton<UIMapMultiplayerController>.Instance.IsReadyTownRecords)
		{
			AdventureState.MapState.CheckPersonalQuests();
		}
	}
}
