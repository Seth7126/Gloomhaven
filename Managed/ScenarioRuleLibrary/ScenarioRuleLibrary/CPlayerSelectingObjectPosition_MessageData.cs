using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CPlayerSelectingObjectPosition_MessageData : CMessageData
{
	public CAbility m_Ability;

	public ScenarioManager.ObjectImportType m_SpawnType;

	public List<CAbilityFilter.EFilterTile> m_TileFilter;

	public CPlayerSelectingObjectPosition_MessageData(CActor actorSpawningMessage)
		: base(MessageType.PlayerSelectingObjectPosition, actorSpawningMessage)
	{
	}
}
