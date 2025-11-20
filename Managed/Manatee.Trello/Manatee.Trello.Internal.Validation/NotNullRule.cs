namespace Manatee.Trello.Internal.Validation;

internal class NotNullRule<T> : IValidationRule<T> where T : class
{
	public static NotNullRule<T> Instance { get; }

	static NotNullRule()
	{
		Instance = new NotNullRule<T>();
	}

	private NotNullRule()
	{
	}

	public string Validate(T oldValue, T newValue)
	{
		if (newValue != null)
		{
			return null;
		}
		return "Value cannot be null";
	}
}
