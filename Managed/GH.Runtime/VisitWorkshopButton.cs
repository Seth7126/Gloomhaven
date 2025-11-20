using UnityEngine;
using UnityEngine.UI;

public class VisitWorkshopButton : MonoBehaviour
{
	[SerializeField]
	private Image m_Background;

	private int m_AnimID;

	private void OnEnable()
	{
		LeanTween.cancel(m_AnimID);
		ExtendedButton component = GetComponent<ExtendedButton>();
		component.onMouseEnter.RemoveAllListeners();
		component.onMouseEnter.AddListener(StopBlinking);
		component.onMouseExit.RemoveAllListeners();
		component.onMouseExit.AddListener(StartBlinking);
		m_AnimID = LeanTween.alpha(m_Background.rectTransform, 0.1f, 0.3f).setLoopPingPong().id;
	}

	private void StopBlinking()
	{
		LeanTween.pause(m_AnimID);
		m_Background.color = Color.white;
	}

	private void StartBlinking()
	{
		LeanTween.resume(m_AnimID);
	}

	public void OpenWorkshop()
	{
		Application.OpenURL("steam://url/SteamWorkshopPage/780290");
	}
}
