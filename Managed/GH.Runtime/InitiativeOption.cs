using UnityEngine;

internal class InitiativeOption : IOption
{
	private int initiativeIncrease;

	private int newInitiative;

	private Sprite pickerBackground;

	public string ID => (initiativeIncrease > 0).ToString();

	public InitiativeOption(int initiativeIncrease, int newInitiative, Sprite pickerBackground)
	{
		this.initiativeIncrease = initiativeIncrease;
		this.newInitiative = newInitiative;
		this.pickerBackground = pickerBackground;
	}

	public Sprite GetPickerIcon()
	{
		return pickerBackground;
	}

	public string GetPickerText()
	{
		return "<margin-left=6>" + ((initiativeIncrease < 0) ? initiativeIncrease.ToString() : $"+{initiativeIncrease}");
	}

	public string GetSelectedText()
	{
		return newInitiative.ToString();
	}

	public Color GetSelectedTextColor()
	{
		return UIInfoTools.Instance.basicTextColor;
	}
}
