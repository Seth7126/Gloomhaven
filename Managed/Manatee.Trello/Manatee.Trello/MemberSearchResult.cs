namespace Manatee.Trello;

public class MemberSearchResult : IMemberSearchResult
{
	public IMember Member { get; }

	public int? Similarity { get; }

	internal MemberSearchResult(IMember member, int? similarity)
	{
		Member = member;
		Similarity = similarity;
	}
}
