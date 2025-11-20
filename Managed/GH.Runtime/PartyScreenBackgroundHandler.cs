using UnityEngine;
using UnityEngine.UI;

public class PartyScreenBackgroundHandler : MonoBehaviour
{
	[SerializeField]
	private Image _backgroundImage;

	public void ToggleBackground(bool isActive)
	{
		_backgroundImage.enabled = isActive;
	}
}
