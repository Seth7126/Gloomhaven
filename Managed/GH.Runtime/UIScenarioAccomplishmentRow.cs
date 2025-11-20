using System.Collections.Generic;
using AsmodeeNet.Utils.Extensions;
using UnityEngine;

public class UIScenarioAccomplishmentRow : MonoBehaviour
{
	[SerializeField]
	private List<UIScenarioAccomplishmentRowValue> valuesText;

	[SerializeField]
	private int characterIconSize = 30;

	public void Initialize(List<int> values)
	{
		int num = values.Max();
		for (int i = 0; i < values.Count; i++)
		{
			valuesText[i].SetValue(values[i].ToString(), num > 0 && num == values[i]);
			valuesText[i].gameObject.SetActive(value: true);
		}
		for (int j = values.Count; j < valuesText.Count; j++)
		{
			valuesText[j].gameObject.SetActive(value: false);
		}
	}
}
