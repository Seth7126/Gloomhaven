namespace ScenarioRuleLibrary;

public class CSpawnerSpawningUnit_MessageData : CMessageData
{
	public CTile m_SpawnTile;

	public CEnemyActor m_SpawnEnemy;

	public CSpawnerSpawningUnit_MessageData()
		: base(MessageType.SpawnerSpawningUnit, null)
	{
	}
}
