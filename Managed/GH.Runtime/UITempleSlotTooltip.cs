using System.Collections.Generic;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using MapRuleLibrary.YML.Locations;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITempleSlotTooltip : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UIPerkAttackModifier modifier;

	[SerializeField]
	private TextMeshProUGUI modifierText;

	[SerializeField]
	private Image modifierIcon;

	[SerializeField]
	private GameObject informationPanel;

	[SerializeField]
	private TextMeshProUGUI informationText;

	[SerializeField]
	private List<Image> imagesToGrayout;

	[SerializeField]
	private bool followPosition;

	[SerializeField]
	private bool trackScreenBound;

	[ConditionalField("trackScreenBound", null, true)]
	[SerializeField]
	private float offsetScreen = 20f;

	private bool isAvailable = true;

	private TempleYML.TempleBlessingDefinition lastBlessing;

	private void Build(TempleYML.TempleBlessingDefinition blessing, RectTransform target, bool isAvailable = true)
	{
		if (lastBlessing != blessing)
		{
			modifier.UpdateCounters(blessing.Quantity);
			modifierText.text = "x" + blessing.Quantity;
			UIInfoTools.EffectInfo effectInfo = ((blessing.TempleBlessingCondition.Type != RewardCondition.EConditionType.Negative) ? UIInfoTools.Instance.GetEffectInfoCondition(blessing.TempleBlessingCondition.PositiveCondition) : UIInfoTools.Instance.GetEffectInfoCondition(blessing.TempleBlessingCondition.NegativeCondition));
			modifierIcon.sprite = ((effectInfo.TempleIcon == null) ? effectInfo.Icon : effectInfo.TempleIcon);
			lastBlessing = blessing;
		}
		if (isAvailable)
		{
			informationPanel.SetActive(value: false);
		}
		else
		{
			informationText.text = "<color=#" + UIInfoTools.Instance.warningColor.ToHex() + ">" + LocalizationManager.GetTranslation("GUI_TEMPLE_BLESS_UNAVAILABLE");
			informationPanel.SetActive(value: true);
		}
		if (this.isAvailable != isAvailable)
		{
			for (int i = 0; i < imagesToGrayout.Count; i++)
			{
				imagesToGrayout[i].material = ((!isAvailable) ? UIInfoTools.Instance.greyedOutMaterial : null);
			}
			modifier.SetCancelled(!isAvailable);
			if (isAvailable)
			{
				modifierText.color = UIInfoTools.Instance.warningColor;
			}
		}
		this.isAvailable = isAvailable;
		base.transform.SetParent(target, worldPositionStays: false);
		base.transform.position += (base.transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
		if (window != null)
		{
			window.Show();
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
		base.enabled = trackScreenBound;
	}

	public void Show(TempleYML.TempleBlessingDefinition blessing, RectTransform target, bool isAvailable = true)
	{
		Build(blessing, target, isAvailable);
	}

	public void Hide()
	{
		if (window != null)
		{
			window.Hide();
			base.enabled = false;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (followPosition)
		{
			base.transform.position += (base.transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
		}
	}
}
