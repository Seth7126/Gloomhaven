namespace Manatee.Trello;

public interface IPowerUp : ICacheable, IRefreshable
{
	string Name { get; }

	bool? IsPublic { get; }
}
