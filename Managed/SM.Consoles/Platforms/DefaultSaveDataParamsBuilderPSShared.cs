#define ENABLE_LOGS
using System;
using SM.Utils;

namespace Platforms;

public class DefaultSaveDataParamsBuilderPSShared : ISaveDataParamsBuilderPSShared
{
	private const string DEFAULT_SAVE_DATA_NAME = "PersistentSaveUserData";

	private const string SAVE_ICON_PATH = "";

	public ulong SaveDataSize => 183500800uL;

	public void GetWriteParams(string originalPath, out string saveDataDirectory, out string innerPathToFile, out SaveDataParams saveDataParams, string[] tags)
	{
		SplitPathToDirectoryAndInnerPath(originalPath, out saveDataDirectory, out innerPathToFile, tags);
		LogUtils.Log("DefaultSaveDataParamsBuilderPSShared Directory - " + saveDataDirectory + ";");
		LogUtils.Log("DefaultSaveDataParamsBuilderPSShared InnerPath - " + innerPathToFile + ";");
		saveDataParams = new SaveDataParams
		{
			Size = SaveDataSize,
			Icon = new IconParams(""),
			Title = saveDataDirectory,
			SubTitle = innerPathToFile,
			Detail = string.Empty
		};
		LogUtils.Log("DefaultSaveDataParamsBuilderPSShared Title - " + saveDataParams.Title + ";");
		LogUtils.Log("DefaultSaveDataParamsBuilderPSShared SubTitle - " + saveDataParams.SubTitle + ";");
		LogUtils.Log("DefaultSaveDataParamsBuilderPSShared Detail - " + saveDataParams.Detail + ";");
	}

	public void GetReadParams(string originalPath, out string saveDataDirectory, out string innerPathToFile, string[] tags)
	{
		SplitPathToDirectoryAndInnerPath(originalPath, out saveDataDirectory, out innerPathToFile, tags);
	}

	private void SplitPathToDirectoryAndInnerPath(string originalPath, out string saveDataDirectory, out string innerPathToFile, string[] tags)
	{
		saveDataDirectory = string.Empty;
		innerPathToFile = string.Empty;
		originalPath = "PersistentSaveUserData/" + originalPath;
		originalPath = originalPath.Replace("//", "/");
		string[] array = originalPath.Split(new char[2] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = new string[array.Length * 2];
		for (int i = 1; i < array.Length; i++)
		{
			int num = i;
			int num2 = (num - 1) * 2;
			array2[num2] = array[num];
			if (num != array.Length - 1)
			{
				array2[num2 + 1] = "/";
			}
		}
		saveDataDirectory = array[0];
		innerPathToFile = string.Concat(array2);
		if (string.IsNullOrEmpty(innerPathToFile))
		{
			innerPathToFile = "/" + saveDataDirectory;
			saveDataDirectory = "PersistentSaveUserData";
		}
	}
}
