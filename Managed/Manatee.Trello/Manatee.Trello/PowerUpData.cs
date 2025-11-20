using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class PowerUpData : IPowerUpData, ICacheable, IRefreshable, IMergeJson<IJsonPowerUpData>
{
	private readonly Field<string> _pluginId;

	private readonly Field<string> _value;

	private readonly PowerUpDataContext _context;

	public string Id { get; }

	public string PluginId => _pluginId.Value;

	public string Value => _value.Value;

	internal IJsonPowerUpData Json
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

	internal PowerUpData(IJsonPowerUpData json, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new PowerUpDataContext(Id, auth);
		_pluginId = new Field<string>(_context, "PluginId");
		_value = new Field<string>(_context, "Value");
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonPowerUpData>.Merge(IJsonPowerUpData json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}
}
