using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class StickerContext : DeletableSynchronizationContext<IJsonSticker>
{
	private static readonly Dictionary<string, object> Parameters;

	private static readonly Sticker.Fields MemberFields;

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

	public ReadOnlyStickerPreviewCollection Previews { get; }

	static StickerContext()
	{
		Parameters = new Dictionary<string, object>();
		MemberFields = Sticker.Fields.Left | Sticker.Fields.Name | Sticker.Fields.Previews | Sticker.Fields.Rotation | Sticker.Fields.Top | Sticker.Fields.Url | Sticker.Fields.ZIndex;
		SynchronizationContext<IJsonSticker>.Properties = new Dictionary<string, Property<IJsonSticker>>
		{
			{
				"Id",
				new Property<IJsonSticker, string>((IJsonSticker d, TrelloAuthorization a) => d.Id, delegate(IJsonSticker d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Left",
				new Property<IJsonSticker, double?>((IJsonSticker d, TrelloAuthorization a) => d.Left, delegate(IJsonSticker d, double? o)
				{
					d.Left = o;
				})
			},
			{
				"Name",
				new Property<IJsonSticker, string>((IJsonSticker d, TrelloAuthorization a) => d.Name, delegate(IJsonSticker d, string o)
				{
					d.Name = o;
				})
			},
			{
				"Previews",
				new Property<IJsonSticker, List<IJsonImagePreview>>((IJsonSticker d, TrelloAuthorization a) => d.Previews, delegate(IJsonSticker d, List<IJsonImagePreview> o)
				{
					d.Previews = o;
				})
			},
			{
				"Rotation",
				new Property<IJsonSticker, int?>((IJsonSticker d, TrelloAuthorization a) => d.Rotation, delegate(IJsonSticker d, int? o)
				{
					d.Rotation = o;
				})
			},
			{
				"Top",
				new Property<IJsonSticker, double?>((IJsonSticker d, TrelloAuthorization a) => d.Top, delegate(IJsonSticker d, double? o)
				{
					d.Top = o;
				})
			},
			{
				"ImageUrl",
				new Property<IJsonSticker, string>((IJsonSticker d, TrelloAuthorization a) => d.Url, delegate(IJsonSticker d, string o)
				{
					d.Url = o;
				})
			},
			{
				"ZIndex",
				new Property<IJsonSticker, int?>((IJsonSticker d, TrelloAuthorization a) => d.ZIndex, delegate(IJsonSticker d, int? o)
				{
					d.ZIndex = o;
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonSticker, bool>((IJsonSticker d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonSticker d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public StickerContext(string id, string ownerId, TrelloAuthorization auth)
		: base(auth, useTimer: true)
	{
		_ownerId = ownerId;
		base.Data.Id = id;
		Previews = new ReadOnlyStickerPreviewCollection(this, auth);
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
			Sticker.Fields enumerationValue = (Sticker.Fields)((int)((uint)Enum.GetValues(typeof(Sticker.Fields)).Cast<Sticker.Fields>().ToList()
				.Cast<int>()
				.Sum() & (uint)MemberFields) & (int)Sticker.DownloadedFields);
			Parameters["fields"] = enumerationValue.GetDescription();
		}
	}

	public override Endpoint GetRefreshEndpoint()
	{
		return EndpointFactory.Build(EntityRequestType.Sticker_Read_Refresh, new Dictionary<string, object>
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
		return EndpointFactory.Build(EntityRequestType.Sticker_Write_Delete, new Dictionary<string, object>
		{
			{ "_cardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
	}

	protected override async Task SubmitData(IJsonSticker json, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Sticker_Write_Update, new Dictionary<string, object>
		{
			{ "_cardId", _ownerId },
			{
				"_id",
				base.Data.Id
			}
		});
		Merge(await JsonRepository.Execute(base.Auth, endpoint, json, ct));
	}
}
