using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UITextInfoPanel : Singleton<UITextInfoPanel>
{
	[Header("Settings")]
	[SerializeField]
	private int _maxNumberOfPanels = 2;

	[Header("References")]
	[SerializeField]
	private GameObject _textInfoElementPrefab;

	[SerializeField]
	private Transform _panelsContainer;

	private bool _doShow = true;

	private string _lastSelectedTitle;

	private string _lastSelectedDescription;

	private bool _shouldBeHidden;

	private bool _isHidden;

	private UIWindow _window;

	private readonly List<UITextInfoElement> _elements = new List<UITextInfoElement>();

	public bool DoShow
	{
		get
		{
			return _doShow;
		}
		set
		{
			_doShow = value;
			HideTemporary(!_doShow);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		SetInstance(this);
		_window = GetComponent<UIWindow>();
		_textInfoElementPrefab.gameObject.SetActive(value: false);
		for (int i = 0; i < _maxNumberOfPanels; i++)
		{
			_elements.Add(Object.Instantiate(_textInfoElementPrefab, _panelsContainer).GetComponent<UITextInfoElement>());
		}
	}

	public void Show(params (string title, string description)[] input)
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown && DoShow)
		{
			for (int i = 0; i < ((input.Length < _maxNumberOfPanels) ? input.Length : _maxNumberOfPanels); i++)
			{
				var (header, information) = input[i];
				_elements[i].Set(header, information);
			}
			if (!_shouldBeHidden)
			{
				_window.Show();
			}
		}
	}

	public void Show(string title, string description = null)
	{
		Show((title, description));
	}

	public void Hide()
	{
		_elements.ForEach(delegate(UITextInfoElement element)
		{
			element.Hide();
		});
		_window.Hide(instant: true);
	}

	public void HideTemporary(bool hide)
	{
		if (hide)
		{
			if (_window.IsVisible)
			{
				_isHidden = true;
				_window.Hide(instant: true);
			}
		}
		else if (_isHidden)
		{
			_window.Show();
			_isHidden = false;
		}
	}

	private void OnHideTooltips()
	{
		_shouldBeHidden = true;
		_elements.ForEach(delegate(UITextInfoElement infoElement)
		{
			infoElement.Hide();
		});
	}

	private void OnShowTooltips()
	{
		_shouldBeHidden = false;
		_elements.ForEach(delegate(UITextInfoElement infoElement)
		{
			infoElement.RestoreLastData();
		});
	}

	public void TryReset()
	{
		bool reseted = false;
		foreach (UITextInfoElement element in _elements)
		{
			element.ResetData(delegate
			{
				reseted = true;
			});
		}
		if (reseted)
		{
			Hide();
		}
	}
}
