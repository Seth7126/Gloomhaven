#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using FFSThreads;
using ScenarioRuleLibrary;
using SharedLibrary;
using UnityEngine;
using UnityEngine.Events;

public static class GloomUtility
{
	public class ValueChangeTracker
	{
		[NonSerialized]
		private Dictionary<string, int> editValueTracking;

		public bool CheckValue(string name, int value)
		{
			if (editValueTracking != null)
			{
				int value2 = 0;
				if (editValueTracking.TryGetValue(name, out value2))
				{
					if (value == value2)
					{
						return false;
					}
					editValueTracking[name] = value;
					return true;
				}
				editValueTracking[name] = value;
				return false;
			}
			editValueTracking = new Dictionary<string, int>();
			editValueTracking[name] = value;
			return false;
		}

		public bool CheckValue(string name, bool value)
		{
			return CheckValue(name, value ? 1 : 0);
		}
	}

	private static Dictionary<string, HashSet<int>> s_ExlusiveIndexSets = new Dictionary<string, HashSet<int>>();

	private static bool isGUIVisible = true;

	public static void ClearExclusiveIndex(string indexName)
	{
		if (s_ExlusiveIndexSets.ContainsKey(indexName))
		{
			s_ExlusiveIndexSets.Remove(indexName);
		}
	}

	public static byte[] ObjectToByteArray(object obj)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	public static T ByteArrayToObject<T>(byte[] arrBytes)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		memoryStream.Write(arrBytes, 0, arrBytes.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		if (binaryFormatter.Deserialize(memoryStream) is T result)
		{
			return result;
		}
		Debug.LogError("Unable to deserialize object of type " + typeof(T).ToString());
		return default(T);
	}

	public static int GetExclusiveIndex(SharedLibrary.Random rng, string indexName, int Count)
	{
		if (Count <= 1)
		{
			return 0;
		}
		int num = 0;
		if (s_ExlusiveIndexSets.TryGetValue(indexName, out var value))
		{
			if (value.Count == Count)
			{
				value.Clear();
			}
			num = ExclusiveRandomNumber(rng, value, Count - 1);
			value.Add(num);
		}
		else
		{
			num = rng.Next(Count);
			s_ExlusiveIndexSets.Add(indexName, new HashSet<int>());
			s_ExlusiveIndexSets[indexName].Add(num);
		}
		return num;
	}

	public static GameObject FindInChildren(this GameObject go, string name, bool includeInactive = false)
	{
		return (from x in go.GetComponentsInChildren<Transform>(includeInactive)
			where x.gameObject.name == name
			select x.gameObject).FirstOrDefault();
	}

	public static GameObject[] FindAllInChildren(this GameObject go, string name, bool includeInactive = false)
	{
		return (from x in go.GetComponentsInChildren<Transform>(includeInactive)
			where x.gameObject.name == name
			select x.gameObject).ToArray();
	}

