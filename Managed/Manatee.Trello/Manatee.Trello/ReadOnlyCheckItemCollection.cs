using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyCheckItemCollection : ReadOnlyCollection<ICheckItem>, IReadOnlyCheckItemCollection, IReadOnlyCollection<ICheckItem>, IEnumerable<ICheckItem>, IEnumerable, IRefreshable, IHandle<EntityUpdatedEvent<IJsonCheckItem>>, IHandle, IHandle<EntityDeletedEvent<IJsonCheckItem>>
{
	private readonly CheckListContext _context;

	public ICheckItem this[string key] => GetByKey(key);

	internal ReadOnlyCheckItemCollection(CheckListContext context, TrelloAuthorization auth)
		: base((Func<string>)(() => context.Data.Id), auth)
	{
		_context = context;
		EventAggregator.Subscribe(this);
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		await _context.Synchronize(force, ct);
		if (_context.Data.CheckItems == null)
		{
			return;
		}
		EventAggregator.Unsubscribe(this);
		foreach (IJsonCheckItem jsonCheckItem in _context.Data.CheckItems)
		{
			ICheckItem checkItem = base.Items.SingleOrDefault((ICheckItem ci) => ci.Id == jsonCheckItem.Id);
			if (checkItem == null)
			{
				base.Items.Add(new CheckItem(jsonCheckItem, _context.Data.Id));
			}
			else
			{
				((CheckItem)checkItem).Json = jsonCheckItem;
			}
		}
		EventAggregator.Subscribe(this);
		foreach (ICheckItem checkItem2 in base.Items.ToList())
		{
			if (_context.Data.CheckItems.All((IJsonCheckItem jci) => jci.Id != checkItem2.Id))
			{
				base.Items.Remove(checkItem2);
			}
		}
	}

	private ICheckItem GetByKey(string key)
	{
		return this.FirstOrDefault((ICheckItem ci) => key.In(ci.Id, ci.Name));
	}

	void IHandle<EntityUpdatedEvent<IJsonCheckItem>>.Handle(EntityUpdatedEvent<IJsonCheckItem> message)
	{
		if (message.Properties.Contains("CheckList"))
		{
			ICheckItem checkItem = base.Items.FirstOrDefault((ICheckItem b) => b.Id == message.Data.Id);
			if (message.Data.CheckList?.Id != base.OwnerId && checkItem != null)
			{
				base.Items.Remove(checkItem);
			}
			else if (message.Data.CheckList?.Id == base.OwnerId && checkItem == null)
			{
				base.Items.Add(message.Data.GetFromCache<CheckItem>(base.Auth));
			}
		}
	}

	void IHandle<EntityDeletedEvent<IJsonCheckItem>>.Handle(EntityDeletedEvent<IJsonCheckItem> message)
	{
		ICheckItem item = base.Items.FirstOrDefault((ICheckItem c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
