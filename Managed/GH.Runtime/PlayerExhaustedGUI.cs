using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExhaustedGUI : MonoBehaviour
{
	public Text m_Text;

	public static PlayerExhaustedGUI s_PlayerExhausedGUI;

	private void Start()
	{
		s_PlayerExhausedGUI = this;
		base.gameObject.SetActive(value: false);
	}

	public void Enable(string text, bool active)
	{
		m_Text.text = text;
		base.gameObject.SetActive(active);
	}

	public void Ok()
	{
		Enable("", active: false);
		ScenarioRuleClient.StepComplete();
	}
}
