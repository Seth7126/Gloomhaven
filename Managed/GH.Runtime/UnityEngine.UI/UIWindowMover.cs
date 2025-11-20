namespace UnityEngine.UI;

public class UIWindowMover : MonoBehaviour
{
	[SerializeField]
	private UIWindowID targetWindow;

	private UIWindow myWindow;

	private void Start()
	{
		myWindow = UIWindow.GetWindow(targetWindow);
	}

	public void OpenWindow()
	{
		myWindow?.Show(instant: false);
	}

	public void CloseWindow()
	{
		myWindow?.Hide(instant: false);
	}
}
