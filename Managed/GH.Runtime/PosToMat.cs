using UnityEngine;

public class PosToMat : MonoBehaviour
{
	private void Update()
	{
		Renderer component = GetComponent<Renderer>();
		Transform component2 = GetComponent<Transform>();
		component.material.SetFloat("_ObjPosY", component2.transform.position.y);
	}
}
