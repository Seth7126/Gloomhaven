using System;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;

public class MerchantMode : GuildmasterMode
{
	public override bool IsUnlocked
	{
		get
		{
			if (AdventureState.MapState.HeadquartersState.MerchantUnlocked)
			{
				if (MapFTUEManager.IsPlaying)
				{
					return Singleton<MapFTUEManager>.Instance.HasCompletedStep(EMapFTUEStep.CreatedSecondCharacter);
				}
				return true;
			}
			return false;
		}
	}

	public MerchantMode(UIGuildmasterButton button, Action onEnter, Action onExit)
		: base(EGuildmasterMode.Merchant, button, onEnter, onExit, EControllerInputAreaType.Shop, AWScreenName.guildmaster_merchant)
	{
	}

	protected override bool CheckNewNotifications()
	{
		return AdventureState.MapState.HeadquartersState.CheckMerchantStock.Exists((CItem it) => it.IsNew && it.YMLData.Slot != CItem.EItemSlot.QuestItem);
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
