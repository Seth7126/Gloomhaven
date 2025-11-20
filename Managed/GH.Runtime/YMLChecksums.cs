using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Permissions;
using UnityEngine;

[Serializable]
public class YMLChecksums : ISerializable
{
	public const string YMLChecksumsFilename = "GloomData.dat";

	public List<string> YMLChecksumsFilenames { get; private set; }

	public List<string> YMLChecksumsValues { get; private set; }

	public string SavePath => Path.Combine(Application.streamingAssetsPath, "GloomData.dat");

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("YMLChecksumsFilenames", YMLChecksumsFilenames);
		info.AddValue("YMLChecksumsValues", YMLChecksumsValues);
	}

	public YMLChecksums(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "YMLChecksumsFilenames"))
				{
					if (name == "YMLChecksumsValues")
					{
						YMLChecksumsValues = (List<string>)info.GetValue("YMLChecksumsValues", typeof(List<string>));
					}
				}
				else
				{
					YMLChecksumsFilenames = (List<string>)info.GetValue("YMLChecksumsFilenames", typeof(List<string>));
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception while trying to deserialize YMLChecksums entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public YMLChecksums()
	{
		YMLChecksumsFilenames = new List<string>();
		YMLChecksumsValues = new List<string>();
	}

	public void CalculateRulebaseChecksums(string rulebasePath)
	{
		YMLChecksumsFilenames = (from s in Directory.GetFiles(RootSaveData.CoreRulebasePath, "*.ruleset", SearchOption.TopDirectoryOnly)
			select s.Substring(RootSaveData.CoreRulebasePath.Length + 1)).ToList();
		foreach (string yMLChecksumsFilename in YMLChecksumsFilenames)
		{
			string path = Path.Combine(rulebasePath, yMLChecksumsFilename);
			using MD5 mD = MD5.Create();
			using FileStream inputStream = File.OpenRead(path);
			YMLChecksumsValues.Add(BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", string.Empty).ToLowerInvariant());
		}
	}

	public bool Save()
	{
		try
		{
			using (FileStream serializationStream = File.Open(SavePath, FileMode.Create, FileAccess.Write))
			{
				new BinaryFormatter().Serialize(serializationStream, this);
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An exception occurred attempting to save YMLChecksums.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return false;
	}

	public bool Load()
	{
		if (File.Exists(SavePath))
		{
			try
			{
				using (FileStream serializationStream = File.Open(SavePath, FileMode.Open, FileAccess.Read))
				{
					YMLChecksums yMLChecksums = new BinaryFormatter().Deserialize(serializationStream) as YMLChecksums;
					YMLChecksumsFilenames = yMLChecksums.YMLChecksumsFilenames;
					YMLChecksumsValues = yMLChecksums.YMLChecksumsValues;
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("Unable to load YMLChecksums data at " + SavePath + ".\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		return false;
	}
}
