using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class NotificationContext : SynchronizationContext<IJsonNotification>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Notification.Fields MemberFields;

	public static Dictionary<string, object> CurrentParameters
	{
		get
		{
			lock (Parameters)
			{
				if (!Parameters.Any())
				{
					GenerateParameters();
				}
				return new Dictionary<string, object>(Parameters);
			}
		}
	}

	public NotificationDataContext NotificationDataContext { get; }

	static NotificationContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Notification.Fields.Data | Notification.Fields.IsUnread | Notification.Fields.Type | Notification.Fields.Date;
		SynchronizationContext<IJsonNotification>.Properties = new Dictionary<string, Property<IJsonNotification>>
		{
			{
				"Creator",
				new Property<IJsonNotification, Member>((IJsonNotification d, TrelloAuthorization a) => d.MemberCreator.GetFromCache<Member, IJsonMember>(a, overwrite: false, Array.Empty<object>()), delegate(IJsonNotification d, Member o)
				{
					if (o != null)
					{
						d.MemberCreator = o.Json;
					}
				})
			},
			{
				"Date",
				new Property<IJsonNotification, DateTime?>((IJsonNotification d, TrelloAuthorization a) => d.Date, delegate(IJsonNotification d, DateTime? o)
				{
					d.Date = o;
				})
			},
			{
				"Id",
				new Property<IJsonNotification, string>((IJsonNotification d, TrelloAuthorization a) => d.Id, delegate(IJsonNotification d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsUnread",
				new Property<IJsonNotification, bool?>((IJsonNotification d, TrelloAuthorization a) => d.Unread, delegate(IJsonNotification d, bool? o)
				{
					d.Unread = o;
				})
			},
			{
				"Type",
				new Property<IJsonNotification, NotificationType?>((IJsonNotification d, TrelloAuthorization a) => d.Type, delegate(IJsonNotification d, NotificationType? o)
				{
					d.Type = o;
				})
			}
		};
	}

	public NotificationContext(string id, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		base.Data.Id = id;
		NotificationDataContext = new NotificationDataContext(base.Auth);
		base.Data.Data = NotificationDataContext.Data;
	}

	public static void UpdateParameters()
	{
		lock (Parameters)
		{
			Parameters.Clear();
		}
	}

	private static void GenerateParameters()
	{
		lock (Parameters)
		{
			Parameters.Clear();
			int num = Enum.GetValues(typeof(Notification.Fields)).Cast<Notification.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Notification.Fields enumerationValue = (Notification.Fields)((int)((uint)num & (uint)MemberFields) & (int)Notification.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Notification.Fields fields = (Notification.Fields)((int)((uint)num & (uint)Notification.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Notification.Fields.Creator))
			{
				Parameters["memberCreator"] = "true";
				Parameters["memberCreator_fields"] = MemberContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Notification_Read_Refresh, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
	}

	protected override Dictionary<string, object> GetParameters()
	{
		return CurrentParameters;
	}

	protected override async Task SubmitData(IJsonNotification json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Notification_Write_Update, new Dictionary<string, object> { 
		{
			"_id",
			base.Data.Id
		} });
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	protected override IEnumerable<string> MergeDependencies(IJsonNotification json, bool overwrite)
	{
		return NotificationDataContext.Merge(json.Data, overwrite);
	}
}
