using System.Collections.Generic;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class NotificationDataContext : LinkedSynchronizationContext<IJsonNotificationData>
{
	static NotificationDataContext()
	{
		SynchronizationContext<IJsonNotificationData>.Properties = new Dictionary<string, Property<IJsonNotificationData>>
		{
			{
				"Attachment",
				new Property<IJsonNotificationData, Attachment>((IJsonNotificationData d, TrelloAuthorization a) => (d.Attachment == null) ? null : new Attachment(d.Attachment, d.Card.Id, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Attachment o)
				{
					if (o != null)
					{
						d.Attachment = o.Json;
					}
				})
			},
			{
				"Board",
				new Property<IJsonNotificationData, Board>((IJsonNotificationData d, TrelloAuthorization a) => (d.Board == null) ? null : new Board(d.Board, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Board o)
				{
					if (o != null)
					{
						d.Board = o.Json;
					}
				})
			},
			{
				"BoardSource",
				new Property<IJsonNotificationData, Board>((IJsonNotificationData d, TrelloAuthorization a) => (d.BoardSource == null) ? null : new Board(d.BoardSource, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Board o)
				{
					if (o != null)
					{
						d.BoardSource = o.Json;
					}
				})
			},
			{
				"BoardTarget",
				new Property<IJsonNotificationData, Board>((IJsonNotificationData d, TrelloAuthorization a) => (d.BoardTarget == null) ? null : new Board(d.BoardTarget, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Board o)
				{
					if (o != null)
					{
						d.BoardTarget = o.Json;
					}
				})
			},
			{
				"Card",
				new Property<IJsonNotificationData, Card>((IJsonNotificationData d, TrelloAuthorization a) => (d.Card == null) ? null : new Card(d.Card, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Card o)
				{
					if (o != null)
					{
						d.Card = o.Json;
					}
				})
			},
			{
				"CardSource",
				new Property<IJsonNotificationData, Card>((IJsonNotificationData d, TrelloAuthorization a) => (d.CardSource == null) ? null : new Card(d.CardSource, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Card o)
				{
					if (o != null)
					{
						d.CardSource = o.Json;
					}
				})
			},
			{
				"CheckItem",
				new Property<IJsonNotificationData, CheckItem>((IJsonNotificationData d, TrelloAuthorization a) => (d.CheckItem == null) ? null : new CheckItem(d.CheckItem, d.CheckList.Id, TrelloAuthorization.Null), delegate(IJsonNotificationData d, CheckItem o)
				{
					if (o != null)
					{
						d.CheckItem = o.Json;
					}
				})
			},
			{
				"CheckList",
				new Property<IJsonNotificationData, CheckList>((IJsonNotificationData d, TrelloAuthorization a) => (d.CheckList == null) ? null : new CheckList(d.CheckList, TrelloAuthorization.Null), delegate(IJsonNotificationData d, CheckList o)
				{
					if (o != null)
					{
						d.CheckList = o.Json;
					}
				})
			},
			{
				"List",
				new Property<IJsonNotificationData, List>((IJsonNotificationData d, TrelloAuthorization a) => (d.List == null) ? null : new List(d.List, TrelloAuthorization.Null), delegate(IJsonNotificationData d, List o)
				{
					if (o != null)
					{
						d.List = o.Json;
					}
				})
			},
			{
				"ListAfter",
				new Property<IJsonNotificationData, List>((IJsonNotificationData d, TrelloAuthorization a) => (d.ListAfter == null) ? null : new List(d.ListAfter, TrelloAuthorization.Null), delegate(IJsonNotificationData d, List o)
				{
					if (o != null)
					{
						d.ListAfter = o.Json;
					}
				})
			},
			{
				"ListBefore",
				new Property<IJsonNotificationData, List>((IJsonNotificationData d, TrelloAuthorization a) => (d.ListBefore == null) ? null : new List(d.ListBefore, TrelloAuthorization.Null), delegate(IJsonNotificationData d, List o)
				{
					if (o != null)
					{
						d.ListBefore = o.Json;
					}
				})
			},
			{
				"Member",
				new Property<IJsonNotificationData, Member>((IJsonNotificationData d, TrelloAuthorization a) => (d.Member == null) ? null : new Member(d.Member, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Member o)
				{
					if (o != null)
					{
						d.Member = o.Json;
					}
				})
			},
			{
				"WasArchived",
				new Property<IJsonNotificationData, bool?>((IJsonNotificationData d, TrelloAuthorization a) => d.Old?.Closed, delegate(IJsonNotificationData d, bool? o)
				{
					if (d.Old != null && o.HasValue)
					{
						d.Old.Closed = o;
					}
				})
			},
			{
				"OldDescription",
				new Property<IJsonNotificationData, string>((IJsonNotificationData d, TrelloAuthorization a) => d.Old?.Desc, delegate(IJsonNotificationData d, string o)
				{
					if (d.Old != null && o != null)
					{
						d.Old.Desc = o;
					}
				})
			},
			{
				"OldList",
				new Property<IJsonNotificationData, List>((IJsonNotificationData d, TrelloAuthorization a) => (d.Old?.List == null) ? null : new List(d.Old.List, TrelloAuthorization.Null), delegate(IJsonNotificationData d, List o)
				{
					if (d.Old != null)
					{
						d.Old.List = o.Json;
					}
				})
			},
			{
				"OldPosition",
				new Property<IJsonNotificationData, Position>(delegate(IJsonNotificationData d, TrelloAuthorization a)
				{
					double? num = d.Old?.Pos;
					return (!num.HasValue) ? null : ((Position)num.GetValueOrDefault());
				}, delegate(IJsonNotificationData d, Position o)
				{
					if (d.Old != null)
					{
						d.Old.Pos = o.Value;
					}
				})
			},
			{
				"OldText",
				new Property<IJsonNotificationData, string>((IJsonNotificationData d, TrelloAuthorization a) => d.Old?.Text, delegate(IJsonNotificationData d, string o)
				{
					if (d.Old != null)
					{
						d.Old.Text = o;
					}
				})
			},
			{
				"Organization",
				new Property<IJsonNotificationData, Organization>((IJsonNotificationData d, TrelloAuthorization a) => (d.Org == null) ? null : new Organization(d.Org, TrelloAuthorization.Null), delegate(IJsonNotificationData d, Organization o)
				{
					if (o != null)
					{
						d.Org = o.Json;
					}
				})
			},
			{
				"Text",
				new Property<IJsonNotificationData, string>((IJsonNotificationData d, TrelloAuthorization a) => d.Text, delegate(IJsonNotificationData d, string o)
				{
					d.Text = o;
				})
			}
		};
	}

	public NotificationDataContext(TrelloAuthorization auth)
		: base(auth)
	{
	}
}
