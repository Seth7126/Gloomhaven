using System;
using System.Collections.Generic;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class ActionDataContext : LinkedSynchronizationContext<IJsonActionData>
{
	static ActionDataContext()
	{
		SynchronizationContext<IJsonActionData>.Properties = new Dictionary<string, Property<IJsonActionData>>
		{
			{
				"Attachment",
				new Property<IJsonActionData, Attachment>((IJsonActionData d, TrelloAuthorization a) => (d.Attachment == null) ? null : new Attachment(d.Attachment, d.Card.Id, TrelloAuthorization.Null), delegate(IJsonActionData d, Attachment o)
				{
					if (o != null)
					{
						d.Attachment = o.Json;
					}
				})
			},
			{
				"Board",
				new Property<IJsonActionData, Board>((IJsonActionData d, TrelloAuthorization a) => (d.Board == null) ? null : new Board(d.Board, TrelloAuthorization.Null), delegate(IJsonActionData d, Board o)
				{
					if (o != null)
					{
						d.Board = o.Json;
					}
				})
			},
			{
				"BoardSource",
				new Property<IJsonActionData, Board>((IJsonActionData d, TrelloAuthorization a) => (d.BoardSource == null) ? null : new Board(d.BoardSource, TrelloAuthorization.Null), delegate(IJsonActionData d, Board o)
				{
					if (o != null)
					{
						d.BoardSource = o.Json;
					}
				})
			},
			{
				"BoardTarget",
				new Property<IJsonActionData, Board>((IJsonActionData d, TrelloAuthorization a) => (d.BoardTarget == null) ? null : new Board(d.BoardTarget, TrelloAuthorization.Null), delegate(IJsonActionData d, Board o)
				{
					if (o != null)
					{
						d.BoardTarget = o.Json;
					}
				})
			},
			{
				"Card",
				new Property<IJsonActionData, Card>((IJsonActionData d, TrelloAuthorization a) => (d.Card == null) ? null : new Card(d.Card, TrelloAuthorization.Null), delegate(IJsonActionData d, Card o)
				{
					if (o != null)
					{
						d.Card = o.Json;
					}
				})
			},
			{
				"CardSource",
				new Property<IJsonActionData, Card>((IJsonActionData d, TrelloAuthorization a) => (d.CardSource == null) ? null : new Card(d.CardSource, TrelloAuthorization.Null), delegate(IJsonActionData d, Card o)
				{
					if (o != null)
					{
						d.CardSource = o.Json;
					}
				})
			},
			{
				"CheckItem",
				new Property<IJsonActionData, CheckItem>((IJsonActionData d, TrelloAuthorization a) => (d.CheckItem == null) ? null : new CheckItem(d.CheckItem, d.CheckList?.Id, TrelloAuthorization.Null), delegate(IJsonActionData d, CheckItem o)
				{
					if (o != null)
					{
						d.CheckItem = o.Json;
					}
				})
			},
			{
				"CheckList",
				new Property<IJsonActionData, CheckList>((IJsonActionData d, TrelloAuthorization a) => (d.CheckList == null) ? null : new CheckList(d.CheckList, TrelloAuthorization.Null), delegate(IJsonActionData d, CheckList o)
				{
					if (o != null)
					{
						d.CheckList = o.Json;
					}
				})
			},
			{
				"CustomField",
				new Property<IJsonActionData, CustomFieldDefinition>((IJsonActionData d, TrelloAuthorization a) => (d.CustomField == null) ? null : new CustomFieldDefinition(d.CustomField, TrelloAuthorization.Null), delegate(IJsonActionData d, CustomFieldDefinition o)
				{
					if (o != null)
					{
						d.CustomField = o.Json;
					}
				})
			},
			{
				"Label",
				new Property<IJsonActionData, Label>((IJsonActionData d, TrelloAuthorization a) => (d.Label == null) ? null : new Label(d.Label, TrelloAuthorization.Null), delegate(IJsonActionData d, Label o)
				{
					if (o != null)
					{
						d.Label = o.Json;
					}
				})
			},
			{
				"LastEdited",
				new Property<IJsonActionData, DateTime?>((IJsonActionData d, TrelloAuthorization a) => d.DateLastEdited, delegate(IJsonActionData d, DateTime? o)
				{
					if (o.HasValue)
					{
						d.DateLastEdited = o;
					}
				})
			},
			{
				"List",
				new Property<IJsonActionData, List>((IJsonActionData d, TrelloAuthorization a) => (d.List == null) ? null : new List(d.List, TrelloAuthorization.Null), delegate(IJsonActionData d, List o)
				{
					if (o != null)
					{
						d.List = o.Json;
					}
				})
			},
			{
				"ListAfter",
				new Property<IJsonActionData, List>((IJsonActionData d, TrelloAuthorization a) => (d.ListAfter == null) ? null : new List(d.ListAfter, TrelloAuthorization.Null), delegate(IJsonActionData d, List o)
				{
					if (o != null)
					{
						d.ListAfter = o.Json;
					}
				})
			},
			{
				"ListBefore",
				new Property<IJsonActionData, List>((IJsonActionData d, TrelloAuthorization a) => (d.ListBefore == null) ? null : new List(d.ListBefore, TrelloAuthorization.Null), delegate(IJsonActionData d, List o)
				{
					if (o != null)
					{
						d.ListBefore = o.Json;
					}
				})
			},
			{
				"Member",
				new Property<IJsonActionData, Member>((IJsonActionData d, TrelloAuthorization a) => (d.Member == null) ? null : new Member(d.Member, TrelloAuthorization.Null), delegate(IJsonActionData d, Member o)
				{
					if (o != null)
					{
						d.Member = o.Json;
					}
				})
			},
			{
				"WasArchived",
				new Property<IJsonActionData, bool?>((IJsonActionData d, TrelloAuthorization a) => d.Old?.Closed, delegate(IJsonActionData d, bool? o)
				{
					if (d.Old != null && o.HasValue)
					{
						d.Old.Closed = o;
					}
				})
			},
			{
				"OldDescription",
				new Property<IJsonActionData, string>((IJsonActionData d, TrelloAuthorization a) => d.Old?.Desc, delegate(IJsonActionData d, string o)
				{
					if (d.Old != null && o != null)
					{
						d.Old.Desc = o;
					}
				})
			},
			{
				"OldList",
				new Property<IJsonActionData, List>((IJsonActionData d, TrelloAuthorization a) => (d.Old?.List == null) ? null : new List(d.Old.List, TrelloAuthorization.Null), delegate(IJsonActionData d, List o)
				{
					if (d.Old != null)
					{
						d.Old.List = o.Json;
					}
				})
			},
			{
				"OldPosition",
				new Property<IJsonActionData, Position>(delegate(IJsonActionData d, TrelloAuthorization a)
				{
					double? num = d.Old?.Pos;
					return (!num.HasValue) ? null : ((Position)num.GetValueOrDefault());
				}, delegate(IJsonActionData d, Position o)
				{
					if (d.Old != null)
					{
						d.Old.Pos = o.Value;
					}
				})
			},
			{
				"OldText",
				new Property<IJsonActionData, string>((IJsonActionData d, TrelloAuthorization a) => d.Old?.Text, delegate(IJsonActionData d, string o)
				{
					if (d.Old != null)
					{
						d.Old.Text = o;
					}
				})
			},
			{
				"Organization",
				new Property<IJsonActionData, Organization>((IJsonActionData d, TrelloAuthorization a) => (d.Org == null) ? null : new Organization(d.Org, TrelloAuthorization.Null), delegate(IJsonActionData d, Organization o)
				{
					if (o != null)
					{
						d.Org = o.Json;
					}
				})
			},
			{
				"PowerUp",
				new Property<IJsonActionData, PowerUpBase>((IJsonActionData d, TrelloAuthorization a) => (d.Plugin == null) ? null : (d.Plugin.GetFromCache<IPowerUp>(a) as PowerUpBase), delegate(IJsonActionData d, PowerUpBase o)
				{
					if (o != null)
					{
						d.Plugin = o.Json;
					}
				})
			},
			{
				"Text",
				new Property<IJsonActionData, string>((IJsonActionData d, TrelloAuthorization a) => d.Text, delegate(IJsonActionData d, string o)
				{
					d.Text = o;
				})
			},
			{
				"Value",
				new Property<IJsonActionData, string>((IJsonActionData d, TrelloAuthorization a) => d.Value, delegate(IJsonActionData d, string o)
				{
					d.Value = o;
				})
			}
		};
	}

	public ActionDataContext(TrelloAuthorization auth)
		: base(auth)
	{
	}
}
