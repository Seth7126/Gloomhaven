using System;
using UnityEngine;
using UnityEngine.Networking;

public class AndroidFileIOHelper : MonoBehaviour
{
	public static string ReadAllText(string filePath)
	{
		using UnityWebRequest unityWebRequest = UnityWebRequest.Get(filePath);
		unityWebRequest.timeout = 2;
		unityWebRequest.SendWebRequest();
		while (!unityWebRequest.isDone)
		{
		}
		if (unityWebRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("Requesting " + filePath + ", please make sure it exists and is a valid config");
			throw new Exception("UnityWebRequest didn't succeed, Result : " + unityWebRequest.result);
		}
		return unityWebRequest.downloadHandler.text;
	}
}
