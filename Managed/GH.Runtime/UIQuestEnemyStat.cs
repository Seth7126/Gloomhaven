using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestEnemyStat : MonoBehaviour
{
	[SerializeField]
	private Image statIcon;

	[SerializeField]
	private TextMeshProUGUI statName;

	[SerializeField]
	private TextMeshProUGUI statValue;

	[SerializeField]
	private Graphic impairMask;

	[SerializeField]
	private Graphic pairMask;

	private Color textColor;

	private void Awake()
	{
		textColor = statName.color;
	}

	public void SetStat(string stateNameLoc, Sprite icon, string value = null)
	{
		statName.text = CreateLayout.LocaliseText(stateNameLoc);
		statIcon.sprite = icon;
		SetStatValue(value);
	}

	public void SetStatValue(string value)
	{
		statValue.text = value;
	}

	public void SetColor(Color textColor, Color? iconColor = null)
	{
		this.textColor = textColor;
		TextMeshProUGUI textMeshProUGUI = statName;
		Color color = (statValue.color = textColor);
		textMeshProUGUI.color = color;
		if (iconColor.HasValue)
		{
			statIcon.color = iconColor.Value;
		}
	}

	private void OnEnable()
	{
		bool flag = base.transform.GetSiblingIndex() % 2 == 0;
		if (pairMask != null)
		{
			pairMask.enabled = flag;
		}
		if (impairMask != null)
		{
			impairMask.enabled = !flag;
		}
	}

	public void SetFocused(bool focused)
	{
		statIcon.material = (focused ? null : UIInfoTools.Instance.greyedOutMaterial);
		TextMeshProUGUI textMeshProUGUI = statName;
		Color color = (statValue.color = (focused ? textColor : UIInfoTools.Instance.greyedOutTextColor));
		textMeshProUGUI.color = color;
	}
}
