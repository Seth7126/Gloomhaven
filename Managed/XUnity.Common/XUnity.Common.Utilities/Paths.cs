using System.IO;
using UnityEngine;

namespace XUnity.Common.Utilities;

public static class Paths
{
	private static string _gameRoot;

	public static string GameRoot
	{
		get
		{
			return _gameRoot ?? GetAndSetGameRoot();
		}
		set
		{
			_gameRoot = value;
		}
	}

	public static void Initialize()
	{
		GetAndSetGameRoot();
	}

	private static string GetAndSetGameRoot()
	{
		return _gameRoot = new DirectoryInfo(Application.dataPath).Parent.FullName;
	}
}
