using System;
using MapRuleLibrary.Adventure;

public class MercenaryLogMode : GuildmasterMode
{
	private bool hasShownNotification;

	public override bool IsUnlocked
	{
		get
		{
			if (AdventureState.MapState.IsCampaign)
			{
				return AdventureState.MapState.MapParty.RetiredCharacterRecords.Count > 0;
			}
			return false;
		}
	}

	public MercenaryLogMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.MercenaryLog, button, onEnter, onExit, EControllerInputAreaType.TownRecords, AWScreenName.town_records)
	{
		hasShownNotification = AdventureState.MapState.HeadquartersState.HasShownIntroTownRecords;
	}

	protected override bool CheckNewNotifications()
	{
		return false;
	}

	public override void Enter()
	{
		NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
		AdventureState.MapState.CheckNonTrophyAchievements();
		if (!hasShownNotification && AdventureState.MapState.HeadquartersState.HasShownIntroTownRecords)
		{
			hasShownNotification = true;
			UIMapNotifications.ShowTownUnlocked();
		}
		base.Enter();
	}

	public override void Exit()
	{
		NewPartyDisplayUI.PartyDisplay.Show(this);
		base.Exit();
	}
}
