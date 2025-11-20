using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public abstract class PowerUpBase : IPowerUp, ICacheable, IRefreshable, IMergeJson<IJsonPowerUp>, IBatchRefresh, IBatchRefreshable
{
	private readonly Field<string> _name;

	private readonly Field<bool?> _isPublic;

	private readonly Field<string> _additionalInfo;

	private readonly PowerUpContext _context;

	public string AdditionalInfo => _additionalInfo.Value;

	public string Id { get; }

	public bool? IsPublic => _isPublic.Value;

	public string Name => _name.Value;

	internal IJsonPowerUp Json
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

	protected PowerUpBase(IJsonPowerUp json, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new PowerUpContext(json, auth);
		_additionalInfo = new Field<string>(_context, "AdditionalInfo");
		_name = new Field<string>(_context, "Name");
		_isPublic = new Field<bool?>(_context, "IsPublic");
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonPowerUp>.Merge(IJsonPowerUp json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonPowerUp json = TrelloConfiguration.Deserializer.Deserialize<IJsonPowerUp>(content);
		_context.Merge(json);
	}

	public override string ToString()
	{
		return Name;
	}
}
