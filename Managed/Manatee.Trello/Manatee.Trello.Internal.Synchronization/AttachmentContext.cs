using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.Caching;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class AttachmentContext : DeletableSynchronizationContext<IJsonAttachment>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Attachment.Fields MemberFields;

	private readonly string _ownerId;

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

	public ReadOnlyAttachmentPreviewCollection Previews { get; }

	static AttachmentContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Attachment.Fields.Bytes | Attachment.Fields.Date | Attachment.Fields.IsUpload | Attachment.Fields.MimeType | Attachment.Fields.Name | Attachment.Fields.Url | Attachment.Fields.EdgeColor | Attachment.Fields.Position;
		SynchronizationContext<IJsonAttachment>.Properties = new Dictionary<string, Property<IJsonAttachment>>
		{
			{
				"Bytes",
				new Property<IJsonAttachment, int?>((IJsonAttachment d, TrelloAuthorization a) => d.Bytes, delegate(IJsonAttachment d, int? o)
				{
					d.Bytes = o;
				})
			},
			{
				"Date",
				new Property<IJsonAttachment, DateTime?>((IJsonAttachment d, TrelloAuthorization a) => d.Date, delegate(IJsonAttachment d, DateTime? o)
				{
					d.Date = o;
				})
			},
			{
				"Member",
				new Property<IJsonAttachment, Member>((IJsonAttachment d, TrelloAuthorization a) => d.Member?.GetFromCache<Member, IJsonMember>(a, overwrite: true, Array.Empty<object>()), delegate(IJsonAttachment d, Member o)
				{
					d.Member = o?.Json;
				})
			},
			{
				"Id",
				new Property<IJsonAttachment, string>((IJsonAttachment d, TrelloAuthorization a) => d.Id, delegate(IJsonAttachment d, string o)
				{
					d.Id = o;
				})
			},
			{
				"IsUpload",
				new Property<IJsonAttachment, bool?>((IJsonAttachment d, TrelloAuthorization a) => d.IsUpload, delegate(IJsonAttachment d, bool? o)
				{
					d.IsUpload = o;
				})
			},
			{
				"MimeType",
				new Property<IJsonAttachment, string>((IJsonAttachment d, TrelloAuthorization a) => d.MimeType, delegate(IJsonAttachment d, string o)
				{
					d.MimeType = o;
				})
			},
			{
				"Name",
				new Property<IJsonAttachment, string>((IJsonAttachment d, TrelloAuthorization a) => d.Name, delegate(IJsonAttachment d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Url",
				new Property<IJsonAttachment, string>((IJsonAttachment d, TrelloAuthorization a) => d.Url, delegate(IJsonAttachment d, string o)
				{
					d.Url = o;
				})
			},
			{
				"Position",
				new Property<IJsonAttachment, Position>((IJsonAttachment d, TrelloAuthorization a) => Position.GetPosition(d.Pos), delegate(IJsonAttachment d, Position o)
				{
					d.Pos = Position.GetJson(o);
				})
			},
			{
				"EdgeColor",
				new Property<IJsonAttachment, WebColor>((IJsonAttachment d, TrelloAuthorization a) => (!d.EdgeColor.IsNullOrWhiteSpace()) ? new WebColor(d.EdgeColor) : null, delegate(IJsonAttachment d, WebColor o)
				{
					d.EdgeColor = o?.ToString();
				})
			}
		};
	}

	public AttachmentContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
		Previews = new ReadOnlyAttachmentPreviewCollection(this, base.Auth);
		Previews.Refreshed += delegate
		{
			OnMerged(new List<string> { "Previews" });
		};
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
			int num = Enum.GetValues(typeof(Attachment.Fields)).Cast<Attachment.Fields>().ToList()
				.Cast<int>()
				.Sum();
			Attachment.Fields enumerationValue = (Attachment.Fields)((int)((uint)num & (uint)MemberFields) & (int)Attachment.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
			Attachment.Fields fields = (Attachment.Fields)((int)((uint)num & (uint)Attachment.DownloadedFields) & (int)(~MemberFields));
			if (fields.HasFlag(Attachment.Fields.Previews))
			{
				Parameters["previews"] = "true";
			}
			if (fields.HasFlag(Attachment.Fields.Member))
			{
				Parameters["member"] = "true";
				Parameters["member_fields"] = MemberContext.CurrentParameters["fields"];
			}
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Attachment_Read_Refresh, new Dictionary<string, object>
		{
			{ "_cardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override Dictionary<string, object> GetParameters()
	{
		return CurrentParameters;
	}

	protected override Endpoint GetDeleteEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Attachment_Write_Delete, new Dictionary<string, object>
		{
			{ "_cardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override async Task SubmitData(IJsonAttachment json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Attachment_Write_Update, new Dictionary<string, object>
		{
			{ "_cardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}

	protected override IEnumerable<string> MergeDependencies(IJsonAttachment json, bool overwrite)
	{
		List<string> list = new List<string>();
		if (json.Previews != null)
		{
			Previews.Update(json.Previews.Select((IJsonImagePreview a) => a.GetFromCache<ImagePreview>(base.Auth)));
			list.Add("Actions");
		}
		return list;
	}
}
