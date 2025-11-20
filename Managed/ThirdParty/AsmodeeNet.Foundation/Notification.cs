namespace AsmodeeNet.Foundation;

public class Notification
{
	public string Name { get; private set; }

	public object Subject { get; private set; }

	public Notification(string name, object subject = null)
	{
		Name = name;
		Subject = subject;
	}
}
