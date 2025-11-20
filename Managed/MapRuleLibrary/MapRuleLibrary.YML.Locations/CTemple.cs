using System;
using System.Collections.Generic;
using SharedLibrary.Client;

namespace MapRuleLibrary.YML.Locations;

public class CTemple
{
	public string ID { get; private set; }

	public List<TempleYML.TempleBlessingDefinition> AvailableBlessings { get; private set; }

	public List<Tuple<int, string>> DonationTable { get; private set; }

	public string FileName { get; private set; }

	public CTemple(string id, List<TempleYML.TempleBlessingDefinition> availableBlessings, List<Tuple<int, string>> donationTable, string fileName)
	{
		ID = id;
		AvailableBlessings = availableBlessings;
		DonationTable = donationTable;
		FileName = fileName;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Temple ID specified for temple in file " + FileName);
			return false;
		}
		if (AvailableBlessings == null || AvailableBlessings.Count < 1)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No valid Blessings specified for temple in file " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(List<TempleYML.TempleBlessingDefinition> availableBlessings, List<Tuple<int, string>> donationTable)
	{
		if (availableBlessings != null && availableBlessings.Count > 0)
		{
			AvailableBlessings = availableBlessings;
		}
		if (donationTable != null && donationTable.Count > 0)
		{
			DonationTable = donationTable;
		}
	}
}
