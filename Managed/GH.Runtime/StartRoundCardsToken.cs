using System.Linq;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class StartRoundCardsToken : StatePropertyToken
{
	private int roundCardCount;

	private int handCardCount;

	private int discardedCardCount;

	private int lostCardCount;

	private int permaLostCardCount;

	private int activatedCardCount;

	public int[] RoundCardIDs { get; set; }

	public int[] HandCardIDs { get; set; }

	public int[] DiscardedCardIDs { get; set; }

	public int[] LostCardIDs { get; set; }

	public int[] PermaLostCardIDs { get; set; }

	public int[] ActivatedCardIDs { get; set; }

	public int InitiativeCardID { get; set; }

	public int SubInitiativeCardID { get; set; }

	public bool HasShortRested { get; set; }

	public bool HasImprovedShortRested { get; set; }

	public int CardBurnedID { get; set; }

	public bool ShortRestCardRedrawn { get; set; }

	public bool ImprovedShortRest { get; set; }

	public bool LongRestSelected { get; set; }

	public StartRoundCardsToken(StartRoundCardState startRoundCardState)
	{
		roundCardCount = startRoundCardState.RoundAbilityCardInstanceIDs.Count;
		handCardCount = startRoundCardState.HandAbilityCardInstanceIDs.Count;
		discardedCardCount = startRoundCardState.DiscardedAbilityCardInstanceIDs.Count;
		lostCardCount = startRoundCardState.LostAbilityCardInstanceIDs.Count;
		permaLostCardCount = startRoundCardState.PermaLostAbilityCardInstanceIDs.Count;
		activatedCardCount = startRoundCardState.ActivatedCardIDs.Count;
		RoundCardIDs = new int[roundCardCount];
		HandCardIDs = new int[handCardCount];
		DiscardedCardIDs = new int[discardedCardCount];
		LostCardIDs = new int[lostCardCount];
		PermaLostCardIDs = new int[permaLostCardCount];
		ActivatedCardIDs = new int[activatedCardCount];
		for (int i = 0; i < roundCardCount; i++)
		{
			RoundCardIDs[i] = startRoundCardState.RoundAbilityCardInstanceIDs[i];
		}
		for (int j = 0; j < handCardCount; j++)
		{
			HandCardIDs[j] = startRoundCardState.HandAbilityCardInstanceIDs[j];
		}
		for (int k = 0; k < discardedCardCount; k++)
		{
			DiscardedCardIDs[k] = startRoundCardState.DiscardedAbilityCardInstanceIDs[k];
		}
		for (int l = 0; l < lostCardCount; l++)
		{
			LostCardIDs[l] = startRoundCardState.LostAbilityCardInstanceIDs[l];
		}
		for (int m = 0; m < permaLostCardCount; m++)
		{
			PermaLostCardIDs[m] = startRoundCardState.PermaLostAbilityCardInstanceIDs[m];
		}
		for (int n = 0; n < activatedCardCount; n++)
		{
			ActivatedCardIDs[n] = startRoundCardState.ActivatedCardIDs[n];
		}
		InitiativeCardID = startRoundCardState.InitiativeCardID;
		SubInitiativeCardID = startRoundCardState.SubInitiativeCardID;
		HasShortRested = startRoundCardState.HasShortRested;
		HasImprovedShortRested = startRoundCardState.HasImprovedShortRest;
		CardBurnedID = startRoundCardState.ShortRestCardBurnedID;
		ShortRestCardRedrawn = startRoundCardState.ShortRestCardRedrawn;
		ImprovedShortRest = startRoundCardState.ImprovedShortRest;
		LongRestSelected = startRoundCardState.LongRestSelected;
	}

	public StartRoundCardsToken()
	{
		roundCardCount = 0;
		handCardCount = 0;
		discardedCardCount = 0;
		lostCardCount = 0;
		permaLostCardCount = 0;
		activatedCardCount = 0;
		RoundCardIDs = new int[roundCardCount];
		HandCardIDs = new int[handCardCount];
		DiscardedCardIDs = new int[discardedCardCount];
		LostCardIDs = new int[lostCardCount];
		PermaLostCardIDs = new int[permaLostCardCount];
		ActivatedCardIDs = new int[activatedCardCount];
		InitiativeCardID = -1;
		SubInitiativeCardID = -1;
		HasShortRested = false;
		HasImprovedShortRested = false;
		CardBurnedID = -1;
		ShortRestCardRedrawn = false;
		ImprovedShortRest = false;
		LongRestSelected = false;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(roundCardCount);
		packet.WriteInt(handCardCount);
		packet.WriteInt(discardedCardCount);
		packet.WriteInt(lostCardCount);
		packet.WriteInt(permaLostCardCount);
		packet.WriteInt(activatedCardCount);
		for (int i = 0; i < roundCardCount; i++)
		{
			packet.WriteInt(RoundCardIDs[i]);
		}
		for (int j = 0; j < handCardCount; j++)
		{
			packet.WriteInt(HandCardIDs[j]);
		}
		for (int k = 0; k < discardedCardCount; k++)
		{
			packet.WriteInt(DiscardedCardIDs[k]);
		}
		for (int l = 0; l < lostCardCount; l++)
		{
			packet.WriteInt(LostCardIDs[l]);
		}
		for (int m = 0; m < permaLostCardCount; m++)
		{
			packet.WriteInt(PermaLostCardIDs[m]);
		}
		for (int n = 0; n < activatedCardCount; n++)
		{
			packet.WriteInt(ActivatedCardIDs[n]);
		}
		packet.WriteInt(InitiativeCardID);
		packet.WriteInt(SubInitiativeCardID);
		packet.WriteBool(HasShortRested);
		packet.WriteBool(HasImprovedShortRested);
		packet.WriteInt(CardBurnedID);
		packet.WriteBool(ShortRestCardRedrawn);
		packet.WriteBool(ImprovedShortRest);
		packet.WriteBool(LongRestSelected);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		roundCardCount = packet.ReadInt();
		handCardCount = packet.ReadInt();
		discardedCardCount = packet.ReadInt();
		lostCardCount = packet.ReadInt();
		permaLostCardCount = packet.ReadInt();
		activatedCardCount = packet.ReadInt();
		RoundCardIDs = new int[roundCardCount];
		for (int i = 0; i < roundCardCount; i++)
		{
			RoundCardIDs[i] = packet.ReadInt();
		}
		HandCardIDs = new int[handCardCount];
		for (int j = 0; j < handCardCount; j++)
		{
			HandCardIDs[j] = packet.ReadInt();
		}
		DiscardedCardIDs = new int[discardedCardCount];
		for (int k = 0; k < discardedCardCount; k++)
		{
			DiscardedCardIDs[k] = packet.ReadInt();
		}
		LostCardIDs = new int[lostCardCount];
		for (int l = 0; l < lostCardCount; l++)
		{
			LostCardIDs[l] = packet.ReadInt();
		}
		PermaLostCardIDs = new int[permaLostCardCount];
		for (int m = 0; m < permaLostCardCount; m++)
		{
			PermaLostCardIDs[m] = packet.ReadInt();
		}
		ActivatedCardIDs = new int[activatedCardCount];
		for (int n = 0; n < activatedCardCount; n++)
		{
			ActivatedCardIDs[n] = packet.ReadInt();
		}
		InitiativeCardID = packet.ReadInt();
		SubInitiativeCardID = packet.ReadInt();
		HasShortRested = packet.ReadBool();
		HasImprovedShortRested = packet.ReadBool();
		CardBurnedID = packet.ReadInt();
		ShortRestCardRedrawn = packet.ReadBool();
		ImprovedShortRest = packet.ReadBool();
		LongRestSelected = packet.ReadBool();
	}

	public override void Print(string customTitle = "")
	{
		Console.LogInfo(customTitle + GetRevisionString() + "Round Card IDs: " + RoundCardIDs.ToStringPretty() + "Hand Card IDs: " + HandCardIDs.ToStringPretty() + "Discarded Card IDs: " + DiscardedCardIDs.ToStringPretty() + "Lost Card IDs: " + LostCardIDs.ToStringPretty() + "Perma Lost Card IDs: " + PermaLostCardIDs.ToStringPretty() + "Activated Card IDs: " + ActivatedCardIDs.ToStringPretty() + "Initiative Card ID: " + InitiativeCardID + "Sub Initiative Card ID: " + SubInitiativeCardID + "Has Short Rested: " + HasShortRested + "Has Short Rested: " + HasImprovedShortRested + "Card Burned ID: " + CardBurnedID + "Short Rest Card Redrawn: " + ShortRestCardRedrawn + "Improved Short Rest: " + ImprovedShortRest + "Long Rest Selected: " + LongRestSelected);
	}

	public StartRoundCardState GetStartRoundCardState()
	{
		return new StartRoundCardState(RoundCardIDs.ToList(), HandCardIDs.ToList(), DiscardedCardIDs.ToList(), LostCardIDs.ToList(), PermaLostCardIDs.ToList(), ActivatedCardIDs.ToList(), InitiativeCardID, SubInitiativeCardID, HasShortRested, HasImprovedShortRested, CardBurnedID, ImprovedShortRest, ShortRestCardRedrawn, LongRestSelected);
	}
}
