using UnityEngine;

namespace AsmodeeNet.Utils;

public class AutoRotate : MonoBehaviour
{
	public float Speed = 1f;

	private void FixedUpdate()
	{
		base.transform.Rotate(0f, 0f, Speed);
	}
}
