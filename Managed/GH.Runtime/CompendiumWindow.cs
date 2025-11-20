using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM.MainMenu;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class CompendiumWindow : Singleton<CompendiumWindow>
{
	public List<UICompendiumButton> sectionButtons;

	public List<UICompendiumButton> subsectionButtons;

	public UIWindow Window;

	[Header("Structure")]
	public ToggleGroup categoryGroup;

	public Transform contentHolder;

	public ScrollRect sectionButtonsHolder;

	public Action<bool> OnSectionSelected;

	[SerializeField]
	private GameObject paginationControls;

	[SerializeField]
	private TextMeshProUGUI paginationText;

	[SerializeField]
	private Image screenshotHolder;

	[SerializeField]
	private ScrollRect contentScroller;

	[SerializeField]
	private ControllerInputAreaLocal controllerArea;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private UICompendiumButton currentSection;

	private int currentSubsection;

	private Dictionary<string, UICompendiumButton> references;

	protected override void Awake()
	{
		base.Awake();
		Window.onHidden.AddListener(OnHidden);
	}

	protected override void OnDestroy()
	{
		Window.onHidden.RemoveListener(OnHidden);
		base.OnDestroy();
	}

	private void Start()
	{
		references = new Dictionary<string, UICompendiumButton>();
		foreach (UICompendiumButton sectionButton in sectionButtons)
		{
			references[sectionButton.Id] = sectionButton;
			sectionButton.OnActivated.AddListener(delegate
			{
				OnActivatedSection(sectionButton);
			});
			sectionButton.OnSelected.AddListener(delegate
			{
				sectionButtonsHolder.ScrollToFit(sectionButton.transform as RectTransform);
			});
			sectionButton.OnSelected.AddListener(delegate
			{
				OnSectionSelected?.Invoke(obj: true);
			});
			sectionButton.OnDeselected.AddListener(delegate
			{
				OnSectionSelected?.Invoke(obj: false);
			});
		}
		foreach (UICompendiumButton subsectionButton in subsectionButtons)
		{
			references[subsectionButton.Id] = subsectionButton;
			subsectionButton.OnActivated.AddListener(delegate
			{
				OnActivatedSubsection(subsectionButton);
			});
			subsectionButton.OnSelected.AddListener(delegate
			{
				sectionButtonsHolder.ScrollToFit(subsectionButton.transform as RectTransform);
			});
		}
		OnActivatedSection(sectionButtons[0]);
	}

	private void OnActivatedSection(UICompendiumButton compendiumSection)
	{
		if (!(currentSection == compendiumSection))
		{
			currentSection = compendiumSection;
			if (compendiumSection.Subsections[0].HasScreenshot())
			{
				_imageSpriteLoader.AddReferenceToSpriteForImage(screenshotHolder, compendiumSection.Subsections[0].GetScreenshot());
			}
			screenshotHolder.gameObject.SetActive(compendiumSection.Subsections[0].HasScreenshot());
			currentSubsection = 0;
			if (compendiumSection.Subsections != null && compendiumSection.Subsections.Count > 0)
			{
				paginationText.text = $"1/{compendiumSection.Subsections.Count}";
				paginationControls.SetActive(value: true);
			}
			else
			{
				paginationControls.SetActive(value: false);
			}
			contentScroller.normalizedPosition = new Vector2(0f, 1f);
		}
	}

	private void OnActivatedSubsection(UICompendiumButton subsection)
	{
		ReferenceToSprite referenceToSprite = subsection.GetScreenshot() ?? currentSection?.GetScreenshot();
		screenshotHolder.gameObject.SetActive(referenceToSprite != null);
		if (referenceToSprite != null)
		{
			_imageSpriteLoader.AddReferenceToSpriteForImage(screenshotHolder, referenceToSprite);
		}
		currentSubsection = currentSection.Subsections.IndexOf(subsection);
		paginationText.text = string.Format("{1}/{0}", currentSection.Subsections.Count, currentSubsection + 1);
		contentScroller.normalizedPosition = new Vector2(0f, 1f);
	}

	public void GoToNextSubsection()
	{
		if (currentSubsection < currentSection.Subsections.Count - 1)
		{
			currentSection.Subsections[currentSubsection + 1].Activate();
		}
	}

	public void GoToPreviousSubsection()
	{
		if (currentSubsection != 0)
		{
			currentSection.Subsections[currentSubsection - 1].Activate();
		}
	}

	public void OnShown()
	{
		if (MainMenuUIManager.Instance != null)
		{
			GameObject logo = MainMenuUIManager.Instance.Logo;
			if (logo != null)
			{
				logo.SetActive(value: false);
			}
		}
		sectionButtons[0].Activate();
		if (sectionButtons[0].Subsections.Count > 0)
		{
			sectionButtons[0].Subsections[0].LocalizeTabTargetContent();
		}
		foreach (UICompendiumButton sectionButton in sectionButtons)
		{
			sectionButton.Localize();
		}
		controllerArea.Enable();
	}

	public void GoToSection(string id)
	{
		UICompendiumButton section = references[id];
		if (!sectionButtons.Contains(section))
		{
			sectionButtons.First((UICompendiumButton it) => it.Subsections.Contains(section)).Activate();
		}
		section.Activate();
	}

	public void OnBackButtonClick()
	{
		Window.Hide();
	}

	private void OnHidden()
	{
		if (MainMenuUIManager.Instance != null)
		{
			GameObject logo = MainMenuUIManager.Instance.Logo;
			if (logo != null)
			{
				logo.SetActive(value: true);
			}
		}
		controllerArea.Destroy();
		_imageSpriteLoader.Release();
	}
}
