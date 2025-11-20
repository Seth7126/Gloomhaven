namespace Manatee.Trello;

public interface IMemberSearchResult
{
	IMember Member { get; }

	int? Similarity { get; }
}
