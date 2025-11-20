using UnityEngine;
using UnityEngine.EventSystems;

namespace AsmodeeNet.UserInterface;

public class ScalerHelper : UIBehaviour
{
	[SerializeField]
	private Vector2 _referenceSize;

	[SerializeField]
	private Transform[] _targets;

	protected override void OnRectTransformDimensionsChange()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		Vector2 vector = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		vector /= _referenceSize;
		float num = Mathf.Min(vector.x, vector.y);
		Vector3 localScale = new Vector3(num, num, num);
		Transform[] targets = _targets;
		for (int i = 0; i < targets.Length; i++)
		{
			targets[i].localScale = localScale;
		}
	}
}
