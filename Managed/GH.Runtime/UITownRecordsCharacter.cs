using System;
using AsmodeeNet.Foundation;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITownRecordsCharacter : UIBehaviour
{
	[SerializeField]
	private TextMeshProUGUI characterNameText;

	[SerializeField]
	private Image characterIcon;

	[SerializeField]
	private Color retiredStateColor;

	[SerializeField]
	private ExtendedButton button;

	[Header("Stats")]
	[SerializeField]
	protected TextMeshProUGUI damageDone;

	[SerializeField]
	protected TextMeshProUGUI kills;

	[SerializeField]
	protected TextMeshProUGUI exhaustions;

	[SerializeField]
	protected TextMeshProUGUI healingDone;

	[SerializeField]
	protected TextMeshProUGUI winrate;

	[Header("Impair")]
	[SerializeField]
	private Color impairNameColor;

	[SerializeField]
	private Color impairTextStatColor;

	[SerializeField]
	private Image impairMask;

	[Header("Pair")]
	[SerializeField]
	private Color pairNameColor;

	[SerializeField]
	private Color pairTextStatColor;

	private Action<UITownRecordsCharacter, bool> onHovered;

	protected override void Awake()
	{
		button.onMouseEnter.AddListener(delegate
		{
			onHovered?.Invoke(this, arg2: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			onHovered?.Invoke(this, arg2: false);
		});
		Refresh();
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Refresh();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
		}
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		Refresh();
	}

	private void Refresh()
	{
		if (base.transform.GetSiblingIndex() % 2 == 0)
		{
			impairMask.enabled = false;
			TextMeshProUGUI textMeshProUGUI = damageDone;
			TextMeshProUGUI textMeshProUGUI2 = healingDone;
			TextMeshProUGUI textMeshProUGUI3 = kills;
			TextMeshProUGUI textMeshProUGUI4 = exhaustions;
			Color color = (winrate.color = pairTextStatColor);
			Color color3 = (textMeshProUGUI4.color = color);
			Color color5 = (textMeshProUGUI3.color = color3);
			Color color7 = (textMeshProUGUI2.color = color5);
			textMeshProUGUI.color = color7;
			characterNameText.color = pairNameColor;
		}
		else
		{
			impairMask.enabled = true;
			TextMeshProUGUI textMeshProUGUI5 = damageDone;
			TextMeshProUGUI textMeshProUGUI6 = healingDone;
			TextMeshProUGUI textMeshProUGUI7 = kills;
			TextMeshProUGUI textMeshProUGUI8 = exhaustions;
			Color color = (winrate.color = impairTextStatColor);
			Color color3 = (textMeshProUGUI8.color = color);
			Color color5 = (textMeshProUGUI7.color = color3);
			Color color7 = (textMeshProUGUI6.color = color5);
			textMeshProUGUI5.color = color7;
			characterNameText.color = impairNameColor;
		}
	}

	public void Display(ITownRecordCharacter characterData, Action<UITownRecordsCharacter, bool> onHovered)
	{
		this.onHovered = onHovered;
		characterIcon.sprite = characterData.Icon;
		characterNameText.text = (characterData.IsRetired ? string.Format("{0} <color=#{2}>{1}</color>", characterData.CharacterName, LocalizationManager.GetTranslation("GUI_RETIRED_STATE"), retiredStateColor.ToHex()) : characterData.CharacterName);
		damageDone.text = characterData.DamageDone.ToString();
		kills.text = characterData.Kills.ToString();
		healingDone.text = characterData.HealingDone.ToString();
		exhaustions.text = characterData.Exhausitons.ToString();
		winrate.text = string.Format($"{characterData.Winrate}%");
	}

	public void EnableNavigation()
	{
		button.SetNavigation(Navigation.Mode.Vertical);
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}
}
