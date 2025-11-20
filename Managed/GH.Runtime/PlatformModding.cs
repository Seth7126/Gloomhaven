#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GLOOM;
using Platforms;
using ScenarioRuleLibrary.CustomLevels;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;

public class PlatformModding : MonoBehaviour, IPlatformModding
{
	[ReadOnlyField]
	public bool ModsLoaded;

	public bool ModdingSupported => true;

	public bool LevelEditorSupported => true;

	public void Initialize(IPlatform platform)
	{
	}

	public void RefreshMods()
	{
		if (!SteamClient.IsValid)
		{
			ModsLoaded = true;
		}
		else
		{
			RefreshAvailableMods();
		}
	}

	public void UploadMod(string modName)
	{
		if (SteamClient.IsValid)
		{
			UploadModToWorkshop(modName);
		}
	}

	public void RefreshLevels()
	{
		if (SteamClient.IsValid)
		{
			RefreshAvailableWorkshopLevels();
		}
	}

	public void UploadLevel(CCustomLevelData levelToUpload, IProgress<float> progress, Action<string> onFail, Action onSuccessful)
	{
		if (SteamClient.IsValid)
		{
			UploadLevelToWorkshop(levelToUpload, progress, onFail, onSuccessful);
		}
	}

	private async void RefreshAvailableMods()
	{
		ModsLoaded = false;
		await RefreshAvailableModsAsync();
	}

