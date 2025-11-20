using UnityEngine;

public class UIBackgroundTaskMask : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener text;

	[SerializeField]
	private GUIAnimator animator;

	public void Show(string textLoc)
	{
		text.SetTextKey(textLoc);
		Show();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		animator.Stop();
	}

	private void OnEnable()
	{
		animator.Play();
	}
}
