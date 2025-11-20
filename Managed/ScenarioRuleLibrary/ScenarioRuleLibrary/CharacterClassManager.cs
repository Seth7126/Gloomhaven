using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class CharacterClassManager
{
	private static List<CCharacterClass> s_Classes = new List<CCharacterClass>();

	private static List<CHeroSummonClass> s_HeroSummonClasses = new List<CHeroSummonClass>();

	private static AbilityCardYMLData s_LongRestLayout = null;

	public static List<CCharacterClass> PerksClass;

	public static List<CCharacterClass> Classes => s_Classes;

	public static List<CHeroSummonClass> HeroSummonClasses => s_HeroSummonClasses;

	public static List<CAbilityCard> AllAbilityCards { get; private set; }

	public static List<CAbilityCard> AllAbilityCardInstances { get; private set; }

	public static AbilityCardYMLData LongRestLayout => s_LongRestLayout;

	public static void Load()
	{
		s_Classes.Clear();
		AllAbilityCards = new List<CAbilityCard>();
		AllAbilityCardInstances = new List<CAbilityCard>();
		s_LongRestLayout = ScenarioRuleClient.SRLYML.AbilityCards.SingleOrDefault((AbilityCardYMLData x) => x.ID == -1);
		foreach (CharacterYMLData character in ScenarioRuleClient.SRLYML.Characters)
		{
			List<CAbilityCard> list = new List<CAbilityCard>();
			foreach (AbilityCardYMLData item in ScenarioRuleClient.SRLYML.AbilityCards.Where((AbilityCardYMLData x) => x.CharacterID == character.ID))
			{
				if (item != null && item.TopActionCardData != null && item.BottomActionCardData != null)
				{
					list.Add(new CAbilityCard(item.Initiative, item.ID, new CAction(null, null, CAbilityMove.CreateDefaultMove(2, isMonster: false), 0), new CAction(null, null, CAbilityAttack.CreateDefaultAttack(2, 1, 1, isMonster: false), 0), item.TopActionCardData, item.BottomActionCardData, item, character.ID));
				}
				else
				{
					DLLDebug.LogError("Ability card " + item.FileName + " unable to load properly");
				}
			}
			if (list.Count > 0)
			{
				s_Classes.Add(new CCharacterClass(character, list, 1));
				AllAbilityCards.AddRange(list);
				AllAbilityCardInstances.AddRange(list);
			}
		}
		ValidateCharacterClassManager();
		s_HeroSummonClasses.Clear();
		foreach (HeroSummonYMLData heroSummon in ScenarioRuleClient.SRLYML.HeroSummons)
		{
			s_HeroSummonClasses.Add(new CHeroSummonClass(heroSummon.ID, heroSummon.Model, heroSummon.LocKey, heroSummon));
		}
	}

	public static void ValidateCharacterClassManager()
	{
	}

	public static CCharacterClass Find(string classID)
	{
		return s_Classes.Find((CCharacterClass x) => x.ID.ToLower() == classID.ToLower());
	}

	public static CHeroSummonClass FindHeroSummonClass(string classID)
	{
		return s_HeroSummonClasses.Find((CHeroSummonClass x) => x.ID.ToLower() == classID.ToLower());
	}

	public static List<CActiveBonus> FindAllActiveBonuses()
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		GameState.CheckActiveBonuses();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			list.AddRange(playerActor.CharacterClass.FindActiveBonuses(playerActor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			list.AddRange(exhaustedPlayer.CharacterClass.FindActiveBonuses(exhaustedPlayer));
		}
		return list;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CActor actor)
	{
		GameState.CheckActiveBonuses();
		List<CActiveBonus> allActiveBonuses = new List<CActiveBonus>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			allActiveBonuses.AddRange(playerActor.CharacterClass.FindActiveBonuses(actor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			allActiveBonuses.AddRange(exhaustedPlayer.CharacterClass.FindActiveBonuses(actor));
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			allActiveBonuses.AddRange(from x in allAliveMonster.MonsterClass.FindActiveBonuses(actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			allActiveBonuses.AddRange(from x in @object.MonsterClass.FindActiveBonuses(actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			allActiveBonuses.AddRange(heroSummon.BaseCard.ActiveBonuses.Where((CActiveBonus x) => (x.Actor == actor || (x.IsAura && x.ValidActorsInRangeOfAura.Contains(actor))) && !allActiveBonuses.Contains(x)));
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			allActiveBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus x) => x.Actor == actor));
		}
		return allActiveBonuses;
	}

	public static List<CActiveBonus> FindAllSongActiveBonuses(CActor actor)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass.CharacterModel == ECharacter.Soothsinger);
		if (cPlayerActor != null && actor != null)
		{
			list.AddRange(cPlayerActor.CharacterClass.FindAllSongActiveBonuses(actor, cPlayerActor));
		}
		return list;
	}

	public static List<CActiveBonus> FindAllDoomActiveBonuses(CActor actor = null, bool checkDeadActor = false)
	{
		List<CActiveBonus> list = new List<CActiveBonus>();
		CPlayerActor cPlayerActor = ScenarioManager.Scenario.PlayerActors.SingleOrDefault((CPlayerActor x) => x.CharacterClass.CharacterModel == ECharacter.Doomstalker);
		if (cPlayerActor != null)
		{
			list.AddRange(cPlayerActor.CharacterClass.FindAllDoomActiveBonuses(actor));
		}
		if (checkDeadActor)
		{
			cPlayerActor = ScenarioManager.Scenario.ExhaustedPlayers.SingleOrDefault((CPlayerActor x) => x.CharacterClass.CharacterModel == ECharacter.Doomstalker);
			if (cPlayerActor != null)
			{
				list.AddRange(cPlayerActor.CharacterClass.FindAllDoomActiveBonuses(actor));
			}
		}
		return list;
	}

	public static List<CActiveBonus> FindAllActiveBonusAuras(CActor actor)
	{
		List<CActiveBonus> allActiveBonuses = new List<CActiveBonus>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			allActiveBonuses.AddRange(playerActor.CharacterClass.FindActiveBonusAuras(actor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			allActiveBonuses.AddRange(exhaustedPlayer.CharacterClass.FindActiveBonusAuras(actor));
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			allActiveBonuses.AddRange(heroSummon.BaseCard.ActiveBonuses.Where((CActiveBonus x) => x.IsAura && x.ValidActorsInRangeOfAura.Contains(actor) && !allActiveBonuses.Contains(x)));
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			allActiveBonuses.AddRange(from x in allAliveMonster.MonsterClass.FindActiveBonusAuras(actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			allActiveBonuses.AddRange(from x in @object.MonsterClass.FindActiveBonusAuras(actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			allActiveBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus x) => x.IsAura && (x.Actor == actor || x.ValidActorsInRangeOfAura.Contains(actor))));
		}
		return allActiveBonuses;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CAbility.EAbilityType type, CActor actor)
	{
		List<CActiveBonus> allActiveBonuses = new List<CActiveBonus>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			allActiveBonuses.AddRange(playerActor.CharacterClass.FindActiveBonuses(type, actor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			allActiveBonuses.AddRange(exhaustedPlayer.CharacterClass.FindActiveBonuses(type, actor));
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			allActiveBonuses.AddRange(heroSummon.BaseCard.ActiveBonuses.Where((CActiveBonus x) => x.Type() == type && x.IsAura && x.ValidActorsInRangeOfAura.Contains(actor) && !allActiveBonuses.Contains(x)));
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			allActiveBonuses.AddRange(from x in allAliveMonster.MonsterClass.FindActiveBonuses(type, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			allActiveBonuses.AddRange(from x in @object.MonsterClass.FindActiveBonuses(type, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			allActiveBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus x) => x.Type() == type && ((!x.IsAura) ? (x.Actor == actor) : x.ValidActorsInRangeOfAura.Contains(actor))));
		}
		return allActiveBonuses;
	}

	public static List<CActiveBonus> FindAllActiveBonuses(CActiveBonus.EActiveBonusDurationType durationType, CActor actor)
	{
		List<CActiveBonus> allActiveBonuses = new List<CActiveBonus>();
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			allActiveBonuses.AddRange(playerActor.CharacterClass.FindActiveBonuses(durationType, actor));
		}
		foreach (CPlayerActor exhaustedPlayer in ScenarioManager.Scenario.ExhaustedPlayers)
		{
			allActiveBonuses.AddRange(exhaustedPlayer.CharacterClass.FindActiveBonuses(durationType, actor));
		}
		foreach (CHeroSummonActor heroSummon in ScenarioManager.Scenario.HeroSummons)
		{
			allActiveBonuses.AddRange(heroSummon.BaseCard.ActiveBonuses.Where((CActiveBonus x) => x.Duration == durationType && x.IsAura && x.ValidActorsInRangeOfAura.Contains(actor) && !allActiveBonuses.Contains(x)));
		}
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			allActiveBonuses.AddRange(from x in allAliveMonster.MonsterClass.FindActiveBonuses(durationType, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CObjectActor @object in ScenarioManager.Scenario.Objects)
		{
			allActiveBonuses.AddRange(from x in @object.MonsterClass.FindActiveBonuses(durationType, actor)
				where !allActiveBonuses.Contains(x)
				select x);
		}
		foreach (CScenarioModifier scenarioModifier in ScenarioManager.CurrentScenarioState.ScenarioModifiers)
		{
			allActiveBonuses.AddRange(scenarioModifier.ActiveBonuses.Where((CActiveBonus x) => x.Duration == durationType && ((!x.IsAura) ? (x.Actor == actor) : x.ValidActorsInRangeOfAura.Contains(actor))));
		}
		return allActiveBonuses;
	}

	public static void SetPerks(List<CharacterPerk> perks)
	{
		foreach (CharacterPerk perk in perks)
		{
			CCharacterClass cCharacterClass = s_Classes.SingleOrDefault((CCharacterClass s) => s.ID == perk.CharacterID);
			if (cCharacterClass != null)
			{
				cCharacterClass.ApplyPerk(perk.Perk);
			}
			else
			{
				DLLDebug.LogError("Unable to create character perk.  Class not found " + perk.CharacterID);
			}
		}
	}

	public static void AddPerks(List<CharacterPerk> perks)
	{
		foreach (CharacterPerk perk in perks)
		{
			CCharacterClass cCharacterClass = s_Classes.SingleOrDefault((CCharacterClass s) => s.ID == perk.CharacterID);
			if (cCharacterClass != null)
			{
				cCharacterClass.AddPerk(perk.Perk);
			}
			else
			{
				DLLDebug.LogError("Unable to create character perk.  Class not found " + perk.CharacterID);
			}
		}
	}

	public static void SetPerksComplete(string characterID)
	{
		foreach (CMap item in ScenarioManager.CurrentScenarioState.Maps.Where((CMap w) => w.Revealed || w.Players.Any((PlayerState p) => p.IsRevealed && !p.HiddenAtStart) || w.Monsters.Any((EnemyState m) => m.IsRevealed) || w.HeroSummons.Any((HeroSummonState h) => h.IsRevealed)))
		{
			foreach (PlayerState playerState in item.Players.Where((PlayerState p) => !p.HiddenAtStart))
			{
				CCharacterClass cCharacterClass = Find(playerState.ClassID);
				if (cCharacterClass.ID == characterID && ScenarioManager.Scenario.InitialPlayers.SingleOrDefault((CPlayerActor s) => s.ActorGuid == playerState.ActorGuid) != null)
				{
					cCharacterClass.ApplyItemModifiersToList(playerState.Items);
				}
			}
		}
	}

	public static string GetCharacterIDFromModelInstanceID(int modelInstanceID)
	{
		if (modelInstanceID == 0)
		{
			DLLDebug.LogWarning("Default modelInstanceID was provided. Returning null as the CharacterID.");
			return null;
		}
		int num = 0;
		while (modelInstanceID > 1000)
		{
			num++;
			modelInstanceID -= 1000;
		}
		List<CCharacterClass> list = Classes.Where((CCharacterClass w) => w.CharacterModel == (ECharacter)modelInstanceID).ToList();
		if (num < list.Count)
		{
			return list[num].CharacterID;
		}
		DLLDebug.LogError("Invalid modelInstanceID '" + modelInstanceID + "' sent to GetCharacterIDFromModelInstanceID\n" + Environment.StackTrace);
		return null;
	}

	public static int GetModelInstanceIDFromCharacterID(string characterID)
	{
		if (string.IsNullOrEmpty(characterID))
		{
			DLLDebug.LogWarning("Default CharacterID was provided. Returning 0 as the ModelInstanceID.");
			return 0;
		}
		CCharacterClass cCharacterClass = Classes.SingleOrDefault((CCharacterClass s) => s.CharacterID == characterID);
		if (cCharacterClass == null)
		{
			DLLDebug.LogError("Invalid characterID '" + characterID + "' sent to GetModelInstanceIDFromCharacterID\n" + Environment.StackTrace);
			return 0;
		}
		return cCharacterClass.ModelInstanceID;
	}

	public static void Reset()
	{
		AllAbilityCardInstances = AllAbilityCards.ToList();
		foreach (CCharacterClass s_Class in s_Classes)
		{
			s_Class.Reset();
		}
		foreach (CHeroSummonClass s_HeroSummonClass in s_HeroSummonClasses)
		{
			s_HeroSummonClass.Reset();
		}
	}
}
