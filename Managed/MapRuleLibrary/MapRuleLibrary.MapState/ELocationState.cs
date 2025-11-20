using System;

namespace MapRuleLibrary.MapState;

[Serializable]
public enum ELocationState
{
	None,
	Locked,
	Unlocked,
	Completed
}
