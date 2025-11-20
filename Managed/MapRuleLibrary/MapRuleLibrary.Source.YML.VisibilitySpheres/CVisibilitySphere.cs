using System.Collections.Generic;
using MapRuleLibrary.YML.Shared;
using MapRuleLibrary.YML.VisibilitySpheres;
using SharedLibrary.Client;

namespace MapRuleLibrary.Source.YML.VisibilitySpheres;

public class CVisibilitySphere
{
	public string ID { get; private set; }

	public List<VisibilitySphereYML.VisibilitySphereDefinition> SphereDefinitions { get; private set; }

	public CUnlockCondition UnlockCondition { get; private set; }

	public string FileName { get; private set; }

	public CVisibilitySphere(string id, List<VisibilitySphereYML.VisibilitySphereDefinition> sphereDefinitions, CUnlockCondition unlockCondition, string fileName)
	{
		ID = id;
		SphereDefinitions = sphereDefinitions;
		UnlockCondition = unlockCondition;
		FileName = fileName;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Visibility Sphere ID specified for VisibilitySphere in file " + FileName);
			return false;
		}
		if (SphereDefinitions == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Sphere Definition specified for VisibilitySphere in file " + FileName);
			return false;
		}
		if (UnlockCondition == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid unlock Condition specified for VisibilitySphere in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(List<VisibilitySphereYML.VisibilitySphereDefinition> sphereDefinitions, CUnlockCondition unlockCondition)
	{
		if (sphereDefinitions != null && sphereDefinitions.Count > 0)
		{
			SphereDefinitions = sphereDefinitions;
		}
		if (unlockCondition != null)
		{
			UnlockCondition = unlockCondition;
		}
	}
}