	public static T DeepCopySerializableObject<T>(this object objSource)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Binder = new SerializationBinding();
		binaryFormatter.Serialize(memoryStream, objSource);
		memoryStream.Position = 0L;
		return (T)binaryFormatter.Deserialize(memoryStream);
	}

	public static object CloneObject(this object objSource)
	{
		Type type = objSource.GetType();
		object obj = Activator.CreateInstance(type);
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!propertyInfo.CanWrite)
			{
				continue;
			}
			if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType.IsEnum || propertyInfo.PropertyType.Equals(typeof(string)))
			{
				propertyInfo.SetValue(obj, propertyInfo.GetValue(objSource, null), null);
				continue;
			}
			object value = propertyInfo.GetValue(objSource, null);
			if (value == null)
			{
				propertyInfo.SetValue(obj, null, null);
			}
			else
			{
				propertyInfo.SetValue(obj, value.CloneObject(), null);
			}
		}
		return obj;
	}

	public static GameObject FindInImmediateChildren(this GameObject go, string name, bool includeInactive = false)
	{
		for (int i = 0; i < go.transform.childCount; i++)
		{
			if (go.transform.GetChild(i).name == name)
			{
				return go.transform.GetChild(i).gameObject;
			}
		}
		return null;
	}

	public static bool MoveFolder(string sourcePath, string targetPath, ThreadMessageSender sender = null)
	{
		try
		{
			string[] directories = PlatformLayer.FileSystem.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
			for (int i = 0; i < directories.Length; i++)
			{
				string text = directories[i].Replace(sourcePath, targetPath);
				if (!PlatformLayer.FileSystem.ExistsDirectory(text))
				{
					Debug.Log("Moving folder: " + text);
					PlatformLayer.FileSystem.CreateDirectory(text);
				}
			}
			string[] files = PlatformLayer.FileSystem.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
			float incrementAmount = 100f / (float)files.Length;
			directories = files;
			foreach (string text2 in directories)
			{
				PlatformLayer.FileSystem.CopyFile(text2, text2.Replace(sourcePath, targetPath), overwrite: true);
				sender?.SendMessage(new ThreadMessage_IncrementProgressBar(incrementAmount));
			}
			DeleteFolder(sourcePath);
			sender?.SendMessage(new ThreadMessage_UpdateProgressBar(0f));
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to move folder " + sourcePath + " to " + targetPath + "\n" + ex.Message + "\n" + ex.StackTrace);
			try
			{
				PlatformLayer.FileSystem.RemoveDirectory(targetPath, recursive: true);
			}
			catch
			{
			}
			return false;
		}
	}

	public static void DeleteFolder(string path, bool topFolder = false)
	{
		string[] files = PlatformLayer.FileSystem.GetFiles(path, "*.*", SearchOption.AllDirectories);
		foreach (string path2 in files)
		{
			try
			{
				PlatformLayer.FileSystem.RemoveFile(path2);
			}
			catch
			{
			}
		}
		files = PlatformLayer.FileSystem.GetDirectories(path);
		foreach (string path3 in files)
		{
			try
			{
				PlatformLayer.FileSystem.RemoveDirectory(path3, recursive: true);
			}
			catch
			{
			}
		}
		if (topFolder)
		{
			try
			{
				PlatformLayer.FileSystem.RemoveDirectory(path);
			}
			catch
			{
			}
		}
	}

	public static IEnumerable<T> FindInImmediateChildren<T>(this GameObject go, bool includeInactive = false)
	{
		for (int i = 0; i < go.transform.childCount; i++)
		{
			T component = go.transform.GetChild(i).GetComponent<T>();
			if (component != null)
			{
				yield return component;
			}
		}
	}

	public static int ExclusiveRandomNumber(SharedLibrary.Random rng, HashSet<int> exclude, int maxValue)
	{
		IEnumerable<int> source = from i in Enumerable.Range(0, maxValue + 1)
			where !exclude.Contains(i)
			select i;
		int num = maxValue - exclude.Count;
		if (num < 0)
		{
			Debug.LogWarning("[RNG] Unable to find exclusive SharedLibrary.Random number - chosen exclusions outnumber total available");
			return rng.Next(0, maxValue);
		}
		int index = rng.Next(0, num);
		return source.ElementAt(index);
	}

	public static IEnumerable<Transform> GetAllChildren(Transform parent)
	{
		if (parent.childCount <= 0)
		{
			yield break;
		}
		foreach (Transform t in parent)
		{
			yield return t;
			IEnumerable<Transform> allChildren = GetAllChildren(t);
			foreach (Transform item in allChildren)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> FindAllDescendentComponents<T>(Transform t, bool inside_matching = true) where T : UnityEngine.Component
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform ch = t.GetChild(i);
			T component = ch.GetComponent<T>();
			if (component != null)
			{
				yield return component;
				if (!inside_matching)
				{
					continue;
				}
			}
			IEnumerable<T> enumerable = FindAllDescendentComponents<T>(ch, inside_matching);
			foreach (T item in enumerable)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> FindAllDescendentComponents<T>(GameObject o, bool inside_matching = true) where T : UnityEngine.Component
	{
		return FindAllDescendentComponents<T>(o.transform, inside_matching);
	}

	public static IEnumerable<T> FindAllDescendentComponents<T>(MonoBehaviour b, bool inside_matching = true) where T : UnityEngine.Component
	{
		return FindAllDescendentComponents<T>(b.transform, inside_matching);
	}

	public static T FindAncestorComponent<T>(Transform t, bool include_start = false)
	{
		if (t != null)
		{
			if (include_start)
			{
				T component = t.GetComponent<T>();
				if (component != null)
				{
					return component;
				}
			}
			return FindAncestorComponent<T>(t.parent, include_start: true);
		}
		return default(T);
	}

	public static T FindAncestorComponent<T>(GameObject o, bool include_start = false)
	{
		return FindAncestorComponent<T>(o.transform, include_start);
	}

	public static T FindAncestorComponent<T>(MonoBehaviour b, bool include_start = false)
	{
		return FindAncestorComponent<T>(b.transform, include_start);
	}

	public static T EnsureComponent<T>(GameObject o) where T : UnityEngine.Component
	{
		T component = o.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		return null;
	}

	public static string GetMapStateHash(string mapStatePath = null)
	{
		return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(PartyAdventureData.ConvertMapStateToJSON(mapStatePath)))).Replace("-", "");
	}

	public static string GetScenarioStateHash(ScenarioState state)
	{
		return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(PartyAdventureData.ConvertScenarioStateToJSON(state)))).Replace("-", "");
	}

	public static bool ByteArrayToFile(string fileName, byte[] byteArray, FileMode fileMode = FileMode.Create)
	{
		try
		{
			using FileStream fileStream = new FileStream(fileName, fileMode, FileAccess.Write);
			fileStream.Write(byteArray, 0, byteArray.Length);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred while writing a byte array to file.\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite = false)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		if (!PlatformLayer.FileSystem.ExistsDirectory(destDirName))
		{
			Debug.Log("Creating directory copy: " + destDirName);
			PlatformLayer.FileSystem.CreateDirectory(destDirName);
		}
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			string destFileName = Path.Combine(destDirName, fileInfo.Name);
			fileInfo.CopyTo(destFileName, overwrite);
		}
		if (copySubDirs)
		{
			DirectoryInfo[] array = directories;
			foreach (DirectoryInfo directoryInfo2 in array)
			{
				string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
				DirectoryCopy(directoryInfo2.FullName, destDirName2, copySubDirs, overwrite);
			}
		}
	}

	public static void DirectoryFilesCopy(string srcDir, string destDir, List<string> files)
	{
		if (!PlatformLayer.FileSystem.ExistsDirectory(srcDir))
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + srcDir);
		}
		if (!PlatformLayer.FileSystem.ExistsDirectory(destDir))
		{
			throw new DirectoryNotFoundException("Destination directory does not exist or could not be found: " + destDir);
		}
		foreach (string file in files)
		{
			string text = srcDir + file;
			string text2 = destDir + file;
			if (PlatformLayer.FileSystem.ExistsFile(text2))
			{
				PlatformLayer.FileSystem.RemoveFile(text2);
			}
			if (!PlatformLayer.FileSystem.ExistsFile(text))
			{
				Debug.Log("File '" + text + "' not available");
			}
			else
			{
				PlatformLayer.FileSystem.CopyFile(text, text2, overwrite: false);
			}
		}
	}

	public static List<string> GetAllFiles(string dir, bool includeSubDirs)
	{
		List<string> list = new List<string>();
		string[] files = PlatformLayer.FileSystem.GetFiles(dir);
		foreach (string item in files)
		{
			list.Add(item);
		}
		files = PlatformLayer.FileSystem.GetDirectories(dir);
		foreach (string text in files)
		{
			string[] files2 = PlatformLayer.FileSystem.GetFiles(text);
			foreach (string item2 in files2)
			{
				list.Add(item2);
			}
			list.AddRange(GetAllFiles(text, includeSubDirs));
		}
		return list;
	}

	public static bool FileCopy(string sourceFilePath, string destinationPath, bool overwrite)
	{
		FileInfo fileInfo = new FileInfo(sourceFilePath);
		if (!fileInfo.Exists)
		{
			throw new FileNotFoundException("Source file does not exist or could not be found: " + sourceFilePath);
		}
		string text = (PlatformLayer.FileSystem.ExistsDirectory(destinationPath) ? Path.Combine(destinationPath, fileInfo.Name) : destinationPath);
		bool result = PlatformLayer.FileSystem.ExistsFile(text);
		fileInfo.CopyTo(text, overwrite);
		return result;
	}

	public static void FileDelete(string fileToDeletePath)
	{
		if (!new FileInfo(fileToDeletePath).Exists)
		{
			throw new FileNotFoundException("File to delete does not exist or could not be found: " + fileToDeletePath);
		}
		PlatformLayer.FileSystem.RemoveFile(fileToDeletePath);
	}

	public static bool HasExecutable(string folder)
	{
		return new string[3] { "*.exe", "*.bat", "*.msi" }.Sum((string pattern) => PlatformLayer.FileSystem.GetFiles(folder, pattern, SearchOption.AllDirectories).Length) > 0;
	}

	public static void CheckIsEnum<T>(bool withFlags)
	{
	}

	public static long GetWithInvertedFlag<T>(this T value, T flag) where T : struct
	{
		CheckIsEnum<T>(withFlags: true);
		long num = Convert.ToInt64(value);
		long num2 = Convert.ToInt64(flag);
		return (num & ~num2) | (~num & num2);
	}

	public static Vector3 CVToV(CVector3 cvector3)
	{
		return new Vector3(cvector3.X, cvector3.Y, cvector3.Z);
	}

	public static CVector3 VToCV(Vector3 vector3)
	{
		return new CVector3(vector3.x, vector3.y, vector3.z);
	}

	public static Vector3Int CVIToVI(CVectorInt3 cvector3)
	{
		return new Vector3Int(cvector3.X, cvector3.Y, cvector3.Z);
	}

	public static CVectorInt3 VIToCVI(Vector3Int vector3)
	{
		return new CVectorInt3(vector3.x, vector3.y, vector3.z);
	}

	public static IEnumerator FadeCanvasGroup(CanvasGroup groupToFade, float delay, float duration, float targetAlpha, AnimationCurve curve, UnityAction<float> onUpdate = null, UnityAction onComplete = null, bool respectPause = false)
	{
		if (respectPause)
		{
			yield return new WaitForSecondsFrozen(delay);
		}
		else
		{
			yield return new WaitForSecondsRealtime(delay);
		}
		float startingAlpha = groupToFade.alpha;
		float timeAtStart = Time.realtimeSinceStartup;
		float timeElapsed = Time.realtimeSinceStartup - timeAtStart;
		while (timeElapsed < duration)
		{
			groupToFade.alpha = curve.Evaluate(Mathf.Lerp(startingAlpha, targetAlpha, curve.Evaluate(timeElapsed / duration)));
			onUpdate?.Invoke(groupToFade.alpha);
			timeElapsed = Time.realtimeSinceStartup - timeAtStart;
			yield return null;
		}
		groupToFade.alpha = targetAlpha;
		onComplete?.Invoke();
	}

	public static void ToggleGUI()
	{
		if (isGUIVisible)
		{
			HideGUI();
		}
		else
		{
			ShowGUI();
		}
	}

	private static void HideGUI()
	{
		Choreographer.s_Choreographer.m_WorldSpaceCanvas.GetComponent<CanvasGroup>().alpha = 0f;
		Choreographer.s_Choreographer.m_OverlayCanvas.GetComponent<CanvasGroup>().alpha = 0f;
		isGUIVisible = false;
		WorldspaceStarHexDisplay.Instance.ToggleHexDisplay(enabled: false);
	}

	private static void ShowGUI()
	{
		Choreographer.s_Choreographer.m_WorldSpaceCanvas.GetComponent<CanvasGroup>().alpha = 1f;
		Choreographer.s_Choreographer.m_OverlayCanvas.GetComponent<CanvasGroup>().alpha = 1f;
		isGUIVisible = true;
		WorldspaceStarHexDisplay.Instance.ToggleHexDisplay(enabled: true);
	}

	public static bool IsGUIVisible()
	{
		return isGUIVisible;
	}

	public static bool IsInPrefabStage()
	{
		return false;
	}

	public static string GetEnumDescription(Enum value)
	{
		DescriptionAttribute[] array = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
		if (array != null && array.Length != 0)
		{
			return array[0].Description;
		}
		return value.ToString();
	}

	public static string GetEnumCategory(Enum value)
	{
		CategoryAttribute[] array = (CategoryAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(CategoryAttribute), inherit: false);
		if (array != null && array.Length != 0)
		{
			return array[0].Category;
		}
		return value.ToString();
	}

	public static void CopyAll(string sourcePath, string targetPath)
	{
		DirectoryInfo source = new DirectoryInfo(sourcePath);
		DirectoryInfo target = new DirectoryInfo(targetPath);
		CopyAll(source, target);
	}

	public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
	{
		if (!(source.FullName.ToLower() == target.FullName.ToLower()))
		{
			if (!PlatformLayer.FileSystem.ExistsDirectory(target.FullName))
			{
				Debug.Log("Creating directory: " + target.FullName);
				PlatformLayer.FileSystem.CreateDirectory(target.FullName);
			}
			FileInfo[] files = source.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				Console.WriteLine("Copying {0}\\{1}", target.FullName, fileInfo.Name);
				fileInfo.CopyTo(Path.Combine(target.ToString(), fileInfo.Name), overwrite: true);
			}
			DirectoryInfo[] directories = source.GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories)
			{
				DirectoryInfo target2 = target.CreateSubdirectory(directoryInfo.Name);
				CopyAll(directoryInfo, target2);
			}
		}
	}

	public static IEnumerator TakeErrorScreenshot(GameObject errorMessageGO)
	{
		if (errorMessageGO != null && !PlatformLayer.Instance.IsConsole)
		{
			errorMessageGO.SetActive(value: false);
			yield return new WaitForEndOfFrame();
			ScreenCapture.CaptureScreenshot(RootSaveData.ScreenCaptureImagePath);
			errorMessageGO.SetActive(value: true);
		}
	}

	public static Texture2D LoadTGA(string fileName)
	{
		using MemoryStream tGAStream = new MemoryStream(PlatformLayer.FileSystem.ReadFile(fileName));
		return LoadTGA(tGAStream);
	}

	public static Texture2D LoadTGA(Stream TGAStream)
	{
		using BinaryReader binaryReader = new BinaryReader(TGAStream);
		binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
		short num = binaryReader.ReadInt16();
		short num2 = binaryReader.ReadInt16();
		int num3 = binaryReader.ReadByte();
		binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
		Texture2D texture2D = new Texture2D(num, num2);
		Color32[] array = new Color32[num * num2];
		switch (num3)
		{
		case 32:
		{
			for (int j = 0; j < num * num2; j++)
			{
				byte b2 = binaryReader.ReadByte();
				byte g2 = binaryReader.ReadByte();
				byte r2 = binaryReader.ReadByte();
				byte a = binaryReader.ReadByte();
				array[j] = new Color32(r2, g2, b2, a);
			}
			break;
		}
		case 24:
		{
			for (int i = 0; i < num * num2; i++)
			{
				byte b = binaryReader.ReadByte();
				byte g = binaryReader.ReadByte();
				byte r = binaryReader.ReadByte();
				array[i] = new Color32(r, g, b, 1);
			}
			break;
		}
		default:
			throw new Exception("TGA texture had non 32/24 bit depth.");
		}
		texture2D.SetPixels32(array);
		texture2D.Apply();
		return texture2D;
	}
}
