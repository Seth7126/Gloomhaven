using AsmodeeNet.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AsmodeeNet.Foundation.Localization;

public class LocalizedText : MonoBehaviour
{
	private const string _documentation = "<b>LocalizedText</b> automatically retrieves the <b>key</b> to localize. Check the <b>Asmodee.net/Localization</b> menu.";

	private const string _kModuleName = "LocalizedText";

	[FormerlySerializedAs("key")]
	[SerializeField]
	private string _key;

	private bool _isInit;

	private Text _txt;

	private TextMeshProUGUI _tmpTxt;

	public string Key
	{
		get
		{
			return _key;
		}
		set
		{
			_key = value;
			if (!_isInit)
			{
				Awake();
			}
			_UpdateDisplayedText(CoreApplication.Instance.LocalizationManager.GetLocalizedText(_key));
		}
	}

	private void Awake()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_txt = GetComponent<Text>();
		if (_txt == null)
		{
			_tmpTxt = GetComponent<TextMeshProUGUI>();
			if (_tmpTxt == null)
			{
				AsmoLogger.Error("LocalizedText", "Missing Text or TextMeshProUGUI component in " + base.gameObject.name);
			}
		}
	}

	private void OnEnable()
	{
		LocalizationManager localizationManager = CoreApplication.Instance.LocalizationManager;
		localizationManager.OnLanguageChanged += _OnLanguageChanged;
		_UpdateDisplayedText(localizationManager.GetLocalizedText(_key));
	}

	private void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			CoreApplication.Instance.LocalizationManager.OnLanguageChanged -= _OnLanguageChanged;
		}
	}

	private void _OnLanguageChanged(LocalizationManager localizationManager)
	{
		_UpdateDisplayedText(localizationManager.GetLocalizedText(_key));
	}

	private void _UpdateDisplayedText(string msg)
	{
		if (_txt != null)
		{
			_txt.text = msg;
		}
		else
		{
			_tmpTxt.text = msg;
		}
	}
}
