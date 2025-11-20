using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExIni;

public class IniFile
{
	private readonly IniComment _comments;

	private readonly List<IniSection> _sections;

	public IniSection this[string sec] => CreateSection(sec);

	public IniComment Comments => _comments;

	public List<IniSection> Sections => _sections;

	public IniFile()
	{
		_comments = new IniComment();
		_sections = new List<IniSection>();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < Sections.Count; i++)
		{
			IniSection iniSection = Sections[i];
			if (iniSection.Comments.Comments.Any())
			{
				stringBuilder.AppendLine(iniSection.Comments.ToString());
			}
			stringBuilder.AppendLine(iniSection.ToString());
			foreach (IniKey key in iniSection.Keys)
			{
				if (key.Comments.Comments.Any())
				{
					stringBuilder.AppendLine(key.Comments.ToString());
				}
				stringBuilder.AppendLine(key.ToString());
			}
			if (i < Sections.Count - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		if (Comments.Comments.Any())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(Comments.ToString());
		}
		return stringBuilder.ToString();
	}

	public IniSection CreateSection(string section)
	{
		IniSection section2 = GetSection(section);
		if (section2 != null)
		{
			return section2;
		}
		IniSection iniSection = new IniSection(section);
		_sections.Add(iniSection);
		return iniSection;
	}

	public bool DeleteSection(string section)
	{
		if (!HasSection(section))
		{
			return false;
		}
		Sections.Remove(GetSection(section));
		return true;
	}

	public IniSection GetSection(string section)
	{
		if (!HasSection(section))
		{
			return null;
		}
		return _sections.FirstOrDefault((IniSection iniSection) => iniSection.Section == section);
	}

	public bool HasSection(string section)
	{
		return _sections.Any((IniSection iniSection) => iniSection.Section == section);
	}

	public void Merge(IniFile ini)
	{
		Comments.Append(ini.Comments.Comments.ToArray());
		foreach (IniSection section in ini.Sections)
		{
			IniSection iniSection = this[section.Section];
			iniSection.Comments.Append(section.Comments.Comments.ToArray());
			foreach (IniKey key in section.Keys)
			{
				IniKey iniKey = iniSection[key.Key];
				iniKey.Comments.Append(key.Comments.Comments.ToArray());
				iniKey.Value = key.Value;
			}
		}
	}

	public void Save(string filePath)
	{
		string directoryName = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllText(filePath, ToString(), Encoding.UTF8);
	}

	public static IniFile FromFile(string iniString)
	{
		return IniParser.Parse(File.ReadAllText(iniString));
	}

	public static IniFile FromString(string iniString)
	{
		return IniParser.Parse(iniString);
	}
}
