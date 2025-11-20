using System;

namespace MapRuleLibrary.Party;

[Serializable]
public enum EAchievementState
{
	None,
	Locked,
	Unlocked,
	Completed,
	RewardsClaimed
}
