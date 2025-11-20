using UnityEngine;

public class RFX4_SimpleDecal : MonoBehaviour
{
	public float Offset = 0.05f;

	public bool UseNormalRotation;

	private Transform t;

	private RaycastHit hit;

	private void OnEnable()
	{
		t = base.transform;
		if (Physics.Raycast(t.position + Vector3.up / 2f, Vector3.down, out hit))
		{
			base.transform.position = hit.point + Vector3.up * Offset;
			if (UseNormalRotation)
			{
				base.transform.rotation = Quaternion.LookRotation(-hit.normal);
			}
		}
	}
}
