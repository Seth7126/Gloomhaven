using UnityEngine;
using UnityEngine.UI;

namespace SM.Gamepad;

public class DebugCursor : MonoBehaviour
{
	private UiNavigationManager _uiNavigationManager;

	private RectTransform _rectTransform;

	private Image _image;

	public void Initialize(UiNavigationManager uiNavigationManager)
	{
		_uiNavigationManager = uiNavigationManager;
		_rectTransform = GetComponent<RectTransform>();
		_image = GetComponent<Image>();
	}

	private void Update()
	{
		if (_uiNavigationManager.CurrentNavigationRoot != null && _uiNavigationManager.CurrentlySelectedElement != null)
		{
			_image.enabled = true;
			base.transform.SetParent(_uiNavigationManager.CurrentNavigationRoot.transform);
			base.transform.position = _uiNavigationManager.CurrentlySelectedElement.GameObject.transform.position;
			_rectTransform.sizeDelta = _uiNavigationManager.CurrentlySelectedElement.GameObject.GetComponent<RectTransform>().sizeDelta;
			base.transform.localScale = Vector3.one;
			base.transform.SetAsLastSibling();
		}
		else
		{
			_image.enabled = false;
		}
	}
}
