using System;
using UnityEngine;

namespace Assets.Script.GUI.MainMenu.Modding;

internal class GHModWrapper : IMod
{
	public GHMod Mod;

	public string Name => Mod.MetaData.Name;

	public string Description => Mod.MetaData.Description;

	public GHModMetaData.EModType ModType => Mod.MetaData.ModType;

	public Texture2D Thumbnail => Mod.MetaData.Thumbnail;

	public string Version => Mod.MetaData.ModVersion.ToString();

	public bool IsCustomMod => Mod.IsLocalMod;

	public int Rating => (int)Math.Ceiling(Mod.Rating * 5f);

	public bool IsValid => Mod.IsValid;

	public string LastValidationResultsFile => Mod.LastValidationResultsFile;

	public GHModWrapper(GHMod mod)
	{
		Mod = mod;
	}

	protected bool Equals(GHModWrapper other)
	{
		return object.Equals(Mod, other.Mod);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((GHModWrapper)obj);
	}

	public override int GetHashCode()
	{
		if (Mod == null)
		{
			return 0;
		}
		return Mod.GetHashCode();
	}

	public static bool operator ==(GHModWrapper left, GHModWrapper right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(GHModWrapper left, GHModWrapper right)
	{
		return !object.Equals(left, right);
	}
}
