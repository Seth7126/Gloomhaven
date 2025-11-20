using System.Collections.Generic;
using UnityEngine;

public class RFX4_RaycastCollision : MonoBehaviour
{
	public float RaycastDistance = 100f;

	public GameObject[] Effects;

	public float Offset;

	public float TimeDelay;

	public float DestroyTime = 3f;

	public bool UsePivotPosition;

	public bool UseNormalRotation = true;

	public bool IsWorldSpace = true;

	public bool RealTimeUpdateRaycast;

	public bool DestroyAfterDisabling;

	[HideInInspector]
	public float HUE = -1f;

	[HideInInspector]
	public List<GameObject> CollidedInstances = new List<GameObject>();

	private bool isInitialized;

	private bool canUpdate;

	private void Start()
	{
		isInitialized = true;
		if (TimeDelay < 0.001f)
		{
			UpdateRaycast();
		}
		else
		{
			Invoke("LateEnable", TimeDelay);
		}
	}

	private void OnEnable()
	{
		CollidedInstances.Clear();
		if (isInitialized)
		{
			if (TimeDelay < 0.001f)
			{
				UpdateRaycast();
			}
			else
			{
				Invoke("LateEnable", TimeDelay);
			}
		}
	}

	private void OnDisable()
	{
		if (!DestroyAfterDisabling)
		{
			return;
		}
		foreach (GameObject collidedInstance in CollidedInstances)
		{
			Object.Destroy(collidedInstance);
		}
	}

	private void Update()
	{
		if (canUpdate)
		{
			UpdateRaycast();
		}
	}

	private void LateEnable()
	{
		UpdateRaycast();
	}

	private void UpdateRaycast()
	{
		if (Physics.Raycast(base.transform.position, base.transform.forward, out var hitInfo, RaycastDistance))
		{
			Vector3 position = ((!UsePivotPosition) ? (hitInfo.point + hitInfo.normal * Offset) : hitInfo.transform.position);
			if (CollidedInstances.Count == 0)
			{
				GameObject[] effects = Effects;
				for (int i = 0; i < effects.Length; i++)
				{
					GameObject gameObject = Object.Instantiate(effects[i], position, default(Quaternion));
					CollidedInstances.Add(gameObject);
					if (HUE > -0.9f)
					{
						RFX4_ColorHelper.ChangeObjectColorByHUE(gameObject, HUE);
					}
					if (!IsWorldSpace)
					{
						gameObject.transform.parent = base.transform;
					}
					if (UseNormalRotation)
					{
						gameObject.transform.LookAt(hitInfo.point + hitInfo.normal);
					}
					if (DestroyTime > 0.0001f)
					{
						Object.Destroy(gameObject, DestroyTime);
					}
				}
			}
			else
			{
				foreach (GameObject collidedInstance in CollidedInstances)
				{
					if (!(collidedInstance == null))
					{
						collidedInstance.transform.position = position;
						if (UseNormalRotation)
						{
							collidedInstance.transform.LookAt(hitInfo.point + hitInfo.normal);
						}
					}
				}
			}
		}
		if (RealTimeUpdateRaycast)
		{
			canUpdate = true;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position, base.transform.position + base.transform.forward * RaycastDistance);
	}
}
