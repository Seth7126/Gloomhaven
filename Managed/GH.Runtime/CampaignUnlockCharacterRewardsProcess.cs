using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public class CampaignUnlockCharacterRewardsProcess : CampaignExtraRewardsProcess
{
	public override ICallbackPromise Process(List<Reward> rewards)
	{
		if (rewards.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		List<string> list = (from it in rewards
			where it.Type == ETreasureType.UnlockCharacter
			select it.CharacterID).Distinct().ToList();
		if (list.Count == 0)
		{
			return CallbackPromise.Resolved();
		}
		ICallbackPromise callbackPromise = CallbackPromise.Resolved();
		for (int num = 0; num < list.Count; num++)
		{
			ECharacter characterClass = CharacterClassManager.Find(list[num]).CharacterModel;
			bool isLast = num == list.Count - 1;
			callbackPromise = callbackPromise.Then(() => ShowUnlockCharacter(characterClass, isLast));
		}
		return callbackPromise;
	}

	private ICallbackPromise ShowUnlockCharacter(ECharacter characterClass, bool isLast)
	{
		CallbackPromise playPromise = new CallbackPromise();
		if (!VideoCamera.s_This.PlayFullscreenVideo("Heroes/" + characterClass, delegate
		{
			InputManager.RequestEnableInput(this, EKeyActionTag.All);
			playPromise.Resolve();
		}, isLast))
		{
			playPromise.Resolve();
		}
		else
		{
			InputManager.RequestDisableInput(this, EKeyActionTag.All);
		}
		return playPromise;
	}

	private void OnDisable()
	{
		InputManager.RequestEnableInput(this, EKeyActionTag.All);
	}
}
