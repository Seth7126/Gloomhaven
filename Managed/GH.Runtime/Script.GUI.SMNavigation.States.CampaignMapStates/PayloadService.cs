namespace Script.GUI.SMNavigation.States.CampaignMapStates;

public class PayloadService<T>
{
	private static T _payload;

	private PayloadService()
	{
	}

	public static void Set(T payload)
	{
		_payload = payload;
	}

	public static T Get()
	{
		return _payload;
	}
}
