using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using Newtonsoft.Json;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class AutoTestData : CCustomLevelData
{
	public ScenarioState ExpectedResultingScenarioState;

	public int ChoreographerStepCountUntilResult;

	public DateTime ExpectedStateTimeStamp;

	public CAutoLog RecordedUIActions;

	public float CustomTimeout;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ExpectedResultingScenarioState", ExpectedResultingScenarioState);
		info.AddValue("ChoreographerStepCountUntilResult", ChoreographerStepCountUntilResult);
		info.AddValue("ExpectedStateTimeStamp", ExpectedStateTimeStamp);
		info.AddValue("RecordedUIActions", RecordedUIActions);
		info.AddValue("CustomTimeout", CustomTimeout);
	}

	public AutoTestData(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "ExpectedResultingScenarioState":
					ExpectedResultingScenarioState = (ScenarioState)info.GetValue("ExpectedResultingScenarioState", typeof(ScenarioState));
					break;
				case "ChoreographerStepCountUntilResult":
					ChoreographerStepCountUntilResult = info.GetInt32("ChoreographerStepCountUntilResult");
					break;
				case "ExpectedStateTimeStamp":
					ExpectedStateTimeStamp = (DateTime)info.GetValue("ExpectedStateTimeStamp", typeof(DateTime));
					break;
				case "RecordedUIActions":
					RecordedUIActions = (CAutoLog)info.GetValue("RecordedUIActions", typeof(CAutoLog));
					break;
				case "CustomTimeout":
					CustomTimeout = info.GetSingle("CustomTimeout");
					break;
				}
			}
			catch (Exception ex)
			{
				LogUtils.LogError("Exception while trying to deserialize AutoTestData entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
		if (CustomTimeout == 0f)
		{
			CustomTimeout = 300f;
		}
	}

	public AutoTestData(CCustomLevelData data)
		: base(data)
	{
		ChoreographerStepCountUntilResult = 0;
		ExpectedStateTimeStamp = DateTime.Now;
		RecordedUIActions = new CAutoLog();
		CustomTimeout = 300f;
	}

	public static string ConvertAutotestToJSON(string filePath)
	{
		AutoTestData autoTestData;
		using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
		{
			SaveData.Instance.CurrentFileBeingDeserialized = filePath;
			autoTestData = new BinaryFormatter
			{
				Binder = new SerializationBinding()
			}.Deserialize(fileStream) as AutoTestData;
			fileStream.Close();
		}
		if (autoTestData != null)
		{
			return JsonConvert.SerializeObject(autoTestData, Formatting.Indented);
		}
		return string.Empty;
	}
}
