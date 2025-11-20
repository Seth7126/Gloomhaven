using UnityEngine;

namespace UnityStandardAssets.Utility;

public class TimedParentDestructor : MonoBehaviour
{
	[SerializeField]
	private float m_TimeOut = 1f;

	[SerializeField]
	private bool m_DetachChildren;

	private GameObject papi;

	private void Awake()
	{
		Invoke("DestroyNow", m_TimeOut);
		papi = ((base.transform.parent == null) ? null : base.transform.parent.gameObject);
	}

	private void DestroyNow()
	{
		if (m_DetachChildren)
		{
			base.transform.DetachChildren();
		}
		if (papi != null)
		{
			Object.Destroy(papi);
		}
	}
}
