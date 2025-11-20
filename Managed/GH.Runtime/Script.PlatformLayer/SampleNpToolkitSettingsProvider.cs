using System.Collections.Generic;
using Platforms.PS4;

namespace Script.PlatformLayer;

public class SampleNpToolkitSettingsProvider : INpToolkitSettingsProvider
{
	public Dictionary<string, int> AgeRestrictions { get; } = new Dictionary<string, int>
	{
		{ "de", 16 },
		{ "hk", 12 },
		{ "pt", 12 },
		{ "hn", 13 },
		{ "dk", 12 },
		{ "lu", 12 },
		{ "py", 13 },
		{ "hr", 12 },
		{ "ua", 12 },
		{ "hu", 12 },
		{ "qa", 12 },
		{ "id", 12 },
		{ "ie", 12 },
		{ "ec", 13 },
		{ "us", 13 },
		{ "il", 12 },
		{ "ae", 12 },
		{ "uy", 13 },
		{ "in", 12 },
		{ "mt", 12 },
		{ "za", 12 },
		{ "is", 12 },
		{ "it", 12 },
		{ "mx", 13 },
		{ "my", 12 },
		{ "es", 12 },
		{ "ar", 13 },
		{ "at", 12 },
		{ "au", 15 },
		{ "ni", 13 },
		{ "ro", 12 },
		{ "nl", 12 },
		{ "no", 12 },
		{ "be", 12 },
		{ "fi", 12 },
		{ "ru", 16 },
		{ "bg", 12 },
		{ "bh", 12 },
		{ "jp", 12 },
		{ "fr", 12 },
		{ "nz", 15 },
		{ "bo", 13 },
		{ "sa", 12 },
		{ "br", 12 },
		{ "se", 12 },
		{ "sg", 12 },
		{ "si", 12 },
		{ "sk", 12 },
		{ "gb", 12 },
		{ "ca", 13 },
		{ "om", 12 },
		{ "sv", 13 },
		{ "ch", 12 },
		{ "kr", 12 },
		{ "cl", 13 },
		{ "gr", 12 },
		{ "co", 13 },
		{ "kw", 12 },
		{ "gt", 13 },
		{ "cr", 13 },
		{ "pa", 13 },
		{ "th", 12 },
		{ "pe", 13 },
		{ "cy", 12 },
		{ "lb", 12 },
		{ "cz", 12 },
		{ "pl", 12 },
		{ "tr", 12 }
	};

	public Dictionary<MemoryPoolType, uint> MemoryPoolsSizes { get; } = new Dictionary<MemoryPoolType, uint>
	{
		{
			MemoryPoolType.JsonPool,
			6291456u
		},
		{
			MemoryPoolType.SslPool,
			1048576u
		},
		{
			MemoryPoolType.MatchingPool,
			2097152u
		},
		{
			MemoryPoolType.MatchingSslPool,
			786432u
		}
	};

	public NotificationFlags NotificationFlags { get; } = NotificationFlags.NewInvitation | NotificationFlags.UpdateBlockedUsersList | NotificationFlags.UpdateFriendPresence | NotificationFlags.UpdateFriendsList | NotificationFlags.NewInGameMessage;
}
