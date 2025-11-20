using UnityEngine;

public class WobblePath : MonoBehaviour
{
	public float speed = 10f;

	public float amplitude = 1f;

	public Vector3 offset;

	private void Update()
	{
		base.transform.localPosition = offset + base.transform.up * Mathf.Sin(Time.time * speed) * amplitude;
	}
}
