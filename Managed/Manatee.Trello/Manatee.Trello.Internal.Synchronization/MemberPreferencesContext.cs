using System.Collections.Generic;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class MemberPreferencesContext : LinkedSynchronizationContext<IJsonMemberPreferences>
{
	static MemberPreferencesContext()
	{
		SynchronizationContext<IJsonMemberPreferences>.Properties = new Dictionary<string, Property<IJsonMemberPreferences>>
		{
			{
				"EnableColorBlindMode",
				new Property<IJsonMemberPreferences, bool?>((IJsonMemberPreferences d, TrelloAuthorization a) => d.ColorBlind, delegate(IJsonMemberPreferences d, bool? o)
				{
					d.ColorBlind = o;
				})
			},
			{
				"MinutesBetweenSummaries",
				new Property<IJsonMemberPreferences, int?>((IJsonMemberPreferences d, TrelloAuthorization a) => d.MinutesBetweenSummaries, delegate(IJsonMemberPreferences d, int? o)
				{
					d.MinutesBetweenSummaries = o;
				})
			}
		};
	}

	public MemberPreferencesContext(TrelloAuthorization auth)
		: base(auth)
	{
	}
}
