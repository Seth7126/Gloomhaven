#define ENABLE_LOGS
using System.IO;
using UnityEngine;

namespace FrameTimeLogger;

public class EditorCSVWriter : ICSVWriter
{
	private string _path;

	public async void Write(string fileName, string csvString)
	{
		_path = Application.persistentDataPath;
		Debug.Log("EditorCSVWriter path=" + _path);
		using StreamWriter streamWriter = File.CreateText(_path + "/" + fileName + ".csv");
		char[] array = csvString.ToCharArray();
		await streamWriter.WriteAsync(array, 0, array.Length);
		streamWriter.Close();
	}
}
