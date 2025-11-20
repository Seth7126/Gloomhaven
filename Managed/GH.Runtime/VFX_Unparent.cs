using UnityEngine;

public class VFX_Unparent : MonoBehaviour
{
	private bool isUnparented;

	public bool zeroRotation;

	public bool randomRotY;

	public bool permomentUnparent;

	private void Start()
	{
		if (permomentUnparent)
		{
			base.transform.parent = null;
		}
		foreach (Transform item in base.gameObject.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		isUnparented = false;
	}

	private void Update()
	{
		if (isUnparented)
		{
			return;
		}
		isUnparented = true;
		base.transform.SetParent(null);
		if (zeroRotation)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, 0f);
		}
		if (randomRotY)
		{
			base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, Random.Range(0, 360), base.transform.eulerAngles.z);
		}
		foreach (Transform item in base.gameObject.transform)
		{
			item.gameObject.SetActive(value: true);
		}
	}
}
