using System.Collections.Generic;
using System.Linq;
using GLOOM;
using I2.Loc;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;

public class LayoutRow : LocalizedListener
{
	public enum EMode
	{
		None,
		GlossaryEntry,
		TextEntry,
		MonsterCardEntry
	}

	private const string LayoutRowRes = "LayoutRow";

	public TextMeshProUGUI TextBox;

	public string IconGlossaryEntryName;

	public int Value;

	public bool Bold;

	public bool PlusX;

	public List<string> IconNames;

	public Color ValueColour;

	public EMode mode;

	public bool IsBuff;

	public bool ShowValueMultiplier;

	public bool FromBaseStats;

	public bool FromControlAbility;

	[SerializeField]
	private bool initialised;

	[SerializeField]
	private string lookupText;

	[SerializeField]
	private CAbility.EAbilityType abilityType;

	[SerializeField]
	private string abilityName;

	[SerializeField]
	private string generatedLanguage;

	public CAbility.EAbilityType AbilityType => abilityType;

	public string AbilityName => abilityName;

	public IconGlossaryYML.IconGlossaryEntry Entry => ScenarioRuleClient.SRLYML.IconGlossary.SingleOrDefault((IconGlossaryYML.IconGlossaryEntry s) => s.Name == IconGlossaryEntryName);

	public List<IconGlossaryYML.IconGlossaryEntry> Icons
	{
		get
		{
			if (IconNames != null)
			{
				return ScenarioRuleClient.SRLYML.IconGlossary.Where((IconGlossaryYML.IconGlossaryEntry w) => IconNames.Contains(w.Name)).ToList();
			}
			return null;
		}
	}

	private void Start()
	{
		if (initialised && generatedLanguage != I2.Loc.LocalizationManager.CurrentLanguage)
		{
			OnLanguageChanged();
		}
	}

	public void Init(IconGlossaryYML.IconGlossaryEntry entry, int? value = null, List<string> icons = null, Color? valueColour = null, float? fontSize = null, bool monsterCard = false, bool isBuff = false, CAbility.EAbilityType mainAbilityType = CAbility.EAbilityType.None, string mainAbilityName = "", bool showValueMultiplier = false, bool fromBaseStats = false)
	{
		TextBox.font = GlobalSettings.Instance.m_GlossaryCardSettings.CardFont;
		TextBox.fontSize = fontSize ?? GlobalSettings.Instance.m_GlossaryCardSettings.StandardFontSize;
		TextBox.enableAutoSizing = true;
		TextBox.autoSizeTextContainer = true;
		TextBox.fontSizeMin = GlobalSettings.Instance.m_GlossaryCardSettings.MinFontSize;
		TextBox.fontSizeMax = fontSize ?? GlobalSettings.Instance.m_GlossaryCardSettings.StandardFontSize;
		IconGlossaryEntryName = entry.Name;
		Value = value ?? int.MaxValue;
		IconNames = icons;
		ValueColour = valueColour ?? TextBox.color;
		initialised = true;
		mode = ((!monsterCard) ? EMode.GlossaryEntry : EMode.MonsterCardEntry);
		IsBuff = isBuff;
		ShowValueMultiplier = showValueMultiplier;
		FromBaseStats = fromBaseStats;
		abilityType = mainAbilityType;
		abilityName = mainAbilityName;
		base.gameObject.name = "Row Container";
		generatedLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		UpdateText();
	}

	public void Init(string text, float? fontSize = null, int? value = null)
	{
		TextBox.font = GlobalSettings.Instance.m_GlossaryCardSettings.CardFont;
		TextBox.fontSize = fontSize ?? GlobalSettings.Instance.m_GlossaryCardSettings.StandardFontSize;
		TextBox.enableAutoSizing = true;
		TextBox.autoSizeTextContainer = true;
		TextBox.fontSizeMin = GlobalSettings.Instance.m_GlossaryCardSettings.MinFontSize;
		TextBox.fontSizeMax = fontSize ?? GlobalSettings.Instance.m_GlossaryCardSettings.StandardFontSize;
		lookupText = text;
		initialised = true;
		mode = EMode.TextEntry;
		Value = (value.HasValue ? value.Value : int.MaxValue);
		base.gameObject.name = "Row Container";
		generatedLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		UpdateText();
	}

