using UnityEngine;
using UnityEngine.UI;

public class BackgroundToggleElement : MonoBehaviour, IElementTogglable
{
	[SerializeField]
	private Image _image;

	private void Awake()
	{
		_image.enabled = false;
	}

	public void Toggle(bool isActive)
	{
		_image.enabled = isActive;
	}
}
