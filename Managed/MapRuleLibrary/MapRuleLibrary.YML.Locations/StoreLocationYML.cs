using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Locations;

public class StoreLocationYML
{
	public const int MinimumFilesRequired = 1;

	public List<CStoreLocation> LoadedYML { get; private set; }

	public StoreLocationYML()
	{
		LoadedYML = new List<CStoreLocation>();
	}

	public bool ProcessFile(StreamReader fileStream, string fileName)
	{
		try
		{
			bool flag = true;
			YamlParser yamlParser = new YamlParser();
			TextInput input = new TextInput(fileStream.ReadToEnd());
			bool success;
			YamlStream yamlStream = yamlParser.ParseYamlStream(input, out success);
			string id = null;
			string localisedName = null;
			string localisedDescription = null;
			string mesh = null;
			TileIndex mapLocation = null;
			CUnlockCondition unlockCondition = null;
			EHQStores eHQStores = EHQStores.None;
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var value))
						{
							id = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedName":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value3))
						{
							localisedName = value3;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "LocalisedDescription":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value2))
						{
							localisedDescription = value2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "Mesh":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, entry.Key.ToString(), fileName, out var value4))
						{
							mesh = value4;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "MapLocation":
					{
						if (YMLShared.GetIntList(entry.Value, entry.Key.ToString(), fileName, out var values))
						{
							if (values.Count == 2)
							{
								mapLocation = new TileIndex(values[0], values[1]);
								break;
							}
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid MapLocation entry " + entry.Key?.ToString() + ".  Must be two integers as coordinates in a list, e.g. [12, 34].  File:\n" + fileName);
							flag = false;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "UnlockCondition":
					{
						if (CUnlockCondition.Parse(entry, fileName, out var unlockCondition2))
						{
							unlockCondition = unlockCondition2;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "StoreType":
						eHQStores = CHeadquarters.HQStores.SingleOrDefault((EHQStores x) => x.ToString() == entry.Value.ToString());
						if (eHQStores == EHQStores.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid HQStoreType: " + entry.Value?.ToString() + ". File:\n" + fileName);
							flag = false;
						}
						break;
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of Village file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (id == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					CStoreLocation cStoreLocation = LoadedYML.SingleOrDefault((CStoreLocation s) => s.ID == id);
					if (cStoreLocation == null)
					{
						LoadedYML.Add(new CStoreLocation(id, localisedName, localisedDescription, mesh, mapLocation, unlockCondition, fileName, eHQStores));
					}
					else
					{
						cStoreLocation.UpdateData(localisedName, localisedDescription, mesh, mapLocation, unlockCondition);
					}
				}
				return flag;
			}
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unable to parse yml. File:\n" + fileName + "\n" + string.Join("\n", yamlParser.Errors.Select((Pair<int, string> x) => x.Right)));
			return false;
		}
		catch (Exception ex)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}
}
