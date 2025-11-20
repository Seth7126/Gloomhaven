namespace Manatee.Trello.Internal.Validation;

internal class NullableHasValueRule<T> : IValidationRule<T?> where T : struct
{
	public static NullableHasValueRule<T> Instance { get; }

	static NullableHasValueRule()
	{
		Instance = new NullableHasValueRule<T>();
	}

	private NullableHasValueRule()
	{
	}

	public string Validate(T? oldValue, T? newValue)
	{
		if (newValue.HasValue)
		{
			return null;
		}
		return "Value cannot be null";
	}
}
