using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.RequestProcessing;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public static class TrelloProcessor
{
	public static Task Flush()
	{
		return RestRequestProcessor.Flush();
	}

	public static void ProcessNotification(string content, TrelloAuthorization auth = null)
	{
		Action action = new Action(TrelloConfiguration.Deserializer.Deserialize<IJsonWebhookNotification>(content).Action, auth);
		foreach (ICanWebhook item in TrelloConfiguration.Cache.OfType<ICanWebhook>())
		{
			item.ApplyAction(action);
		}
	}

	public static Task Refresh(IEnumerable<IBatchRefreshable> entities, CancellationToken ct = default(CancellationToken))
	{
		return Task.WhenAll(from g in (from e in entities.OfType<IBatchRefresh>()
				group e by e.Auth).SelectMany((IGrouping<TrelloAuthorization, IBatchRefresh> ga) => from gc in ga.Select((IBatchRefresh entity, int index) => new { entity, index })
				group gc.entity by gc.index / 10 into g
				select g.ToList())
			select _RefreshBatch(g, ct));
	}

	private static async Task _RefreshBatch(List<IBatchRefresh> batch, CancellationToken ct)
	{
		TrelloAuthorization auth = batch.First().Auth;
		IEnumerable<Endpoint> source = batch.Select((IBatchRefresh e) => e.GetRefreshEndpoint());
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Batch_Read_Refresh);
		Dictionary<string, object> parameters = new Dictionary<string, object> { ["urls"] = string.Join(",", source.Select((Endpoint e) => $"/{e}")) };
		foreach (IJsonBatchItem item in (await JsonRepository.Execute<IJsonBatch>(auth, endpoint, ct, parameters)).Items.Where((IJsonBatchItem r) => r.Error == null))
		{
			batch.FirstOrDefault((IBatchRefresh e) => e.Id == item.EntityId)?.Apply(item.Content);
		}
	}
}
