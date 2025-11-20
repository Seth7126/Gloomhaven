namespace Manatee.Trello;

public interface ITokenPermission
{
	bool? CanRead { get; }

	bool? CanWrite { get; }
}
