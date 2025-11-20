using Google.Protobuf.Reflection;

namespace Hydra.Api.Push;

public enum PushMessageType
{
	[OriginalName("PUSH_MESSAGE_TYPE_UNDEFINED")]
	Undefined,
	[OriginalName("PUSH_MESSAGE_TYPE_PRESENCE_USER_UPDATE")]
	PresenceUserUpdate,
	[OriginalName("PUSH_MESSAGE_TYPE_PRESENCE_PARTY_UPDATE")]
	PresencePartyUpdate,
	[OriginalName("PUSH_MESSAGE_TYPE_PRESENCE_SESSION_UPDATE")]
	PresenceSessionUpdate,
	[OriginalName("PUSH_MESSAGE_TYPE_ECONOMY_USER_TRANSACTIONS_UPDATE")]
	EconomyUserTransactionsUpdate,
	[OriginalName("PUSH_MESSAGE_TYPE_SIGNALING")]
	Signaling,
	[OriginalName("PUSH_MESSAGE_TYPE_MESSAGING_USER_UPDATE")]
	MessagingUserUpdate,
	[OriginalName("PUSH_MESSAGE_TYPE_CHALLENGES_INCREMENTAL_UPDATE")]
	ChallengesIncrementalUpdate
}
