using System;
using TMPro;
using UnityEngine;

public class UITextInfoElement : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _header;

	[SerializeField]
	private TMP_Text _information;

	private string _lastHeader;

	private string _lastInformation;

	public void Set(string header, string information)
	{
		_header.text = header;
		_lastHeader = header;
		base.gameObject.SetActive(!header.IsNullOrEmpty());
		bool flag = !information.IsNullOrEmpty();
		_information.gameObject.SetActive(flag);
		if (flag)
		{
			_information.SetText(information);
			_lastInformation = information;
		}
	}

	public void RestoreLastData()
	{
		Set(_lastHeader, _lastInformation);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void ResetData(Action onResetAccepted)
	{
		if (!_lastHeader.IsNullOrEmpty())
		{
			onResetAccepted?.Invoke();
			_lastHeader = null;
			_lastInformation = null;
		}
	}
}
