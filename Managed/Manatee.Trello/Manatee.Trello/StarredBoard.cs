using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class StarredBoard : IStarredBoard, ICacheable, IRefreshable, IMergeJson<IJsonStarredBoard>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	private readonly Field<IBoard> _board;

	private readonly Field<Position> _position;

	private readonly StarredBoardContext _context;

	public IBoard Board => _board.Value;

	public string Id { get; }

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

	internal IJsonStarredBoard Json
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

	public event Action<IStarredBoard, IEnumerable<string>> Updated;

	internal StarredBoard(string memberId, IJsonStarredBoard json, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new StarredBoardContext(memberId, Id, auth ?? TrelloAuthorization.Default);
		_context.Synchronized.Add(this);
		_board = new Field<IBoard>(_context, "Board");
		_position = new Field<Position>(_context, "Position");
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	void IMergeJson<IJsonStarredBoard>.Merge(IJsonStarredBoard json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	public Task Delete(CancellationToken ct = default(CancellationToken))
	{
		return _context.Delete(ct);
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonStarredBoard json = TrelloConfiguration.Deserializer.Deserialize<IJsonStarredBoard>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		this.Updated?.Invoke(this, properties);
	}
}
