using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class Search : ISearch, IRefreshable
{
	private readonly Field<IEnumerable<Action>> _actions;

	private readonly Field<IEnumerable<Board>> _boards;

	private readonly Field<IEnumerable<Card>> _cards;

	private readonly Field<IEnumerable<Member>> _members;

	private readonly Field<IEnumerable<Organization>> _organizations;

	private readonly Field<string> _query;

	private readonly Field<List<IQueryable>> _queryContext;

	private readonly Field<SearchModelType?> _modelTypes;

	private readonly Field<int?> _limit;

	private readonly Field<bool?> _isPartial;

	private readonly SearchContext _context;

	public IEnumerable<IAction> Actions => _actions.Value;

	public IEnumerable<IBoard> Boards => _boards.Value;

	public IEnumerable<ICard> Cards => _cards.Value;

	public IEnumerable<IMember> Members => _members.Value;

	public IEnumerable<IOrganization> Organizations => _organizations.Value;

	public string Query
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

	internal List<IQueryable> Context
	{
		get
		{
			return _queryContext.Value;
		}
		private set
		{
			_queryContext.Value = value;
		}
	}

	internal SearchModelType? Types
	{
		get
		{
			return _modelTypes.Value;
		}
		private set
		{
			_modelTypes.Value = value;
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

	internal bool? IsPartial
	{
		get
		{
			return _isPartial.Value;
		}
		private set
		{
			_isPartial.Value = value;
		}
	}

	public Search(ISearchQuery query, int? limit = null, SearchModelType modelTypes = SearchModelType.All, IEnumerable<IQueryable> context = null, TrelloAuthorization auth = null, bool isPartial = false)
		: this(query.ToString(), limit, modelTypes, context, auth, isPartial)
	{
	}

	public Search(string query, int? limit = null, SearchModelType modelTypes = SearchModelType.All, IEnumerable<IQueryable> context = null, TrelloAuthorization auth = null, bool isPartial = false)
	{
		_context = new SearchContext(auth);
		_actions = new Field<IEnumerable<Action>>(_context, "Actions");
		_boards = new Field<IEnumerable<Board>>(_context, "Boards");
		_cards = new Field<IEnumerable<Card>>(_context, "Cards");
		_members = new Field<IEnumerable<Member>>(_context, "Members");
		_organizations = new Field<IEnumerable<Organization>>(_context, "Organizations");
		_query = new Field<string>(_context, "Query");
		_query.AddRule(NotNullOrWhiteSpaceRule.Instance);
		_queryContext = new Field<List<IQueryable>>(_context, "Context");
		_modelTypes = new Field<SearchModelType?>(_context, "Types");
		_limit = new Field<int?>(_context, "Limit");
		_limit.AddRule(NullableHasValueRule<int>.Instance);
		_limit.AddRule(new NumericRule<int>
		{
			Min = 1,
			Max = 1000
		});
		_isPartial = new Field<bool?>(_context, "IsPartial");
		Query = query;
		if (context != null)
		{
			Context = context.ToList();
		}
		Types = modelTypes;
		Limit = limit;
		IsPartial = isPartial;
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}
}
