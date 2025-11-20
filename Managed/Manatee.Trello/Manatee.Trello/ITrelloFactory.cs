using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Manatee.Trello;

public interface ITrelloFactory
{
	IAction Action(string id, TrelloAuthorization auth = null);

	IBoard Board(string id, TrelloAuthorization auth = null);

	ICard Card(string id, TrelloAuthorization auth = null);

	ICheckList CheckList(string id, TrelloAuthorization auth = null);

	IList List(string id, TrelloAuthorization auth = null);

	Task<IMe> Me(TrelloAuthorization auth = null, CancellationToken ct = default(CancellationToken));

	IMember Member(string id, TrelloAuthorization auth = null);

	IMemberSearch MemberSearch(string query, int? limit = null, IBoard board = null, IOrganization organization = null, bool? restrictToOrganization = null, TrelloAuthorization auth = null);

	INotification Notification(string id, TrelloAuthorization auth = null);

	IOrganization Organization(string id, TrelloAuthorization auth = null);

	ISearchQuery SearchQuery();

	ISearch Search(ISearchQuery query, int? limit = null, SearchModelType modelTypes = SearchModelType.All, IEnumerable<IQueryable> context = null, bool isPartial = false, TrelloAuthorization auth = null);

	ISearch Search(string query, int? limit = null, SearchModelType modelTypes = SearchModelType.All, IEnumerable<IQueryable> context = null, bool isPartial = false, TrelloAuthorization auth = null);

	IToken Token(string id, TrelloAuthorization auth = null);

	Task<IWebhook<T>> Webhook<T>(T target, string callBackUrl, string description = null, TrelloAuthorization auth = null, CancellationToken ct = default(CancellationToken)) where T : class, ICanWebhook;

	IWebhook<T> Webhook<T>(string id, TrelloAuthorization auth = null) where T : class, ICanWebhook;

	IDropDownOption DropDownOption(string text, LabelColor color = LabelColor.None);
}
