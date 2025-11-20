using System.Collections.Generic;
using UnityEngine;

namespace Platforms.Utils;

public class TempDebug : MonoBehaviour
{
	private static List<string> _debugStrings = new List<string>();

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		int num = 10;
		foreach (string debugString in _debugStrings)
		{
			GUI.Label(new Rect(10f, num, 3000f, 40f), debugString);
			num += 24;
		}
	}

	public static void Log(string log)
	{
		_debugStrings.Add(log);
	}
}
