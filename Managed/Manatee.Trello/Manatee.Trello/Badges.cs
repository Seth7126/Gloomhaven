using System;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;

namespace Manatee.Trello;

public class Badges : IBadges
{
	private readonly Field<int?> _attachments;

	private readonly Field<int?> _checkItems;

	private readonly Field<int?> _checkItemsChecked;

	private readonly Field<int?> _comments;

	private readonly Field<DateTime?> _dueDate;

	private readonly Field<string> _fogBugz;

	private readonly Field<bool?> _hasDescription;

	private readonly Field<bool?> _hasVoted;

	private readonly Field<bool?> _isComplete;

	private readonly Field<bool?> _isSubscribed;

	private readonly Field<int?> _votes;

	private readonly BadgesContext _context;

	public int? Attachments => _attachments.Value;

	public int? CheckItems => _checkItems.Value;

	public int? CheckItemsChecked => _checkItemsChecked.Value;

	public int? Comments => _comments.Value;

	public DateTime? DueDate => _dueDate.Value;

	public string FogBugz => _fogBugz.Value;

	public bool? HasDescription => _hasDescription.Value;

	public bool? HasVoted => _hasVoted.Value;

	public bool? IsComplete => _isComplete.Value;

	public bool? IsSubscribed => _isSubscribed.Value;

	public int? Votes => _votes.Value;

	internal Badges(BadgesContext context)
	{
		_context = context;
		_attachments = new Field<int?>(_context, "Attachments");
		_checkItems = new Field<int?>(_context, "CheckItems");
		_checkItemsChecked = new Field<int?>(_context, "CheckItemsChecked");
		_comments = new Field<int?>(_context, "Comments");
		_dueDate = new Field<DateTime?>(_context, "DueDate");
		_fogBugz = new Field<string>(_context, "FogBugz");
		_hasDescription = new Field<bool?>(_context, "HasDescription");
		_hasVoted = new Field<bool?>(_context, "HasVoted");
		_isComplete = new Field<bool?>(_context, "IsComplete");
		_isSubscribed = new Field<bool?>(_context, "IsSubscribed");
		_votes = new Field<int?>(_context, "Votes");
	}
}
