using TMPro;
using UnityEngine;

public class UIScenarioAccomplishments : MonoBehaviour
{
	[SerializeField]
	private TMP_Text enemiesKilledText;

	[SerializeField]
	private TMP_Text damageDoneText;

	[SerializeField]
	private TMP_Text damageTakenText;

	[SerializeField]
	private TMP_Text healingDoneText;

	[SerializeField]
	private TMP_Text goldPilesLootedText;

	[SerializeField]
	private TMP_Text chestsLootedText;

	[SerializeField]
	private TMP_Text abilityXPEarnedText;

	[SerializeField]
	private TMP_Text itemsUsedText;

	public void NewAdventureInitialize(int accomplishmentsIndex)
	{
		if (accomplishmentsIndex == 0)
		{
			StatsDataStorage statsDataStorage = SaveData.Instance.Global.m_StatsDataStorage;
			base.gameObject.SetActive(value: true);
			enemiesKilledText.SetText(statsDataStorage.m_ScenarioStats.m_EnemiesKilledLastScenario + "/" + statsDataStorage.m_TotalEnemiesLastScenario);
			damageDoneText.SetText(statsDataStorage.m_ScenarioStats.m_TotalDamageDoneLastScenario.ToString());
			damageTakenText.SetText(statsDataStorage.m_ScenarioStats.m_TotalDamageTakenLastScenario.ToString());
			healingDoneText.SetText(statsDataStorage.m_ScenarioStats.m_TotalHealingDoneLastScenario.ToString());
			goldPilesLootedText.SetText(statsDataStorage.m_ScenarioStats.m_GoldPilesLootedLastScenario + "/" + statsDataStorage.m_ScenarioStats.m_ScenarioGoldLastScenario);
			chestsLootedText.SetText(statsDataStorage.m_ScenarioStats.m_ChestsLootedLastScenario + "/" + statsDataStorage.m_ScenarioStats.m_ScenarioChestsLastScenario);
			abilityXPEarnedText.SetText(statsDataStorage.m_ScenarioStats.m_TotalXPEarnedLastScenario.ToString());
			itemsUsedText.SetText(statsDataStorage.m_ScenarioStats.m_ItemsUsedLastScenario.ToString());
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
