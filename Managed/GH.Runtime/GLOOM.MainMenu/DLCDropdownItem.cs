using AsmodeeNet.Utils.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class DLCDropdownItem : MonoBehaviour
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private UITextTooltipTarget tooltip;

	public bool IsOwned
	{
		set
		{
			toggle.interactable = !value;
			tooltip.enabled = value;
			if (value)
			{
				text.CrossFadeColor(UIInfoTools.Instance.greyedOutTextColor, 0f, ignoreTimeScale: true, useAlpha: true);
				tooltip.SetText("<b><color=#" + UIInfoTools.Instance.mainColor.ToHex() + ">" + text.text + "</color></b>\n" + LocalizationManager.GetTranslation("GUI_ALREADY_ADDED_DLC"));
			}
		}
	}
}
