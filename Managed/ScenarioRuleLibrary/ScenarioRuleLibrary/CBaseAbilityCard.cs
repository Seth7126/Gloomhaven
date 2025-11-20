using System.Linq;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CBaseAbilityCard : CBaseCard
{
	public int Initiative { get; private set; }

	public string ClassID { get; private set; }

	public string ClassModel
	{
		get
		{
			switch (base.CardType)
			{
			case ECardType.CharacterAbility:
				return CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == ClassID).DefaultModel;
			case ECardType.HeroSummonAbility:
				return CharacterClassManager.HeroSummonClasses.Single((CHeroSummonClass s) => s.ID == ClassID).DefaultModel;
			case ECardType.MonsterAbility:
				return MonsterClassManager.Find(ClassID).DefaultModel;
			default:
				DLLDebug.LogError("Invalid Card Type for CBaseAbilityCard " + base.CardType);
				return null;
			}
		}
	}

	public string ClassCharacterConfig
	{
		get
		{
			switch (base.CardType)
			{
			case ECardType.CharacterAbility:
				return CharacterClassManager.Classes.Single((CCharacterClass s) => s.CharacterID == ClassID).CharacterYML.CustomCharacterConfig;
			case ECardType.HeroSummonAbility:
			case ECardType.MonsterAbility:
				return null;
			default:
				DLLDebug.LogError("Invalid Card Type for CBaseAbilityCard " + base.CardType);
				return null;
			}
		}
	}

	public CBaseAbilityCard(int initiative, int id, string classID, ECardType cardType, string stringID)
		: base(id, cardType, stringID)
	{
		Initiative = initiative;
		ClassID = classID;
		base.ActionHasHappened = false;
	}

	public CBaseAbilityCard()
	{
	}

	public CBaseAbilityCard(CBaseAbilityCard state, ReferenceDictionary references)
		: base(state, references)
	{
		Initiative = state.Initiative;
		ClassID = state.ClassID;
	}
}
