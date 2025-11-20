using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LevelEditorUnitPanel : MonoBehaviour
{
	public LevelEditorUnitsPanel.UnitType type;

	public GameObject UnitButton;

	public GameObject ParentPanel;

	public RectTransform Container;

	private void Awake()
	{
		switch (type)
		{
		case LevelEditorUnitsPanel.UnitType.Player:
		{
			foreach (CCharacterClass charClass in CharacterClassManager.Classes.OrderBy((CCharacterClass x) => x.ID))
			{
				CharacterConfigUI characterConfigUI = UIInfoTools.Instance.characterConfigsUI.FirstOrDefault((CharacterConfigUI c) => c.character == charClass.CharacterModel);
				if (characterConfigUI != null && (GlobalData.CompletedCharacters.Contains(charClass.CharacterID) || !characterConfigUI.hasToReveal))
				{
					AddCharacterButton(charClass.CharacterID, charClass.LocKey);
				}
			}
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Enemy:
		{
			foreach (CMonsterClass item in from x in MonsterClassManager.Classes
				where x.NonEliteVariant == null
				orderby x.ID
				select x)
			{
				if (AssetBundleManager.Instance.BundleConfigForPrefab(CActor.EType.Enemy, item.DefaultModel) != null)
				{
					AddMonsterButton(item.ID, item.LocKey);
				}
			}
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Objects:
		{
			foreach (CObjectClass objectClass in MonsterClassManager.ObjectClasses)
			{
				if (!(objectClass.ID == "PropDummyObject") && AssetBundleManager.Instance.BundleConfigForPrefab(CActor.EType.Enemy, objectClass.DefaultModel) != null)
				{
					AddMonsterButton(objectClass.ID, objectClass.LocKey);
				}
			}
			break;
		}
		case LevelEditorUnitsPanel.UnitType.Prop:
			foreach (EPropType item2 in CObjectProp.PropTypes.Where((EPropType x) => x != EPropType.None))
			{
				AddPropButton(item2.ToString());
			}
			{
				foreach (ESpecificPropType item3 in CObjectProp.SpecificPropTypes.Where((ESpecificPropType x) => x != ESpecificPropType.None))
				{
					AddPropButton(item3.ToString(), generic: false);
				}
				break;
			}
		case LevelEditorUnitsPanel.UnitType.Spawner:
			AddSpawnerButton("Spawner");
			AddSpawnerButton("SewerPipeSpawner");
			AddSpawnerButton("GraveSingleSpawner");
			AddSpawnerButton("GraveDoubleSpawner");
			break;
		}
	}

	private void AddMonsterButton(string unitName, string locKey)
	{
		Object.Instantiate(UnitButton, (Container == null) ? base.transform : Container).GetComponent<LevelEditorUnitButton>().SetUnitEnemy(unitName, locKey, ParentPanel.GetComponent<LevelEditorUnitsPanel>());
	}

	private void AddPropButton(string propType, bool generic = true)
	{
		int numberOfTilesRequired = 1;
		CAreaEffect areaEffect = null;
		switch (propType)
		{
		case "ThreeHexObstacle":
			areaEffect = CardProcessingShared.CreateArea("0,0,R|-1,1,R|1,1,R", "Editor", "Editor");
			break;
		case "ThreeHexCurvedObstacle":
			areaEffect = CardProcessingShared.CreateArea("0,0,R|-2,0,R|1,1,R", "Editor", "Editor");
			break;
		case "ThreeHexStraightObstacle":
			areaEffect = CardProcessingShared.CreateArea("0,0,R|-2,0,R|2,0,R", "Editor", "Editor");
			break;
		case "TwoHexObstacle":
			areaEffect = CardProcessingShared.CreateArea("0,0,R|0,1,R", "Editor", "Editor");
			break;
		}
		Object.Instantiate(UnitButton, (Container == null) ? base.transform : Container).GetComponent<LevelEditorUnitButton>().SetUnitApparanceProp(propType, ParentPanel.GetComponent<LevelEditorUnitsPanel>(), generic, numberOfTilesRequired, areaEffect);
	}

	private void AddSpawnerButton(string unitName)
	{
		LevelEditorUnitButton component = Object.Instantiate(UnitButton, (Container == null) ? base.transform : Container).GetComponent<LevelEditorUnitButton>();
		CAreaEffect areaEffect = null;
		if (unitName == "GraveDouble")
		{
			areaEffect = CardProcessingShared.CreateArea("0,0,R|0,1,R", "Editor", "Editor");
		}
		component.SetUnitSpawner(unitName, ParentPanel.GetComponent<LevelEditorUnitsPanel>(), 1, areaEffect);
	}

	private void AddSpecificPropButton(ESpecificPropType propType)
	{
	}

	private void AddCharacterButton(string characterID, string locKey)
	{
		Object.Instantiate(UnitButton, (Container == null) ? base.transform : Container).GetComponent<LevelEditorUnitButton>().SetUnitCharacter(characterID, locKey, ParentPanel.GetComponent<LevelEditorUnitsPanel>());
	}
}
