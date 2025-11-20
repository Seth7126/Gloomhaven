using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.GUI.PartyDisplay;

[ExecuteInEditMode]
public class DimmerUIElements : MonoBehaviour
{
	[SerializeField]
	private List<Graphic> _elementsToIgnore = new List<Graphic>();

	[HideInInspector]
	[SerializeField]
	private List<DimmingColorModel> _dimmingColorPairsContainer = new List<DimmingColorModel>();

	public int ColorShift { get; set; } = 190;

	private void OnDestroy()
	{
	}

	public void Initialize()
	{
		InitializeImages();
		InitializeTexts();
	}

	public void DarkenElements()
	{
		foreach (DimmingColorModel item in _dimmingColorPairsContainer)
		{
			if (!_elementsToIgnore.Contains(item.Graphic) && item.Graphic != null)
			{
				item.Graphic.color = item.AfterDimming;
			}
		}
	}

	public void LightenToStandard()
	{
		foreach (DimmingColorModel item in _dimmingColorPairsContainer)
		{
			if (!_elementsToIgnore.Contains(item.Graphic) && item.Graphic != null)
			{
				item.Graphic.color = item.BeforeDimming;
			}
		}
	}

	public void InitializeDimmerColor()
	{
		InitializeDimmingColorImages();
		InitializeDimmingColorTexts();
	}

	private void InitializeImages()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		InitializeNewImages(componentsInChildren);
		RemoveDeletedImages(componentsInChildren);
		InitializeDimmingColorImages();
	}

	private void InitializeNewImages(Image[] imagesInChildren)
	{
		foreach (Image image in imagesInChildren)
		{
			DimmingColorModel item = new DimmingColorModel(image, image.color, image.color);
			if (!_dimmingColorPairsContainer.Contains(item))
			{
				_dimmingColorPairsContainer.Add(item);
			}
		}
	}

	private void RemoveDeletedImages(Image[] imagesInChildren)
	{
		List<DimmingColorModel> list = new List<DimmingColorModel>();
		foreach (DimmingColorModel item in _dimmingColorPairsContainer)
		{
			if (item.Graphic is Image)
			{
				if (item.Graphic == null)
				{
					list.Add(item);
				}
				else if (!imagesInChildren.Contains(item.Graphic))
				{
					list.Add(item);
				}
			}
		}
		foreach (DimmingColorModel item2 in list)
		{
			item2.Graphic.color = item2.BeforeDimming;
			_dimmingColorPairsContainer.Remove(item2);
		}
	}

	private void InitializeDimmingColorImages()
	{
		foreach (DimmingColorModel item in _dimmingColorPairsContainer)
		{
			if (!(item.Graphic == null))
			{
				Color afterDimming = new Color(item.BeforeDimming.r - (float)ColorShift / 255f, item.BeforeDimming.g - (float)ColorShift / 255f, item.BeforeDimming.b - (float)ColorShift / 255f, item.BeforeDimming.a);
				item.AfterDimming = afterDimming;
			}
		}
	}

	private void InitializeTexts()
	{
		TextMeshProUGUI[] componentsInChildren = GetComponentsInChildren<TextMeshProUGUI>();
		InitializeNewTexts(componentsInChildren);
		RemoveDeletedTexts(componentsInChildren);
		InitializeDimmingColorTexts();
	}

	private void InitializeNewTexts(TextMeshProUGUI[] textsInChildren)
	{
		foreach (TextMeshProUGUI textMeshProUGUI in textsInChildren)
		{
			DimmingColorModel item = new DimmingColorModel(textMeshProUGUI, textMeshProUGUI.color, textMeshProUGUI.color);
			if (!_dimmingColorPairsContainer.Contains(item))
			{
				_dimmingColorPairsContainer.Add(item);
			}
		}
	}

	private void RemoveDeletedTexts(TextMeshProUGUI[] textsInChildren)
	{
		List<DimmingColorModel> list = new List<DimmingColorModel>();
		foreach (DimmingColorModel item in _dimmingColorPairsContainer)
		{
			if (item.Graphic is TextMeshProUGUI)
			{
				if (item.Graphic == null)
				{
					list.Add(item);
				}
				else if (!textsInChildren.Contains(item.Graphic))
				{
					list.Add(item);
				}
			}
		}
		foreach (DimmingColorModel item2 in list)
		{
			_dimmingColorPairsContainer.Remove(item2);
		}
	}

	private void InitializeDimmingColorTexts()
	{
		foreach (DimmingColorModel item in _dimmingColorPairsContainer)
		{
			if (!(item.Graphic == null))
			{
				Color afterDimming = new Color(item.BeforeDimming.r - (float)ColorShift / 255f, item.BeforeDimming.g - (float)ColorShift / 255f, item.BeforeDimming.b - (float)ColorShift / 255f, item.BeforeDimming.a);
				item.AfterDimming = afterDimming;
			}
		}
	}
}
