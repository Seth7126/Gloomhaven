using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.Source.YML.VisibilitySpheres;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.VisibilitySpheres;

public class VisibilitySphereYML
{
	public class VisibilitySphereDefinition : IEquatable<VisibilitySphereDefinition>
	{
		public CVector3 MapLocation { get; private set; }

		public int Radius { get; private set; }

		public VisibilitySphereDefinition(TileIndex mapLocation, int radius)
		{
			MapLocation = MapYMLShared.GetScreenPointFromMap(mapLocation.X, mapLocation.Y);
			Radius = radius;
		}

		public VisibilitySphereDefinition(CVector3 mapLocation, int radius)
		{
			MapLocation = mapLocation;
			Radius = radius;
		}

		public bool Equals(VisibilitySphereDefinition other)
		{
			if (!CVector3.Compare(MapLocation, other.MapLocation))
			{
				return false;
			}
			if (Radius != other.Radius)
			{
				return false;
			}
			return true;
		}
	}

	public const int MinimumFilesRequired = 1;

	public static int MinimumRadius = 10;

	public List<CVisibilitySphere> LoadedYML { get; private set; }

	public VisibilitySphereYML()
	{
		LoadedYML = new List<CVisibilitySphere>();
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
			List<VisibilitySphereDefinition> list = new List<VisibilitySphereDefinition>();
			CUnlockCondition unlockCondition = null;
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
					case "SphereDefinitions":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence))
						{
							foreach (DataItem entry2 in sequence.Entries)
							{
								if (entry2 is Mapping)
								{
									if (ParseSphereDefinition((entry2 as Mapping).Entries, fileName, out var visibilitySphereDefinition))
									{
										list.Add(visibilitySphereDefinition);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									flag = false;
								}
							}
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
					}
				}
				if (id == null)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "No ID specified in " + fileName);
					flag = false;
				}
				if (flag)
				{
					CVisibilitySphere cVisibilitySphere = LoadedYML.SingleOrDefault((CVisibilitySphere s) => s.ID == id);
					if (cVisibilitySphere == null)
					{
						LoadedYML.Add(new CVisibilitySphere(id, list, unlockCondition, fileName));
					}
					else
					{
						cVisibilitySphere.UpdateData(list, unlockCondition);
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

	public static bool ParseSphereDefinition(List<MappingEntry> entries, string fileName, out VisibilitySphereDefinition visibilitySphereDefinition)
	{
		bool flag = true;
		visibilitySphereDefinition = null;
		TileIndex mapLocation = null;
		int value = 0;
		foreach (MappingEntry entry in entries)
		{
			string text = entry.Key.ToString();
			List<int> values;
			if (!(text == "MapLocation"))
			{
				if (text == "Radius")
				{
					if (!YMLShared.GetIntPropertyValue(entry.Value, entry.Key.ToString(), fileName, out value))
					{
						flag = false;
					}
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of MapMessage file " + fileName);
					flag = false;
				}
			}
			else if (YMLShared.GetIntList(entry.Value, entry.Key.ToString(), fileName, out values))
			{
				if (values.Count == 2)
				{
					mapLocation = new TileIndex(values[0], values[1]);
					continue;
				}
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid MapLocation entry " + entry.Key?.ToString() + ".  Must be two integers as coordinates in a list, e.g. [12, 34].  File:\n" + fileName);
				flag = false;
			}
			else
			{
				flag = false;
			}
		}
		if (value < MinimumRadius)
		{
			SharedClient.ValidationRecord.RecordParseFailure(fileName, "VisibilitySphereDefinition Radius is less than minimum value of " + MinimumRadius + ", set to minimum value. File: " + fileName);
			flag = false;
		}
		if (flag)
		{
			visibilitySphereDefinition = new VisibilitySphereDefinition(mapLocation, value);
		}
		return flag;
	}
}
