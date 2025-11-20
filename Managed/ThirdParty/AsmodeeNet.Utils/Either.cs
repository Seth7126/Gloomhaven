namespace AsmodeeNet.Utils;

public class Either<T, U>
{
	private T _value;

	private U _error;

	public T Value => _value;

	public U Error => _error;

	public bool HasError => Error != null;

	protected Either(T value, U error)
	{
		_value = value;
		_error = error;
	}

	public static Either<T, U> newWithValue(T value)
	{
		return new Either<T, U>(value, default(U));
	}

	public static Either<T, U> newWithError(U error)
	{
		return new Either<T, U>(default(T), error);
	}
}
