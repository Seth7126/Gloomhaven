using System;
using System.Collections.Generic;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class BadgesContext : LinkedSynchronizationContext<IJsonBadges>
{
	static BadgesContext()
	{
		SynchronizationContext<IJsonBadges>.Properties = new Dictionary<string, Property<IJsonBadges>>
		{
			{
				"Attachments",
				new Property<IJsonBadges, int?>((IJsonBadges d, TrelloAuthorization a) => d.Attachments, delegate(IJsonBadges d, int? o)
				{
					d.Attachments = o;
				})
			},
			{
				"CheckItems",
				new Property<IJsonBadges, int?>((IJsonBadges d, TrelloAuthorization a) => d.CheckItems, delegate(IJsonBadges d, int? o)
				{
					d.CheckItems = o;
				})
			},
			{
				"CheckItemsChecked",
				new Property<IJsonBadges, int?>((IJsonBadges d, TrelloAuthorization a) => d.CheckItemsChecked, delegate(IJsonBadges d, int? o)
				{
					d.CheckItemsChecked = o;
				})
			},
			{
				"Comments",
				new Property<IJsonBadges, int?>((IJsonBadges d, TrelloAuthorization a) => d.Comments, delegate(IJsonBadges d, int? o)
				{
					d.Comments = o;
				})
			},
			{
				"DueDate",
				new Property<IJsonBadges, DateTime?>((IJsonBadges d, TrelloAuthorization a) => d.Due, delegate(IJsonBadges d, DateTime? o)
				{
					d.Due = o;
				})
			},
			{
				"FogBugz",
				new Property<IJsonBadges, string>((IJsonBadges d, TrelloAuthorization a) => d.Fogbugz, delegate(IJsonBadges d, string o)
				{
					d.Fogbugz = o;
				})
			},
			{
				"HasDescription",
				new Property<IJsonBadges, bool?>((IJsonBadges d, TrelloAuthorization a) => d.Description, delegate(IJsonBadges d, bool? o)
				{
					d.Description = o;
				})
			},
			{
				"HasVoted",
				new Property<IJsonBadges, bool?>((IJsonBadges d, TrelloAuthorization a) => d.ViewingMemberVoted, delegate(IJsonBadges d, bool? o)
				{
					d.ViewingMemberVoted = o;
				})
			},
			{
				"IsComplete",
				new Property<IJsonBadges, bool?>((IJsonBadges d, TrelloAuthorization a) => d.DueComplete, delegate(IJsonBadges d, bool? o)
				{
					d.DueComplete = o;
				})
			},
			{
				"IsSubscribed",
				new Property<IJsonBadges, bool?>((IJsonBadges d, TrelloAuthorization a) => d.Subscribed, delegate(IJsonBadges d, bool? o)
				{
					d.Subscribed = o;
				})
			},
			{
				"Votes",
				new Property<IJsonBadges, int?>((IJsonBadges d, TrelloAuthorization a) => d.Votes, delegate(IJsonBadges d, int? o)
				{
					d.Votes = o;
				})
			}
		};
	}

	public BadgesContext(TrelloAuthorization auth)
		: base(auth)
	{
	}
}
