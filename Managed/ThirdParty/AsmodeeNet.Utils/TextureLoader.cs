using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace AsmodeeNet.Utils;

public class TextureLoader
{
	private const string _debugModuleName = "TextureLoader";

	public static IEnumerator LoadTexture(string url, MaskableGraphic image, Action<bool, byte[]> afterLoading = null)
	{
		byte[] downloadedBytes;
		Texture2D texture2D;
		if (url.StartsWith("http"))
		{
			UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
			yield return request.SendWebRequest();
			if (request.isNetworkError || request.isHttpError)
			{
				Hashtable extraInfo = new Hashtable
				{
					{ "url", url },
					{ "error", request.error }
				};
				AsmoLogger.Error("TextureLoader", "Download failed", extraInfo);
				afterLoading?.Invoke(arg1: false, null);
				yield break;
			}
			downloadedBytes = request.downloadHandler.data;
			texture2D = DownloadHandlerTexture.GetContent(request);
		}
		else
		{
			downloadedBytes = File.ReadAllBytes(url);
			texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false, linear: false);
			texture2D.LoadImage(downloadedBytes);
			texture2D.anisoLevel = 16;
		}
		if (image != null && image.gameObject != null && texture2D != null)
		{
			if (image is Image)
			{
				((Image)image).sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
			}
			else if (image is RawImage)
			{
				((RawImage)image).texture = texture2D;
			}
			else
			{
				AsmoLogger.Error("TextureLoader", "Parameter 'image' is not of type Image or RawImage");
			}
			yield return null;
			image.enabled = true;
			image.gameObject.SetActive(value: true);
			yield return null;
			afterLoading?.Invoke(arg1: true, downloadedBytes);
		}
		else
		{
			Hashtable extraInfo2 = new Hashtable
			{
				{ "url", url },
				{ "image", image },
				{ "texture", texture2D }
			};
			AsmoLogger.Error("TextureLoader", "Something was wrong with the image or the texture", extraInfo2);
			afterLoading(arg1: false, downloadedBytes);
		}
	}

	private static string GetResourcePath(string path)
	{
		path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
		int num = path.IndexOf("Resources");
		path = path.Substring(num + "Resources/".Length).Replace("\\", "/");
		return path;
	}

	public static bool StartWithJPEGHeader(byte[] data)
	{
		if (data == null || data.Length <= 4)
		{
			return false;
		}
		if (data[0] == byte.MaxValue && data[1] == 216 && data[^2] == byte.MaxValue)
		{
			return data[^1] == 217;
		}
		return false;
	}

	public static bool StartWithPNGHeader(byte[] data)
	{
		if (data[0] == 137 && data[1] == 80 && data[2] == 78 && data[3] == 71 && data[4] == 13 && data[5] == 10 && data[6] == 26)
		{
			return data[7] == 10;
		}
		return false;
	}
}
