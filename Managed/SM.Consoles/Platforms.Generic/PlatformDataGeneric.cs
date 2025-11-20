using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Platforms.PlatformData;
using UnityEngine;

namespace Platforms.Generic;

public class PlatformDataGeneric : IPlatformData
{
	public event Action<bool, string> DataRestorationComplete;

	public void FileExistsAsync(string path, Action<OperationResult, string, bool> resultCallback, string[] tags = null)
	{
		resultCallback(OperationResult.Success, string.Empty, PlayerPrefs.HasKey(path));
	}

	public string GetDataPath()
	{
		return Application.dataPath;
	}

	public string GetPersistantDataPath()
	{
		return Application.persistentDataPath;
	}

	public void WriteDataAsync(byte[] data, string path, Action<OperationResult, string> resultCallback, string[] tags = null)
	{
		string value = Encoding.UTF8.GetString(data);
		PlayerPrefs.SetString(path, value);
		resultCallback(OperationResult.Success, string.Empty);
	}

	public void ReadDataAsync(string path, Action<OperationResult, string, byte[]> resultCallback, string[] tags = null)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(PlayerPrefs.GetString(path));
		resultCallback(OperationResult.Success, string.Empty, bytes);
	}

	public void WriteData(byte[] data, string path, out OperationResult operationResult, out string detailedMessage, string[] tags = null)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		File.WriteAllBytes(path, data);
	}

	public byte[] ReadData(string path, out OperationResult operationResult, out string detailedMessage, string[] tags = null)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		return File.ReadAllBytes(path);
	}

	public List<RecordInfo> GetRecords()
	{
		return null;
	}

	public string[] GetFiles(string path, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		return Directory.GetFiles(path);
	}

	public bool ExistsFile(string path, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		return File.Exists(path);
	}

	public void CreateFile(string path, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		File.Create(path).Close();
	}

	public void CopyFile(string sourceFileName, string destFileName, bool overwrite, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		File.Copy(sourceFileName, destFileName, overwrite);
	}

	public void MoveFile(string sourceFileName, string destFileName, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		File.Move(sourceFileName, destFileName);
	}

	public void DeleteFile(string path, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		File.Delete(path);
	}

	public void CreateDirectory(string path, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		Directory.CreateDirectory(path);
	}

	public void MoveDirectory(string sourceDirectoryName, string destDirectoryName, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		Directory.Move(sourceDirectoryName, destDirectoryName);
	}

	public string[] GetDirectories(string path, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		return Directory.GetDirectories(path);
	}

	public bool ExistsDirectory(string path, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		return Directory.Exists(path);
	}

	public void DeleteDirectory(string path, bool recursive, out OperationResult operationResult, out string detailedMessage)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		Directory.Delete(path, recursive);
	}

	public void DeleteFileAsync(string path, Action<OperationResult, string> resultCallback, out OperationResult operationResult, out string detailedMessage, string[] tags = null)
	{
		operationResult = OperationResult.Success;
		detailedMessage = string.Empty;
		PlayerPrefs.DeleteKey(path);
		resultCallback(OperationResult.Success, string.Empty);
	}

	public void Dispose()
	{
	}
}
