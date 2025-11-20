using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIFX_Image_Get_RectFormat : MonoBehaviour
{
	public string shaderProperty = "_RectFormat";

	private Vector2 rectFormat;

	public bool adjust;

	private RectTransform myRect;

	[Header("UIElements")]
	public Image ThisImage;

	[ContextMenu("Manually adjust")]
	public void doAdjust()
	{
		rectFormat = ThisImage.rectTransform.sizeDelta;
		ThisImage.material = new Material(ThisImage.material);
		ThisImage.material.SetVector(shaderProperty, rectFormat);
	}
}
