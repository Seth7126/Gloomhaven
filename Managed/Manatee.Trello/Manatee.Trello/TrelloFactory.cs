using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public class TrelloFactory : ITrelloFactory
{
	public IAction Action(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<Action>(id) ?? new Action(id, auth);
	}

	public IBoard Board(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<Board>(id) ?? new Board(id, auth);
	}

	public ICard Card(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<Card>(id) ?? new Card(id, auth);
	}

	public ICheckList CheckList(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<CheckList>(id) ?? new CheckList(id, auth);
	}

	public IDropDownOption DropDownOption(string text, LabelColor color = LabelColor.None)
	{
		return Manatee.Trello.DropDownOption.Create(text, color);
	}

	public IList List(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<List>(id) ?? new List(id, auth);
	}

	public async Task<IMe> Me(TrelloAuthorization auth = null, CancellationToken ct = default(CancellationToken))
	{
		string id = await Manatee.Trello.Me.GetId(auth ?? TrelloAuthorization.Default, ct);
		Me me = TrelloConfiguration.Cache.Find<Me>(id) ?? new Me(id, auth ?? TrelloAuthorization.Default);
		await me.Refresh(force: false, ct);
		return me;
	}

	public IMember Member(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<Member>(id) ?? new Member(id, auth);
	}

	public IMemberSearch MemberSearch(string query, int? limit = null, IBoard board = null, IOrganization organization = null, bool? restrictToOrganization = null, TrelloAuthorization auth = null)
	{
		return new MemberSearch(query, limit, board, organization, restrictToOrganization, auth);
	}

	public INotification Notification(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<Notification>(id) ?? new Notification(id, auth);
	}

	public IOrganization Organization(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<Organization>(id) ?? new Organization(id, auth);
	}

	public ISearchQuery SearchQuery()
	{
		return new SearchQuery();
	}

	public ISearch Search(ISearchQuery query, int? limit = null, SearchModelType modelTypes = SearchModelType.All, IEnumerable<IQueryable> context = null, bool isPartial = false, TrelloAuthorization auth = null)
	{
		return new Search(query, limit, modelTypes, context, auth, isPartial);
	}

	public ISearch Search(string query, int? limit = null, SearchModelType modelTypes = SearchModelType.All, IEnumerable<IQueryable> context = null, bool isPartial = false, TrelloAuthorization auth = null)
	{
		return new Search(query, limit, modelTypes, context, auth, isPartial);
	}

	public IToken Token(string id, TrelloAuthorization auth = null)
	{
		return TrelloConfiguration.Cache.Find<Token>(id) ?? new Token(id, auth);
	}

	public async Task<IWebhook<T>> Webhook<T>(T target, string callBackUrl, string description = null, TrelloAuthorization auth = null, CancellationToken ct = default(CancellationToken)) where T : class, ICanWebhook
	{
		return await Manatee.Trello.Webhook<T>.Create(target, callBackUrl, description, auth, ct);
	}

	public IWebhook<T> Webhook<T>(string id, TrelloAuthorization auth = null) where T : class, ICanWebhook
	{
		return TrelloConfiguration.Cache.Find<Webhook<T>>(id) ?? new Webhook<T>(id, auth);
	}
}
