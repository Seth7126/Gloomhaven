using System;
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

public class Label : ILabel, ICacheable, IRefreshable, IMergeJson<IJsonLabel>, IBatchRefresh, IBatchRefreshable
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "board")]
		Board = 1,
		[Display(Description = "color")]
		Color = 2,
		[Display(Description = "name")]
		Name = 4,
		[Display(Description = "uses")]
		[Obsolete("Trello no longer supports this feature.")]
		Uses = 8
	}

	private readonly Field<Board> _board;

	private readonly Field<LabelColor?> _color;

	private readonly Field<string> _name;

	private readonly LabelContext _context;

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
			LabelContext.UpdateParameters();
		}
	}

	public IBoard Board => _board.Value;

	public LabelColor? Color
	{
		get
		{
			return _color.Value;
		}
		set
		{
			_color.Value = value;
		}
	}

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

	public string Id { get; }

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

	[Obsolete("Trello no longer supports this feature.")]
	public int? Uses => null;

	internal IJsonLabel Json
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

	static Label()
	{
		DownloadedFields = (Fields)(Enum.GetValues(typeof(Fields)).Cast<int>().Sum() & -2);
	}

	internal Label(IJsonLabel json, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new LabelContext(Id, auth);
		_board = new Field<Board>(_context, "Board");
		_color = new Field<LabelColor?>(_context, "Color");
		_color.AddRule(EnumerationRule<LabelColor?>.Instance);
		_name = new Field<string>(_context, "Name");
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

	void IMergeJson<IJsonLabel>.Merge(IJsonLabel json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonLabel json = TrelloConfiguration.Deserializer.Deserialize<IJsonLabel>(content);
		_context.Merge(json);
	}

	public override string ToString()
	{
		if (!Name.IsNullOrWhiteSpace())
		{
			return Name + " (" + (Color?.ToString() ?? "No color") + ")";
		}
		return "(" + (Color?.ToString() ?? "No color") + ")";
	}
}
