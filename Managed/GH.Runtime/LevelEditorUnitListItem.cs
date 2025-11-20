using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorUnitListItem : MonoBehaviour
{
	public LevelEditorUnitsPanel.UnitType type;

	public TextMeshProUGUI text;

	public Button button;

	[HideInInspector]
	public CPlayerActor playerObj;

	[HideInInspector]
	public CEnemyActor enemyObj;

	[HideInInspector]
	public CObjectActor objectObj;

	[HideInInspector]
	public CObjectProp propObj;

	[HideInInspector]
	public CSpawner spawnerObj;

	public void Initialise(LevelEditorUnitsPanel.UnitType typeToSet, string ID, string locKey, CClientTile tilePlacedIn)
	{
		type = typeToSet;
		switch (type)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
			playerObj = ScenarioManager.Scenario.FindPlayerAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			text.text = LocalizationManager.GetTranslation(playerObj.ActorLocKey());
			break;
		case LevelEditorUnitsPanel.UnitType.Enemy:
			enemyObj = ScenarioManager.Scenario.FindEnemyAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			if (enemyObj == null)
			{
				enemyObj = ScenarioManager.Scenario.FindAllyMonsterAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			}
			if (enemyObj == null)
			{
				enemyObj = ScenarioManager.Scenario.FindNeutralMonsterAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			}
			if (enemyObj == null)
			{
				enemyObj = ScenarioManager.Scenario.FindEnemy2MonsterAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			}
			text.text = LocalizationManager.GetTranslation(enemyObj.ActorLocKey());
			break;
		case LevelEditorUnitsPanel.UnitType.Objects:
			objectObj = ScenarioManager.Scenario.FindObjectActorAt(tilePlacedIn.m_Tile.m_ArrayIndex);
			text.text = LocalizationManager.GetTranslation(objectObj.ActorLocKey());
			break;
		case LevelEditorUnitsPanel.UnitType.Prop:
		{
			ScenarioManager.ObjectImportType objectImportType = GlobalSettings.GetObjectImportType(ID);
			propObj = tilePlacedIn.m_Tile.FindProp(objectImportType);
			text.text = locKey;
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Spawner:
			spawnerObj = tilePlacedIn.m_Tile.FindSpawner(ID);
			text.text = locKey;
			break;
		}
	}

	public void SetValuesFromObject(CPlayerActor playerActor)
	{
		text.text = LocalizationManager.GetTranslation(playerActor.ActorLocKey());
		playerObj = playerActor;
	}

	public void SetValuesFromObject(CEnemyActor enemyActor)
	{
		text.text = LocalizationManager.GetTranslation(enemyActor.ActorLocKey());
		enemyObj = enemyActor;
	}

	public void SetValuesFromObject(CObjectActor objectActor)
	{
		text.text = LocalizationManager.GetTranslation(objectActor.ActorLocKey());
		objectObj = objectActor;
	}

	public void SetValuesFromObject(CObjectProp prop)
	{
		propObj = prop;
		text.text = prop.ObjectType.ToString();
	}

	public void SetValuesFromObject(CSpawner prop)
	{
		spawnerObj = prop;
		text.text = spawnerObj.SpawnerGuid;
	}
}
