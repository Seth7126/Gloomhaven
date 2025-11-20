using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class NotificationData : INotificationData
{
	private readonly Field<Attachment> _attachment;

	private readonly Field<Board> _board;

	private readonly Field<Board> _boardSource;

	private readonly Field<Board> _boardTarget;

	private readonly Field<Card> _card;

	private readonly Field<Card> _cardSource;

	private readonly Field<CheckItem> _checkItem;

	private readonly Field<CheckList> _checkList;

	private readonly Field<List> _list;

	private readonly Field<List> _listAfter;

	private readonly Field<List> _listBefore;

	private readonly Field<Member> _member;

	private readonly Field<bool?> _wasArchived;

	private readonly Field<string> _oldDescription;

	private readonly Field<List> _oldList;

	private readonly Field<Position> _oldPosition;

	private readonly Field<string> _oldText;

	private readonly Field<Organization> _organization;

	private readonly Field<string> _text;

	private readonly NotificationDataContext _context;

	public IAttachment Attachment => _attachment.Value;

	public IBoard Board => _board.Value;

	public IBoard BoardSource => _boardSource.Value;

	public IBoard BoardTarget => _boardTarget.Value;

	public ICard Card => _card.Value;

	public ICard CardSource => _cardSource.Value;

	public ICheckItem CheckItem => _checkItem.Value;

	public ICheckList CheckList => _checkList.Value;

	public IList List => _list.Value;

	public IList ListAfter => _listAfter.Value;

	public IList ListBefore => _listBefore.Value;

	public IMember Member => _member.Value;

	public string OldDescription => _oldDescription.Value;

	public IList OldList => _oldList.Value;

	public Position OldPosition => _oldPosition.Value;

	public string OldText => _oldText.Value;

	public IOrganization Organization => _organization.Value;

	public string Text
	{
		get
		{
			return _text.Value;
		}
		set
		{
			_text.Value = value;
		}
	}

	public bool? WasArchived => _wasArchived.Value;

	internal NotificationData(NotificationDataContext context)
	{
		_context = context;
		_attachment = new Field<Attachment>(_context, "Attachment");
		_board = new Field<Board>(_context, "Board");
		_boardSource = new Field<Board>(_context, "BoardSource");
		_boardTarget = new Field<Board>(_context, "BoardTarget");
		_card = new Field<Card>(_context, "Card");
		_cardSource = new Field<Card>(_context, "CardSource");
		_checkItem = new Field<CheckItem>(_context, "CheckItem");
		_checkList = new Field<CheckList>(_context, "CheckList");
		_list = new Field<List>(_context, "List");
		_listAfter = new Field<List>(_context, "ListAfter");
		_listBefore = new Field<List>(_context, "ListBefore");
		_member = new Field<Member>(_context, "Member");
		_wasArchived = new Field<bool?>(_context, "WasArchived");
		_oldDescription = new Field<string>(_context, "OldDescription");
		_oldList = new Field<List>(_context, "OldList");
		_oldPosition = new Field<Position>(_context, "OldPosition");
		_oldText = new Field<string>(_context, "OldText");
		_organization = new Field<Organization>(_context, "Organization");
		_text = new Field<string>(_context, "Text");
		_text.AddRule(OldValueNotNullOrWhiteSpaceRule.Instance);
	}
}
