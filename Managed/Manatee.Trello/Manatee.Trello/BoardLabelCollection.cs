using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Eventing;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class BoardLabelCollection : ReadOnlyCollection<ILabel>, IBoardLabelCollection, IReadOnlyCollection<ILabel>, IEnumerable<ILabel>, IEnumerable, IRefreshable, IHandle<EntityDeletedEvent<IJsonLabel>>, IHandle
{
	internal BoardLabelCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
		EventAggregator.Subscribe(this);
	}

	public async Task<ILabel> Add(string name, LabelColor? color, CancellationToken ct = default(CancellationToken))
	{
		IJsonLabel jsonLabel = TrelloConfiguration.JsonFactory.Create<IJsonLabel>();
		jsonLabel.Name = name ?? string.Empty;
		jsonLabel.Color = color;
		jsonLabel.ForceNullColor = !color.HasValue;
		jsonLabel.Board = TrelloConfiguration.JsonFactory.Create<IJsonBoard>();
		jsonLabel.Board.Id = base.OwnerId;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Write_AddLabel);
		return new Label(await JsonRepository.Execute(base.Auth, endpoint, jsonLabel, ct), base.Auth);
	}

	public void Filter(LabelColor labelColor)
	{
		base.AdditionalParameters["filter"] = labelColor.GetDescription();
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		IncorporateLimit();
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_Labels, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonLabel> source = await JsonRepository.Execute<List<IJsonLabel>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonLabel jb)
		{
			Label fromCache = jb.GetFromCache<Label, IJsonLabel>(base.Auth, overwrite: true, Array.Empty<object>());
			fromCache.Json = jb;
			return fromCache;
		}));
	}

	void IHandle<EntityDeletedEvent<IJsonLabel>>.Handle(EntityDeletedEvent<IJsonLabel> message)
	{
		ILabel item = base.Items.FirstOrDefault((ILabel c) => c.Id == message.Data.Id);
		base.Items.Remove(item);
	}
}
