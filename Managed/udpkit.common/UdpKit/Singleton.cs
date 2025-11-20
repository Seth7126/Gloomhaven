namespace UdpKit;

public class Singleton<T> where T : class, new()
{
	protected static readonly object instanceLock = new object();

	private static T instance = null;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new T();
					}
				}
			}
			return instance;
		}
	}
}
