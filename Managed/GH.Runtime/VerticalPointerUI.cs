using UnityEngine;

public class VerticalPointerUI : MonoBehaviour
{
	public void PointAt(RectTransform rectTransform)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		base.transform.position = new Vector3(base.transform.position.x, array[1].y + (array[0].y - array[1].y) / 2f, base.transform.position.z);
	}
}
