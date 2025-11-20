namespace Platforms.Utils;

public class ByRef<T>
{
	private T _data;

	public T Data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	public ByRef(T data)
	{
		_data = data;
	}
}
