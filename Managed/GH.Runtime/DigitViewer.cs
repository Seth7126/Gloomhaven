using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Script.GUI.Utils;
using UnityEngine;
using UnityEngine.UI;

public class DigitViewer : MonoBehaviour
{
	[UsedImplicitly]
	[SerializeField]
	private List<Sprite> _digitSprites;

	[UsedImplicitly]
	[SerializeField]
	private List<Image> _digits;

	[UsedImplicitly]
	[SerializeField]
	private List<SpecialSymbolSprite> _specialSymbols;

	private int _maxValue;

	public void Initialize()
	{
		_maxValue = Mathf.RoundToInt(Mathf.Pow(10f, _digits.Count)) - 1;
		Hide();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void ShowValue(int value, bool show = true)
	{
		if (show)
		{
			Show();
		}
		bool flag = false;
		value = Mathf.Min(value, _maxValue);
		List<int> list = new List<int>();
		for (int i = 0; i < _digits.Count; i++)
		{
			if (flag)
			{
				_digits[i].gameObject.SetActive(value: false);
				continue;
			}
			int num = value % 10;
			list.Add(num);
			_digits[i].gameObject.SetActive(value: true);
			_digits[i].sprite = _digitSprites[num];
			if (value >= 10)
			{
				value /= 10;
			}
			else
			{
				flag = true;
			}
		}
	}

	public void ViewSpecialSymbol(string specialSymbol)
	{
		if (string.IsNullOrEmpty(specialSymbol))
		{
			Hide();
			return;
		}
		Show();
		foreach (SpecialSymbolSprite specialSymbol2 in _specialSymbols)
		{
			if (specialSymbol2.SpecialSymbol == specialSymbol)
			{
				_digits[0].sprite = specialSymbol2.Sprite;
				_digits[0].gameObject.SetActive(value: true);
				for (int i = 1; i < _digits.Count; i++)
				{
					_digits[i].gameObject.SetActive(value: false);
				}
				return;
			}
		}
		throw new Exception("Failed to find " + specialSymbol);
	}

	public void SetColor(Color color)
	{
		foreach (Image digit in _digits)
		{
			digit.color = color;
		}
	}
}
