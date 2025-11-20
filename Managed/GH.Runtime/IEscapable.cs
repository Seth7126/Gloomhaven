public interface IEscapable
{
	bool IsAllowedToEscapeDuringSave { get; }

	bool Escape();

	int Order();
}
