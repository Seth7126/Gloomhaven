namespace Manatee.Trello.Json;

public interface IJsonStarredBoard : IJsonCacheable
{
	IJsonBoard Board { get; set; }

	IJsonPosition Pos { get; set; }
}
