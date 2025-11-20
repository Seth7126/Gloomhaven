using System;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Me : Member, IMe, IMember, ICanWebhook, ICacheable, IRefreshable
{
	private static IJsonMember _myJson;

	private readonly Field<string> _email;

	[Obsolete("Trello has deprecated this property.")]
	public new AvatarSource? AvatarSource
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public new string Bio
	{
		get
		{
			return base.Bio;
		}
		set
		{
			base.Bio = value;
		}
	}

	public new IBoardCollection Boards => (IBoardCollection)base.Boards;

	public new IBoardBackgroundCollection BoardBackgrounds => (IBoardBackgroundCollection)base.BoardBackgrounds;

	public string Email
	{
		get
		{
			return _email.Value;
		}
		set
		{
			_email.Value = value;
		}
	}

	public new string FullName
	{
		get
		{
			return base.FullName;
		}
		set
		{
			base.FullName = value;
		}
	}

	public new string Initials
	{
		get
		{
			return base.Initials;
		}
		set
		{
			base.Initials = value;
		}
	}

	public IReadOnlyNotificationCollection Notifications => _context.Notifications;

	public new IOrganizationCollection Organizations => (IOrganizationCollection)base.Organizations;

	public IMemberPreferences Preferences { get; }

	public new IStarredBoardCollection StarredBoards => (IStarredBoardCollection)base.StarredBoards;

	public new string UserName
	{
		get
		{
			return base.UserName;
		}
		set
		{
			base.UserName = value;
		}
	}

	internal Me(string id, TrelloAuthorization auth)
		: base(id, isMe: true, auth)
	{
		_email = new Field<string>(_context, "Email");
		Preferences = new MemberPreferences(_context.MemberPreferencesContext);
		_context.Merge(_myJson);
	}

	internal static async Task<string> GetId(TrelloAuthorization auth, CancellationToken ct)
	{
		Endpoint endpoint = EndpointFactory.Build(EntityRequestType.Service_Read_Me);
		_myJson = await JsonRepository.Execute<IJsonMember>(auth, endpoint, ct);
		Member member = TrelloConfiguration.Cache.Find<Member>(_myJson.Id);
		if (member != null)
		{
			TrelloConfiguration.Cache.Remove(member);
		}
		return _myJson.Id;
	}
}
