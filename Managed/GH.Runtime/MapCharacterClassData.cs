using System.Collections.Generic;
using System.Linq;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

public class MapCharacterClassData : ICharacterCreatorClass
{
	public string ID => Data.ID;

	public bool IsNew => AdventureState.MapState.MapParty.NewUnlockedCharacterIDs.Contains(Data.ID);

	public List<CAbilityCard> OwnedAbilityCards { get; }

	public CharacterYMLData Data { get; }

	public int Health => Data.HealthTable[1];

	public int StartingPerks => AdventureState.MapState.HeadquartersState.CurrentStartingPerksAmount;

	public MapCharacterClassData(CharacterYMLData character)
	{
		Data = character;
		CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == character.ID);
		OwnedAbilityCards = cCharacterClass.AbilityCardsPool.Where((CAbilityCard c) => c.Level <= 1).Distinct().ToList();
	}

	protected bool Equals(MapCharacterClassData other)
	{
		return object.Equals(Data.ID, other.Data.ID);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((MapCharacterClassData)obj);
	}

	public override int GetHashCode()
	{
		if (Data == null)
		{
			return 0;
		}
		return Data.GetHashCode();
	}

	public static bool operator ==(MapCharacterClassData left, MapCharacterClassData right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(MapCharacterClassData left, MapCharacterClassData right)
	{
		return !object.Equals(left, right);
	}
}
