using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello.Internal.Synchronization;

internal class BoardBackgroundContext : LinkedSynchronizationContext<IJsonBoardBackground>
{
	private readonly string _ownerId;

	private bool _deleted;

	static BoardBackgroundContext()
	{
		SynchronizationContext<IJsonBoardBackground>.Properties = new Dictionary<string, Property<IJsonBoardBackground>>
		{
			{
				"Id",
				new Property<IJsonBoardBackground, string>((IJsonBoardBackground d, TrelloAuthorization a) => d.Id, delegate(IJsonBoardBackground d, string o)
				{
					d.Id = o;
				})
			},
			{
				"Color",
				new Property<IJsonBoardBackground, WebColor>((IJsonBoardBackground d, TrelloAuthorization a) => (!d.Color.IsNullOrWhiteSpace()) ? new WebColor(d.Color) : null, delegate(IJsonBoardBackground d, WebColor o)
				{
					d.Color = o?.ToString();
				})
			},
			{
				"BottomColor",
				new Property<IJsonBoardBackground, WebColor>((IJsonBoardBackground d, TrelloAuthorization a) => (!d.BottomColor.IsNullOrWhiteSpace()) ? new WebColor(d.BottomColor) : null, delegate(IJsonBoardBackground d, WebColor o)
				{
					d.BottomColor = o?.ToString();
				})
			},
			{
				"TopColor",
				new Property<IJsonBoardBackground, WebColor>((IJsonBoardBackground d, TrelloAuthorization a) => (!d.TopColor.IsNullOrWhiteSpace()) ? new WebColor(d.TopColor) : null, delegate(IJsonBoardBackground d, WebColor o)
				{
					d.TopColor = o?.ToString();
				})
			},
			{
				"Image",
				new Property<IJsonBoardBackground, string>((IJsonBoardBackground d, TrelloAuthorization a) => d.Image, delegate(IJsonBoardBackground d, string o)
				{
					d.Image = o;
				})
			},
			{
				"IsTiled",
				new Property<IJsonBoardBackground, bool?>((IJsonBoardBackground d, TrelloAuthorization a) => d.Tile, delegate(IJsonBoardBackground d, bool? o)
				{
					d.Tile = o;
				})
			},
			{
				"Brightness",
				new Property<IJsonBoardBackground, BoardBackgroundBrightness?>((IJsonBoardBackground d, TrelloAuthorization a) => d.Brightness, delegate(IJsonBoardBackground d, BoardBackgroundBrightness? o)
				{
					d.Brightness = o;
				})
			},
			{
				"Type",
				new Property<IJsonBoardBackground, BoardBackgroundType?>((IJsonBoardBackground d, TrelloAuthorization a) => d.Type, delegate(IJsonBoardBackground d, BoardBackgroundType? o)
				{
					d.Type = o;
				})
			},
			{
				"ValidForMerge",
				new Property<IJsonBoardBackground, bool>((IJsonBoardBackground d, TrelloAuthorization a) => d.ValidForMerge, delegate(IJsonBoardBackground d, bool o)
				{
					d.ValidForMerge = o;
				}, isHidden: true)
			}
		};
	}

	public BoardBackgroundContext(TrelloAuthorization auth)
		: base(auth)
	{
	}

	public BoardBackgroundContext(string ownerId, TrelloAuthorization auth)
		: base(auth)
	{
		_ownerId = ownerId;
	}

	public async Task Delete(CancellationToken ct)
	{
		if (!_deleted)
		{
			CancelUpdate();
			Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Member_Write_DeleteBoardBackground, new Dictionary<string, object>
			{
				{ "_idMember", _ownerId },
				{
					"_id",
					base.Data.Id
				}
			});
			await JsonRepository.Execute(base.Auth, endpoint, ct);
			_deleted = true;
			RaiseDeleted();
		}
	}
}
