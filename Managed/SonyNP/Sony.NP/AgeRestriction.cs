namespace Sony.NP;

public struct AgeRestriction
{
	internal string countryCode;

	internal int age;

	public Core.CountryCode CountryCode
	{
		get
		{
			Core.CountryCode countryCode = new Core.CountryCode();
			countryCode.code = this.countryCode;
			return countryCode;
		}
		set
		{
			countryCode = value.code;
		}
	}

	public int Age
	{
		get
		{
			return age;
		}
		set
		{
			age = value;
		}
	}

	public AgeRestriction(int age, Core.CountryCode countryCode)
	{
		this.age = age;
		this.countryCode = countryCode.code;
	}

	public void Init()
	{
		countryCode = "";
		age = 0;
	}
}
