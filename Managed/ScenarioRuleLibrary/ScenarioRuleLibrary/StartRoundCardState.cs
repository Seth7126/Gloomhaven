using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary;

public class StartRoundCardState
{
	public List<int> RoundAbilityCardInstanceIDs;

	public List<int> HandAbilityCardInstanceIDs;

	public List<int> DiscardedAbilityCardInstanceIDs;

	public List<int> LostAbilityCardInstanceIDs;

	public List<int> PermaLostAbilityCardInstanceIDs;

	public List<int> ActivatedCardIDs;

	public int InitiativeCardID;

	public int SubInitiativeCardID;

	public bool HasShortRested;

	public bool HasImprovedShortRest;

	public int ShortRestCardBurnedID;

	public bool ImprovedShortRest;

	public bool ShortRestCardRedrawn;

	public bool LongRestSelected;

	public StartRoundCardState(List<CAbilityCard> roundAbilityCards, List<CAbilityCard> handAbilityCards, List<CAbilityCard> discardedAbilityCards, List<CAbilityCard> lostAbilityCards, List<CAbilityCard> permaLostAbilityCards, List<CBaseCard> activatedCards, CAbilityCard initiativeCard, CAbilityCard subInitiativeCard, bool hasShortRested, bool hasImprovedShortRested, CAbilityCard shortRestCardBurned, bool improvedShortRest, bool shortRestCardRedrawn, bool longRestSelected)
	{
		RoundAbilityCardInstanceIDs = roundAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		HandAbilityCardInstanceIDs = handAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		DiscardedAbilityCardInstanceIDs = discardedAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		LostAbilityCardInstanceIDs = lostAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		PermaLostAbilityCardInstanceIDs = permaLostAbilityCards.Select((CAbilityCard x) => x.CardInstanceID).ToList();
		ActivatedCardIDs = activatedCards.Select((CBaseCard x) => x.ID).ToList();
		InitiativeCardID = initiativeCard?.CardInstanceID ?? (-1);
		SubInitiativeCardID = subInitiativeCard?.CardInstanceID ?? (-1);
		HasShortRested = hasShortRested;
		HasImprovedShortRest = hasImprovedShortRested;
		ShortRestCardBurnedID = shortRestCardBurned?.CardInstanceID ?? (-1);
		ImprovedShortRest = improvedShortRest;
		ShortRestCardRedrawn = shortRestCardRedrawn;
		LongRestSelected = longRestSelected;
	}

	public StartRoundCardState(List<int> roundAbilityCardInstanceIDs, List<int> handAbilityCardInstanceIDs, List<int> discardedAbilityCardInstanceIDs, List<int> lostAbilityCardInstanceIDs, List<int> permaLostAbilityCardInstanceIDs, List<int> activatedAbilityCardInstanceIDs, int initiativeCardID, int subInitiativeCardID, bool hasShortRested, bool hasImprovedShortRested, int shortRestCardBurnedID, bool improvedShortRest, bool shortRestCardRedrawn, bool longRestSelected)
	{
		RoundAbilityCardInstanceIDs = roundAbilityCardInstanceIDs.ToList();
		HandAbilityCardInstanceIDs = handAbilityCardInstanceIDs.ToList();
		DiscardedAbilityCardInstanceIDs = discardedAbilityCardInstanceIDs.ToList();
		LostAbilityCardInstanceIDs = lostAbilityCardInstanceIDs.ToList();
		PermaLostAbilityCardInstanceIDs = permaLostAbilityCardInstanceIDs.ToList();
		ActivatedCardIDs = activatedAbilityCardInstanceIDs.ToList();
		InitiativeCardID = initiativeCardID;
		SubInitiativeCardID = subInitiativeCardID;
		HasShortRested = hasShortRested;
		HasImprovedShortRest = hasImprovedShortRested;
		ShortRestCardBurnedID = shortRestCardBurnedID;
		ImprovedShortRest = improvedShortRest;
		ShortRestCardRedrawn = shortRestCardRedrawn;
		LongRestSelected = longRestSelected;
	}
}
