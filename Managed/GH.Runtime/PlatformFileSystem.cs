#define ENABLE_LOGS
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Platforms;
using Platforms.PlatformData;
using SM.Utils;
using UnityEngine;
using Utilities;

public class PlatformFileSystem : MonoBehaviour, IFileService
{
	private const float _FAKE_DATA_RESTORATION_DELAY = 10f;

	private bool _isFileSystemBusy;

	private bool _startFakeDataRestoration;

	private float _fakeDataRestorationTimer;

	private string _persistentDataPath;

	public bool FileSystemCorrupt { get; set; }

	public void Initialize(IPlatform platform)
	{
		_persistentDataPath = Application.persistentDataPath;
	}

	private void FixedUpdate()
	{
		if (_startFakeDataRestoration)
		{
			_startFakeDataRestoration = false;
			_fakeDataRestorationTimer = 10f;
			Debug.LogError("Fake data restoration start!");
		}
		if (_fakeDataRestorationTimer > 0f)
		{
			_fakeDataRestorationTimer -= Time.fixedDeltaTime;
			if (_fakeDataRestorationTimer.Equals(0f) || _fakeDataRestorationTimer < 0f)
			{
				_fakeDataRestorationTimer = -1f;
				FileSystemCorrupt = false;
				OnRestorationComplete(withSuccess: true, string.Empty);
				Debug.LogError("Fake data restoration finished!");
			}
		}
	}

	private void CheckFileSystemAvailability()
	{
		if (_isFileSystemBusy)
		{
			LogUtils.LogError($"Error: File system access while executing file writing {SaveData.Instance.IsSavingThreadActive} {SaveData.Instance.SaveQueue.IsAnyOperationExecuting}");
		}
	}

	public virtual void CreateDirectory(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		Directory.CreateDirectory(path);
	}

	public void MoveDirectory(string sourceDirectoryName, string destDirectoryName)
	{
		if (sourceDirectoryName.StartsWith(_persistentDataPath, StringComparison.Ordinal) && FileSystemCorrupt)
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		Directory.Move(sourceDirectoryName, destDirectoryName);
	}

	public virtual string[] GetFiles(string path, string pattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return null;
		}
		CheckFileSystemAvailability();
		return Directory.GetFiles(path, pattern, searchOption);
	}

	public virtual string[] GetDirectories(string path, string pattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return null;
		}
		CheckFileSystemAvailability();
		return Directory.GetDirectories(path, pattern, searchOption);
	}

	public virtual bool ExistsDirectory(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return false;
		}
		CheckFileSystemAvailability();
		return Directory.Exists(path);
	}

	public virtual void RemoveDirectory(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		Directory.Delete(path);
	}

	public virtual void RemoveDirectory(string path, bool recursive)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		Directory.Delete(path, recursive);
	}

	public virtual bool ExistsFile(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return false;
		}
		CheckFileSystemAvailability();
		return File.Exists(path);
	}

	public virtual bool ExistsFile(string path, out OperationResult operationResult, out string message)
	{
		operationResult = OperationResult.Success;
		message = string.Empty;
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return false;
		}
		CheckFileSystemAvailability();
		return File.Exists(path);
	}

	public virtual void WriteFile(byte[] data, string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		File.WriteAllBytes(path, data);
	}

	public async void WriteFileAsync(byte[] data, string path, Action<OperationResult, string> resultCallback)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		_isFileSystemBusy = true;
		await Task.Run(delegate
		{
			File.WriteAllBytes(path, data);
		});
		_isFileSystemBusy = false;
		resultCallback(OperationResult.Success, string.Empty);
	}

	public virtual void FileWriteAllText(string path, string content)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		File.WriteAllText(path, content);
	}

	public virtual void CopyFile(string sourceFileName, string destFileName, bool overwrite)
	{
		if (sourceFileName.StartsWith(_persistentDataPath, StringComparison.Ordinal) && FileSystemCorrupt)
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		File.Copy(sourceFileName, destFileName, overwrite);
	}

	public void MoveFile(string sourceFileName, string destFileName)
	{
		if (sourceFileName.StartsWith(_persistentDataPath, StringComparison.Ordinal) && FileSystemCorrupt)
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		File.Move(sourceFileName, destFileName);
	}

	public virtual string FileReadAllText(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return null;
		}
		CheckFileSystemAvailability();
		return File.ReadAllText(path);
	}

	public virtual byte[] ReadFile(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return null;
		}
		CheckFileSystemAvailability();
		return File.ReadAllBytes(path);
	}

	public virtual void RemoveFile(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		File.Delete(path);
	}

	public void CreateFile(string path)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		File.Create(path);
	}

	public void FileAppendAllText(string path, string content)
	{
		if (FileSystemCorrupt && path.StartsWith(_persistentDataPath, StringComparison.Ordinal))
		{
			HandleDataCorruption();
			return;
		}
		CheckFileSystemAvailability();
		File.AppendAllText(path, content);
	}

	public bool CheckFileForCorrupt(string path)
	{
		return File.ReadAllBytes(path).All((byte x) => x == 0);
	}

	private void HandleDataCorruption()
	{
		if (SceneController.Instance.DataRestoring)
		{
			LogUtils.LogWarning("[PlatformFileSystem] Data already restoring => abort");
			return;
		}
		_startFakeDataRestoration = true;
		SceneController.Instance.DataRestoring = true;
		SceneController.Instance.WaitForDataRestorationAndReturnToMainMenu();
	}

	private void OnRestorationComplete(bool withSuccess, string detailedMessage)
	{
		LogUtils.Log("[PlatformFileSystem] Data restored!");
		SceneController.Instance.DataRestoring = false;
	}
}
