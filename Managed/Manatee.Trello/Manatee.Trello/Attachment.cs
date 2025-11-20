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

public class Attachment : IAttachment, ICacheable, IRefreshable, IMergeJson<IJsonAttachment>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "bytes")]
		Bytes = 1,
		[Display(Description = "date")]
		Date = 2,
		[Display(Description = "isUpload")]
		IsUpload = 4,
		[Display(Description = "member")]
		Member = 8,
		[Display(Description = "mimeType")]
		MimeType = 0x10,
		[Display(Description = "name")]
		Name = 0x20,
		[Display(Description = "previews")]
		Previews = 0x40,
		[Display(Description = "url")]
		Url = 0x80,
		[Display(Description = "edgeColor")]
		EdgeColor = 0x100,
		[Display(Description = "pos")]
		Position = 0x200
	}

	private readonly Field<int?> _bytes;

	private readonly Field<DateTime?> _date;

	private readonly Field<bool?> _isUpload;

	private readonly Field<Member> _member;

	private readonly Field<string> _mimeType;

	private readonly Field<string> _name;

	private readonly Field<string> _url;

	private readonly Field<Position> _position;

	private readonly Field<WebColor> _edgeColor;

	private readonly AttachmentContext _context;

	private DateTime? _creation;

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
			AttachmentContext.UpdateParameters();
		}
	}

	public int? Bytes => _bytes.Value;

	public DateTime CreationDate
	{
		get
		{
			if (!_creation.HasValue)
			{
				_creation = Id.ExtractCreationDate();
			}
			return _creation.Value;
		}
	}

	public DateTime? Date => _date.Value;

	public WebColor EdgeColor => _edgeColor.Value;

	public string Id { get; private set; }

	public bool? IsUpload => _isUpload.Value;

	public IMember Member => _member.Value;

	public string MimeType => _mimeType.Value;

	public string Name
	{
		get
		{
			return _name.Value;
		}
		set
		{
			_name.Value = value;
		}
	}

	public Position Position
	{
		get
		{
			return _position.Value;
		}
		set
		{
			_position.Value = value;
		}
	}

	public IReadOnlyCollection<IImagePreview> Previews => _context.Previews;

	public string Url => _url.Value;

	internal IJsonAttachment Json
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

	public event Action<IAttachment, IEnumerable<string>> Updated;

	static Attachment()
	{
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	internal Attachment(IJsonAttachment json, string ownerId, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new AttachmentContext(Id, ownerId, auth);
		_context.Synchronized.Add(this);
		_bytes = new Field<int?>(_context, "Bytes");
		_date = new Field<DateTime?>(_context, "Date");
		_member = new Field<Member>(_context, "Member");
		_edgeColor = new Field<WebColor>(_context, "EdgeColor");
		_isUpload = new Field<bool?>(_context, "IsUpload");
		_mimeType = new Field<string>(_context, "MimeType");
		_name = new Field<string>(_context, "Name");
		_name.AddRule(NotNullOrWhiteSpaceRule.Instance);
		_position = new Field<Position>(_context, "Position");
		_position.AddRule(PositionRule.Instance);
		_url = new Field<string>(_context, "Url");
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
		_context.Merge(json);
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

	public override string ToString()
	{
		return Name;
	}

	void IMergeJson<IJsonAttachment>.Merge(IJsonAttachment json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonAttachment json = TrelloConfiguration.Deserializer.Deserialize<IJsonAttachment>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
