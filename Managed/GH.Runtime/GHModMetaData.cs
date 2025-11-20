using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using UnityEngine;

[Serializable]
public class GHModMetaData : ISerializable
{
	public enum EModType
	{
		None,
		Guildmaster,
		Campaign,
		Global,
		CustomLevels,
		Language
	}

	public static EModType[] ModTypes = (EModType[])Enum.GetValues(typeof(EModType));

	public string Name { get; private set; }

	public string Description { get; private set; }

	public EModType ModType { get; private set; }

	public Texture2D Thumbnail { get; private set; }

	public int ModVersion { get; set; }

	public string BuildVersion { get; set; }

	public ulong PublishedFileId { get; set; }

	public List<string> AppliedFiles { get; private set; }

	public bool IsOldMod { get; private set; }

	public int BuildSVNRevision
	{
		get
		{
			if (BuildVersion != null)
			{
				int num = BuildVersion.LastIndexOf(".");
				if (BuildVersion.Length > num + 1 && int.TryParse(BuildVersion.Substring(num + 1), out var result))
				{
					return result;
				}
			}
			return -1;
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Name", Name);
		info.AddValue("Description", Description);
		info.AddValue("ModType", ModType.ToString());
		info.AddValue("ModVersion", ModVersion);
		info.AddValue("BuildVersion", BuildVersion);
		info.AddValue("PublishedFileId", PublishedFileId);
		info.AddValue("AppliedFiles", AppliedFiles);
		info.AddValue("Thumbnail", Thumbnail.EncodeToPNG());
	}

	public GHModMetaData(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "Name":
					Name = info.GetString("Name");
					break;
				case "Description":
					Description = info.GetString("Description");
					break;
				case "ModType":
					ModType = ModTypes.SingleOrDefault((EModType s) => s.ToString() == info.GetString("ModType"));
					break;
				case "ModVersion":
					ModVersion = info.GetInt32("ModVersion");
					break;
				case "BuildVersion":
					BuildVersion = info.GetString("BuildVersion");
					break;
				case "PublishedFileId":
					PublishedFileId = (ulong)info.GetValue("PublishedFileId", typeof(ulong));
					break;
				case "AppliedFiles":
					AppliedFiles = (List<string>)info.GetValue("AppliedFiles", typeof(List<string>));
					break;
				case "Thumbnail":
				{
					byte[] data = (byte[])info.GetValue("Thumbnail", typeof(byte[]));
					Thumbnail = new Texture2D(2, 2);
					Thumbnail.LoadImage(data);
					break;
				}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to deserialize GHModMetaData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		IsOldMod = false;
	}

	public GHModMetaData(string name, string description, EModType modType, Texture2D thumbnail)
	{
		Name = name;
		Description = description;
		ModType = modType;
		Thumbnail = thumbnail;
		ModVersion = 1;
		BuildVersion = Application.version;
		PublishedFileId = 0uL;
		AppliedFiles = new List<string>();
		IsOldMod = false;
	}

	public GHModMetaData(ModMetadata oldMetaData, string name, Texture2D thumbnail)
	{
		Name = name;
		Description = string.Empty;
		ModType = EModType.Language;
		Thumbnail = thumbnail;
		ModVersion = oldMetaData.Version;
		BuildVersion = oldMetaData.BuildVersion;
		PublishedFileId = oldMetaData.PublishedFileId;
		AppliedFiles = oldMetaData.AppliedFiles;
		IsOldMod = true;
	}

	public bool Save(string saveDir)
	{
		try
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.ShowProgress();
			}
			string path = Path.Combine(saveDir, "gloom.mod");
			using MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, this);
			PlatformLayer.FileSystem.WriteFile(memoryStream.ToArray(), path);
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while trying to save Mod Metadata.\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
		finally
		{
			if (Singleton<AutoSaveProgress>.Instance != null)
			{
				Singleton<AutoSaveProgress>.Instance.HideProgress();
			}
		}
		return true;
	}

	public string WritePreviewFile(string saveDir)
	{
		string text = Path.Combine(saveDir, "preview.png");
		try
		{
			if (PlatformLayer.FileSystem.ExistsFile(text))
			{
				PlatformLayer.FileSystem.RemoveFile(text);
			}
			PlatformLayer.FileSystem.WriteFile(Thumbnail.EncodeToPNG(), text);
			return text;
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while trying to write mod preview file.\n" + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
	}
}
