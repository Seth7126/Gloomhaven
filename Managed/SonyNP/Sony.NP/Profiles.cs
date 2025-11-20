namespace Sony.NP;

public class Profiles
{
	public class RealName
	{
		public const int MAX_SIZE_FIRST_NAME = 16;

		public const int MAX_SIZE_MIDDLE_NAME = 16;

		public const int MAX_SIZE_LAST_NAME = 16;

		internal string firstName = "";

		internal string middleName = "";

		internal string lastName = "";

		public string FirstName => firstName;

		public string MiddleName => middleName;

		public string LastName => lastName;

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RealNameBegin);
			buffer.ReadString(ref firstName);
			buffer.ReadString(ref middleName);
			buffer.ReadString(ref lastName);
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.RealNameEnd);
		}
	}

	public class Profile
	{
		public enum RelationTypes
		{
			notSet,
			me,
			friends,
			requestingFriend,
			requestedFriend,
			blocked,
			friendOfFriends,
			noRelationship
		}

		public enum PersonalDetailsTypes
		{
			none,
			realName,
			verifiedAccountDisplayName
		}

		public const int MAX_SIZE_ABOUT_ME = 140;

		public const int MAX_SIZE_AVATAR_URL = 128;

		public const int MAX_NUM_LANGUAGES_USED = 3;

		public const int MAX_SIZE_VERIFIED_ACCOUNT_DISPLAY_NAME = 32;

		public const int MAX_SIZE_PROFILE_PICTURE_URL = 256;

		internal Core.OnlineUser onlineUser = new Core.OnlineUser();

		internal RelationTypes relationType;

		internal Core.LanguageCode[] languagesUsed = new Core.LanguageCode[3];

		internal Core.CountryCode country = new Core.CountryCode();

		internal PersonalDetailsTypes personalDetailsType;

		internal RealName realName;

		internal string verifiedAccountDisplayName;

		internal string aboutMe = "";

		internal string avatarUrl = "";

		internal string profilePictureUrl = "";

		internal bool isVerifiedAccount;

		public Core.OnlineUser OnlineUser => onlineUser;

		public RelationTypes RelationType => relationType;

		public Core.CountryCode Country => country;

		public Core.LanguageCode[] LanguagesUsed => languagesUsed;

		public PersonalDetailsTypes PersonalDetailsType => personalDetailsType;

		public RealName RealName
		{
			get
			{
				if (personalDetailsType != PersonalDetailsTypes.realName)
				{
					throw new NpToolkitException("Can't access RealName unless PersonalDetailsType is PersonalDetailsType.realName");
				}
				return realName;
			}
		}

		public string VerifiedAccountDisplayName
		{
			get
			{
				if (personalDetailsType != PersonalDetailsTypes.verifiedAccountDisplayName)
				{
					throw new NpToolkitException("Can't access VerifiedAccountDisplayName unless PersonalDetailsType is PersonalDetailsType.verifiedAccountDisplayName");
				}
				return verifiedAccountDisplayName;
			}
		}

		public bool IsVerifiedAccount => isVerifiedAccount;

		public string AboutMe => aboutMe;

		public string AvatarUrl => avatarUrl;

		public string ProfilePictureUrl => profilePictureUrl;

		public Profile()
		{
			for (int i = 0; i < 3; i++)
			{
				languagesUsed[i] = new Core.LanguageCode();
			}
		}

		internal void Read(MemoryBuffer buffer)
		{
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProfileBegin);
			onlineUser.Read(buffer);
			relationType = (RelationTypes)buffer.ReadUInt32();
			uint num = buffer.ReadUInt32();
			if (num != 3)
			{
				throw new NpToolkitException("Unexpected language array size in Profile. Should be " + 3);
			}
			for (int i = 0; i < 3; i++)
			{
				languagesUsed[i].Read(buffer);
			}
			country.Read(buffer);
			personalDetailsType = (PersonalDetailsTypes)buffer.ReadUInt32();
			if (personalDetailsType == PersonalDetailsTypes.realName)
			{
				realName = new RealName();
				realName.Read(buffer);
			}
			else if (personalDetailsType == PersonalDetailsTypes.verifiedAccountDisplayName)
			{
				verifiedAccountDisplayName = "";
				buffer.ReadString(ref verifiedAccountDisplayName);
			}
			buffer.ReadString(ref aboutMe);
			buffer.ReadString(ref avatarUrl);
			buffer.ReadString(ref profilePictureUrl);
			isVerifiedAccount = buffer.ReadBool();
			buffer.CheckMarker(MemoryBuffer.BufferIntegrityChecks.ProfileEnd);
		}

		public override string ToString()
		{
			string text = "";
			text += $"{OnlineUser.ToString()} : Relation ({RelationType}) CC ({Country.ToString()}) PT ({PersonalDetailsType}) Lang1 ({languagesUsed[0].ToString()})\n";
			if (PersonalDetailsType == PersonalDetailsTypes.realName)
			{
				text += $" RN ({RealName.FirstName} {RealName.MiddleName} {RealName.LastName})\n";
			}
			else if (PersonalDetailsType == PersonalDetailsTypes.verifiedAccountDisplayName)
			{
				text += $" VDN ({VerifiedAccountDisplayName})\n";
			}
			return text + $" Verified Account ({IsVerifiedAccount})";
		}
	}
}
