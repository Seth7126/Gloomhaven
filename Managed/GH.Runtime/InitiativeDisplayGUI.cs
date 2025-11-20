using UnityEngine;
using UnityEngine.UI;

public class InitiativeDisplayGUI : MonoBehaviour
{
	public Text m_textValue;

	public void DisplayValue(int value)
	{
		m_textValue.text = value.ToString();
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}
}