	private async Task RefreshAvailableModsAsync()
	{
		try
		{
			if (!Directory.Exists(RootSaveData.ModValidationFolder))
			{
				Debug.Log("Creating mod validation folder: " + RootSaveData.ModValidationFolder);
				Directory.CreateDirectory(RootSaveData.ModValidationFolder);
			}
			if (!Directory.Exists(RootSaveData.ModsFolder))
			{
				Debug.Log("Creating SteamModsFolder: " + RootSaveData.ModsFolder);
				Directory.CreateDirectory(RootSaveData.ModsFolder);
			}
			else
			{
				SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.ModLoadingWithProgress);
				string[] directories = Directory.GetDirectories(RootSaveData.ModsFolder);
				float amount = 50f / (float)directories.Length;
				string[] array = directories;
				foreach (string text in array)
				{
					GHMod gHMod = SceneController.Instance.Modding.LoadMod(text, Path.GetFileName(text), -1f, isLocalMod: true);
					if (gHMod != null)
					{
						SceneController.Instance.Modding.Mods.Add(gHMod);
					}
					SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(amount);
				}
			}
			ResultPage? resultPage = await Query.All.WhereUserSubscribed().GetPageAsync(1);
			Debug.Log($"[Modding]: TotalCount of subscribed items - {resultPage?.TotalCount}");
			float amount2 = 50f / (float)resultPage.Value.Entries.Count();
			foreach (Item entry in resultPage.Value.Entries)
			{
				GHMod gHMod2 = SceneController.Instance.Modding.Mods.SingleOrDefault((GHMod s) => s.MetaData.Name == entry.Title);
				if (gHMod2 == null)
				{
					GHMod gHMod3 = SceneController.Instance.Modding.LoadMod(entry.Directory, entry.Title, entry.Score, isLocalMod: false);
					if (gHMod3 != null)
					{
						SceneController.Instance.Modding.Mods.Add(gHMod3);
					}
				}
				else
				{
					gHMod2.Rating = entry.Score;
				}
				SceneController.Instance.LoadingScreenInstance.IncrementProgressBar(amount2);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Unable to intialise modding.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		ModsLoaded = true;
	}

	private async void UploadModToWorkshop(string modName)
	{
		await UploadModToWorkshopAsync(modName);
	}

	private async Task UploadModToWorkshopAsync(string modName)
	{
		GHMod mod = SceneController.Instance.Modding.Mods.SingleOrDefault((GHMod s) => s.MetaData.Name == modName);
		if (mod == null || !Directory.Exists(mod.LocalPath))
		{
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_UNABLE_TO_FIND_MOD", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
			return;
		}
		mod.MetaData.Save(mod.LocalPath);
		string path = Path.Combine(mod.LocalPath, "meta.dat");
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		long num = Directory.GetFiles(mod.LocalPath, "*", SearchOption.AllDirectories).Sum((string f) => new FileInfo(f).Length);
		Debug.Log("[Modding]: Mod uploading size - " + num);
		if (num > 20971520)
		{
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MOD_TOO_BIG", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
			return;
		}
		if (GloomUtility.HasExecutable(mod.LocalPath))
		{
			Debug.Log("[Modding]: Mod has executable, it's not allowed.");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MOD_HAS_EXECUTABLES", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
			return;
		}
		string text = mod.MetaData.WritePreviewFile(mod.LocalPath);
		if (!File.Exists(text))
		{
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_FAILED_TO_CREATE_PREVIEW", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
			return;
		}
		mod.MetaData.ModVersion++;
		mod.MetaData.BuildVersion = Application.version;
		if (!mod.MetaData.Save(mod.LocalPath))
		{
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_ERROR_SAVING_MOD", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
			return;
		}
		SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.ModUploadingWithProgress);
		SceneController.Instance.ShowLoadingScreen();
		PublishResult result;
		if (mod.MetaData.PublishedFileId == 0L)
		{
			Debug.Log("[Modding] Uploading new Mod from dir: " + mod.LocalPath);
			result = await Editor.NewCommunityFile.WithTitle(modName).WithContent(mod.LocalPath).WithPreviewFile(text)
				.WithPublicVisibility()
				.SubmitAsync(SceneController.Instance.Modding);
		}
		else
		{
			Debug.Log("[Modding] Updating existing Mod from dir: " + mod.LocalPath);
			result = await new Editor(mod.MetaData.PublishedFileId).WithContent(mod.LocalPath).WithPreviewFile(text).WithPublicVisibility()
				.SubmitAsync(SceneController.Instance.Modding);
		}
		SceneController.Instance.DisableLoadingScreen();
		SceneController.Instance.LoadingScreenInstance.SetMode(LoadingScreen.ELoadingScreenMode.Loading);
		if (result.Success)
		{
			mod.MetaData.PublishedFileId = result.FileId;
			mod.MetaData.Save(mod.LocalPath);
			if (result.NeedsWorkshopAgreement)
			{
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_NEED_WORKSHOP_AGREEMENT", "GUI_OK", Environment.StackTrace, delegate
				{
					PublishedFileId fileId = result.FileId;
					Application.OpenURL("steam://url/CommunityFilePage/" + fileId.ToString());
				}, null, showErrorReportButton: false, trackDebug: false);
			}
			else
			{
				UIModdingNotifications.ShowUploadedModNotification(mod.MetaData.Name);
			}
		}
		else if (result.NeedsWorkshopAgreement)
		{
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_NEED_WORKSHOP_AGREEMENT", "GUI_OK", Environment.StackTrace, delegate
			{
				PublishedFileId fileId = result.FileId;
				Application.OpenURL("steam://url/CommunityFilePage/" + fileId.ToString());
			}, null, showErrorReportButton: false, trackDebug: false);
			mod.MetaData.ModVersion--;
			mod.MetaData.Save(mod.LocalPath);
		}
		else
		{
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("GUI_MODDING_UPLOAD_FAILED", "GUI_OK", Environment.StackTrace, SceneController.Instance.GlobalErrorMessage.Hide, null, showErrorReportButton: false, trackDebug: false);
			mod.MetaData.ModVersion--;
			mod.MetaData.Save(mod.LocalPath);
			Debug.LogError("Upload Failed!  Result: " + result.Result);
		}
	}

	public async void UploadLevelToWorkshop(CCustomLevelData levelToUpload, IProgress<float> progress, Action<string> onFail = null, Action onSuccessful = null)
	{
		string path = SaveData.Instance.LevelEditorDataManager.LevelDataSaveFilePath(levelToUpload.Name);
		string levelSaveFileDir = Path.GetDirectoryName(path);
		if (!Directory.Exists(levelSaveFileDir))
		{
			string obj = "Could not find the level you're trying to upload";
			onFail?.Invoke(obj);
			return;
		}
		if (Directory.GetFiles(levelSaveFileDir, "*", SearchOption.AllDirectories).Sum((string f) => new FileInfo(f).Length) > 20971520)
		{
			string obj2 = LocalizationManager.GetTranslation("GUI_MOD_TOO_BIG").Replace("\\n", "\n");
			onFail?.Invoke(obj2);
			return;
		}
		if (GloomUtility.HasExecutable(levelSaveFileDir))
		{
			string obj3 = LocalizationManager.GetTranslation("GUI_MOD_HAS_EXECUTABLES").Replace("\\n", "\n");
			onFail?.Invoke(obj3);
			return;
		}
		if (!SaveData.Instance.LevelEditorDataManager.CustomLevelMetadata.ContainsKey(levelToUpload))
		{
			SaveData.Instance.LevelEditorDataManager.CustomLevelMetadata.Add(levelToUpload, new ModMetadata());
		}
		ModMetadata metaData = SaveData.Instance.LevelEditorDataManager.GetModMetadataForCustomLevel(levelToUpload);
		metaData.Version++;
		metaData.BuildVersion = Application.version;
		metaData.Type = ModMetadata.ModType.Level;
		SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelToUpload, levelToUpload.Name);
		string previewFile = Path.Combine(levelSaveFileDir, "preview.png");
		if (!File.Exists(previewFile))
		{
			File.Copy(LevelEditorDataManager.DefaultLevelThumbnailPath, previewFile);
		}
		try
		{
			PublishResult publishResult;
			if (metaData.PublishedFileId == 0L)
			{
				publishResult = await Editor.NewCommunityFile.WithTitle(levelToUpload.Name).WithContent(levelSaveFileDir).WithPreviewFile(previewFile)
					.WithPublicVisibility()
					.SubmitAsync(progress);
				if (!publishResult.Success)
				{
					onFail?.Invoke(publishResult.Result.ToString());
					return;
				}
				onSuccessful?.Invoke();
				metaData.PublishedFileId = publishResult.FileId;
			}
			publishResult = await new Editor(metaData.PublishedFileId).WithContent(levelSaveFileDir).WithPreviewFile(previewFile).WithPublicVisibility()
				.SubmitAsync(progress);
			if (!publishResult.Success)
			{
				onFail?.Invoke(publishResult.Result.ToString());
				return;
			}
			onSuccessful?.Invoke();
			SaveData.Instance.LevelEditorDataManager.SaveLevelEditorBaseData(levelToUpload, levelToUpload.Name);
			if (publishResult.NeedsWorkshopAgreement)
			{
				PublishedFileId fileId = publishResult.FileId;
				Application.OpenURL("steam://url/CommunityFilePage/" + fileId.ToString());
			}
		}
		catch
		{
			string obj5 = "Failed to connect to Steam";
			onFail?.Invoke(obj5);
		}
	}

	public async void RefreshAvailableWorkshopLevels()
	{
		await RefreshAvailableWorkshopLevelsAsync();
	}

	public async Task RefreshAvailableWorkshopLevelsAsync()
	{
		try
		{
			SaveData.Instance.LevelEditorDataManager.AvailableWorkshopLevels = new Dictionary<ulong, string>();
			foreach (Item entry in (await Query.All.WhereUserSubscribed().GetPageAsync(1)).Value.Entries)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(entry.Directory);
				FileInfo[] files = directoryInfo.GetFiles("*.lvldat", SearchOption.AllDirectories);
				if (files.Length == 0)
				{
					continue;
				}
				FileInfo fileInfo = files[0];
				ModMetadata customLevelMetaDataForLevelFile = SaveData.Instance.LevelEditorDataManager.GetCustomLevelMetaDataForLevelFile(fileInfo);
				if (customLevelMetaDataForLevelFile == null || customLevelMetaDataForLevelFile.Type != ModMetadata.ModType.Level)
				{
					continue;
				}
				SaveData.Instance.LevelEditorDataManager.AvailableWorkshopLevels.Add(customLevelMetaDataForLevelFile.PublishedFileId, entry.Directory);
				string directoryName = Path.GetDirectoryName(SaveData.Instance.LevelEditorDataManager.WorkshopLevelSaveFilePath(Path.GetFileNameWithoutExtension(fileInfo.Name), customLevelMetaDataForLevelFile.PublishedFileId.ToString()));
				if (!Directory.Exists(directoryName))
				{
					Debug.Log("Creating local mod directory: " + directoryName);
					Directory.CreateDirectory(directoryName);
					FileInfo[] files2 = directoryInfo.GetFiles();
					foreach (FileInfo fileInfo2 in files2)
					{
						File.Copy(fileInfo2.FullName, Path.Combine(directoryName, fileInfo2.Name));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Unable to refresh steam workshop custom levels.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}
}
