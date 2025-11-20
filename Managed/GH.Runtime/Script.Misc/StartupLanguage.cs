using System.Collections.Generic;
using Newtonsoft.Json;
using Platforms;
using UnityEngine;

namespace Script.Misc;

public class StartupLanguage
{
	private readonly IPlatformUserManagement _userManagement;

	private const string LanguageStorageName = "StartupLanguageStorage";

	public StartupLanguage(IPlatformUserManagement userManagement)
	{
		_userManagement = userManagement;
	}

	public string GetStartupLanguage()
	{
		string text = Application.systemLanguage.ToString();
		if (PlayerPrefs.HasKey("StartupLanguageStorage"))
		{
			Platforms.IPlatformUserData currentUser = _userManagement.GetCurrentUser();
			if (currentUser == null)
			{
				Log("CurrenUser is null. Return system language.");
				return text;
			}
			string platformUniqueUserID = currentUser.GetPlatformUniqueUserID();
			if (string.IsNullOrEmpty(platformUniqueUserID))
			{
				Log("Uid is not set. Return system language.");
				return text;
			}
			Dictionary<string, string> languageBase = GetLanguageBase();
			if (languageBase.ContainsKey(platformUniqueUserID))
			{
				string text2 = languageBase[platformUniqueUserID];
				Log("Founded " + text2 + " language for " + platformUniqueUserID);
				return text2;
			}
			Log("Not found saved startup language for " + platformUniqueUserID + " user. Return system language.");
			return text;
		}
		Log("Language storage is not created. Save and return system language: " + text + ".");
		SaveLanguage(text);
		return text;
	}

	public void SaveLanguage(string language)
	{
		Dictionary<string, string> languageBase = GetLanguageBase();
		Platforms.IPlatformUserData currentUser = _userManagement.GetCurrentUser();
		if (currentUser == null)
		{
			Log("Current User is null. Startup Language is not saved");
			return;
		}
		string platformUniqueUserID = currentUser.GetPlatformUniqueUserID();
		if (string.IsNullOrEmpty(platformUniqueUserID))
		{
			Log("Uid is not set. Startup Language is not saved");
			return;
		}
		if (languageBase.ContainsKey(platformUniqueUserID))
		{
			languageBase[platformUniqueUserID] = language;
		}
		else
		{
			languageBase.Add(platformUniqueUserID, language);
		}
		Log(language + " language saved as startup for " + platformUniqueUserID);
		PlayerPrefs.SetString("StartupLanguageStorage", JsonConvert.SerializeObject(languageBase));
		PlayerPrefs.Save();
	}

	private Dictionary<string, string> GetLanguageBase()
	{
		if (PlayerPrefs.HasKey("StartupLanguageStorage"))
		{
			return JsonConvert.DeserializeObject<Dictionary<string, string>>(PlayerPrefs.GetString("StartupLanguageStorage"));
		}
		return new Dictionary<string, string>();
	}

	private void Log(string message)
	{
		UnityEngine.Debug.Log("[StartupLanguageProxy] " + message);
	}
}
