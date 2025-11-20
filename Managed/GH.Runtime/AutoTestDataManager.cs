#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

[Serializable]
public class AutoTestDataManager
{
	private string currentAutotestPath;

	[NonSerialized]
	public List<AutoTestData> LoadedAutoTests = new List<AutoTestData>();

	public List<FileInfo> AutoTestFiles { get; private set; }

	public string CurrentlyRunningAutotestPath { get; set; }

	public FileInfo CurrentlyRunningAutotestFile { get; set; }

	public string CurrentAutotestPath
	{
		get
		{
			if (currentAutotestPath == null)
			{
				currentAutotestPath = RootSaveData.AutoTestPath;
			}
			return currentAutotestPath;
		}
		set
		{
			SaveData.Instance.Global.LastAutotestFolder = value;
			currentAutotestPath = value;
		}
	}

	public string AutoTestSaveFolder(string fileName)
	{
		return Path.Combine(CurrentAutotestPath, fileName + ".testdat");
	}

	public AutoTestDataManager()
	{
		AutoTestFiles = new List<FileInfo>();
	}

	public void DetermineAvailableFilesFromLoadFolder()
	{
		string path = CurrentAutotestPath;
		if (Directory.Exists(path))
		{
			AutoTestFiles.Clear();
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			AutoTestFiles = (from o in directoryInfo.GetFiles("*.testdat", SearchOption.AllDirectories)
				orderby Path.GetFileName(o.Name)
				select o).ToList();
		}
	}

	public void LoadAllAutoTestDataFromFile()
	{
		try
		{
			LoadedAutoTests.Clear();
			AutoTestFiles.Clear();
			string autoTestFolder = SaveData.Instance.AutoTestFolder;
			if (string.IsNullOrEmpty(autoTestFolder))
			{
				autoTestFolder = CurrentAutotestPath;
			}
			string[] files = Directory.GetFiles(autoTestFolder, "*.testdat", SearchOption.AllDirectories);
			Array.Sort(files, new AlphanumComparatorFast());
			string[] array = files;
			foreach (string text in array)
			{
				using (FileStream fileStream = File.Open(text, FileMode.Open, FileAccess.Read))
				{
					SaveData.Instance.CurrentFileBeingDeserialized = text;
					AutoTestData item = new BinaryFormatter
					{
						Binder = new SerializationBinding()
					}.Deserialize(fileStream) as AutoTestData;
					LoadedAutoTests.Add(item);
					fileStream.Close();
				}
				AutoTestFiles.Add(new FileInfo(text));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load autotest file data.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public AutoTestData GetAutotestDataFromFile(FileInfo file)
	{
		try
		{
			using FileStream fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read);
			SaveData.Instance.CurrentFileBeingDeserialized = file.FullName;
			AutoTestData result = new BinaryFormatter
			{
				Binder = new SerializationBinding()
			}.Deserialize(fileStream) as AutoTestData;
			fileStream.Close();
			return result;
		}
		catch (Exception ex)
		{
			Debug.LogError("Unable to load autotest file data.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return null;
	}

	public bool SaveAutoTestData(AutoTestData autoTestData, FileInfo file, bool saveReadableJson = false)
	{
		try
		{
			string fullName = file.FullName;
			string directoryName = Path.GetDirectoryName(fullName);
			if (!Directory.Exists(directoryName))
			{
				Debug.Log("Creating auto test data directory: " + fullName);
				Directory.CreateDirectory(directoryName);
			}
			using (FileStream fileStream = File.Open(fullName, FileMode.Create, FileAccess.Write))
			{
				new BinaryFormatter().Serialize(fileStream, autoTestData);
				fileStream.Close();
			}
			if (saveReadableJson)
			{
				using FileStream serializationStream = File.Open(AutoTestSaveFolder("JSON_" + Path.GetFileNameWithoutExtension(file.FullName) + ".json"), FileMode.Create, FileAccess.Write);
				string graph = JsonConvert.SerializeObject(autoTestData, Formatting.Indented);
				new BinaryFormatter().Serialize(serializationStream, graph);
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to save Autotest data.\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}
}
