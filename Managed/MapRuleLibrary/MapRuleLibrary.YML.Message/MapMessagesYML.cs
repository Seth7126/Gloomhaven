using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapRuleLibrary.YML.Shared;
using ScenarioRuleLibrary;
using SharedLibrary.Client;
using SharedLibrary.YML;
using YamlFormats;

namespace MapRuleLibrary.YML.Message;

public class MapMessagesYML
{
	public const int MinimumFilesRequired = 1;

	public List<CMapMessage> LoadedYML { get; private set; }

	public MapMessagesYML()
	{
		LoadedYML = new List<CMapMessage>();
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
			string name = string.Empty;
			List<MapDialogueLine> list = new List<MapDialogueLine>();
			EMapMessageTrigger eMapMessageTrigger = EMapMessageTrigger.None;
			CUnlockCondition unlockCondition = null;
			if (success)
			{
				foreach (MappingEntry entry in (yamlStream.Documents[0].Root as Mapping).Entries)
				{
					switch (entry.Key.ToString())
					{
					case "ID":
					{
						if (!YMLShared.GetStringPropertyValue(entry.Value, "ID", fileName, out var _))
						{
							flag = false;
						}
						break;
					}
					case "Name":
					{
						if (YMLShared.GetStringPropertyValue(entry.Value, "Name", fileName, out var value))
						{
							name = value;
						}
						else
						{
							flag = false;
						}
						break;
					}
					case "DialogueLines":
					{
						if (YMLShared.GetSequence(entry, fileName, out var sequence))
						{
							foreach (DataItem entry2 in sequence.Entries)
							{
								if (entry2 is Mapping)
								{
									if (ParseDialogueLine((entry2 as Mapping).Entries, fileName, out var mapDialogueLine))
									{
										list.Add(mapDialogueLine);
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected DialogueLines entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
									flag = false;
								}
							}
						}
						else
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected DialogueLines entry, must be sequence (list) of Key: Value mappings. File: " + fileName);
							flag = false;
						}
						break;
					}
					case "MapMessageTrigger":
						eMapMessageTrigger = CMapMessage.MessageTriggers.SingleOrDefault((EMapMessageTrigger x) => x.ToString() == entry.Value.ToString());
						if (eMapMessageTrigger == EMapMessageTrigger.None)
						{
							SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid ShowAt: " + entry.Value?.ToString() + ". File:\n" + fileName);
							flag = false;
						}
						break;
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
					default:
						SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of MapMessage file " + fileName);
						flag = false;
						break;
					case "Parser":
						break;
					}
				}
				if (flag)
				{
					CMapMessage cMapMessage = LoadedYML.SingleOrDefault((CMapMessage s) => s.MessageID == name);
					if (cMapMessage == null)
					{
						LoadedYML.Add(new CMapMessage(name, list, eMapMessageTrigger, unlockCondition, fileName));
					}
					else
					{
						cMapMessage.UpdateData(list, eMapMessageTrigger, unlockCondition);
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

	public static bool ParseDialogueLine(List<MappingEntry> entries, string fileName, out MapDialogueLine mapDialogueLine)
	{
		mapDialogueLine = null;
		bool flag = true;
		string text = string.Empty;
		string character = string.Empty;
		MapDialogueLine.EExpression eExpression = MapDialogueLine.EExpression.Default;
		TileIndex cameraLocation = null;
		string narrativeImageID = null;
		string text2 = null;
		foreach (MappingEntry entry in entries)
		{
			switch (entry.Key.ToString())
			{
			case "Text":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "Text", fileName, out var value4))
				{
					text = value4;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Character":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "Character", fileName, out var value2))
				{
					character = value2;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "Expression":
				eExpression = MapDialogueLine.Expressions.SingleOrDefault((MapDialogueLine.EExpression x) => x.ToString() == entry.Value.ToString());
				if (eExpression == MapDialogueLine.EExpression.Default)
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid Expression: " + entry.Value?.ToString() + ". File:\n" + fileName);
					flag = false;
				}
				break;
			case "CameraLocation":
			{
				if (YMLShared.GetIntList(entry.Value, entry.Key.ToString(), fileName, out var values))
				{
					if (values.Count == 2)
					{
						cameraLocation = new TileIndex(values[0], values[1]);
						break;
					}
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CameraLocation entry " + entry.Key?.ToString() + ".  Must be two integers as coordinates in a list, e.g. [12, 34].  File:\n" + fileName);
					flag = false;
				}
				else
				{
					SharedClient.ValidationRecord.RecordParseFailure(fileName, "Invalid CameraLocation entry " + entry.Key?.ToString() + ".  File:\n" + fileName);
					flag = false;
				}
				break;
			}
			case "NarrativeImageId":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "NarrativeImageId", fileName, out var value3))
				{
					narrativeImageID = value3;
				}
				else
				{
					flag = false;
				}
				break;
			}
			case "NarrativeAudioId":
			{
				if (YMLShared.GetStringPropertyValue(entry.Value, "NarrativeAudioId", fileName, out var value))
				{
					text2 = value;
				}
				else
				{
					flag = false;
				}
				break;
			}
			default:
				SharedClient.ValidationRecord.RecordParseFailure(fileName, "Unexpected entry " + entry.Key.ToString() + " in root of MapMessage file " + fileName);
				flag = false;
				break;
			}
		}
		if (flag)
		{
			mapDialogueLine = new MapDialogueLine(text, character, eExpression, cameraLocation, narrativeImageID, text2 ?? text);
		}
		return flag;
	}
}
