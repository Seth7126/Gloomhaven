using System.Collections.Generic;
using System.Linq;
using Manatee.Trello.Internal;
using Manatee.Trello.Internal.Searching;

namespace Manatee.Trello;

public class SearchQuery : ISearchQuery
{
	private readonly List<ISearchParameter> _parameters = new List<ISearchParameter>();

	public ISearchQuery Text(string text)
	{
		_parameters.Add(new TextSearchParameter(text));
		return this;
	}

	public ISearchQuery TextInName(string text)
	{
		_parameters.Add(new TextInCardNameSearchParameter(text));
		return this;
	}

	public ISearchQuery TextInDescription(string text)
	{
		_parameters.Add(new TextInCardDescriptionSearchParameter(text));
		return this;
	}

	public ISearchQuery TextInComments(string text)
	{
		_parameters.Add(new TextInCardCommentSearchParameter(text));
		return this;
	}

	public ISearchQuery TextInCheckLists(string text)
	{
		_parameters.Add(new TextInCardCheckListSearchParameter(text));
		return this;
	}

	public ISearchQuery Member(IMember member)
	{
		_parameters.Add(new MemberSearchParameter((Member)member));
		return this;
	}

	public ISearchQuery Label(ILabel label)
	{
		_parameters.Add(new LabelSearchParameter((Label)label));
		return this;
	}

	public ISearchQuery Label(LabelColor labelColor)
	{
		_parameters.Add(new LabelSearchParameter(labelColor));
		return this;
	}

	public ISearchQuery IsArchived()
	{
		_parameters.Add(IsSearchParameter.Archived);
		return this;
	}

	public ISearchQuery IsOpen()
	{
		_parameters.Add(IsSearchParameter.Open);
		return this;
	}

	public ISearchQuery IsStarred()
	{
		_parameters.Add(IsSearchParameter.Starred);
		return this;
	}

	public ISearchQuery DueWithinDay()
	{
		_parameters.Add(DueSearchParameter.Day);
		return this;
	}

	public ISearchQuery DueWithinWeek()
	{
		_parameters.Add(DueSearchParameter.Week);
		return this;
	}

	public ISearchQuery DueWithinMonth()
	{
		_parameters.Add(DueSearchParameter.Month);
		return this;
	}

	public ISearchQuery DueWithinDays(int days)
	{
		_parameters.Add(new DueSearchParameter(days));
		return this;
	}

	public ISearchQuery Overdue()
	{
		_parameters.Add(DueSearchParameter.Overdue);
		return this;
	}

	public ISearchQuery CreatedWithinDay()
	{
		_parameters.Add(CreatedSearchParameter.Day);
		return this;
	}

	public ISearchQuery CreatedWithinWeek()
	{
		_parameters.Add(CreatedSearchParameter.Week);
		return this;
	}

	public ISearchQuery CreatedWithinMonth()
	{
		_parameters.Add(CreatedSearchParameter.Month);
		return this;
	}

	public ISearchQuery CreatedWithinDays(int days)
	{
		_parameters.Add(new CreatedSearchParameter(days));
		return this;
	}

	public ISearchQuery EditedWithinDay()
	{
		_parameters.Add(EditedSearchParameter.Day);
		return this;
	}

	public ISearchQuery EditedWithinWeek()
	{
		_parameters.Add(EditedSearchParameter.Week);
		return this;
	}

	public ISearchQuery EditedWithinMonth()
	{
		_parameters.Add(EditedSearchParameter.Month);
		return this;
	}

	public ISearchQuery EditedWithinDays(int days)
	{
		_parameters.Add(new EditedSearchParameter(days));
		return this;
	}

	public override string ToString()
	{
		return _parameters.Select((ISearchParameter p) => p.Query).Join(" ");
	}
}
