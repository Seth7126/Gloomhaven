using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Sticker : ISticker, ICacheable, IRefreshable, IMergeJson<IJsonSticker>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "left")]
		Left = 1,
		[Display(Description = "image")]
		Name = 2,
		[Display(Description = "imageScaled")]
		Previews = 4,
		[Display(Description = "rotate")]
		Rotation = 8,
		[Display(Description = "top")]
		Top = 0x10,
		[Display(Description = "url")]
		Url = 0x20,
		[Display(Description = "zIndex")]
		ZIndex = 0x40
	}

	public const string Check = "check";

	public const string Heart = "heart";

	public const string Warning = "warning";

	public const string Clock = "clock";

	public const string Smile = "smile";

	public const string Laugh = "laugh";

	public const string Huh = "huh";

	public const string Frown = "frown";

	public const string ThumbsUp = "thumbsup";

	public const string ThumbsDown = "thumbsdown";

	public const string Star = "star";

	public const string RocketShip = "rocketship";

	private readonly Field<double?> _left;

	private readonly Field<string> _name;

	private readonly Field<int?> _rotation;

	private readonly Field<double?> _top;

	private readonly Field<string> _url;

	private readonly Field<int?> _zIndex;

	private readonly StickerContext _context;

	private static Fields _downloadedFields;

	public static Fields DownloadedFields
	{
		get
		{
			return _downloadedFields;
		}
		set
		{
			_downloadedFields = value;
			StickerContext.UpdateParameters();
		}
	}

	public string Id { get; private set; }

	public double? Left
	{
		get
		{
			return _left.Value;
		}
		set
		{
			_left.Value = value;
		}
	}

	public string Name => _name.Value;

	public IReadOnlyCollection<IImagePreview> Previews => _context.Previews;

	public int? Rotation
	{
		get
		{
			return _rotation.Value;
		}
		set
		{
			_rotation.Value = value;
		}
	}

	public double? Top
	{
		get
		{
			return _top.Value;
		}
		set
		{
			_top.Value = value;
		}
	}

	public string ImageUrl => _url.Value;

	public int? ZIndex
	{
		get
		{
			return _zIndex.Value;
		}
		set
		{
			_zIndex.Value = value;
		}
	}

	internal IJsonSticker Json
	{
		get
		{
			return _context.Data;
		}
		set
		{
			_context.Merge(value);
		}
	}

	TrelloAuthorization IBatchRefresh.Auth => _context.Auth;

	public event Action<ISticker, IEnumerable<string>> Updated;

	static Sticker()
	{
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	internal Sticker(IJsonSticker json, string ownerId, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new StickerContext(Id, ownerId, auth);
		_context.Synchronized.Add(this);
		_left = new Field<double?>(_context, "Left");
		_left.AddRule(NullableHasValueRule<double>.Instance);
		_name = new Field<string>(_context, "Name");
		_rotation = new Field<int?>(_context, "Rotation");
		_rotation.AddRule(NullableHasValueRule<int>.Instance);
		_rotation.AddRule(new NumericRule<int>
		{
			Min = 0,
			Max = 359
		});
		_top = new Field<double?>(_context, "Top");
		_top.AddRule(NullableHasValueRule<double>.Instance);
		_url = new Field<string>(_context, "ImageUrl");
		_zIndex = new Field<int?>(_context, "ZIndex");
		_zIndex.AddRule(NullableHasValueRule<int>.Instance);
		_context.Merge(json);
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	public async Task Delete(CancellationToken ct = default(CancellationToken))
	{
		await _context.Delete(ct);
		if (TrelloConfiguration.RemoveDeletedItemsFromCache)
		{
			TrelloConfiguration.Cache.Remove(this);
		}
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonSticker>.Merge(IJsonSticker json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonSticker json = TrelloConfiguration.Deserializer.Deserialize<IJsonSticker>(content);
		_context.Merge(json);
	}

	public override string ToString()
	{
		return Name;
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
