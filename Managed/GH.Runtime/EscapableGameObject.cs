using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EscapableGameObject : MonoBehaviour, IEscapable
{
	[SerializeField]
	private bool m_IsAllowedToEscapeDuringSave;

	public UnityEvent OnEscaped;

	public bool IsAllowedToEscapeDuringSave => m_IsAllowedToEscapeDuringSave;

	private void OnDestroy()
	{
		OnEscaped = null;
	}

	public void OnEnable()
	{
		UIWindowManager.RegisterEscapable(this);
	}

	public void OnDisable()
	{
		UIWindowManager.UnregisterEscapable(this);
	}

	public bool Escape()
	{
		base.gameObject.SetActive(value: false);
		OnEscaped.Invoke();
		return true;
	}

	public int Order()
	{
		return 0;
	}
}
