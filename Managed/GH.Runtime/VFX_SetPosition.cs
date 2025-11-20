using UnityEngine;

public class VFX_SetPosition : MonoBehaviour
{
	private bool modifyHeight = true;

	public float worldHeight;

	private Vector3 pos;

	private Transform parente;

	private void Update()
	{
		if (modifyHeight)
		{
			modifyHeight = false;
			pos = base.transform.position;
			parente = base.transform.parent;
			base.transform.SetParent(null);
			base.transform.position = new Vector3(pos.x, worldHeight, pos.z);
			base.transform.SetParent(parente);
		}
	}
}
