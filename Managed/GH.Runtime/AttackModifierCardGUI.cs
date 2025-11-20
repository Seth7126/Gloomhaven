using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class AttackModifierCardGUI : MonoBehaviour
{
	public Text m_MathModifier;

	public Toggle m_Shuffle;

	public void Set(AttackModifierYMLData attackModifierCard)
	{
		m_MathModifier.text = attackModifierCard.MathModifier;
		m_Shuffle.isOn = attackModifierCard.Shuffle;
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}
}
