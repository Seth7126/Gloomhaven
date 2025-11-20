using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdventureFirstTimeTooltip : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI information;

	private void Start()
	{
		window.onHidden.AddListener(OnHidden);
	}

	public void Show(string titleTag, string textTag)
	{
		title.text = LocalizationManager.GetTranslation(titleTag);
		information.text = LocalizationManager.GetTranslation(textTag);
		Singleton<UIBlackOverlay>.Instance.ShowControlled(window);
		window.Show(instant: true);
	}

	private void OnHidden()
	{
		Singleton<UIBlackOverlay>.Instance.HideControlled(window);
	}

	public void Hide()
	{
		window.Hide(instant: true);
	}
}
