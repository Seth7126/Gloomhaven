using System;
using UnityEngine;

namespace EPOOutline;

[ExecuteAlways]
public class TargetStateListener : MonoBehaviour
{
	public event Action OnVisibilityChanged;

	private void Awake()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}

	public void ForceUpdate()
	{
		if (this.OnVisibilityChanged != null)
		{
			this.OnVisibilityChanged();
		}
	}

	private void OnBecameVisible()
	{
		if (this.OnVisibilityChanged != null)
		{
			this.OnVisibilityChanged();
		}
	}

	private void OnBecameInvisible()
	{
		if (this.OnVisibilityChanged != null)
		{
			this.OnVisibilityChanged();
		}
	}
}
