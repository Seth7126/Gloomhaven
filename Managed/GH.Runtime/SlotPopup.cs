using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class SlotPopup : MonoBehaviour
{
	private Transform holder;

	private UIWindow window;

	protected virtual void Awake()
	{
		window = base.gameObject.GetComponent<UIWindow>();
	}

	public void Init(Transform holder)
	{
		this.holder = holder;
	}

	public void Show()
	{
		if (holder != null)
		{
			base.transform.position = holder.position;
		}
		window.Show();
	}

	public void Hide()
	{
		window.Hide();
	}
}
