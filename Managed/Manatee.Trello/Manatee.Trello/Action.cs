using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.DataAccess;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Json;

namespace Manatee.Trello;

public class Action : IAction, ICacheable, IRefreshable, IMergeJson<IJsonAction>, IBatchRefresh, IBatchRefreshable, IHandleSynchronization
{
	[Flags]
	public enum Fields
	{
		[Display(Description = "data")]
		Data = 1,
		[Display(Description = "date")]
		Date = 2,
		[Display(Description = "memberCreator")]
		Creator = 4,
		[Display(Description = "type")]
		Type = 8,
		[Display(Description = "reactions")]
		Reactions = 0x10
	}

	private static readonly Dictionary<ActionType, Func<Action, string>> StringDefinitions;

	private readonly Field<Member> _creator;

	private readonly Field<DateTime?> _date;

	private readonly Field<ActionType?> _type;

	private readonly ActionContext _context;

	private string _id;

	private DateTime? _creation;

	private static Fields _downloadedFields;

	public static Fields DownloadedFields
	{
		get
		{
			return _downloadedFields;
		}
		set
		{
			_downloadedFields = value;
			ActionContext.UpdateParameters();
		}
	}

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

	public IMember Creator => _creator.Value;

	public IActionData Data { get; }

	public DateTime? Date => _date.Value;

	public string Id
	{
		get
		{
			if (!_context.HasValidId)
			{
				_context.Synchronize(force: true, CancellationToken.None).Wait();
			}
			return _id;
		}
		private set
		{
			_id = value;
		}
	}

	public ICommentReactionCollection Reactions => _context.Reactions;

	public ActionType? Type => _type.Value;

	internal IJsonAction Json
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

	public event Action<IAction, IEnumerable<string>> Updated;

