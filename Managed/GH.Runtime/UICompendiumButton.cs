using System.Collections.Generic;
using AsmodeeNet.Foundation;
using GLOOM;
using JetBrains.Annotations;
using Script.GUI.Utils;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UITab))]
public class UICompendiumButton : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private UITab tab;

	[SerializeField]
	private CompendiumSection section;

	[SerializeField]
	public List<UICompendiumButton> Subsections;

	[SerializeField]
	public string Id;

	[SerializeField]
	private ReferenceToSprite Screenshot;

	[SerializeField]
	private ReferenceToSprite ScreenshotConsole;

	public UnityEvent OnActivated;

	public const string TITLE_PREFIX = "Title = ";

	public const string TEXT_PREFIX = "Text = ";

	public const string POSTFIX_CONSOLE = " [CONSOLE]";

	public const string POSTFIX_PC = " [PC]";

	private string buttonLocKey = "";

	private bool isNavigationEnabled;

	public UnityEvent OnSelected => tab.OnSelected;

	public UnityEvent OnDeselected => tab.OnDeselected;

	private void Awake()
	{
		if (string.IsNullOrEmpty(buttonLocKey))
		{
			buttonLocKey = buttonText.text;
		}
		buttonText.text = LocalizationManager.GetTranslation(buttonLocKey);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		tab.onValueChanged.RemoveAllListeners();
		OnActivated = null;
		tab.OnSelected = null;
	}

	private void OnEnable()
	{
		if (string.IsNullOrEmpty(buttonLocKey))
		{
			buttonLocKey = buttonText.text;
		}
		buttonText.text = LocalizationManager.GetTranslation(buttonLocKey);
		tab.onValueChanged.AddListener(OnTabChanged);
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			tab.onValueChanged.RemoveListener(OnTabChanged);
		}
	}

	public void Localize()
	{
		if (string.IsNullOrEmpty(buttonLocKey))
		{
			buttonLocKey = buttonText.text;
		}
		buttonText.text = LocalizationManager.GetTranslation(buttonLocKey);
		foreach (UICompendiumButton subsection in Subsections)
		{
			subsection.Localize();
		}
	}

	public bool HasScreenshot()
	{
		if (PlatformLayer.Instance.IsConsole)
		{
			return ScreenshotConsole != null;
		}
		return Screenshot != null;
	}

	public ReferenceToSprite GetScreenshot()
	{
		if (PlatformLayer.Instance.IsConsole)
		{
			return ScreenshotConsole;
		}
		return Screenshot;
	}

	public void Init(CompendiumSection section, List<UICompendiumButton> subsections, ToggleGroup group)
	{
		this.section = section;
		buttonText.text = section.Title;
		Id = section.Id.ToString();
		Subsections = subsections;
		tab.group = group;
	}

	public void Init(CompendiumSubsection subsection, GameObject contentInstance, ToggleGroup group)
	{
		buttonText.text = subsection.Title;
		Id = subsection.Id.ToString();
		Screenshot = subsection.ReferenceToScreenshot;
		tab.TargetContent = contentInstance;
		tab.group = group;
	}

	public void OnTabChanged(bool state)
	{
		if (Subsections != null)
		{
			foreach (UICompendiumButton subsection in Subsections)
			{
				if (!state)
				{
					subsection.DeactivateSubsection();
				}
				subsection.gameObject.SetActive(state);
			}
		}
		if (state)
		{
			LocalizeTabTargetContent();
			OnActivated.Invoke();
			List<UICompendiumButton> subsections = Subsections;
			if (subsections != null && subsections.Count > 0)
			{
				Subsections[0].Activate();
				Subsections[0].LocalizeTabTargetContent();
			}
		}
	}

	public void LocalizeTabTargetContent()
	{
		if (!(tab.TargetContent != null))
		{
			return;
		}
		foreach (GameObject titleAndTextContainer in GetTitleAndTextContainers(tab.TargetContent))
		{
			if (TryParseTabLocalizationKey(titleAndTextContainer.name, out var key))
			{
				TextMeshProUGUI component = titleAndTextContainer.GetComponent<TextMeshProUGUI>();
				if (component != null)
				{
					component.text = LocalizationManager.GetTranslation(key).RemoveStartSpaces();
				}
			}
			else
			{
				titleAndTextContainer.SetActive(value: false);
			}
		}
	}

	private bool TryParseTabLocalizationKey(string gameObjectName, out string key)
	{
		key = gameObjectName;
		if (!key.TryReplaceWithEmpty("Title = ", out key))
		{
			key.TryReplaceWithEmpty("Text = ", out key);
		}
		if (key.TryReplaceWithEmpty(" [CONSOLE]", out key))
		{
			if (!InputManager.GamePadInUse)
			{
				return false;
			}
			key = "Consoles/" + key;
		}
		else if (key.TryReplaceWithEmpty(" [PC]", out key) && InputManager.GamePadInUse)
		{
			return false;
		}
		return true;
	}

	public void Activate()
	{
		tab.Activate();
	}

	private void DeactivateSubsection()
	{
		if (tab.isOn)
		{
			tab.SetValue(value: false);
			if (tab.TargetContent != null)
			{
				tab.TargetContent.SetActive(value: false);
			}
		}
	}

	private IEnumerable<GameObject> GetTitleAndTextContainers(GameObject parent)
	{
		for (int x = 0; x < parent.transform.childCount; x++)
		{
			Transform child = parent.transform.GetChild(x);
			if (child.name.StartsWith("Title = ") || child.name.StartsWith("Text = "))
			{
				yield return child.gameObject;
				continue;
			}
			foreach (GameObject titleAndTextContainer in GetTitleAndTextContainers(child.gameObject))
			{
				yield return titleAndTextContainer;
			}
		}
	}
}
