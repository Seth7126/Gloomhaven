using UnityEngine;

namespace Assets.Script.GUI.NewAdventureMode.Guildmaster;

public class UIQuestLogGroupMarkerHighlight : MonoBehaviour
{
	[SerializeField]
	private GUIAnimator _highlightAnimator;

	private void OnEnable()
	{
		_highlightAnimator.gameObject.SetActive(value: true);
		_highlightAnimator.Play(fromStart: true);
	}

	private void OnDisable()
	{
		_highlightAnimator.gameObject.SetActive(value: false);
		_highlightAnimator.StopAndReset();
	}
}
