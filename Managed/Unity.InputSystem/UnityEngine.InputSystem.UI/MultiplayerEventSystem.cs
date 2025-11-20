using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.UI;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/UISupport.html#multiplayer-uis")]
public class MultiplayerEventSystem : EventSystem
{
	[Tooltip("If set, only process mouse events for any game objects which are children of this game object.")]
	[SerializeField]
	private GameObject m_PlayerRoot;

	private CanvasGroup m_CanvasGroup;

	private bool m_CanvasGroupWasAddedByUs;

	private static int s_MultiplayerEventSystemCount;

	private static MultiplayerEventSystem[] s_MultiplayerEventSystems;

	public GameObject playerRoot
	{
		get
		{
			return m_PlayerRoot;
		}
		set
		{
			m_PlayerRoot = value;
			InitializeCanvasGroup();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ArrayHelpers.AppendWithCapacity(ref s_MultiplayerEventSystems, ref s_MultiplayerEventSystemCount, this);
		InitializeCanvasGroup();
	}

	private void InitializeCanvasGroup()
	{
		if (m_PlayerRoot != null)
		{
			m_CanvasGroup = m_PlayerRoot.GetComponent<CanvasGroup>();
			if (m_CanvasGroup == null)
			{
				m_CanvasGroup = m_PlayerRoot.AddComponent<CanvasGroup>();
				m_CanvasGroupWasAddedByUs = true;
			}
			else
			{
				m_CanvasGroupWasAddedByUs = false;
			}
		}
		else
		{
			m_CanvasGroup = null;
		}
	}

	protected override void OnDisable()
	{
		int num = s_MultiplayerEventSystems.IndexOfReference(this);
		if (num != -1)
		{
			s_MultiplayerEventSystems.EraseAtWithCapacity(ref s_MultiplayerEventSystemCount, num);
		}
		if (m_CanvasGroupWasAddedByUs)
		{
			Object.Destroy(m_CanvasGroup);
		}
		m_CanvasGroup = null;
		m_CanvasGroupWasAddedByUs = false;
		base.OnDisable();
	}

	protected override void Update()
	{
		for (int i = 0; i < s_MultiplayerEventSystemCount; i++)
		{
			MultiplayerEventSystem multiplayerEventSystem = s_MultiplayerEventSystems[i];
			if (!(multiplayerEventSystem.m_PlayerRoot == null))
			{
				multiplayerEventSystem.m_CanvasGroup.interactable = multiplayerEventSystem == this;
			}
		}
		EventSystem eventSystem = EventSystem.current;
		EventSystem.current = this;
		try
		{
			base.Update();
		}
		finally
		{
			EventSystem.current = eventSystem;
		}
	}
}
