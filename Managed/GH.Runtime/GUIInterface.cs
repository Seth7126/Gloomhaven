using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class GUIInterface : MonoBehaviour
{
	public Text[] m_SRLQueue;

	public Text[] m_ClientQueue;

	public Text[] m_Canvas_Status;

	public GameObject m_StatusGUI;

	public AttackModifierCardGUI m_AttackModifierCardGUI;

	public static GUIInterface s_GUIInterface;

	public static bool s_InitialStatusGUIState;

	[UsedImplicitly]
	private void Awake()
	{
		s_GUIInterface = this;
		m_StatusGUI.SetActive(value: false);
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		s_GUIInterface = null;
	}

	public void ToggleStatusWindow(bool active)
	{
		m_StatusGUI.SetActive(active);
	}

	public void SetStatusText(string text)
	{
		if (text.Length != 0)
		{
			for (int num = m_Canvas_Status.GetLength(0) - 1; num > 0; num--)
			{
				m_Canvas_Status[num].text = m_Canvas_Status[num - 1].text;
			}
			m_Canvas_Status[0].text = text;
		}
	}

	public void SetClientQueueStatusText(string text)
	{
		if (Thread.CurrentThread == Choreographer.s_Choreographer.m_MainThread && text.Length != 0)
		{
			for (int num = m_ClientQueue.GetLength(0) - 1; num > 0; num--)
			{
				m_ClientQueue[num].text = m_ClientQueue[num - 1].text;
			}
			m_ClientQueue[0].text = text;
		}
	}

	public void SetSRLQueueStatusText(string text)
	{
		if (text.Length != 0)
		{
			for (int num = m_SRLQueue.GetLength(0) - 1; num > 0; num--)
			{
				m_SRLQueue[num].text = m_SRLQueue[num - 1].text;
			}
			m_SRLQueue[0].text = text;
		}
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}
}
