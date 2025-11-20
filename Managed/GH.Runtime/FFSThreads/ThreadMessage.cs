namespace FFSThreads;

public class ThreadMessage
{
	public EThreadMessageType Type { get; private set; }

	public ThreadMessage(EThreadMessageType type)
	{
		Type = type;
	}
}
