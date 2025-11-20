using System;
using System.Runtime.InteropServices;

namespace Sony.NP;

public struct ContentRestriction
{
	public const int MAX_AGE_RESTRICTIONS = 100;

	public const int NP_NO_AGE_RESTRICTION = 0;

	internal int defaultAgeRestriction;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
	internal AgeRestriction[] ageRestrictions;

	internal int numAgeRestictions;

	[MarshalAs(UnmanagedType.I1)]
	internal bool applyContentRestriction;

	public int DefaultAgeRestriction
	{
		get
		{
			return defaultAgeRestriction;
		}
		set
		{
			defaultAgeRestriction = value;
		}
	}

	public AgeRestriction[] AgeRestrictions
	{
		get
		{
			if (numAgeRestictions == 0)
			{
				return null;
			}
			AgeRestriction[] array = new AgeRestriction[numAgeRestictions];
			Array.Copy(ageRestrictions, array, numAgeRestictions);
			return array;
		}
		set
		{
			if (value != null)
			{
				if (value.Length > 100)
				{
					throw new NpToolkitException("The size of the array is larger than " + 100);
				}
				value.CopyTo(ageRestrictions, 0);
				numAgeRestictions = value.Length;
			}
			else
			{
				numAgeRestictions = 0;
			}
		}
	}

	public bool ApplyContentRestriction
	{
		get
		{
			return applyContentRestriction;
		}
		set
		{
			applyContentRestriction = value;
		}
	}

	public void Init()
	{
		defaultAgeRestriction = 0;
		ageRestrictions = new AgeRestriction[100];
		numAgeRestictions = 0;
		applyContentRestriction = true;
		for (int i = 0; i < 100; i++)
		{
			ageRestrictions[i].Init();
		}
	}
}
