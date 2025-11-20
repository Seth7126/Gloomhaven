using UnityEngine;

namespace AsmodeeNet.Utils.Extensions;

public static class ScreenExtension
{
	public static float DiagonalLengthInch
	{
		get
		{
			float num = Screen.dpi;
			if (num < 25f || num > 1000f)
			{
				num = 150f;
			}
			return Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / num;
		}
	}
}