	static Action()
	{
		StringDefinitions = new Dictionary<ActionType, Func<Action, string>>
		{
			{
				ActionType.AddAttachmentToCard,
				(Action a) => $"{a.Creator} attached {a.Data.Attachment} to card {a.Data.Card}."
			},
			{
				ActionType.AddChecklistToCard,
				(Action a) => $"{a.Creator} added checklist {a.Data.CheckList} to card {a.Data.Card}."
			},
			{
				ActionType.AddMemberToBoard,
				(Action a) => $"{a.Creator} added member {a.Data.Member} to board {a.Data.Board}."
			},
			{
				ActionType.AddMemberToCard,
				(Action a) => $"{a.Creator} assigned member {a.Data.Member} to card {a.Data.Card}."
			},
			{
				ActionType.AddMemberToOrganization,
				(Action a) => $"{a.Creator} added member {a.Data.Member} to organization {a.Data.Organization}."
			},
			{
				ActionType.AddToOrganizationBoard,
				(Action a) => $"{a.Creator} moved board {a.Data.Board} into organization {a.Data.Organization}."
			},
			{
				ActionType.CommentCard,
				(Action a) => $"{a.Creator} commented on card {a.Data.Card}: '{a.Data.Text}'."
			},
			{
				ActionType.ConvertToCardFromCheckItem,
				(Action a) => $"{a.Creator} converted checkitem {a.Data.Card} to a card."
			},
			{
				ActionType.CopyBoard,
				(Action a) => $"{a.Creator} copied board {a.Data.Board} from board {a.Data.BoardSource}."
			},
			{
				ActionType.CopyCard,
				(Action a) => $"{a.Creator} copied card {a.Data.Card} from card {a.Data.CardSource}."
			},
			{
				ActionType.CopyCommentCard,
				(Action a) => $"{a.Creator} copied a comment from {a.Data.Card}: '{a.Data.Text}'."
			},
			{
				ActionType.CreateBoard,
				(Action a) => $"{a.Creator} created board {a.Data.Board}."
			},
			{
				ActionType.CreateCard,
				(Action a) => $"{a.Creator} created card {a.Data.Card}."
			},
			{
				ActionType.CreateList,
				(Action a) => $"{a.Creator} created list {a.Data.List}."
			},
			{
				ActionType.CreateOrganization,
				(Action a) => $"{a.Creator} created organization {a.Data.Organization}."
			},
			{
				ActionType.DeleteAttachmentFromCard,
				(Action a) => $"{a.Creator} removed attachment {a.Data.Attachment} from card {a.Data.Card}."
			},
			{
				ActionType.DeleteBoardInvitation,
				(Action a) => $"{a.Creator} rescinded an invitation."
			},
			{
				ActionType.DeleteCard,
				(Action a) => $"{a.Creator} deleted card #{((Card)a.Data.Card).Json.IdShort} from {a.Data.Board}."
			},
			{
				ActionType.DeleteOrganizationInvitation,
				(Action a) => $"{a.Creator} rescinded an invitation."
			},
			{
				ActionType.DisablePowerUp,
				(Action a) => $"{a.Creator} disabled power-up {a.Data.Value}."
			},
			{
				ActionType.EmailCard,
				(Action a) => $"{a.Creator} added card {a.Data.Card} by email."
			},
			{
				ActionType.EnablePowerUp,
				(Action a) => $"{a.Creator} enabled power-up {a.Data.Value}."
			},
			{
				ActionType.MakeAdminOfBoard,
				(Action a) => $"{a.Creator} made member {a.Data.Member} an admin of board {a.Data.Board}."
			},
			{
				ActionType.MakeNormalMemberOfBoard,
				(Action a) => $"{a.Creator} made member {a.Data.Member} a normal user of board {a.Data.Board}."
			},
			{
				ActionType.MakeNormalMemberOfOrganization,
				(Action a) => $"{a.Creator} made member {a.Data.Member} a normal user of organization {a.Data.Organization}."
			},
			{
				ActionType.MakeObserverOfBoard,
				(Action a) => $"{a.Creator} made member {a.Data.Member} an observer of board {a.Data.Board}."
			},
			{
				ActionType.MemberJoinedTrello,
				(Action a) => $"{a.Creator} joined Trello!."
			},
			{
				ActionType.MoveCardFromBoard,
				(Action a) => $"{a.Creator} moved card {a.Data.Card} from board {a.Data.Board} to board {a.Data.BoardTarget}."
			},
			{
				ActionType.MoveCardToBoard,
				(Action a) => $"{a.Creator} moved card {a.Data.Card} from board {a.Data.BoardSource} to board {a.Data.Board}."
			},
			{
				ActionType.MoveListFromBoard,
				(Action a) => $"{a.Creator} moved list {a.Data.List} from board {a.Data.Board}."
			},
			{
				ActionType.MoveListToBoard,
				(Action a) => $"{a.Creator} moved list {a.Data.List} to board {a.Data.Board}."
			},
			{
				ActionType.RemoveChecklistFromCard,
				(Action a) => $"{a.Creator} deleted checklist {a.Data.CheckList} from card {a.Data.Card}."
			},
			{
				ActionType.RemoveFromOrganizationBoard,
				(Action a) => $"{a.Creator} removed board {a.Data.Board} from organization {a.Data.Organization}."
			},
			{
				ActionType.RemoveMemberFromCard,
				(Action a) => $"{a.Creator} removed member {a.Data.Member} from card {a.Data.Card}."
			},
			{
				ActionType.UnconfirmedBoardInvitation,
				(Action a) => $"{a.Creator} invited {a.Data.Member} to board {a.Data.Board}."
			},
			{
				ActionType.UnconfirmedOrganizationInvitation,
				(Action a) => $"{a.Creator} invited {a.Data.Member} to organization {a.Data.Organization}."
			},
			{
				ActionType.UpdateBoard,
				(Action a) => $"{a.Creator} updated board {a.Data.Board}."
			},
			{
				ActionType.UpdateCard,
				(Action a) => $"{a.Creator} updated card {a.Data.Card}."
			},
			{
				ActionType.UpdateCheckItemStateOnCard,
				(Action a) => $"{a.Creator} updated checkitem {a.Data.CheckItem}."
			},
			{
				ActionType.UpdateChecklist,
				(Action a) => $"{a.Creator} updated checklist {a.Data.CheckList}."
			},
			{
				ActionType.UpdateList,
				(Action a) => $"{a.Creator} updated list {a.Data.List}."
			},
			{
				ActionType.UpdateMember,
				(Action a) => $"{a.Creator} updated their profile."
			},
			{
				ActionType.UpdateOrganization,
				(Action a) => $"{a.Creator} updated organization {a.Data.Organization}."
			},
			{
				ActionType.EnablePlugin,
				(Action a) => $"{a.Creator} enabled plugin {a.Data.PowerUp}."
			},
			{
				ActionType.DisablePlugin,
				(Action a) => $"{a.Creator} disabled plugin {a.Data.PowerUp}."
			},
			{
				ActionType.AddAdminToBoard,
				(Action a) => $"{a.Creator} added {a.Data.Member} to board {a.Data.Board} as an admin."
			},
			{
				ActionType.AddAdminToOrganization,
				(Action a) => $"{a.Creator} added {a.Data.Member} to organization {a.Data.Organization} as an admin."
			},
			{
				ActionType.AddBoardsPinnedToMember,
				(Action a) => $"{a.Creator} pinned board {a.Data.Board}."
			},
			{
				ActionType.AddLabelToCard,
				(Action a) => $"{a.Creator} added label {a.Data.Label} to card {a.Data.Card}."
			},
			{
				ActionType.CopyChecklist,
				(Action a) => $"{a.Creator} copied {a.Data.CheckList}."
			},
			{
				ActionType.CreateBoardInvitation,
				(Action a) => $"{a.Creator} invited {a.Data.Member} to board {a.Data.Board}."
			},
			{
				ActionType.CreateBoardPreference,
				(Action a) => $"{a.Creator} updated preferences on board {a.Data.Board}."
			},
			{
				ActionType.CreateChecklist,
				(Action a) => $"{a.Creator} created checklist {a.Data.CheckList}."
			},
			{
				ActionType.CreateCustomField,
				(Action a) => $"{a.Creator} created custom field {a.Data.CustomField}."
			},
			{
				ActionType.CreateLabel,
				(Action a) => $"{a.Creator} created label {a.Data.Label}."
			},
			{
				ActionType.CreateOrganizationInvitation,
				(Action a) => $"{a.Creator} invited {a.Data.Member} to organization {a.Data.Organization}."
			},
			{
				ActionType.DeleteCheckItem,
				(Action a) => $"{a.Creator} deleted check item {a.Data.CheckItem}."
			},
			{
				ActionType.DeleteCustomField,
				(Action a) => $"{a.Creator} deleted custom field {a.Data.CustomField}."
			},
			{
				ActionType.DeleteLabel,
				(Action a) => $"{a.Creator} deleted label {a.Data.Label}."
			},
			{
				ActionType.MakeAdminOfOrganization,
				(Action a) => $"{a.Creator} made {a.Data.Member} of organization {a.Data.Organization}."
			},
			{
				ActionType.RemoveAdminFromBoard,
				(Action a) => $"{a.Creator} removed {a.Data.Member} as an admin from board {a.Data.Board}."
			},
			{
				ActionType.RemoveAdminFromOrganization,
				(Action a) => $"{a.Creator} removed {a.Data.Member} as an admin from organization {a.Data.Organization}."
			},
			{
				ActionType.RemoveBoardsPinnedFromMember,
				(Action a) => $"{a.Creator} unpinned board {a.Data.Board}."
			},
			{
				ActionType.RemoveLabelFromCard,
				(Action a) => $"{a.Creator} removed label {a.Data.Label} from card {a.Data.Card}."
			},
			{
				ActionType.RemoveMemberFromBoard,
				(Action a) => $"{a.Creator} removed {a.Data.Member} from board {a.Data.Board}."
			},
			{
				ActionType.RemoveMemberFromOrganization,
				(Action a) => $"{a.Creator} removed {a.Data.Member} from organization {a.Data.Organization}."
			},
			{
				ActionType.UpdateCheckItem,
				(Action a) => $"{a.Creator} updated check item {a.Data.CheckItem}."
			},
			{
				ActionType.UpdateLabel,
				(Action a) => $"{a.Creator} updated label {a.Data.Label}."
			},
			{
				ActionType.VoteOnCard,
				(Action a) => $"{a.Creator} voted for card {a.Data.Card}."
			},
			{
				ActionType.UpdateCustomField,
				(Action a) => $"{a.Creator} updated the definition of custom field {a.Data.CustomField} on board {a.Data.Board}."
			},
			{
				ActionType.UpdateCustomFieldItem,
				(Action a) => $"{a.Creator} updated custom field {a.Data.CustomField} on card {a.Data.Card}."
			},
			{
				ActionType.ReopenBoard,
				(Action a) => $"{a.Creator} reopened board {a.Data.Board}."
			},
			{
				ActionType.RemoveOrganizationFromEnterprise,
				(Action a) => $"{a.Creator} removed organization {a.Data.Organization} from an enterprise account."
			},
			{
				ActionType.ReactionAdded,
				(Action a) => $"{a.Creator} reacted to a comment on card {a.Data.Card}."
			},
			{
				ActionType.ReactionRemoved,
				(Action a) => $"{a.Creator} removed a reaction from a comment on card {a.Data.Card}."
			},
			{
				ActionType.ReactivatedMemberInBoard,
				(Action a) => $"{a.Creator} reactivated a member for board {a.Data.Board}."
			},
			{
				ActionType.ReactivatedMemberInEnterprise,
				(Action a) => $"{a.Creator} reactivated a member for an enterprise account."
			},
			{
				ActionType.ReactivatedMemberInOrganization,
				(Action a) => $"{a.Creator} reactivated a member for organization {a.Data.Organization}."
			},
			{
				ActionType.RemoveFromEnterprisePluginWhitelist,
				(Action a) => $"{a.Creator} removed a plugin from an enterprise plugin whitelist."
			},
			{
				ActionType.EnableEnterprisePluginWhitelist,
				(Action a) => $"{a.Creator} enabled the plugin whitelist for an enterprise account."
			},
			{
				ActionType.DisableEnterprisePluginWhitelist,
				(Action a) => $"{a.Creator} disabled the plugin whitelist for an enterprise account."
			},
			{
				ActionType.DeactivatedMemberInBoard,
				(Action a) => $"{a.Creator} deactivated {a.Data.Member} on board {a.Data.Board}."
			},
			{
				ActionType.DeactivatedMemberInEnterprise,
				(Action a) => $"{a.Creator} deactivated {a.Data.Member} in an enterprise account."
			},
			{
				ActionType.DeactivatedMemberInOrganization,
				(Action a) => $"{a.Creator} deactivated {a.Data.Member} in organization {a.Data.Organization}."
			},
			{
				ActionType.AddOrganizationToEnterprise,
				(Action a) => $"{a.Creator} added organization {a.Data.Organization} to an enterprise account."
			},
			{
				ActionType.AddToEnterprisePluginWhitelist,
				(Action a) => $"{a.Creator} added a plugin to an enterprise plugin whitelist."
			},
			{
				ActionType.AcceptEnterpriseJoinRequest,
				(Action a) => $"{a.Creator} accepted an invitation to join an enterprise account."
			}
		};
		DownloadedFields = (Fields)Enum.GetValues(typeof(Fields)).Cast<int>().Sum();
	}

