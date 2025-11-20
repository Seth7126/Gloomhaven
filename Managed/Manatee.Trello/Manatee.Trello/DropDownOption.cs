using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class DropDownOption : IDropDownOption, ICacheable, IRefreshable, IMergeJson<IJsonCustomDropDownOption>, IBatchRefresh, IBatchRefreshable
{
	private readonly Field<CustomFieldDefinition> _field;

	private readonly Field<string> _text;

	private readonly Field<LabelColor?> _labelColor;

	private readonly Field<Position> _position;

	private readonly DropDownOptionContext _context;

	public string Id => Json.Id;

	public ICustomFieldDefinition Field => _field.Value;

	public string Text => _text.Value;

	public LabelColor? Color => _labelColor.Value;

	public Position Position => _position.Value;

	internal IJsonCustomDropDownOption Json
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

	internal DropDownOption(IJsonCustomDropDownOption json, TrelloAuthorization auth, bool created = false)
	{
		_context = new DropDownOptionContext(json, auth, created);
		_field = new Field<CustomFieldDefinition>(_context, "Field");
		_text = new Field<string>(_context, "Text");
		_labelColor = new Field<LabelColor?>(_context, "Color");
		_position = new Field<Position>(_context, "Position");
		if (!created && auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	public static IDropDownOption Create(string text, LabelColor color = LabelColor.None)
	{
		IJsonCustomDropDownOption jsonCustomDropDownOption = TrelloConfiguration.JsonFactory.Create<IJsonCustomDropDownOption>();
		jsonCustomDropDownOption.Text = text;
		jsonCustomDropDownOption.Color = color;
		jsonCustomDropDownOption.ValidForMerge = true;
		return new DropDownOption(jsonCustomDropDownOption, null, created: true);
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
		return Text;
	}

	void IMergeJson<IJsonCustomDropDownOption>.Merge(IJsonCustomDropDownOption json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonCustomDropDownOption json = TrelloConfiguration.Deserializer.Deserialize<IJsonCustomDropDownOption>(content);
		_context.Merge(json);
	}
}
