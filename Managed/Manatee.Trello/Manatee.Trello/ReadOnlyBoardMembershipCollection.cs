using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class ReadOnlyBoardMembershipCollection : ReadOnlyCollection<IBoardMembership>, IReadOnlyBoardMembershipCollection, IReadOnlyCollection<IBoardMembership>, IEnumerable<IBoardMembership>, IEnumerable, IRefreshable
{
	public IBoardMembership this[string key] => GetByKey(key);

	internal ReadOnlyBoardMembershipCollection(Func<string> getOwnerId, TrelloAuthorization auth)
		: base(getOwnerId, auth)
	{
	}

	internal sealed override async Task PerformRefresh(bool force, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Board_Read_Memberships, new Dictionary<string, object> { { "_id", base.OwnerId } });
		List<IJsonBoardMembership> source = await JsonRepository.Execute<List<IJsonBoardMembership>>(base.Auth, endpoint, ct, base.AdditionalParameters);
		base.Items.Clear();
		base.Items.AddRange(source.Select(delegate(IJsonBoardMembership jbm)
		{
			BoardMembership obj = TrelloConfiguration.Cache.Find<BoardMembership>(jbm.Id) ?? new BoardMembership(jbm, base.OwnerId, base.Auth);
			obj.Json = jbm;
			return obj;
		}));
	}

	private IBoardMembership GetByKey(string key)
	{
		return this.FirstOrDefault((IBoardMembership bm) => key.In(bm.Id, bm.Member.Id, bm.Member.FullName, bm.Member.UserName));
	}

	public void Filter(MembershipFilter membership)
	{
		IEnumerable<MembershipFilter> memberships = membership.GetFlags().Cast<MembershipFilter>();
		Filter(memberships);
	}

	public void Filter(IEnumerable<MembershipFilter> memberships)
	{
		string text = (string)base.AdditionalParameters["filter"];
		if (!text.IsNullOrWhiteSpace())
		{
			text += ",";
		}
		text += memberships.Select((MembershipFilter a) => a.GetDescription()).Join(",");
		base.AdditionalParameters["filter"] = text;
	}
}
