using System;
using System.IO;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using YamlFormats;

namespace SharedLibrary.YML;

public class SeedYML
{
	private static SeedYML _loadedYML;

	public static SeedYML LoadedYML
	{
		get
		{
			if (_loadedYML == null)
			{
				_loadedYML = new SeedYML();
				_loadedYML.ProcessItem(Path.GetFullPath(Path.Combine(SharedClient.RulebaseDataRoot, "..", "Seed.yml")));
			}
			return _loadedYML;
		}
	}

	public int Seed { get; private set; }

	public bool Random { get; private set; }

	public string FileName { get; private set; }

	public string FileContents { get; private set; }

	public void ProcessItem(string fileName)
	{
		int? num = null;
		if (File.Exists(fileName))
		{
			try
			{
				FileName = fileName;
				FileContents = File.ReadAllText(fileName);
				Random = false;
				YamlParser yamlParser = new YamlParser();
				TextInput input = new TextInput(FileContents);
				bool success = false;
				YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
				if (success)
				{
					foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
					{
						if (entry.Key.ToString() == "Seed" && YMLShared.GetStringPropertyValue(entry.Value, "Seed", fileName, out var value))
						{
							int result;
							if (value.ToLower().Trim() == "random")
							{
								Random = true;
								Random random = new Random();
								num = random.Next();
							}
							else if (int.TryParse(value, out result))
							{
								num = result;
							}
							else
							{
								SharedClient.ValidationRecord.RecordParseFailure(fileName, " Invalid Seed value in " + fileName);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Failed to parse " + fileName + "\n" + ex.Message + "\n" + ex.StackTrace);
			}
		}
		if (!num.HasValue)
		{
			Random = true;
			Random random2 = new Random();
			num = random2.Next();
		}
		Seed = num.Value;
		DLLDebug.LogInfo("Seed has been set to " + num);
	}
}
