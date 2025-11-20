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

public class OrganizationMembership : IOrganizationMembership, ICacheable, IRefreshable, IMergeJson<IJsonOrganizationMembership>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	private readonly Field<Member> _member;

	private readonly Field<OrganizationMembershipType?> _memberType;

	private readonly Field<bool?> _isUnconfirmed;

	private readonly OrganizationMembershipContext _context;

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

	public bool? IsUnconfirmed => _isUnconfirmed.Value;

	public IMember Member => _member.Value;

	public OrganizationMembershipType? MemberType
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

	internal IJsonOrganizationMembership Json
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

	public event Action<IOrganizationMembership, IEnumerable<string>> Updated;

	internal OrganizationMembership(IJsonOrganizationMembership json, string ownerId, TrelloAuthorization auth)
	{
		Id = json.Id;
		_context = new OrganizationMembershipContext(Id, ownerId, auth);
		_context.Synchronized.Add(this);
		_member = new Field<Member>(_context, "Member");
		_memberType = new Field<OrganizationMembershipType?>(_context, "MemberType");
		_memberType.AddRule(NullableHasValueRule<OrganizationMembershipType>.Instance);
		_memberType.AddRule(EnumerationRule<OrganizationMembershipType?>.Instance);
		_isUnconfirmed = new Field<bool?>(_context, "IsUnconfirmed");
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

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}

	void IMergeJson<IJsonOrganizationMembership>.Merge(IJsonOrganizationMembership json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonOrganizationMembership json = TrelloConfiguration.Deserializer.Deserialize<IJsonOrganizationMembership>(content);
		_context.Merge(json);
	}
}