	public Action(string id, TrelloAuthorization auth = null)
	{
		Id = id;
		_context = new ActionContext(id, auth);
		_context.Synchronized.Add(this);
		_creator = new Field<Member>(_context, "Creator");
		_date = new Field<DateTime?>(_context, "Date");
		Data = new ActionData(_context.ActionDataContext);
		_type = new Field<ActionType?>(_context, "Type");
		if (auth != TrelloAuthorization.Null)
		{
			TrelloConfiguration.Cache.Add(this);
		}
	}

	internal Action(IJsonAction json, TrelloAuthorization auth)
		: this(json.Id, auth)
	{
		_context.Merge(json);
	}

	public async Task Delete(CancellationToken ct = default(CancellationToken))
	{
		await _context.Delete(ct);
		if (TrelloConfiguration.RemoveDeletedItemsFromCache)
		{
			TrelloConfiguration.Cache.Remove(this);
		}
	}

	public Task Refresh(bool force = false, CancellationToken ct = default(CancellationToken))
	{
		return _context.Synchronize(force, ct);
	}

	void IMergeJson<IJsonAction>.Merge(IJsonAction json, bool overwrite)
	{
		_context.Merge(json, overwrite);
	}

	public override string ToString()
	{
		if (!Type.HasValue || !(Type != ActionType.Unknown))
		{
			return "Action type could not be determined.";
		}
		return StringDefinitions[Type.Value](this);
	}

	Endpoint IBatchRefresh.GetRefreshEndpoint()
	{
		return _context.GetRefreshEndpoint();
	}

	void IBatchRefresh.Apply(string content)
	{
		IJsonAction json = TrelloConfiguration.Deserializer.Deserialize<IJsonAction>(content);
		_context.Merge(json);
	}

	void IHandleSynchronization.HandleSynchronized(IEnumerable<string> properties)
	{
		Id = _context.Data.Id;
		this.Updated?.Invoke(this, properties);
	}
}
