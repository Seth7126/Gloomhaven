namespace Script.GUI;

public class MonoHotkeySessionOnEnable : MonoHotkeySession
{
	private void OnEnable()
	{
		Show();
	}

	private void OnDisable()
	{
		Hide();
	}
}
