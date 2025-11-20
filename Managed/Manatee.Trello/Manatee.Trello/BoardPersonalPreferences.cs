using System;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Synchronization;
using Manatee.Trello.Internal.Validation;

namespace Manatee.Trello;

public class BoardPersonalPreferences : IBoardPersonalPreferences
{
	private readonly Field<List> _emailList;

	private readonly Field<Position> _emailPosition;

	private readonly Field<bool?> _showListGuide;

	private readonly Field<bool?> _showSidebar;

	private readonly Field<bool?> _showSidebarActivity;

	private readonly Field<bool?> _showSidebarBoardActions;

	private readonly Field<bool?> _showSidebarMembers;

	private readonly BoardPersonalPreferencesContext _context;

	public IList EmailList
	{
		get
		{
			return _emailList.Value;
		}
		set
		{
			_emailList.Value = (List)value;
		}
	}

	public Position EmailPosition
	{
		get
		{
			return _emailPosition.Value;
		}
		set
		{
			_emailPosition.Value = value;
		}
	}

	public bool? ShowListGuide
	{
		get
		{
			return _showListGuide.Value;
		}
		set
		{
			_showListGuide.Value = value;
		}
	}

	public bool? ShowSidebar
	{
		get
		{
			return _showSidebar.Value;
		}
		set
		{
			_showSidebar.Value = value;
		}
	}

	public bool? ShowSidebarActivity
	{
		get
		{
			return _showSidebarActivity.Value;
		}
		set
		{
			_showSidebarActivity.Value = value;
		}
	}

	public bool? ShowSidebarBoardActions
	{
		get
		{
			return _showSidebarBoardActions.Value;
		}
		set
		{
			_showSidebarBoardActions.Value = value;
		}
	}

	public bool? ShowSidebarMembers
	{
		get
		{
			return _showSidebarMembers.Value;
		}
		set
		{
			_showSidebarMembers.Value = value;
		}
	}

	internal BoardPersonalPreferences(Func<string> getOwnerId, TrelloAuthorization auth)
	{
		_context = new BoardPersonalPreferencesContext(getOwnerId, auth);
		_emailList = new Field<List>(_context, "EmailList");
		_emailList.AddRule(NotNullRule<List>.Instance);
		_emailPosition = new Field<Position>(_context, "EmailPosition");
		_emailPosition.AddRule(NotNullRule<Position>.Instance);
		_showListGuide = new Field<bool?>(_context, "ShowListGuide");
		_showListGuide.AddRule(NullableHasValueRule<bool>.Instance);
		_showSidebar = new Field<bool?>(_context, "ShowSidebar");
		_showSidebar.AddRule(NullableHasValueRule<bool>.Instance);
		_showSidebarActivity = new Field<bool?>(_context, "ShowSidebarActivity");
		_showSidebarActivity.AddRule(NullableHasValueRule<bool>.Instance);
		_showSidebarBoardActions = new Field<bool?>(_context, "ShowSidebarBoardActions");
		_showSidebarBoardActions.AddRule(NullableHasValueRule<bool>.Instance);
		_showSidebarMembers = new Field<bool?>(_context, "ShowSidebarMembers");
		_showSidebarMembers.AddRule(NullableHasValueRule<bool>.Instance);
	}
}
