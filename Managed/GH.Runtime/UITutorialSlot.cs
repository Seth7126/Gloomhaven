using System;
using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private TextLocalizedListener title;

	[SerializeField]
	private TextLocalizedListener description;

	[SerializeField]
	private Image image;

	[SerializeField]
	private GameObject completedMask;

	[SerializeField]
	private ExtendedButton loadButton;

	private Action onSelected;

	private Action<UITutorialSlot, bool> onHovered;

	private void Awake()
	{
		loadButton.onClick.AddListener(delegate
		{
			onSelected?.Invoke();
		});
		button.onMouseEnter.AddListener(delegate
		{
			onHovered?.Invoke(this, arg2: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			onHovered?.Invoke(this, arg2: false);
		});
	}

	private void OnDestroy()
	{
		loadButton.onClick.RemoveAllListeners();
	}

	public void SetTutorial(ITutorial tutorial, bool completed, Action onSelected, Action<UITutorialSlot, bool> onHovered)
	{
		this.onSelected = onSelected;
		this.onHovered = onHovered;
		title.SetTextKey(tutorial.TitleLocText);
		description.SetTextKey(tutorial.DescriptionLocText);
		image.sprite = tutorial.Image;
		completedMask.SetActive(completed);
		loadButton.TextLanguageKey = (completed ? "GUI_REPLY" : "GUI_LOAD");
	}

	public void EnableNavigation()
	{
		button.SetNavigation(Navigation.Mode.Vertical);
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
		}
	}
}
