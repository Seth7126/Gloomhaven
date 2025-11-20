using UnityEngine;
using UnityEngine.UI;

namespace AsmodeeNet.Utils;

public class ActivityIndicatorButton : MonoBehaviour
{
	[SerializeField]
	private GameObject _text;

	[SerializeField]
	private GameObject _spinner;

	[SerializeField]
	private Button _button;

	private bool _waiting;

	public bool Waiting
	{
		get
		{
			return _waiting;
		}
		set
		{
			_waiting = value;
			_text.SetActive(!_waiting);
			_spinner.SetActive(_waiting);
		}
	}

	public bool Interactable
	{
		get
		{
			return _button.interactable;
		}
		set
		{
			_button.interactable = value;
		}
	}
}
