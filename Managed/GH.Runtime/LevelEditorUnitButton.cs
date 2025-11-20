using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class LevelEditorUnitButton : MonoBehaviour
{
	public enum UnitType
	{
		None,
		Monster,
		Character,
		ApparanceProp,
		SpecificProp,
		Spawner
	}

	private string m_UnitID;

	private UnitType m_UnitType;

	private LevelEditorUnitsPanel m_UnitsPanel;

	private int m_NumberOfTilesRequired;

	private CAreaEffect m_AreaEffect;

	public Image unitImage;

	public Text unitText;

	public void SetUnitEnemy(string unitID, string locKey, LevelEditorUnitsPanel unitsPanel, int numberOfTilesRequired = 1)
	{
		m_UnitID = unitID;
		m_UnitType = UnitType.Monster;
		unitText.text = $"<i>{unitID}</i>\n<b>{LocalizationManager.GetTranslation(locKey)}</b>";
		m_UnitsPanel = unitsPanel;
		m_NumberOfTilesRequired = numberOfTilesRequired;
	}

	public void SetUnitCharacter(string characterID, string locKey, LevelEditorUnitsPanel unitsPanel, int numberOfTilesRequired = 1)
	{
		m_UnitID = characterID;
		m_UnitType = UnitType.Character;
		unitText.text = $"<i>{characterID}</i>\n<b>{LocalizationManager.GetTranslation(locKey)}</b>";
		m_UnitsPanel = unitsPanel;
		m_NumberOfTilesRequired = numberOfTilesRequired;
	}

	public void SetUnitSpawner(string spawnerName, LevelEditorUnitsPanel unitsPanel, int numberOfTilesRequired = 1, CAreaEffect areaEffect = null)
	{
		m_UnitID = spawnerName;
		m_UnitType = UnitType.Spawner;
		unitText.text = m_UnitID;
		m_UnitsPanel = unitsPanel;
		m_NumberOfTilesRequired = numberOfTilesRequired;
		m_AreaEffect = areaEffect;
	}

	public void SetUnitApparanceProp(string unitName, LevelEditorUnitsPanel unitsPanel, bool generic, int numberOfTilesRequired = 1, CAreaEffect areaEffect = null)
	{
		m_UnitID = unitName;
		m_UnitType = UnitType.ApparanceProp;
		unitText.text = (generic ? "GENERIC\n" : "SPECIFIC\n") + m_UnitID;
		m_UnitsPanel = unitsPanel;
		m_NumberOfTilesRequired = numberOfTilesRequired;
		m_AreaEffect = areaEffect;
	}

	public void SetUnitSpecificProp(string unitName, LevelEditorUnitsPanel unitsPanel, int numberOfTilesRequired = 1)
	{
		m_UnitID = unitName;
		m_UnitType = UnitType.SpecificProp;
		unitText.text = m_UnitID;
		m_UnitsPanel = unitsPanel;
		m_NumberOfTilesRequired = numberOfTilesRequired;
	}

	public void OnClick()
	{
		m_UnitsPanel.TryPlaceUnitInHex(m_UnitID, m_NumberOfTilesRequired, m_AreaEffect);
	}
}
