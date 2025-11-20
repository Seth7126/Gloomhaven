using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class MemberSearch : IMemberSearch, IRefreshable
{
	private readonly Field<Board> _board;

	private readonly Field<int?> _limit;

	private readonly Field<Organization> _organization;

	private readonly Field<string> _query;

	private readonly Field<bool?> _restrictToOrganization;

	private readonly Field<IEnumerable<MemberSearchResult>> _results;

	private readonly MemberSearchContext _context;

	public IEnumerable<MemberSearchResult> Results => _results.Value;

	internal IBoard Board
	{
		get
		{
			return _board.Value;
		}
		private set
		{
			_board.Value = (Board)value;
		}
	}

	internal int? Limit
	{
		get
		{
			return _limit.Value;
		}
		private set
		{
			_limit.Value = value;
		}
	}

	internal IOrganization Organization
	{
		get
		{
			return _organization.Value;
		}
		private set
		{
			_organization.Value = (Organization)value;
		}
	}

	internal string Query
	{
		get
		{
			return _query.Value;
		}
		private set
		{
			_query.Value = value;
		}
	}

	internal bool? RestrictToOrganization
	{
		get
		{
			return _restrictToOrganization.Value;
		}
		private set
		{
			_restrictToOrganization.Value = value;
		}
	}

	public MemberSearch(string query, int? limit = null, IBoard board = null, IOrganization organization = null, bool? restrictToOrganization = null, TrelloAuthorization auth = null)
	{
		_context = new MemberSearchContext(auth);
		_board = new Field<Board>(_context, "Board");
		_limit = new Field<int?>(_context, "Limit");
		_limit.AddRule(NullableHasValueRule<int>.Instance);
		_limit.AddRule(new NumericRule<int>
		{
			Min = 1,
			Max = 20
		});
		_organization = new Field<Organization>(_context, "Organization");
		_query = new Field<string>(_context, "Query");
		_query.AddRule(NotNullOrWhiteSpaceRule.Instance);
		_restrictToOrganization = new Field<bool?>(_context, "RestrictToOrganization");
		_results = new Field<IEnumerable<MemberSearchResult>>(_context, "Results");
		Query = query;
		Limit = limit;
		Board = board;
		Organization = organization;
		RestrictToOrganization = restrictToOrganization;
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}
}
