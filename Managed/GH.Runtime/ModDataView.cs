using UnityEngine;

public class ModDataView
{
	public string Name { get; set; }

	public string Description { get; set; }

	public GHModMetaData.EModType ModType { get; set; }

	public Texture2D Thumbnail { get; set; }

	public ModDataView(string name, string description, GHModMetaData.EModType modType, Texture2D thumbnail)
	{
		Name = name;
		Description = description;
		ModType = modType;
		Thumbnail = thumbnail;
	}
}
