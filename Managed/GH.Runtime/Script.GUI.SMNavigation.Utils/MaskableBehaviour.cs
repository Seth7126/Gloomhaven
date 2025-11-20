using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation.Utils;

public class MaskableBehaviour : MonoBehaviour
{
	public bool maskable = true;

	private void OnEnable()
	{
		MaskableGraphic[] componentsInChildren = base.gameObject.GetComponentsInChildren<MaskableGraphic>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].maskable = maskable;
		}
	}
}
