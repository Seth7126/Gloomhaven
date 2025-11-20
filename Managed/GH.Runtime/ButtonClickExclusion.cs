using UnityEngine;
using UnityEngine.UI;

public class ButtonClickExclusion : MonoBehaviour
{
	public Button Button;

	public void DisableButtonOnClick()
	{
		Button.interactable = false;
	}

	private void OnDisable()
	{
		Button.interactable = true;
	}
}
