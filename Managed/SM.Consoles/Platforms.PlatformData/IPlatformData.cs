using System;
using System.Collections.Generic;

namespace Platforms.PlatformData;

public interface IPlatformData
{
	event Action<bool, string> DataRestorationComplete;

	string GetDataPath();

	string GetPersistantDataPath();

	void WriteDataAsync(byte[] data, string path, Action<OperationResult, string> resultCallback, string[] tags = null);

	void ReadDataAsync(string path, Action<OperationResult, string, byte[]> resultCallback, string[] tags = null);

	void WriteData(byte[] data, string path, out OperationResult operationResult, out string detailedMessage, string[] tags = null);

	byte[] ReadData(string path, out OperationResult operationResult, out string detailedMessage, string[] tags = null);

	List<RecordInfo> GetRecords();

	string[] GetFiles(string path, out OperationResult operationResult, out string detailedMessage);

	bool ExistsFile(string path, out OperationResult operationResult, out string detailedMessage);

	void CreateFile(string path, out OperationResult operationResult, out string detailedMessage);

	void CopyFile(string sourceFileName, string destFileName, bool overwrite, out OperationResult operationResult, out string detailedMessage);

	void MoveFile(string sourceFileName, string destFileName, out OperationResult operationResult, out string detailedMessage);

	void DeleteFile(string path, out OperationResult operationResult, out string detailedMessage);

	void CreateDirectory(string path, out OperationResult operationResult, out string detailedMessage);

	void MoveDirectory(string sourceDirectoryName, string destDirectoryName, out OperationResult operationResult, out string detailedMessage);

	string[] GetDirectories(string path, out OperationResult operationResult, out string detailedMessage);

	bool ExistsDirectory(string path, out OperationResult operationResult, out string detailedMessage);

	void DeleteDirectory(string path, bool recursive, out OperationResult operationResult, out string detailedMessage);
}
