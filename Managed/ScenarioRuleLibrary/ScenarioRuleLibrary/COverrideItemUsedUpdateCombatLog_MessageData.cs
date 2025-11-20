using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class COverrideItemUsedUpdateCombatLog_MessageData : CMessageData
{
	public List<CItem> m_ItemsToUpdate = new List<CItem>();

	public COverrideItemUsedUpdateCombatLog_MessageData(CActor actorSpawningMessage)
		: base(MessageType.OverrideItemUsedUpdateCombatLog, actorSpawningMessage)
	{
	}
}
