namespace UnityEngine.UI;

public class UIToggleWindowAndTab : MonoBehaviour
{
	public UIWindow window;

	public UITab tab;

	public void Toggle()
	{
		if (!(window == null) && !(tab == null))
		{
			if (window.IsOpen && tab.isOn)
			{
				window.Hide();
				return;
			}
			window.Show();
			tab.Activate();
		}
	}
}
