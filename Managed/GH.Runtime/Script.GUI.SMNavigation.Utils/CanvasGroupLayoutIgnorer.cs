using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.SMNavigation.Utils;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupLayoutIgnorer : MonoBehaviour, ILayoutIgnorer
{
	private CanvasGroup _canvasGroup;

	public bool ignoreLayout => CanvasGroup.alpha == 0f;

	private CanvasGroup CanvasGroup => _canvasGroup ?? (_canvasGroup = GetComponent<CanvasGroup>());
}
