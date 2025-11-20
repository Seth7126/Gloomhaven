using TMPro;
using UnityEngine;

public class UINewNotificationTip : MonoBehaviour
{
	[SerializeField]
	private bool _disableOnAwake;

	public GameObject notification;

	public TextMeshProUGUI text;

	public string format = "+{0}";

	private void Awake()
	{
		if (_disableOnAwake)
		{
			notification.SetActive(value: false);
		}
	}

	public void Show(int value)
	{
		Show(string.Format(format, value));
	}

	public void Show(string message)
	{
		text.text = message;
		Show();
	}

	public void Show()
	{
		notification.SetActive(value: true);
	}

	public void Hide()
	{
		notification.SetActive(value: false);
	}
}
