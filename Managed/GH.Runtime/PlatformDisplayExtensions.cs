using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PlatformDisplayExtensions
{
	public static List<Resolution> GetResolutions(this PlatformLayer platformLayer)
	{
		int num = 2;
		List<Resolution> list = new List<Resolution>();
		if (platformLayer.GetCurrentPlatform() == DeviceType.XboxSeriesX)
		{
			Resolution item = new Resolution
			{
				width = 2560,
				height = 1440,
				refreshRate = 60
			};
			list.Add(item);
			item = new Resolution
			{
				width = 3840,
				height = 2160,
				refreshRate = 60
			};
			list.Add(item);
		}
		else if (platformLayer.GetCurrentPlatform() == DeviceType.XboxSeriesS)
		{
			Resolution item2 = new Resolution
			{
				width = 1920,
				height = 1080,
				refreshRate = 60
			};
			list.Add(item2);
			item2 = new Resolution
			{
				width = 2560,
				height = 1440,
				refreshRate = 30
			};
			list.Add(item2);
		}
		else if (platformLayer.GetCurrentPlatform() == DeviceType.XboxOneX)
		{
			Resolution item3 = new Resolution
			{
				width = 2560,
				height = 1440,
				refreshRate = 30
			};
			list.Add(item3);
		}
		else if (platformLayer.GetCurrentPlatform() == DeviceType.PlayStation4)
		{
			Resolution item4 = new Resolution
			{
				width = 1920,
				height = 1080,
				refreshRate = 30
			};
			list.Add(item4);
		}
		else if (platformLayer.GetCurrentPlatform() == DeviceType.PlayStation4Pro)
		{
			Resolution item5 = new Resolution
			{
				width = 2560,
				height = 1440,
				refreshRate = 30
			};
			list.Add(item5);
		}
		else
		{
			Resolution[] resolutions = Screen.resolutions;
			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution item6 = resolutions[i];
				if (platformLayer.IsConsole)
				{
					if (platformLayer.GetCurrentPlatform() != DeviceType.PlayStation5 && platformLayer.GetCurrentPlatform() != DeviceType.PlayStation4OnPS5 && item6.width == 1920 && item6.height == 1080 && !list.Any((Resolution resolutionInList) => resolutionInList.width == 1920 && resolutionInList.height == 1080))
					{
						Resolution item7 = new Resolution
						{
							width = 1920,
							height = 1080,
							refreshRate = 30 * num
						};
						list.Add(item7);
					}
					if (item6.width == 2560 && item6.height == 1440 && !list.Any((Resolution resolutionInList) => resolutionInList.width == 2560 && resolutionInList.height == 1440))
					{
						Resolution item8 = new Resolution
						{
							width = 2560,
							height = 1440,
							refreshRate = 30 * num
						};
						list.Add(item8);
					}
					if (item6.width == 3840 && item6.height == 2160 && !list.Any((Resolution resolutionInList) => resolutionInList.width == 3840 && resolutionInList.height == 2160))
					{
						Resolution item9 = new Resolution
						{
							width = 3840,
							height = 2160,
							refreshRate = 30
						};
						list.Add(item9);
					}
				}
				else
				{
					list.Add(item6);
				}
			}
		}
		return list;
	}
}
