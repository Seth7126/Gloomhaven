using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Achievements;
using ScenarioRuleLibrary;
using UnityEngine;

public class TownRecordsService : ITownRecordsService
{
	private class TownRecordMapCharacter : ITownRecordCharacter
	{
		private readonly CPlayerRecords playerRecords;

		public string CharacterName
		{
			get
			{
				if (!playerRecords.CharacterName.IsNOTNullOrEmpty())
				{
					return LocalizationManager.GetTranslation(CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == playerRecords.CharacterID).LocKey);
				}
				return playerRecords.CharacterName;
			}
		}

		public Sprite Icon => UIInfoTools.Instance.GetCharacterMarker(CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == playerRecords.CharacterID).CharacterModel, CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == playerRecords.CharacterID).CharacterYML.CustomCharacterConfig);

		public int DamageDone => playerRecords.LifetimeTotal.DamageDone;

		public int HealingDone => playerRecords.LifetimeTotal.HealingDone;

		public int Kills => playerRecords.LifetimeTotal.Kills;

		public int Exhausitons => playerRecords.LifetimeTotal.Exhaustions;

		public float Winrate
		{
			get
			{
				if (playerRecords.LifetimeTotal.ScenariosPlayed != 0)
				{
					return (float)Math.Round((float)playerRecords.LifetimeTotal.ScenariosWon / (float)playerRecords.LifetimeTotal.ScenariosPlayed * 100f, 2);
				}
				return 0f;
			}
		}

		public bool IsRetired { get; }

		public TownRecordMapCharacter(CPlayerRecords playerRecords, bool isRetired)
		{
			this.playerRecords = playerRecords;
			IsRetired = isRetired;
		}
	}

	private bool HasToShowIntro
	{
		get
		{
			if (!AdventureState.MapState.HeadquartersState.HasShownIntroTownRecords)
			{
				return AdventureState.MapState.MapParty.RetiredCharacterRecords.Count > 0;
			}
			return false;
		}
	}

	public List<ITownRecordCharacter> GetCharacters()
	{
		return ((IEnumerable<CMapCharacter>)AdventureState.MapState.MapParty.CheckCharacters).Select((Func<CMapCharacter, ITownRecordCharacter>)((CMapCharacter it) => new TownRecordMapCharacter(it.PlayerRecords, isRetired: false))).Concat(((IEnumerable<CPlayerRecords>)AdventureState.MapState.MapParty.RetiredCharacterRecords).Select((Func<CPlayerRecords, ITownRecordCharacter>)((CPlayerRecords it) => new TownRecordMapCharacter(it, isRetired: true)))).ToList();
	}

	public List<DialogLineDTO> GetStory()
	{
		List<DialogLineDTO> list = new List<DialogLineDTO>();
		if (HasToShowIntro)
		{
			AdventureState.MapState.HeadquartersState.HasShownIntroTownRecords = true;
			list.Add(new DialogLineDTO("GUI_TOWN_RECORDS_INTRO_STORY_1", "Narrator", EExpression.Default, null, null, "GUI_TOWN_RECORDS_INTRO_STORY_1"));
			list.Add(new DialogLineDTO("GUI_TOWN_RECORDS_INTRO_STORY_2", "Narrator", EExpression.Default, null, null, "GUI_TOWN_RECORDS_INTRO_STORY_2"));
		}
		return list;
	}

	public List<CPartyAchievement> GetAchievementsToClaim()
	{
		return AdventureState.MapState.MapParty.Achievements.FindAll((CPartyAchievement it) => it.State == EAchievementState.Completed && it.Achievement.AchievementType == EAchievementType.TownRecord);
	}

	public void ClaimAchievement(CPartyAchievement achievement)
	{
		achievement.ClaimRewards(applyRewards: false);
		SaveData.Instance.SaveCurrentAdventureData();
	}
}
