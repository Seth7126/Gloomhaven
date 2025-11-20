using System;
using System.IO;
using Platforms;
using Platforms.PlatformData;

namespace Utilities;

public interface IFileService
{
	void Initialize(IPlatform platform);

	void CreateDirectory(string path);

	void MoveDirectory(string sourceDirectoryName, string destDirectoryName);

	string[] GetFiles(string path, string pattern = "", SearchOption searchOption = SearchOption.TopDirectoryOnly);

	string[] GetDirectories(string path, string pattern = "", SearchOption searchOption = SearchOption.TopDirectoryOnly);

	bool ExistsDirectory(string path);

	void RemoveDirectory(string path);

	void RemoveDirectory(string path, bool recursive);

	bool ExistsFile(string path);

	bool ExistsFile(string path, out OperationResult operationResult, out string message);

	void CreateFile(string path);

	void WriteFile(byte[] data, string path);

	void WriteFileAsync(byte[] data, string path, Action<OperationResult, string> resultCallback);

	void CopyFile(string sourceFileName, string destFileName, bool overwrite);

	void MoveFile(string sourceFileName, string destFileName);

	void FileAppendAllText(string path, string content);

	void FileWriteAllText(string path, string content);

	string FileReadAllText(string path);

	byte[] ReadFile(string path);

	void RemoveFile(string path);
}
