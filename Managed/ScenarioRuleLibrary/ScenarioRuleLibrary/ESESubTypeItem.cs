using System;

namespace ScenarioRuleLibrary;

[Serializable]
public enum ESESubTypeItem
{
	None,
	ItemAdded,
	ItemSpent,
	ItemConsumed,
	ItemUnrestrictedUsed,
	ItemRefreshed,
	ItemSelected,
	ItemDeSelected,
	ItemUsable,
	ItemNoLongerUsable,
	ItemActive
}
