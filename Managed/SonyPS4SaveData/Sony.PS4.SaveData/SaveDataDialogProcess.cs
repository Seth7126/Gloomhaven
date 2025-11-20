using System.Collections;

namespace Sony.PS4.SaveData;

public class SaveDataDialogProcess
{
	private enum SaveState
	{
		Begin,
		Searching,
		ShowListStart,
		ShowList,
		ShowNoDataStart,
		ShowSaveStart,
		ShowSaveWaitForDialog,
		ShowSave,
		ShowLoadStart,
		ShowLoadWaitForDialog,
		ShowLoad,
		ShowDeleteStart,
		ShowDeleteWaitForDialog,
		ShowDelete,
		OverwriteStart,
		Overwrite,
		ConfirmDeleteStart,
		ConfirmDelete,
		ConfirmRestoreStart,
		ConfirmRestore,
		ShowRestoreStart,
		ShowRetoreWaitForDialog,
		ShowRestore,
		ShowErrorNoSpaceStart,
		ShowErrorNoSpace,
		ShowErrorStart,
		ShowError,
		Finished,
		Exit
	}

	public delegate bool AllowNewItemTest(Searching.DirNameSearchResponse response);

	private static DirName emptyDirName = default(DirName);

	public static IEnumerator StartSaveDialogProcess(int userId, Dialogs.NewItem newItem, DirName newDirName, ulong newSaveDataBlocks, SaveDataParams saveDataParams, FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse, bool backup, AllowNewItemTest allowNewCB)
	{
		SaveState currentState = SaveState.Begin;
		Searching.DirNameSearchResponse searchResponse = new Searching.DirNameSearchResponse();
		Dialogs.OpenDialogResponse openDialogResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse progressBarResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse overwriteResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse confirmRestoreResponse = new Dialogs.OpenDialogResponse();
		EmptyResponse restoreResponse = new EmptyResponse();
		Dialogs.OpenDialogResponse errorResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse noSpaceResponse = new Dialogs.OpenDialogResponse();
		Mounting.MountResponse mountResponse = new Mounting.MountResponse();
		int errorCode = 0;
		DirName selectedDirName = default(DirName);
		while (true)
		{
			switch (currentState)
			{
			case SaveState.Begin:
				errorCode = FullSearch(userId, searchResponse);
				currentState = ((errorCode >= 0) ? SaveState.Searching : SaveState.ShowErrorStart);
				break;
			case SaveState.Searching:
				if (!searchResponse.Locked)
				{
					if (searchResponse.IsErrorCode)
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = searchResponse.ReturnCodeValue;
					}
					else
					{
						currentState = SaveState.ShowListStart;
					}
				}
				break;
			case SaveState.ShowListStart:
			{
				bool allowNew = allowNewCB?.Invoke(searchResponse) ?? true;
				errorCode = ListDialog(userId, Dialogs.DialogType.Save, openDialogResponse, searchResponse, allowNew ? newItem : null);
				currentState = ((errorCode >= 0) ? SaveState.ShowList : SaveState.ShowErrorStart);
				break;
			}
			case SaveState.ShowList:
			{
				if (openDialogResponse.Locked)
				{
					break;
				}
				if (openDialogResponse.IsErrorCode)
				{
					currentState = SaveState.ShowErrorStart;
					errorCode = openDialogResponse.ReturnCodeValue;
					break;
				}
				Dialogs.DialogResult result = openDialogResponse.Result;
				if (result.CallResult == Dialogs.DialogCallResults.OK)
				{
					if (result.DirName.IsEmpty)
					{
						currentState = SaveState.ShowSaveStart;
						break;
					}
					selectedDirName = result.DirName;
					currentState = SaveState.OverwriteStart;
				}
				else
				{
					currentState = SaveState.Finished;
				}
				break;
			}
			case SaveState.ShowSaveStart:
			{
				Dialogs.NewItem newItem2 = newItem;
				DirName loadDirName = newDirName;
				if (!selectedDirName.IsEmpty)
				{
					newItem2 = null;
					loadDirName = selectedDirName;
				}
				errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Save, progressBarResponse, newItem2, Dialogs.ProgressSystemMessageType.Progress, loadDirName);
				currentState = ((errorCode >= 0) ? SaveState.ShowSaveWaitForDialog : SaveState.ShowErrorStart);
				break;
			}
			case SaveState.ShowSaveWaitForDialog:
			{
				Dialogs.DialogStatus dialogStatus = Dialogs.DialogGetStatus();
				if (dialogStatus == Dialogs.DialogStatus.Running && Dialogs.DialogIsReadyToDisplay())
				{
					currentState = SaveState.ShowSave;
				}
				break;
			}
			case SaveState.ShowSave:
			{
				Mounting.MountModeFlags flags = Mounting.MountModeFlags.ReadWrite | Mounting.MountModeFlags.Create2;
				DirName dirName = (selectedDirName.IsEmpty ? newDirName : selectedDirName);
				errorCode = MountSaveData(userId, newSaveDataBlocks, mountResponse, dirName, flags);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (mountResponse.Locked)
				{
					yield return null;
				}
				if (mountResponse.IsErrorCode)
				{
					if (mountResponse.ReturnCode == ReturnCodes.DATA_ERROR_NO_SPACE_FS)
					{
						currentState = SaveState.ShowErrorNoSpaceStart;
					}
					else if (mountResponse.ReturnCode == ReturnCodes.SAVE_DATA_ERROR_BROKEN)
					{
						Backups.CheckBackupResponse backupResponse = new Backups.CheckBackupResponse();
						errorCode = CheckBackup(userId, backupResponse, dirName);
						if (errorCode < 0)
						{
							currentState = SaveState.ShowErrorStart;
							break;
						}
						while (backupResponse.Locked)
						{
							yield return null;
						}
						if (backupResponse.IsErrorCode)
						{
							currentState = SaveState.ShowErrorStart;
							errorCode = mountResponse.ReturnCodeValue;
						}
						else
						{
							currentState = SaveState.ConfirmRestoreStart;
						}
					}
					else
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = mountResponse.ReturnCodeValue;
					}
					break;
				}
				Mounting.MountPoint mp = mountResponse.MountPoint;
				fileRequest.MountPointName = mp.PathName;
				fileRequest.Async = true;
				fileRequest.UserId = userId;
				errorCode = FileOps.CustomFileOp(fileRequest, fileResponse);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (fileResponse.Locked)
				{
					Dialogs.ProgressBarSetValue((uint)(fileResponse.Progress * 100f));
					yield return null;
				}
				Dialogs.ProgressBarSetValue((uint)(fileResponse.Progress * 100f));
				EmptyResponse iconResponse = new EmptyResponse();
				errorCode = WriteIcon(userId, iconResponse, mp, newItem);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				EmptyResponse paramsResponse = new EmptyResponse();
				errorCode = WriteParams(userId, paramsResponse, mp, saveDataParams);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (iconResponse.Locked || paramsResponse.Locked)
				{
					yield return null;
				}
				EmptyResponse unmountResponse = new EmptyResponse();
				errorCode = UnmountSaveData(userId, unmountResponse, mp, backup);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (unmountResponse.Locked)
				{
					yield return null;
				}
				ForceCloseDialog();
				currentState = SaveState.Finished;
				break;
			}
			case SaveState.OverwriteStart:
				errorCode = ConfirmDialog(userId, Dialogs.DialogType.Save, overwriteResponse, Dialogs.SystemMessageType.Overwrite, selectedDirName, Dialogs.Animation.Off, Dialogs.Animation.On, null, 0uL);
				currentState = ((errorCode >= 0) ? SaveState.Overwrite : SaveState.ShowErrorStart);
				break;
			case SaveState.Overwrite:
				if (!overwriteResponse.Locked)
				{
					if (overwriteResponse.IsErrorCode)
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = overwriteResponse.ReturnCodeValue;
					}
					else
					{
						Dialogs.DialogResult result = overwriteResponse.Result;
						currentState = ((result.CallResult != Dialogs.DialogCallResults.OK) ? SaveState.Finished : ((result.ButtonId != Dialogs.DialogButtonIds.OK) ? SaveState.Finished : SaveState.ShowSaveStart));
					}
				}
				break;
			case SaveState.ConfirmRestoreStart:
				ForceCloseDialog(Dialogs.Animation.Off);
				while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
				{
					yield return null;
				}
				errorCode = ConfirmDialog(userId, Dialogs.DialogType.Save, confirmRestoreResponse, Dialogs.SystemMessageType.CurruptedAndRestore, selectedDirName, Dialogs.Animation.Off, Dialogs.Animation.On, null, 0uL);
				currentState = ((errorCode >= 0) ? SaveState.ConfirmRestore : SaveState.ShowErrorStart);
				break;
			case SaveState.ConfirmRestore:
				if (!confirmRestoreResponse.Locked)
				{
					if (confirmRestoreResponse.IsErrorCode)
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = confirmRestoreResponse.ReturnCodeValue;
					}
					else
					{
						Dialogs.DialogResult result = confirmRestoreResponse.Result;
						currentState = ((result.CallResult != Dialogs.DialogCallResults.OK) ? SaveState.Finished : ((result.ButtonId != Dialogs.DialogButtonIds.OK) ? SaveState.Finished : SaveState.ShowRestoreStart));
					}
				}
				break;
			case SaveState.ShowRestoreStart:
				progressBarResponse = new Dialogs.OpenDialogResponse();
				errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Save, progressBarResponse, null, Dialogs.ProgressSystemMessageType.Restore, emptyDirName);
				currentState = ((errorCode >= 0) ? SaveState.ShowRetoreWaitForDialog : SaveState.ShowErrorStart);
				break;
			case SaveState.ShowRetoreWaitForDialog:
			{
				Dialogs.DialogStatus dialogStatus = Dialogs.DialogGetStatus();
				if (dialogStatus == Dialogs.DialogStatus.Running && Dialogs.DialogIsReadyToDisplay())
				{
					Progress.ClearProgress();
					errorCode = RestoreBackup(userId, restoreResponse, selectedDirName);
					currentState = ((errorCode >= 0) ? SaveState.ShowRestore : SaveState.ShowErrorStart);
				}
				break;
			}
			case SaveState.ShowRestore:
				if (!restoreResponse.Locked)
				{
					ForceCloseDialog(Dialogs.Animation.Off);
					while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
					{
						yield return null;
					}
					currentState = SaveState.ShowSaveStart;
				}
				else
				{
					float progress = Progress.GetProgress();
					Dialogs.ProgressBarSetValue((uint)(progress * 100f));
				}
				break;
			case SaveState.ShowErrorNoSpaceStart:
			{
				ForceCloseDialog(Dialogs.Animation.Off);
				while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
				{
					yield return null;
				}
				Dialogs.NewItem useNewItem = newItem;
				if (!selectedDirName.IsEmpty)
				{
					useNewItem = null;
				}
				errorCode = ConfirmDialog(userId, Dialogs.DialogType.Save, noSpaceResponse, Dialogs.SystemMessageType.NoSpace, emptyDirName, Dialogs.Animation.On, Dialogs.Animation.On, useNewItem, mountResponse.RequiredBlocks);
				currentState = ((errorCode >= 0) ? SaveState.ShowErrorNoSpace : SaveState.ShowErrorStart);
				break;
			}
			case SaveState.ShowErrorNoSpace:
				while (noSpaceResponse.Locked)
				{
					yield return null;
				}
				if (noSpaceResponse.IsErrorCode)
				{
					currentState = SaveState.ShowErrorStart;
					errorCode = noSpaceResponse.ReturnCodeValue;
				}
				else
				{
					currentState = SaveState.Finished;
				}
				break;
			case SaveState.ShowErrorStart:
				ForceCloseDialog(Dialogs.Animation.Off);
				while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
				{
					yield return null;
				}
				currentState = (ErrorDialog(userId, Dialogs.DialogType.Save, errorResponse, errorCode) ? SaveState.ShowError : SaveState.Finished);
				break;
			case SaveState.ShowError:
				if (!errorResponse.Locked)
				{
					currentState = SaveState.Finished;
				}
				break;
			case SaveState.Finished:
				currentState = SaveState.Exit;
				break;
			case SaveState.Exit:
				yield break;
			}
			yield return null;
		}
	}

	public static IEnumerator StartLoadDialogProcess(int userId, FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse)
	{
		SaveState currentState = SaveState.Begin;
		Searching.DirNameSearchResponse searchResponse = new Searching.DirNameSearchResponse();
		Dialogs.OpenDialogResponse openDialogResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse progressBarResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse confirmRestoreResponse = new Dialogs.OpenDialogResponse();
		EmptyResponse restoreResponse = new EmptyResponse();
		Dialogs.OpenDialogResponse errorResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse noDataResponse = new Dialogs.OpenDialogResponse();
		int errorCode = 0;
		DirName selectedDirName = default(DirName);
		while (true)
		{
			switch (currentState)
			{
			case SaveState.Begin:
				errorCode = FullSearch(userId, searchResponse);
				currentState = ((errorCode >= 0) ? SaveState.Searching : SaveState.ShowErrorStart);
				break;
			case SaveState.Searching:
				if (!searchResponse.Locked)
				{
					if (searchResponse.IsErrorCode)
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = searchResponse.ReturnCodeValue;
					}
					else
					{
						currentState = ((searchResponse.SaveDataItems == null || searchResponse.SaveDataItems.Length <= 0) ? SaveState.ShowNoDataStart : SaveState.ShowListStart);
					}
				}
				break;
			case SaveState.ShowNoDataStart:
				errorCode = ConfirmDialog(userId, Dialogs.DialogType.Load, noDataResponse, Dialogs.SystemMessageType.NoData, emptyDirName, Dialogs.Animation.On, Dialogs.Animation.On, null, 0uL);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (noDataResponse.Locked)
				{
					yield return null;
				}
				if (noDataResponse.IsErrorCode)
				{
					currentState = SaveState.ShowErrorStart;
					errorCode = noDataResponse.ReturnCodeValue;
				}
				else
				{
					currentState = SaveState.Finished;
				}
				break;
			case SaveState.ShowListStart:
				errorCode = ListDialog(userId, Dialogs.DialogType.Load, openDialogResponse, searchResponse, null);
				currentState = ((errorCode >= 0) ? SaveState.ShowList : SaveState.ShowErrorStart);
				break;
			case SaveState.ShowList:
			{
				if (openDialogResponse.Locked)
				{
					break;
				}
				if (openDialogResponse.IsErrorCode)
				{
					currentState = SaveState.ShowErrorStart;
					errorCode = openDialogResponse.ReturnCodeValue;
					break;
				}
				Dialogs.DialogResult result = openDialogResponse.Result;
				if (result.CallResult == Dialogs.DialogCallResults.OK)
				{
					selectedDirName = result.DirName;
					currentState = SaveState.ShowLoadStart;
				}
				else
				{
					currentState = SaveState.Finished;
				}
				break;
			}
			case SaveState.ShowLoadStart:
				errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Load, progressBarResponse, null, Dialogs.ProgressSystemMessageType.Progress, selectedDirName);
				currentState = ((errorCode >= 0) ? SaveState.ShowLoadWaitForDialog : SaveState.ShowErrorStart);
				break;
			case SaveState.ShowLoadWaitForDialog:
			{
				Dialogs.DialogStatus dialogStatus = Dialogs.DialogGetStatus();
				if (dialogStatus == Dialogs.DialogStatus.Running && Dialogs.DialogIsReadyToDisplay())
				{
					currentState = SaveState.ShowLoad;
				}
				break;
			}
			case SaveState.ShowLoad:
			{
				Mounting.MountResponse mountResponse = new Mounting.MountResponse();
				Mounting.MountModeFlags flags = Mounting.MountModeFlags.ReadOnly;
				DirName dirName = selectedDirName;
				errorCode = MountSaveData(userId, 0uL, mountResponse, dirName, flags);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (mountResponse.Locked)
				{
					yield return null;
				}
				if (mountResponse.IsErrorCode)
				{
					if (mountResponse.ReturnCode == ReturnCodes.SAVE_DATA_ERROR_BROKEN)
					{
						Backups.CheckBackupResponse backupResponse = new Backups.CheckBackupResponse();
						errorCode = CheckBackup(userId, backupResponse, dirName);
						if (errorCode < 0)
						{
							currentState = SaveState.ShowErrorStart;
							break;
						}
						while (backupResponse.Locked)
						{
							yield return null;
						}
						if (backupResponse.IsErrorCode)
						{
							currentState = SaveState.ShowErrorStart;
							errorCode = mountResponse.ReturnCodeValue;
						}
						else
						{
							currentState = SaveState.ConfirmRestoreStart;
						}
					}
					else
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = mountResponse.ReturnCodeValue;
					}
					break;
				}
				Mounting.MountPoint mp = mountResponse.MountPoint;
				fileRequest.MountPointName = mp.PathName;
				fileRequest.Async = true;
				fileRequest.UserId = userId;
				errorCode = FileOps.CustomFileOp(fileRequest, fileResponse);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (fileResponse.Locked)
				{
					Dialogs.ProgressBarSetValue((uint)(fileResponse.Progress * 100f));
					yield return null;
				}
				Dialogs.ProgressBarSetValue((uint)(fileResponse.Progress * 100f));
				yield return null;
				EmptyResponse unmountResponse = new EmptyResponse();
				errorCode = UnmountSaveData(userId, unmountResponse, mp, backup: false);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (unmountResponse.Locked)
				{
					yield return null;
				}
				ForceCloseDialog();
				currentState = SaveState.Finished;
				break;
			}
			case SaveState.ConfirmRestoreStart:
				ForceCloseDialog(Dialogs.Animation.Off);
				while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
				{
					yield return null;
				}
				errorCode = ConfirmDialog(userId, Dialogs.DialogType.Load, confirmRestoreResponse, Dialogs.SystemMessageType.CurruptedAndRestore, selectedDirName, Dialogs.Animation.Off, Dialogs.Animation.On, null, 0uL);
				currentState = ((errorCode >= 0) ? SaveState.ConfirmRestore : SaveState.ShowErrorStart);
				break;
			case SaveState.ConfirmRestore:
				if (!confirmRestoreResponse.Locked)
				{
					if (confirmRestoreResponse.IsErrorCode)
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = confirmRestoreResponse.ReturnCodeValue;
					}
					else
					{
						Dialogs.DialogResult result = confirmRestoreResponse.Result;
						currentState = ((result.CallResult != Dialogs.DialogCallResults.OK) ? SaveState.Finished : ((result.ButtonId != Dialogs.DialogButtonIds.OK) ? SaveState.Finished : SaveState.ShowRestoreStart));
					}
				}
				break;
			case SaveState.ShowRestoreStart:
				progressBarResponse = new Dialogs.OpenDialogResponse();
				errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Load, progressBarResponse, null, Dialogs.ProgressSystemMessageType.Restore, selectedDirName);
				currentState = ((errorCode >= 0) ? SaveState.ShowRetoreWaitForDialog : SaveState.ShowErrorStart);
				break;
			case SaveState.ShowRetoreWaitForDialog:
			{
				Dialogs.DialogStatus dialogStatus = Dialogs.DialogGetStatus();
				if (dialogStatus == Dialogs.DialogStatus.Running && Dialogs.DialogIsReadyToDisplay())
				{
					Progress.ClearProgress();
					errorCode = RestoreBackup(userId, restoreResponse, selectedDirName);
					currentState = ((errorCode >= 0) ? SaveState.ShowRestore : SaveState.ShowErrorStart);
				}
				break;
			}
			case SaveState.ShowRestore:
				if (!restoreResponse.Locked)
				{
					ForceCloseDialog(Dialogs.Animation.Off);
					while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
					{
						yield return null;
					}
					currentState = SaveState.ShowLoadStart;
				}
				else
				{
					float progress = Progress.GetProgress();
					Dialogs.ProgressBarSetValue((uint)(progress * 100f));
				}
				break;
			case SaveState.ShowErrorStart:
				ForceCloseDialog(Dialogs.Animation.Off);
				while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
				{
					yield return null;
				}
				currentState = (ErrorDialog(userId, Dialogs.DialogType.Load, errorResponse, errorCode) ? SaveState.ShowError : SaveState.Finished);
				break;
			case SaveState.ShowError:
				if (!errorResponse.Locked)
				{
					currentState = SaveState.Finished;
				}
				break;
			case SaveState.Finished:
				currentState = SaveState.Exit;
				break;
			case SaveState.Exit:
				yield break;
			}
			yield return null;
		}
	}

	public static IEnumerator StartDeleteDialogProcess(int userId)
	{
		SaveState currentState = SaveState.Begin;
		Searching.DirNameSearchResponse searchResponse = new Searching.DirNameSearchResponse();
		Dialogs.OpenDialogResponse openDialogResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse progressBarResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse confirmDeleteResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse errorResponse = new Dialogs.OpenDialogResponse();
		Dialogs.OpenDialogResponse noDataResponse = new Dialogs.OpenDialogResponse();
		int errorCode = 0;
		DirName selectedDirName = default(DirName);
		while (currentState != SaveState.Exit)
		{
			switch (currentState)
			{
			case SaveState.Begin:
				errorCode = FullSearch(userId, searchResponse);
				currentState = ((errorCode >= 0) ? SaveState.Searching : SaveState.ShowErrorStart);
				break;
			case SaveState.Searching:
				if (!searchResponse.Locked)
				{
					if (searchResponse.IsErrorCode)
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = searchResponse.ReturnCodeValue;
					}
					else
					{
						currentState = ((searchResponse.SaveDataItems == null || searchResponse.SaveDataItems.Length <= 0) ? SaveState.ShowNoDataStart : SaveState.ShowListStart);
					}
				}
				break;
			case SaveState.ShowNoDataStart:
				errorCode = ConfirmDialog(userId, Dialogs.DialogType.Delete, noDataResponse, Dialogs.SystemMessageType.NoData, emptyDirName, Dialogs.Animation.On, Dialogs.Animation.On, null, 0uL);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (noDataResponse.Locked)
				{
					yield return null;
				}
				if (noDataResponse.IsErrorCode)
				{
					currentState = SaveState.ShowErrorStart;
					errorCode = noDataResponse.ReturnCodeValue;
				}
				else
				{
					currentState = SaveState.Finished;
				}
				break;
			case SaveState.ShowListStart:
				errorCode = ListDialog(userId, Dialogs.DialogType.Delete, openDialogResponse, searchResponse, null);
				currentState = ((errorCode >= 0) ? SaveState.ShowList : SaveState.ShowErrorStart);
				break;
			case SaveState.ShowList:
			{
				if (openDialogResponse.Locked)
				{
					break;
				}
				if (openDialogResponse.IsErrorCode)
				{
					currentState = SaveState.ShowErrorStart;
					errorCode = openDialogResponse.ReturnCodeValue;
					break;
				}
				Dialogs.DialogResult result = openDialogResponse.Result;
				if (result.CallResult == Dialogs.DialogCallResults.OK)
				{
					selectedDirName = result.DirName;
					currentState = SaveState.ConfirmDeleteStart;
				}
				else
				{
					currentState = SaveState.Finished;
				}
				break;
			}
			case SaveState.ShowDeleteStart:
				errorCode = ProgressBarDialog(userId, Dialogs.DialogType.Delete, progressBarResponse, null, Dialogs.ProgressSystemMessageType.Progress, selectedDirName);
				currentState = ((errorCode >= 0) ? SaveState.ShowDeleteWaitForDialog : SaveState.ShowErrorStart);
				break;
			case SaveState.ShowDeleteWaitForDialog:
			{
				Dialogs.DialogStatus dialogStatus = Dialogs.DialogGetStatus();
				if (dialogStatus == Dialogs.DialogStatus.Running && Dialogs.DialogIsReadyToDisplay())
				{
					currentState = SaveState.ShowDelete;
				}
				break;
			}
			case SaveState.ShowDelete:
			{
				EmptyResponse deleteResponse = new EmptyResponse();
				Progress.ClearProgress();
				errorCode = DeleteSaveData(userId, deleteResponse, selectedDirName);
				if (errorCode < 0)
				{
					currentState = SaveState.ShowErrorStart;
					break;
				}
				while (deleteResponse.Locked)
				{
					float progress = Progress.GetProgress();
					Dialogs.ProgressBarSetValue((uint)(progress * 100f));
					yield return null;
				}
				Dialogs.ProgressBarSetValue(100u);
				yield return null;
				if (deleteResponse.IsErrorCode)
				{
					currentState = SaveState.ShowErrorStart;
					errorCode = deleteResponse.ReturnCodeValue;
				}
				else
				{
					ForceCloseDialog();
					currentState = SaveState.Finished;
				}
				break;
			}
			case SaveState.ConfirmDeleteStart:
				ForceCloseDialog(Dialogs.Animation.Off);
				while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
				{
					yield return null;
				}
				errorCode = ConfirmDialog(userId, Dialogs.DialogType.Delete, confirmDeleteResponse, Dialogs.SystemMessageType.Confirm, selectedDirName, Dialogs.Animation.Off, Dialogs.Animation.On, null, 0uL);
				currentState = ((errorCode >= 0) ? SaveState.ConfirmDelete : SaveState.ShowErrorStart);
				break;
			case SaveState.ConfirmDelete:
				if (!confirmDeleteResponse.Locked)
				{
					if (confirmDeleteResponse.IsErrorCode)
					{
						currentState = SaveState.ShowErrorStart;
						errorCode = confirmDeleteResponse.ReturnCodeValue;
					}
					else
					{
						Dialogs.DialogResult result = confirmDeleteResponse.Result;
						currentState = ((result.CallResult != Dialogs.DialogCallResults.OK) ? SaveState.Finished : ((result.ButtonId != Dialogs.DialogButtonIds.OK) ? SaveState.Finished : SaveState.ShowDeleteStart));
					}
				}
				break;
			case SaveState.ShowErrorStart:
				ForceCloseDialog(Dialogs.Animation.Off);
				while (Dialogs.DialogGetStatus() == Dialogs.DialogStatus.Running)
				{
					yield return null;
				}
				currentState = (ErrorDialog(userId, Dialogs.DialogType.Delete, errorResponse, errorCode) ? SaveState.ShowError : SaveState.Finished);
				break;
			case SaveState.ShowError:
				if (!errorResponse.Locked)
				{
					currentState = SaveState.Finished;
				}
				break;
			case SaveState.Finished:
				currentState = SaveState.Exit;
				break;
			}
			yield return null;
		}
	}

	internal static void ForceCloseDialog(Dialogs.Animation anim = Dialogs.Animation.On)
	{
		try
		{
			Dialogs.DialogStatus dialogStatus = Dialogs.DialogGetStatus();
			if (dialogStatus == Dialogs.DialogStatus.Running)
			{
				Dialogs.CloseParam closeParam = new Dialogs.CloseParam();
				closeParam.Anim = anim;
				Dialogs.Close(closeParam);
			}
		}
		catch
		{
		}
	}

	internal static int DeleteSaveData(int userId, EmptyResponse deleteResponse, DirName dirName)
	{
		int result = -2135425010;
		try
		{
			Deleting.DeleteRequest deleteRequest = new Deleting.DeleteRequest();
			deleteRequest.UserId = userId;
			deleteRequest.IgnoreCallback = true;
			deleteRequest.DirName = dirName;
			Deleting.Delete(deleteRequest, deleteResponse);
			result = 0;
		}
		catch
		{
			if (deleteResponse.ReturnCodeValue < 0)
			{
				result = deleteResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int MountSaveData(int userId, ulong blocks, Mounting.MountResponse mountResponse, DirName dirName, Mounting.MountModeFlags flags)
	{
		int result = -2135425010;
		try
		{
			Mounting.MountRequest mountRequest = new Mounting.MountRequest();
			mountRequest.UserId = userId;
			mountRequest.IgnoreCallback = true;
			mountRequest.DirName = dirName;
			mountRequest.MountMode = flags;
			if (blocks < 96)
			{
				blocks = 96uL;
			}
			mountRequest.Blocks = blocks;
			Mounting.Mount(mountRequest, mountResponse);
			result = 0;
		}
		catch
		{
			if (mountResponse.ReturnCodeValue < 0)
			{
				result = mountResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int UnmountSaveData(int userId, EmptyResponse unmountResponse, Mounting.MountPoint mp, bool backup)
	{
		int result = -2135425010;
		try
		{
			Mounting.UnmountRequest unmountRequest = new Mounting.UnmountRequest();
			unmountRequest.UserId = userId;
			unmountRequest.MountPointName = mp.PathName;
			unmountRequest.Backup = backup;
			unmountRequest.IgnoreCallback = true;
			Mounting.Unmount(unmountRequest, unmountResponse);
			result = 0;
		}
		catch
		{
			if (unmountResponse.ReturnCodeValue < 0)
			{
				result = unmountResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int CheckBackup(int userId, Backups.CheckBackupResponse backupResponse, DirName dirName)
	{
		int result = -2135425010;
		try
		{
			Backups.CheckBackupRequest checkBackupRequest = new Backups.CheckBackupRequest();
			checkBackupRequest.UserId = userId;
			checkBackupRequest.DirName = dirName;
			checkBackupRequest.IncludeParams = false;
			checkBackupRequest.IncludeIcon = false;
			checkBackupRequest.IgnoreCallback = true;
			Backups.CheckBackup(checkBackupRequest, backupResponse);
			result = 0;
		}
		catch
		{
			if (backupResponse.ReturnCodeValue < 0)
			{
				result = backupResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int RestoreBackup(int userId, EmptyResponse restoreResponse, DirName dirName)
	{
		int result = -2135425010;
		try
		{
			Backups.RestoreBackupRequest restoreBackupRequest = new Backups.RestoreBackupRequest();
			restoreBackupRequest.UserId = userId;
			restoreBackupRequest.DirName = dirName;
			restoreBackupRequest.IgnoreCallback = true;
			Backups.RestoreBackup(restoreBackupRequest, restoreResponse);
			result = 0;
		}
		catch
		{
			if (restoreResponse.ReturnCodeValue < 0)
			{
				result = restoreResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int WriteIcon(int userId, EmptyResponse iconResponse, Mounting.MountPoint mp, Dialogs.NewItem newItem)
	{
		int result = -2135425010;
		try
		{
			Mounting.SaveIconRequest saveIconRequest = new Mounting.SaveIconRequest();
			if (mp == null)
			{
				return result;
			}
			saveIconRequest.UserId = userId;
			saveIconRequest.MountPointName = mp.PathName;
			saveIconRequest.RawPNG = newItem.RawPNG;
			saveIconRequest.IconPath = newItem.IconPath;
			saveIconRequest.IgnoreCallback = true;
			Mounting.SaveIcon(saveIconRequest, iconResponse);
			result = 0;
		}
		catch
		{
			if (iconResponse.ReturnCodeValue < 0)
			{
				result = iconResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int WriteParams(int userId, EmptyResponse paramsResponse, Mounting.MountPoint mp, SaveDataParams saveDataParams)
	{
		int result = -2135425010;
		try
		{
			Mounting.SetMountParamsRequest setMountParamsRequest = new Mounting.SetMountParamsRequest();
			if (mp == null)
			{
				return result;
			}
			setMountParamsRequest.UserId = userId;
			setMountParamsRequest.MountPointName = mp.PathName;
			setMountParamsRequest.IgnoreCallback = true;
			setMountParamsRequest.Params = saveDataParams;
			Mounting.SetMountParams(setMountParamsRequest, paramsResponse);
			result = 0;
		}
		catch
		{
			if (paramsResponse.ReturnCodeValue < 0)
			{
				result = paramsResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static bool ErrorDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse errorResponse, int errorCode)
	{
		try
		{
			Dialogs.OpenDialogRequest openDialogRequest = new Dialogs.OpenDialogRequest();
			openDialogRequest.UserId = userId;
			openDialogRequest.Mode = Dialogs.DialogMode.ErrorCode;
			openDialogRequest.DispType = displayType;
			Dialogs.ErrorCodeParam errorCodeParam = new Dialogs.ErrorCodeParam();
			errorCodeParam.ErrorCode = errorCode;
			openDialogRequest.ErrorCode = errorCodeParam;
			openDialogRequest.Animations = new Dialogs.AnimationParam(Dialogs.Animation.On, Dialogs.Animation.On);
			openDialogRequest.IgnoreCallback = true;
			Dialogs.OpenDialog(openDialogRequest, errorResponse);
			return true;
		}
		catch
		{
		}
		return false;
	}

	internal static int ConfirmDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse sysMesgResponse, Dialogs.SystemMessageType msgType, DirName selectedDirName, Dialogs.Animation okAnim = Dialogs.Animation.Off, Dialogs.Animation cancelAnim = Dialogs.Animation.On, Dialogs.NewItem newItem = null, ulong systemMsgValue = 0uL)
	{
		int result = -2135425010;
		try
		{
			Dialogs.OpenDialogRequest openDialogRequest = new Dialogs.OpenDialogRequest();
			openDialogRequest.UserId = userId;
			openDialogRequest.Mode = Dialogs.DialogMode.SystemMsg;
			openDialogRequest.DispType = displayType;
			Dialogs.SystemMessageParam systemMessageParam = new Dialogs.SystemMessageParam();
			systemMessageParam.SysMsgType = msgType;
			systemMessageParam.Value = systemMsgValue;
			openDialogRequest.SystemMessage = systemMessageParam;
			openDialogRequest.Animations = new Dialogs.AnimationParam(okAnim, cancelAnim);
			if (!selectedDirName.IsEmpty)
			{
				DirName[] dirNames = new DirName[1] { selectedDirName };
				Dialogs.Items items = new Dialogs.Items();
				items.DirNames = dirNames;
				openDialogRequest.Items = items;
			}
			openDialogRequest.NewItem = newItem;
			openDialogRequest.IgnoreCallback = true;
			Dialogs.OpenDialog(openDialogRequest, sysMesgResponse);
			result = 0;
		}
		catch
		{
			if (sysMesgResponse.ReturnCodeValue < 0)
			{
				result = sysMesgResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int FullSearch(int userId, Searching.DirNameSearchResponse searchResponse)
	{
		int result = -2135425010;
		try
		{
			Searching.DirNameSearchRequest dirNameSearchRequest = new Searching.DirNameSearchRequest();
			dirNameSearchRequest.UserId = userId;
			dirNameSearchRequest.Key = Searching.SearchSortKey.DirName;
			dirNameSearchRequest.Order = Searching.SearchSortOrder.Ascending;
			dirNameSearchRequest.IncludeBlockInfo = true;
			dirNameSearchRequest.IncludeParams = true;
			dirNameSearchRequest.MaxDirNameCount = 1024u;
			dirNameSearchRequest.IgnoreCallback = true;
			Searching.DirNameSearch(dirNameSearchRequest, searchResponse);
			result = 0;
		}
		catch
		{
			if (searchResponse.ReturnCodeValue < 0)
			{
				result = searchResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int ListDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse openDialogResponse, Searching.DirNameSearchResponse searchResponse, Dialogs.NewItem newItem)
	{
		int result = -2135425010;
		try
		{
			DirName[] array = null;
			if (searchResponse.SaveDataItems.Length > 0)
			{
				array = new DirName[searchResponse.SaveDataItems.Length];
				for (int i = 0; i < searchResponse.SaveDataItems.Length; i++)
				{
					ref DirName reference = ref array[i];
					reference = searchResponse.SaveDataItems[i].DirName;
				}
			}
			Dialogs.OpenDialogRequest openDialogRequest = new Dialogs.OpenDialogRequest();
			openDialogRequest.UserId = userId;
			openDialogRequest.Mode = Dialogs.DialogMode.List;
			openDialogRequest.DispType = displayType;
			Dialogs.Items items = new Dialogs.Items();
			if (array != null)
			{
				items.DirNames = array;
			}
			items.FocusPos = Dialogs.FocusPos.DataLatest;
			items.ItemStyle = Dialogs.ItemStyle.SubtitleDataSize;
			openDialogRequest.Items = items;
			openDialogRequest.Animations = new Dialogs.AnimationParam(Dialogs.Animation.Off, Dialogs.Animation.On);
			openDialogRequest.NewItem = newItem;
			openDialogRequest.IgnoreCallback = true;
			Dialogs.OpenDialog(openDialogRequest, openDialogResponse);
			result = 0;
		}
		catch
		{
			if (openDialogResponse.ReturnCodeValue < 0)
			{
				result = openDialogResponse.ReturnCodeValue;
			}
		}
		return result;
	}

	internal static int ProgressBarDialog(int userId, Dialogs.DialogType displayType, Dialogs.OpenDialogResponse progressBarResponse, Dialogs.NewItem newItem, Dialogs.ProgressSystemMessageType msgType, DirName loadDirName)
	{
		int result = -2135425010;
		try
		{
			Dialogs.OpenDialogRequest openDialogRequest = new Dialogs.OpenDialogRequest();
			openDialogRequest.UserId = userId;
			openDialogRequest.Mode = Dialogs.DialogMode.ProgressBar;
			openDialogRequest.DispType = displayType;
			if (!loadDirName.IsEmpty)
			{
				Dialogs.Items items = new Dialogs.Items();
				items.DirNames = new DirName[1] { loadDirName };
				openDialogRequest.Items = items;
			}
			Dialogs.ProgressBarParam progressBarParam = new Dialogs.ProgressBarParam();
			progressBarParam.BarType = Dialogs.ProgressBarType.Percentage;
			progressBarParam.SysMsgType = msgType;
			openDialogRequest.ProgressBar = progressBarParam;
			openDialogRequest.NewItem = newItem;
			openDialogRequest.Animations = new Dialogs.AnimationParam(Dialogs.Animation.Off, Dialogs.Animation.On);
			openDialogRequest.IgnoreCallback = true;
			Dialogs.OpenDialog(openDialogRequest, progressBarResponse);
			result = 0;
		}
		catch
		{
			if (progressBarResponse.ReturnCodeValue < 0)
			{
				result = progressBarResponse.ReturnCodeValue;
			}
		}
		return result;
	}
}
