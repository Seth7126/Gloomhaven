using System;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;

public class TrainerMode : GuildmasterMode
{
	public override bool IsUnlocked => AdventureState.MapState.HeadquartersState.TrainerUnlocked;

	public TrainerMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.Trainer, button, onEnter, onExit, EControllerInputAreaType.Trainer, AWScreenName.guildmaster_trainer)
	{
		RefreshButtonHighlight();
	}

	public override void Enter()
	{
		button.Highlight(enable: false);
		base.Enter();
	}

	public override void Exit()
	{
		RefreshButtonHighlight();
		base.Exit();
	}

	public override void RefreshUnlocked()
	{
		base.RefreshUnlocked();
		RefreshButtonHighlight();
	}

	private void RefreshButtonHighlight()
	{
		button.Highlight(AdventureState.MapState.MapParty.Achievements.Exists((CPartyAchievement it) => it.State == EAchievementState.Completed && it.Achievement.AchievementType != EAchievementType.Trophy));
	}

	protected override bool CheckNewNotifications()
	{
		return AdventureState.MapState.MapParty.Achievements.Exists((CPartyAchievement it) => it.Achievement.AchievementType != EAchievementType.Trophy && (it.State == EAchievementState.Completed || (it.State == EAchievementState.Unlocked && it.IsNew)));
	}
}
