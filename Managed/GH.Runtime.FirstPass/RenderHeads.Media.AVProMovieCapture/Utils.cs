#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SM.Utils;
using UnityEngine;

namespace RenderHeads.Media.AVProMovieCapture;

public class Utils
{
	public static Camera GetUltimateRenderCamera()
	{
		Camera result = Camera.main;
		float num = float.MinValue;
		Camera[] allCameras = Camera.allCameras;
		foreach (Camera camera in allCameras)
		{
			if (camera != null && (camera.hideFlags & HideFlags.HideInHierarchy) != HideFlags.HideInHierarchy && camera.targetTexture == null && camera.pixelRect.width > 0f && (float)camera.pixelHeight > 0f && camera.depth >= num)
			{
				num = camera.depth;
				result = camera;
			}
		}
		return result;
	}

	public static bool HasContributingCameras(Camera parentCamera)
	{
		bool result = true;
		if (parentCamera.rect == new Rect(0f, 0f, 1f, 1f) && (parentCamera.clearFlags == CameraClearFlags.Skybox || parentCamera.clearFlags == CameraClearFlags.Color))
		{
			result = false;
		}
		return result;
	}

	public static Camera[] FindContributingCameras(Camera parentCamera)
	{
		List<Camera> list = new List<Camera>(8);
		Camera[] array = (Camera[])Resources.FindObjectsOfTypeAll(typeof(Camera));
		foreach (Camera camera in array)
		{
			if (camera != null && camera != parentCamera && camera.depth <= parentCamera.depth && (camera.hideFlags & HideFlags.HideInHierarchy) != HideFlags.HideInHierarchy && camera.targetTexture == parentCamera.targetTexture && camera.pixelRect.width > 0f && camera.pixelHeight > 0)
			{
				list.Add(camera);
			}
		}
		if (list.Count > 1)
		{
			list.Sort(delegate(Camera a, Camera b)
			{
				if (a != b)
				{
					if (a.depth < b.depth)
					{
						return -1;
					}
					if (a.depth > b.depth)
					{
						return 1;
					}
					if (a.depth == b.depth)
					{
						LogUtils.LogWarning("[AVProMovieCapture] Cameras '" + a.name + "' and '" + b.name + "' have the same depth value - unable to determine render order: " + a.depth);
					}
				}
				return 0;
			});
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].rect == new Rect(0f, 0f, 1f, 1f) && (list[num].clearFlags == CameraClearFlags.Skybox || list[num].clearFlags == CameraClearFlags.Color))
				{
					list.RemoveRange(0, num);
					break;
				}
			}
		}
		return list.ToArray();
	}

	public static bool ShowInExplorer(string itemPath)
	{
		bool result = false;
		itemPath = Path.GetFullPath(itemPath.Replace("/", "\\"));
		if (File.Exists(itemPath))
		{
			Process.Start("explorer.exe", "/select," + itemPath);
			result = true;
		}
		else if (Directory.Exists(itemPath))
		{
			Application.OpenURL(itemPath);
			result = true;
		}
		return result;
	}

	public static bool OpenInDefaultApp(string itemPath)
	{
		bool result = false;
		itemPath = Path.GetFullPath(itemPath.Replace("/", "\\"));
		if (File.Exists(itemPath))
		{
			Application.OpenURL(itemPath);
			result = true;
		}
		else if (Directory.Exists(itemPath))
		{
			Application.OpenURL(itemPath);
			result = true;
		}
		return result;
	}

	public static long GetFileSize(string filename)
	{
		return new FileInfo(filename).Length;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

	public static bool DriveFreeBytes(string folderName, out ulong freespace)
	{
		freespace = 0uL;
		if (string.IsNullOrEmpty(folderName))
		{
			throw new ArgumentNullException("folderName");
		}
		if (!folderName.EndsWith("\\"))
		{
			folderName += "\\";
		}
		ulong lpFreeBytesAvailable = 0uL;
		ulong lpTotalNumberOfBytes = 0uL;
		ulong lpTotalNumberOfFreeBytes = 0uL;
		if (GetDiskFreeSpaceEx(folderName, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes))
		{
			freespace = lpFreeBytesAvailable;
			return true;
		}
		return false;
	}
}
