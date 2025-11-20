using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class UIScenarioAccomplishmentsList : MonoBehaviour
{
	private struct CharacterStats
	{
		public int kills;

		public int damageDone;

		public int damageTaken;

		public int healingDone;

		public int goldLooted;

		public int chestsLooted;

		public int xpEarned;

		public int itemsUsed;

		public CharacterStats(CMapCharacter character, CPlayerStats stats, int goldLooted, int chestsLooted, int xpEarned, int itemsUsed)
		{
			kills = stats?.Kills.Count() ?? 0;
			damageDone = stats?.DamageDealt.Sum((CPlayerStatsDamage it2) => it2.FinalDamageAmount) ?? 0;
			damageTaken = stats?.DamageReceived.Sum((CPlayerStatsDamage it2) => it2.FinalDamageAmount) ?? 0;
			healingDone = stats?.Heals.Sum((CPlayerStatsHeal it2) => it2.HealAmount) ?? 0;
			this.goldLooted = goldLooted;
			this.chestsLooted = chestsLooted;
			this.xpEarned = xpEarned;
			this.itemsUsed = itemsUsed;
		}
	}

	[SerializeField]
	private List<TextMeshProUGUI> headers;

	[SerializeField]
	private UIScenarioAccomplishmentRow enemiesKilledRow;

	[SerializeField]
	private UIScenarioAccomplishmentRow damageDoneRow;

	[SerializeField]
	private UIScenarioAccomplishmentRow damageTakenRow;

	[SerializeField]
	private UIScenarioAccomplishmentRow healingDoneRow;

	[SerializeField]
	private UIScenarioAccomplishmentRow goldPilesLootedRow;

	[SerializeField]
	private UIScenarioAccomplishmentRow chestsLootedRow;

	[SerializeField]
	private UIScenarioAccomplishmentRow abilityXPEarnedRow;

	[SerializeField]
	private UIScenarioAccomplishmentRow itemsUsedRow;

	public void ShowStats(StatsDataStorage stats, List<CMapCharacter> characters, List<CPlayerStatsScenario> lastScenarioStats, bool won, string soloID)
	{
		List<CharacterStats> list = new List<CharacterStats>();
		int i;
		for (i = 0; i < characters.Count; i++)
		{
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == characters[i].CharacterID);
			headers[i].text = LocalizationManager.GetTranslation(cCharacterClass.LocKey);
			headers[i].gameObject.SetActive(value: true);
			CPlayerStatsScenario cPlayerStatsScenario = lastScenarioStats.SingleOrDefault((CPlayerStatsScenario x) => x.CharacterID == characters[i].CharacterID);
			CharacterStats item = CalculateStats(stats, characters[i], cPlayerStatsScenario);
			list.Add(item);
			if (soloID == "" || soloID == characters[i].CharacterID)
			{
				characters[i].PlayerRecords.UpdatePlayerRecord(item.kills, item.damageDone, item.damageTaken, item.healingDone, item.goldLooted, item.chestsLooted, item.xpEarned, item.itemsUsed, cPlayerStatsScenario.RoundsPlayed, !cPlayerStatsScenario.PlayerSurvivedScenario, won);
			}
		}
		for (int num = characters.Count; num < headers.Count; num++)
		{
			headers[num].gameObject.SetActive(value: false);
		}
		enemiesKilledRow.Initialize(list.Select((CharacterStats it) => it.kills).ToList());
		damageDoneRow.Initialize(list.Select((CharacterStats it) => it.damageDone).ToList());
		damageTakenRow.Initialize(list.Select((CharacterStats it) => it.damageTaken).ToList());
		healingDoneRow.Initialize(list.Select((CharacterStats it) => it.healingDone).ToList());
		goldPilesLootedRow.Initialize(list.Select((CharacterStats it) => it.goldLooted).ToList());
		chestsLootedRow.Initialize(list.Select((CharacterStats it) => it.chestsLooted).ToList());
		abilityXPEarnedRow.Initialize(list.Select((CharacterStats it) => it.xpEarned).ToList());
		itemsUsedRow.Initialize(list.Select((CharacterStats it) => it.itemsUsed).ToList());
	}

	private CharacterStats CalculateStats(StatsDataStorage stats, CMapCharacter character, CPlayerStatsScenario lastScenarioStats)
	{
		return new CharacterStats(character, lastScenarioStats, stats.m_ScenarioStats.m_GoldPilesLootedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? stats.m_ScenarioStats.m_GoldPilesLootedLastScenarioByCharacter[character.CharacterID] : 0, stats.m_ScenarioStats.m_ChestsLootedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? stats.m_ScenarioStats.m_ChestsLootedLastScenarioByCharacter[character.CharacterID] : 0, stats.m_ScenarioStats.m_TotalXPEarnedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? stats.m_ScenarioStats.m_TotalXPEarnedLastScenarioByCharacter[character.CharacterID] : 0, stats.m_ScenarioStats.m_ItemsUsedLastScenarioByCharacter.ContainsKey(character.CharacterID) ? stats.m_ScenarioStats.m_ItemsUsedLastScenarioByCharacter[character.CharacterID] : 0);
	}
}
