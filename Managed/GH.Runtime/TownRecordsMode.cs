using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;

public class TownRecordsMode : GuildmasterMode
{
	private HashSet<object> disabledRequests;

	public override bool IsUnlocked
	{
		get
		{
			if (AdventureState.MapState.IsCampaign && AdventureState.MapState.MapParty.RetiredCharacterRecords.Count > 0 && AdventureState.MapState.MapParty.Achievements.Exists((CPartyAchievement it) => it.State == EAchievementState.Completed && it.Achievement.AchievementType == EAchievementType.TownRecord))
			{
				if (FFSNetwork.IsOnline && !FFSNetwork.IsHost)
				{
					return Singleton<UIMapMultiplayerController>.Instance.IsReadyTownRecords;
				}
				return true;
			}
			return false;
		}
	}

	public TownRecordsMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.TownRecords, button, onEnter, onExit, EControllerInputAreaType.TownRecords, AWScreenName.town_records, delegate
		{
			if (FFSNetwork.IsOnline)
			{
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.None);
				Singleton<UIGuildmasterHUD>.Instance.UpdateCurrentMode(EGuildmasterMode.WorldMap);
				button.Deselect();
				Singleton<UIMapMultiplayerController>.Instance.OnSelectedTownRecords();
				return false;
			}
			return true;
		})
	{
		disabledRequests = new HashSet<object>();
	}

	protected override bool CheckNewNotifications()
	{
		return true;
	}

	public override void Enter()
	{
		Singleton<MapChoreographer>.Instance.RequestPauseActionProgression(this);
		AdventureState.MapState.CheckNonTrophyAchievements();
		NewPartyDisplayUI.PartyDisplay.Hide(this, instant: true);
		base.Enter();
	}

	public override void Exit()
	{
		Singleton<MapChoreographer>.Instance.RequestResumeActionProgression(this);
		NewPartyDisplayUI.PartyDisplay.Show(this);
		base.Exit();
		RefreshUnlocked();
	}

	public override void RefreshUnlocked()
	{
		base.RefreshUnlocked();
		RefreshDisabled();
		button.Highlight(button.gameObject.activeSelf && FFSNetwork.IsOnline && FFSNetwork.IsClient);
	}

	private void RefreshDisabled()
	{
		if (Singleton<MapChoreographer>.Instance.QueuedRetirements.Count > 0 || AdventureState.MapState.MapParty.ExistsCharacterToRetire())
		{
			button.ToggleGreyOut(greyedOut: true, LocalizationManager.GetTranslation("GUI_TOWN_RECORDS_BLOCKED"));
		}
		else if (FFSNetwork.IsOnline && (!PlayerRegistry.AllPlayers.All((NetworkPlayer x) => x.IsParticipant) || (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers != null && PlayerRegistry.JoiningPlayers.Count > 0) || (FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining)))
		{
			button.ToggleGreyOut();
		}
		else if (disabledRequests.Any())
		{
			button.ToggleGreyOut();
		}
		else
		{
			button.ToggleGreyOut(greyedOut: false);
		}
	}

	public void RequestEnable(object request)
	{
		disabledRequests.Remove(request);
		RefreshDisabled();
	}

	public void RequestDisable(object request)
	{
		disabledRequests.Add(request);
		RefreshDisabled();
	}
}
