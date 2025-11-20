using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class BoardMembership : IBoardMembership, ICacheable, IRefreshable, IMergeJson<IJsonBoardMembership>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	private readonly Field<Member> _member;

	private readonly Field<BoardMembershipType?> _memberType;

	private readonly Field<bool?> _isDeactivated;

	private readonly BoardMembershipContext _context;

	private DateTime? _creation;

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

	public string Id { get; private set; }

	public bool? IsDeactivated => _isDeactivated.Value;

	public IMember Member => _member.Value;

	public BoardMembershipType? MemberType
	{
		get
		{
			return _memberType.Value;
		}
		set
		{
			_memberType.Value = value;
		}
	}

	internal IJsonBoardMembership Json
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

	public event Action<IBoardMembership, IEnumerable<string>> Updated;

	internal BoardMembership(IJsonBoardMembership json, string ownerId, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new BoardMembershipContext(Id, ownerId, auth);
		_context.Synchronized.Add(this);
		_member = new Field<Member>(_context, "Member");
		_memberType = new Field<BoardMembershipType?>(_context, "MemberType");
		_memberType.AddRule(NullableHasValueRule<BoardMembershipType>.Instance);
		_memberType.AddRule(EnumerationRule<BoardMembershipType?>.Instance);
		_isDeactivated = new Field<bool?>(_context, "IsDeactivated");
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
		_context.Merge(json);
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	public override string ToString()
	{
		return $"{Member} ({MemberType})";
	}

	void IMergeJson<IJsonBoardMembership>.Merge(IJsonBoardMembership json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonBoardMembership json = TrelloConfiguration.Deserializer.Deserialize<IJsonBoardMembership>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
