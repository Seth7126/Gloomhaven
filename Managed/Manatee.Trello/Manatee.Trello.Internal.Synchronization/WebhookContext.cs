using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class WebhookContext<T> : DeletableSynchronizationContext<IJsonWebhook> where T : class, ICanWebhook
{
	static WebhookContext()
	{
		SynchronizationContext<IJsonWebhook>.Properties = new Dictionary<string, Property<IJsonWebhook>>
		{
			{
				"CallBackUrl",
				new Property<IJsonWebhook, string>((IJsonWebhook d, TrelloAuthorization a) => d.CallbackUrl, delegate(IJsonWebhook d, string o)
				{
					d.CallbackUrl = o;
				})
			},
			{
				"Description",
				new Property<IJsonWebhook, string>((IJsonWebhook d, TrelloAuthorization a) => d.Description, delegate(IJsonWebhook d, string o)
				{
					d.Description = o;
				})
			},
			{
				"Id",
				new Property<IJsonWebhook, string>((IJsonWebhook d, TrelloAuthorization a) => d.Id, delegate(IJsonWebhook d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsActive",
				new Property<IJsonWebhook, bool?>((IJsonWebhook d, TrelloAuthorization a) => d.Active, delegate(IJsonWebhook d, bool? o)
				{
					d.Active = o;
				})
			},
			{
				"Target",
				new Property<IJsonWebhook, T>(delegate(IJsonWebhook d, TrelloAuthorization a)
				{
					object obj;
					if (d.IdModel != null)
					{
						obj = TrelloConfiguration.Cache.Find<T>(d.IdModel);
						if (obj == null)
						{
							return BuildModel(d.IdModel, a);
						}
					}
					else
					{
						obj = null;
					}
					return (T)obj;
				}, delegate(IJsonWebhook d, T o)
				{
					d.IdModel = o?.Id;
				})
			}
		};
	}

	public WebhookContext(TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
	}

	public WebhookContext(string id, TrelloAuthorization auth)
		: this(auth)
	{
		base.Data.Id = id;
	}

	public async Task<string> Create(ICanWebhook target, string description, string callBackUrl, CancellationToken ct)
	{
		IJsonWebhook jsonWebhook = TrelloConfiguration.JsonFactory.Create<IJsonWebhook>();
		jsonWebhook.IdModel = target.Id;
		jsonWebhook.Description = description;
		jsonWebhook.CallbackUrl = callBackUrl;
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Webhook_Write_Entity);
		IJsonWebhook jsonWebhook2 = await JsonRepository.Execute(base.Auth, endpoint, jsonWebhook, ct);
		Merge(jsonWebhook2);
		return jsonWebhook2.Id;
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Webhook_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Webhook_Write_Delete, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override async Task SubmitData(IJsonWebhook json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Webhook_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	private static T BuildModel(string id, TrelloAuthorization auth)
	{
		return (T)Activator.CreateInstance(typeof(T), id, auth);
	}
}
