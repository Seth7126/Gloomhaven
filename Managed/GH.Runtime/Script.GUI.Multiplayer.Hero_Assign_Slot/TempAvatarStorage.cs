using System.Collections;
using System.Collections.Generic;
using SM.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;

namespace Script.GUI.Multiplayer.Hero_Assign_Slot;

public static class TempAvatarStorage
{
	private static Dictionary<string, ITempUserAvatarData> _userAvatarData = new Dictionary<string, ITempUserAvatarData>();

	public static ITempUserAvatarData CreateTempAvatar(string avatarUrl)
	{
		if (_userAvatarData.ContainsKey(avatarUrl))
		{
			return _userAvatarData[avatarUrl];
		}
		TempUserAvatarData tempUserAvatarData = new TempUserAvatarData
		{
			AvatarUrl = avatarUrl
		};
		tempUserAvatarData.Coroutine = CoroutineHelper.RunCoroutine(DownloadAvatarImage(tempUserAvatarData));
		_userAvatarData.Add(avatarUrl, tempUserAvatarData);
		return tempUserAvatarData;
	}

	private static IEnumerator DownloadAvatarImage(TempUserAvatarData tempUserAvatarData)
	{
		UnityWebRequest request = (tempUserAvatarData.UnityWebRequest = UnityWebRequestTexture.GetTexture(tempUserAvatarData.AvatarUrl));
		yield return request.SendWebRequest();
		if (request.result == UnityWebRequest.Result.Success)
		{
			tempUserAvatarData.Texture2D = ((DownloadHandlerTexture)request.downloadHandler).texture;
			tempUserAvatarData.Texture2D.hideFlags = HideFlags.HideAndDontSave;
			tempUserAvatarData.Sprite = Sprite.Create(tempUserAvatarData.Texture2D, new Rect(0f, 0f, tempUserAvatarData.Texture2D.width, tempUserAvatarData.Texture2D.height), new Vector2(0.5f, 0.5f));
			tempUserAvatarData.OnAvatarDownloaded();
		}
	}

	public static void RemoveTempAvatar(string avatarUrl)
	{
		if (_userAvatarData.ContainsKey(avatarUrl))
		{
			Dispose(_userAvatarData[avatarUrl] as TempUserAvatarData);
			_userAvatarData.Remove(avatarUrl);
		}
	}

	private static void Dispose(TempUserAvatarData tempUserAvatarData)
	{
		LogUtils.LogError("Temp_Dispose!!!");
		CoroutineHelper.StopCoroutineHelper(tempUserAvatarData.Coroutine);
		if (tempUserAvatarData.UnityWebRequest != null)
		{
			tempUserAvatarData.UnityWebRequest?.Abort();
			tempUserAvatarData.UnityWebRequest = null;
		}
		if (tempUserAvatarData.Texture2D != null)
		{
			RuntimeUtilities.Destroy(tempUserAvatarData.Texture2D);
			RuntimeUtilities.Destroy(tempUserAvatarData.Sprite);
			tempUserAvatarData.Texture2D = null;
			tempUserAvatarData.Sprite = null;
		}
	}
}
