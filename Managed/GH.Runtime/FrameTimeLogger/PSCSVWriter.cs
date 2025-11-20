#define ENABLE_LOGS
using System;
using System.IO;
using System.Text;
using Platforms.PlatformData;

namespace FrameTimeLogger;

public class PSCSVWriter : ICSVWriter
{
	private readonly string _fileDirectory = "FPS_Captures";

	public void Write(string fileName, string csvString)
	{
		Debug.Log("PSCSVWriter starting");
		SaveData.Instance.PerformCustomFileOperation(SaveFPSCapture);
		void SaveFPSCapture(Action onCompleted)
		{
			string path = Path.Combine(PlatformLayer.Platform.PlatformData.GetPersistantDataPath(), _fileDirectory);
			Debug.Log("PSCSVWriter path=" + path);
			if (!PlatformLayer.Platform.PlatformData.ExistsDirectory(path, out var operationResult, out var detailedMessage))
			{
				PlatformLayer.Platform.PlatformData.CreateDirectory(path, out operationResult, out detailedMessage);
			}
			Debug.Log("PSCSVWriter CreateFile path=" + path);
			PlatformLayer.Platform.PlatformData.CreateFile(path + "/" + fileName + ".csv", out operationResult, out detailedMessage);
			byte[] bytes = Encoding.UTF8.GetBytes(csvString);
			Debug.Log("PSCSVWriter WriteDataAsync path=" + path);
			PlatformLayer.Platform.PlatformData.WriteDataAsync(bytes, path + "/" + fileName + ".csv", delegate(OperationResult result, string s)
			{
				Debug.Log($"[PSCVWriter] Write Async path={path} operation result:{result}, string:{s}");
				onCompleted();
			});
		}
	}
}
