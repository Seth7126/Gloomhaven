using System;
using MapRuleLibrary.Adventure;

public class TempleMode : GuildmasterMode
{
	public override bool IsUnlocked
	{
		get
		{
			if (AdventureState.MapState.HeadquartersState.TempleUnlocked)
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

	public TempleMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.Temple, button, onEnter, onExit, EControllerInputAreaType.Temple, AWScreenName.guildmaster_temple)
	{
	}

	protected override bool CheckNewNotifications()
	{
		return false;
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
