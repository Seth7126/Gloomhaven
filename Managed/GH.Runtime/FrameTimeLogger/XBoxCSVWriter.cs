#define ENABLE_LOGS
using System.IO;

namespace FrameTimeLogger;

public class XBoxCSVWriter : ICSVWriter
{
	private readonly string _path = "D:\\FPS_Captures";

	public async void Write(string fileName, string csvString)
	{
		Debug.Log("XBoxCSVWriter path=" + _path);
		if (!Directory.Exists(_path))
		{
			Directory.CreateDirectory(_path);
		}
		using StreamWriter streamWriter = File.CreateText(_path + "/" + fileName + ".csv");
		char[] array = csvString.ToCharArray();
		await streamWriter.WriteAsync(array, 0, array.Length);
		streamWriter.Close();
	}
}
