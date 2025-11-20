public interface IControllerInputArea
{
	string Id { get; }

	bool BlockOthersFocusWhileIsFocused { get; }

	bool IsFocused { get; }

	void EnableGroup(bool isFocused);

	void DisableGroup();

	void Focus();

	void Unfocus();
}