	public void UpdateText(bool bold = false, bool plusX = false)
	{
		Bold = bold;
		PlusX = plusX;
		RefreshText();
	}

	public void RefreshText(List<string> additionalMainlineConditions = null)
	{
		if (!initialised)
		{
			return;
		}
		string text = string.Empty;
		if (mode == EMode.GlossaryEntry || mode == EMode.MonsterCardEntry)
		{
			if (Entry?.Text != null)
			{
				text = CreateLayout.LocaliseText(Entry.Text) + " " + Entry.Sprite;
			}
			else
			{
				Debug.LogError("Card row IconGlossary Entry.Text was null");
			}
			if (additionalMainlineConditions != null)
			{
				int i;
				for (i = 0; i < additionalMainlineConditions.Count; i++)
				{
					text += ((i == additionalMainlineConditions.Count - 1) ? (" " + GLOOM.LocalizationManager.GetTranslation("and") + " ") : ", ");
					IconGlossaryYML.IconGlossaryEntry iconGlossaryEntry = ScenarioRuleClient.SRLYML.IconGlossary.SingleOrDefault((IconGlossaryYML.IconGlossaryEntry s) => s.Name == additionalMainlineConditions[i]);
					text = text + "<nobr>" + CreateLayout.LocaliseText(iconGlossaryEntry.Text) + " " + iconGlossaryEntry.Sprite + "</nobr>";
				}
			}
			if (Value != int.MaxValue)
			{
				int num = Mathf.Abs(Value);
				string text2 = Mathf.Max(0, Value).ToString();
				if (ShowValueMultiplier)
				{
					text2 = "x" + num;
				}
				else if ((mode == EMode.MonsterCardEntry && Entry.ShowSymbol) || IsBuff)
				{
					text2 = ((Value >= 0) ? "+" : "-") + num;
				}
				text = ((!IsBuff) ? (text + " <color=#" + ColorUtility.ToHtmlStringRGB(ValueColour) + ">" + text2 + "</color>") : ("<color=#" + ColorUtility.ToHtmlStringRGB(ValueColour) + ">" + text2 + "</color> " + text));
				text = "<nobr>" + text + "</nobr>";
			}
			if (Icons != null && Icons.Count > 0)
			{
				foreach (IconGlossaryYML.IconGlossaryEntry icon in Icons)
				{
					text = text + " " + icon.Sprite;
				}
			}
		}
		else if (mode == EMode.TextEntry)
		{
			text = CreateLayout.LocaliseText(lookupText);
			if (Value.HasValue() && text.Contains('*'.ToString()))
			{
				text = CardLayoutRow.ReplaceKey(text, '*', Value.ToString());
			}
		}
		if (text != string.Empty)
		{
			if (Bold)
			{
				text = "<b>" + text;
			}
			if (PlusX)
			{
				text += "+X";
			}
			TextBox.text = text;
		}
	}

	public static LayoutRow CreateLayoutRow(string text, float? fontSize = null)
	{
		LayoutRow component = Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_gui", "LayoutRow", "gui")).GetComponent<LayoutRow>();
		component.Init(text, fontSize);
		return component;
	}

	public static LayoutRow CreateLayoutRow(string text, int? value, float? fontSize = null)
	{
		LayoutRow component = Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_gui", "LayoutRow", "gui")).GetComponent<LayoutRow>();
		component.Init(text, fontSize, value);
		return component;
	}

	public static LayoutRow CreateLayoutRow(string glossaryName, int? value = null, List<string> icons = null, Color? valueColour = null, float? fontSize = null, bool monsterCard = false, bool isBuff = false, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string abilityName = "", bool showValueMultiplier = false, bool fromBaseStats = false)
	{
		LayoutRow component = Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_gui", "LayoutRow", "gui")).GetComponent<LayoutRow>();
		IconGlossaryYML.IconGlossaryEntry iconGlossaryEntry = ScenarioRuleClient.SRLYML.IconGlossary.SingleOrDefault((IconGlossaryYML.IconGlossaryEntry x) => x.Name == glossaryName);
		if (iconGlossaryEntry != null)
		{
			component.Init(iconGlossaryEntry, value, icons, valueColour, fontSize, monsterCard, isBuff, abilityType, abilityName, showValueMultiplier, fromBaseStats);
		}
		return component;
	}

	protected override void OnLanguageChanged()
	{
		RefreshText();
	}
}
