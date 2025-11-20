using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class ModMetadata
{
	public enum ModType
	{
		None,
		Rulebase,
		Level
	}

	public int Version;

	public string BuildVersion = "";

	public ulong PublishedFileId;

	[NonSerialized]
	public Texture2D Thumbnail;

	[OptionalField]
	public ModType Type;

	public List<string> AppliedFiles { get; set; }
}
