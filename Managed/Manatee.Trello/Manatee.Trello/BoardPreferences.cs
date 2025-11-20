using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class BoardPreferences : IBoardPreferences
{
	private readonly Field<BoardPermissionLevel?> _permissionLevel;

	private readonly Field<BoardVotingPermission?> _voting;

	private readonly Field<BoardCommentPermission?> _commenting;

	private readonly Field<BoardInvitationPermission?> _invitations;

	private readonly Field<bool?> _allowSelfJoin;

	private readonly Field<bool?> _showCardCovers;

	private readonly Field<bool?> _isCalendarFeedEnabled;

	private readonly Field<CardAgingStyle?> _cardAgingStyle;

	private readonly Field<BoardBackground> _background;

	private readonly BoardPreferencesContext _context;

	public BoardPermissionLevel? PermissionLevel
	{
		get
		{
			return _permissionLevel.Value;
		}
		set
		{
			_permissionLevel.Value = value;
		}
	}

	public BoardVotingPermission? Voting
	{
		get
		{
			return _voting.Value;
		}
		set
		{
			_voting.Value = value;
		}
	}

	public BoardCommentPermission? Commenting
	{
		get
		{
			return _commenting.Value;
		}
		set
		{
			_commenting.Value = value;
		}
	}

	public BoardInvitationPermission? Invitations
	{
		get
		{
			return _invitations.Value;
		}
		set
		{
			_invitations.Value = value;
		}
	}

	public bool? AllowSelfJoin
	{
		get
		{
			return _allowSelfJoin.Value;
		}
		set
		{
			_allowSelfJoin.Value = value;
		}
	}

	public bool? ShowCardCovers
	{
		get
		{
			return _showCardCovers.Value;
		}
		set
		{
			_showCardCovers.Value = value;
		}
	}

	public bool? IsCalendarFeedEnabled
	{
		get
		{
			return _isCalendarFeedEnabled.Value;
		}
		set
		{
			_isCalendarFeedEnabled.Value = value;
		}
	}

	public CardAgingStyle? CardAgingStyle
	{
		get
		{
			return _cardAgingStyle.Value;
		}
		set
		{
			_cardAgingStyle.Value = value;
		}
	}

	public IBoardBackground Background
	{
		get
		{
			return _background.Value;
		}
		set
		{
			_background.Value = (BoardBackground)value;
		}
	}

	internal BoardPreferences(BoardPreferencesContext context)
	{
		_context = context;
		_permissionLevel = new Field<BoardPermissionLevel?>(_context, "PermissionLevel");
		_permissionLevel.AddRule(NullableHasValueRule<BoardPermissionLevel>.Instance);
		_permissionLevel.AddRule(EnumerationRule<BoardPermissionLevel?>.Instance);
		_voting = new Field<BoardVotingPermission?>(_context, "Voting");
		_voting.AddRule(NullableHasValueRule<BoardVotingPermission>.Instance);
		_voting.AddRule(EnumerationRule<BoardVotingPermission?>.Instance);
		_commenting = new Field<BoardCommentPermission?>(_context, "Commenting");
		_commenting.AddRule(NullableHasValueRule<BoardCommentPermission>.Instance);
		_commenting.AddRule(EnumerationRule<BoardCommentPermission?>.Instance);
		_invitations = new Field<BoardInvitationPermission?>(_context, "Invitations");
		_invitations.AddRule(NullableHasValueRule<BoardInvitationPermission>.Instance);
		_invitations.AddRule(EnumerationRule<BoardInvitationPermission?>.Instance);
		_allowSelfJoin = new Field<bool?>(_context, "AllowSelfJoin");
		_allowSelfJoin.AddRule(NullableHasValueRule<bool>.Instance);
		_showCardCovers = new Field<bool?>(_context, "ShowCardCovers");
		_showCardCovers.AddRule(NullableHasValueRule<bool>.Instance);
		_isCalendarFeedEnabled = new Field<bool?>(_context, "IsCalendarFeedEnabled");
		_isCalendarFeedEnabled.AddRule(NullableHasValueRule<bool>.Instance);
		_cardAgingStyle = new Field<CardAgingStyle?>(_context, "CardAgingStyle");
		_cardAgingStyle.AddRule(NullableHasValueRule<Manatee.Trello.CardAgingStyle>.Instance);
		_cardAgingStyle.AddRule(EnumerationRule<Manatee.Trello.CardAgingStyle?>.Instance);
		_background = new Field<BoardBackground>(_context, "Background");
		_background.AddRule(NotNullRule<BoardBackground>.Instance);
	}
}
