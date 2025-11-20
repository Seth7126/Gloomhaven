using UnityEngine;
using UnityEngine.UI;

public class Description : MonoBehaviour
{
	public Text m_TitleText;

	public GameObject m_StatsText;

	public Text m_ID_NumberText;

	public Text m_DescriptionText;

	public RectTransform m_Description_RectTransform;

	private bool m_ShowDescription;

	private static Description s_This;

	private static float s_RenderTargetImage_OriginalX;

	public static void Show(CInteractable interactable, CInteractableActor actor)
	{
		if (!s_This)
		{
			s_This = UIManager.Instance.UICanvas.transform.Find("MainGUILevel").Find("Description").GetComponent<Description>();
		}
		if ((bool)interactable)
		{
			s_This.m_StatsText.SetActive(actor != null);
			s_This.m_Description_RectTransform.localScale = Vector3.zero;
			s_This.gameObject.SetActive(value: true);
		}
		s_This.m_ShowDescription = interactable != null;
	}

	private void Update()
	{
		float x = m_Description_RectTransform.localScale.x;
		if (m_ShowDescription)
		{
			x = Mathf.Min(1f, x + Main.s_NonPausedDeltaTime * 5f);
		}
		else
		{
			x = Mathf.Max(0f, x - Main.s_NonPausedDeltaTime * 5f);
			if (x <= 0.1f)
			{
				m_Description_RectTransform.gameObject.SetActive(value: false);
				m_ShowDescription = false;
			}
		}
		m_Description_RectTransform.localScale = new Vector3(x, x, x);
	}
}
